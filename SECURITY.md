# Security Policy

## Overview

Xcaciv.Command provides a secure plugin execution framework with built-in protections:

- **Instance-based assembly loading** with path restrictions
- **Input validation and sanitization** for all command parameters
- **Environment variable access control** via isolated contexts
- **Audit logging capability** for command execution and environment changes
- **Trusted plugin directories** with verified directory enforcement

This document outlines the security model, threat mitigations, plugin development guidelines, and reporting procedures.

---

## Plugin Security Model

### Trust Boundary

Xcaciv.Command enforces strict trust boundaries for plugin execution:

- **Plugins are loaded from verified directories only**: Directory verification prevents unauthorized plugin loading
- **Each plugin is restricted to its own directory**: No access to parent paths or sibling directories
- **Path-based sandboxing via Xcaciv.Loader 2.0.1**: Enforces `basePathRestriction` to plugin's own directory
- **Security violations are logged**: `SecurityException` raised on path violations; plugins are skipped
- **Framework continues on plugin failure**: Single plugin failure doesn't crash the entire framework

### Plugin Directory Structure

```
verified_base_directory/
??? PluginName1/
?   ??? bin/
?       ??? PluginName1.dll
??? PluginName2/
?   ??? bin/
?       ??? PluginName2.dll
??? PluginName3/
    ??? bin/
        ??? PluginName3.dll
```

Each plugin is isolated in its own directory with:
- Read-only access to its own `bin` directory
- No access to parent directory or sibling plugins
- Load context restricted to `Path.GetDirectoryName(binPath)`

### Threat: Directory Traversal

**Risk**: Plugin attempts to load files from parent directories or system paths using paths like `../../../Windows/System32/`.

**Mitigation**: `basePathRestriction` in AssemblyContext is set to the plugin's own directory:

```csharp
using var context = new AssemblyContext(
    packagePath,
    basePathRestriction: Path.GetDirectoryName(packagePath) ?? Directory.GetCurrentDirectory(),
    securityPolicy: AssemblySecurityPolicy.Default);
```

Any attempt to load assemblies outside this boundary raises `SecurityException`, which is caught, logged, and the plugin is skipped.

### Threat: Plugin Tampering

**Risk**: Attackers modify plugin DLLs after placement in trusted directory.

**Current Status**: Runtime verification via Xcaciv.Loader

**Planned Mitigation**: Digital signatures (Phase X) via Xcaciv.Loader with certificate pinning.

### Threat: Malicious Plugin Execution

**Risk**: Plugin executes unauthorized code (network access, file system operations, etc.).

**Mitigation**: In-process execution model provides:
- No automatic network access (explicit code required)
- No automatic file system access (explicit code required)
- No automatic registry access (explicit code required)
- Environment variables via IEnvironmentContext only (sandboxed)
- Command output only via IIoContext (monitored)

Plugin developers MUST NOT bypass these interfaces.

---

## Plugin Development Guidelines

### Safe Environment Variable Access

Environment variables are accessed via `IEnvironmentContext` with explicit defaults:

**Recommended Pattern:**

```csharp
public override IAsyncEnumerable<string> HandleExecution(IIoContext input, IEnvironmentContext env)
{
    return SafeExecuteAsync(input, env);
}

private async IAsyncEnumerable<string> SafeExecuteAsync(IIoContext input, IEnvironmentContext env)
{
    var apiKey = env.GetValue("PLUGIN_API_KEY", defaultValue: "");
    
    if (string.IsNullOrEmpty(apiKey))
    {
        yield return "Warning: PLUGIN_API_KEY not configured";
        yield break;
    }
    
    try
    {
        // Use apiKey safely
        yield return "Successfully configured";
    }
    catch (Exception ex)
    {
        yield return $"Error: {ex.Message}";
    }
}
```

**Key Points:**
- Always provide a default value (don't assume env var is set)
- Never log or output sensitive values (API keys, tokens, passwords)
- Only approved commands should set environment variables
- Use graceful degradation when required variables are missing

### Safe Input Handling

All command inputs are sanitized before reaching the plugin:

**Validation Rules:**
- Command names: Regex validation `[^-_\da-zA-Z ]` (alphanumeric, dash, underscore, space only)
- Parameters: Already filtered and validated by framework
- Trust level: Parameters are already sanitized; do not perform additional string operations

**Safe Pattern:**

```csharp
public override IAsyncEnumerable<string> HandleExecution(IIoContext input, IEnvironmentContext env)
{
    // Parameters are already sanitized by framework
    if (input.Parameters == null || input.Parameters.Length == 0)
    {
        yield return "No parameters provided";
        yield break;
    }
    
    // Safe to use parameters directly
    foreach (var param in input.Parameters)
    {
        yield return $"Processing: {param}";
    }
}
```

**Anti-Pattern (UNSAFE):**

```csharp
// DON'T DO THIS - Parameters already sanitized, no need for additional parsing
var command = $"cmd.exe /c echo {input.Parameters[0]}"; // Shell injection risk
```

### Safe Output Handling

Command output is passed directly to the output stream:

**Rules:**
- Output is consumed as plain text by default
- Consider HTML encoding if output is consumed by web UI
- Do NOT output secrets (API keys, tokens, passwords, env var values)
- Do NOT output binary data (use serialization formats)

**Safe Pattern:**

```csharp
public override IAsyncEnumerable<string> HandleExecution(IIoContext input, IEnvironmentContext env)
{
    var data = FetchData();
    var serialized = JsonConvert.SerializeObject(data);
    
    yield return serialized; // Text output
}
```

**Anti-Pattern (UNSAFE):**

```csharp
var apiKey = env.GetValue("API_KEY", "");
yield return $"Current API key: {apiKey}"; // NEVER output secrets
```

### Using Audit Logging

Audit logging is automatically performed by the framework:

```csharp
// Command execution is automatically logged with:
// - Command name
// - Parameters (sanitized)
// - Execution time
// - Success/failure status
// - Error message (if failed)

// Environment changes are automatically logged with:
// - Variable name
// - Old value
// - New value
// - Changed timestamp
```

Plugins don't need to implement logging themselves; the framework handles it.

---

## Known Vulnerabilities & Mitigations

### Vulnerability 1: Unbounded Pipeline Memory

**Risk Level:** MEDIUM

**Description**: Infinite or very large output from plugin exhausts memory through unbounded pipeline channels.

**Current Status**: No backpressure; channels are unbounded by default.

**Example Attack**:
```csharp
// Malicious plugin
public override IAsyncEnumerable<string> HandleExecution(IIoContext input, IEnvironmentContext env)
{
    while (true)
    {
        yield return new string('A', 1_000_000); // 1MB per second
    }
}
```

**Mitigation (Planned - Phase 6):**
- Bounded channels with configurable size limits
- Backpressure policies (drop oldest, drop newest, block)
- Execution timeouts with CancellationToken
- Memory monitoring and alerts

**Workaround (Current)**:
- Run command with resource limits (OS-level)
- Monitor process memory usage
- Use Phase 6 when available

### Vulnerability 2: Command Injection via Parameters

**Risk Level:** LOW (mitigated by framework)

**Description**: Parameters contain shell metacharacters that could be injected into system commands.

**Current Status**: Regex sanitization prevents most injection vectors; allow-lists for critical parameters.

**Validation**:
- Command names: `[^-_\da-zA-Z ]` - No special characters
- Parameters: Already filtered before reaching plugin

**Example (SAFE)**:
```csharp
// Framework already sanitized parameters
var param = input.Parameters[0]; // Contains only alphanumeric, dash, underscore, space
```

**Mitigation**: See "Safe Input Handling" section above.

### Vulnerability 3: Environment Variable Leakage

**Risk Level:** LOW (not applicable)

**Description**: Child process inherits all parent environment variables.

**Current Status**: Not applicable - framework uses in-process execution only.

**Mitigation**: Explicit environment variable passing via `IEnvironmentContext` only. Variables are sandboxed per environment context and do not leak to OS process.

### Vulnerability 4: Audit Log Tampering

**Risk Level:** LOW (depends on logger implementation)

**Description**: Audit logs could be modified or deleted after recording.

**Current Status**: Audit logging interface is provided (`IAuditLogger`). Security of audit logs depends on the implementation (e.g., Serilog with secure sink, database with constraints, Azure Application Insights).

**Mitigation**:
- Use secure audit logging implementation (e.g., Serilog to database)
- Apply read-only permissions to audit log storage
- Implement log rotation and archival
- Monitor audit logs for tampering
- Use centralized logging (e.g., Azure Monitor)

**Example (Secure)**:
```csharp
// Use Serilog with SQL Server sink (immutable audit tables)
var logger = new LoggerConfiguration()
    .WriteTo.MSSqlServer(connectionString, new MSSqlServerSinkOptions 
    { 
        TableName = "AuditLog",
        AutoCreateSqlTable = false // Use pre-created immutable table
    })
    .CreateLogger();

var auditLogger = new SerilogAuditLogger(logger);
var controller = new CommandController { AuditLogger = auditLogger };
```

---

## Reporting Security Issues

Security vulnerabilities in Xcaciv.Command should be reported **privately** and **responsibly**.

### Do NOT

- Open public GitHub issues for security vulnerabilities
- Disclose vulnerabilities on social media or forums
- Share exploit code publicly

### DO

Email security reports to: **security@xcaciv.dev**

Include the following information:

1. **Description of vulnerability**: What is the security issue?
2. **Steps to reproduce**: How can the vulnerability be triggered?
3. **Potential impact**: What damage could this cause?
4. **Suggested fix** (if known): How would you fix it?
5. **Your contact information**: How should we reach you?

### Response Timeline

- **48 hours**: Initial acknowledgment of receipt
- **7 days**: Initial assessment and response
- **30 days**: Fix development and testing
- **Release**: Security patch released with credit (if desired)

Example report:

```
Subject: [SECURITY] Vulnerability in Parameter Validation

Description:
The parameter validation regex does not properly escape Unicode characters,
allowing potential injection attacks through special Unicode normalization forms.

Steps to Reproduce:
1. Create a command with parameter: "<U+0041>" (Unicode-encoded 'A')
2. Execute the command
3. Observe that Unicode characters bypass validation

Potential Impact:
Medium - Could allow injection of special characters in specific scenarios

Suggested Fix:
Add Unicode normalization (NFC) before validation and additional checks
for combining characters and other Unicode categories.

Contact: researcher@example.com
```

---

## Security Checklist for Plugin Developers

Use this checklist when developing plugins for Xcaciv.Command:

### Input Validation

- [ ] All parameters are validated (type, length, format)
- [ ] Never assume parameters are safe; re-validate if needed
- [ ] Use allow-lists instead of deny-lists for critical parameters
- [ ] Default to deny (reject unknown values)

### Environment Variables

- [ ] Always provide default values for env vars
- [ ] Handle missing env vars gracefully
- [ ] Never log or output sensitive env var values
- [ ] Only approved commands set env vars

### Output Handling

- [ ] Output is plain text or serialized data (JSON, XML)
- [ ] Never output secrets (keys, tokens, passwords)
- [ ] Never output binary data (use encoding)
- [ ] Consider HTML encoding for web consumption

### Error Handling

- [ ] Exceptions don't leak sensitive information
- [ ] Error messages are user-friendly
- [ ] Stack traces are logged to audit trail only
- [ ] All exceptions are caught and handled

### Resource Management

- [ ] Dispose of resources properly (using statements)
- [ ] Monitor memory usage in loops
- [ ] Avoid infinite loops or very long-running operations
- [ ] Use timeouts for external operations

### Audit & Logging

- [ ] Critical operations are logged via framework
- [ ] Audit logging is enabled in production
- [ ] Logs are regularly reviewed
- [ ] Audit logs are stored securely

---

## Security Best Practices

### For Plugin Developers

1. **Principle of Least Privilege**: Request only necessary permissions
2. **Defense in Depth**: Multiple layers of validation
3. **Fail Securely**: Default to deny on errors
4. **Keep It Simple**: Complex code is harder to audit
5. **Code Review**: Have security experts review your code

### For Framework Users

1. **Verify Plugins**: Only load plugins from trusted sources
2. **Monitor Execution**: Use audit logging in production
3. **Update Regularly**: Keep Xcaciv.Command updated
4. **Segregate Permissions**: Run with minimal required permissions
5. **Test Thoroughly**: Test plugins in staging before production

### For Administrators

1. **Restricted Directories**: Store plugins in read-only directories
2. **Audit Trail**: Enable audit logging; review regularly
3. **Network Isolation**: Run in isolated network if possible
4. **Resource Limits**: Set OS-level resource limits
5. **Incident Response**: Have a plan for security incidents

---

## References

- **Xcaciv.Loader**: Assembly loading with security context
- **OWASP Top 10**: Common vulnerabilities and mitigations
- **CWE/SANS**: Common Weakness Enumeration
- **Microsoft Security Best Practices**: .NET security guidelines

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | December 2024 | Initial security policy |

---

**Last Updated:** December 2024  
**Maintained By:** Xcaciv.Command Team  
**Contact:** security@xcaciv.dev
