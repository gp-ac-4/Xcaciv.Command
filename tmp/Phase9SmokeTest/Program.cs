using Xcaciv.Command;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Tests.TestImpementations;

namespace SmokeTest
{
    /// <summary>
    /// Smoke test to verify Phase 9 API compatibility and basic functionality.
    /// This test ensures existing code patterns continue to work after SSEM improvements.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Phase 9 Smoke Test - API Compatibility ===");
            Console.WriteLine();

            try
            {
                // Test 1: Basic command execution (existing pattern)
                Console.WriteLine("Test 1: Basic Command Execution");
                var controller = new CommandController();
                controller.EnableDefaultCommands();
                var env = new EnvironmentContext();
                var textio = new TestTextIo();

                await controller.Run("Say Hello World", textio, env);
                
                // Say command creates child contexts - check children output
                var output = textio.Children.Any() && textio.Children.First().Output.Any() 
                    ? string.Join("", textio.Children.First().Output)
                    : string.Join("", textio.Output);
                    
                Console.WriteLine($"  Output received: '{output}'");
                Console.WriteLine($"  Children count: {textio.Children.Count}");
                Console.WriteLine($"  Direct output count: {textio.Output.Count}");
                
                bool hasExpectedOutput = output.Contains("Hello World") || 
                    (textio.Children.Any() && textio.Children.First().Output.Any(o => o.Contains("Hello World")));
                
                if (hasExpectedOutput)
                {
                    Console.WriteLine("? PASS: Basic command execution works");
                }
                else
                {
                    Console.WriteLine($"? FAIL: Expected output containing 'Hello World'");
                    Console.WriteLine($"  Actual output: {output}");
                    if (textio.Children.Any())
                    {
                        Console.WriteLine($"  Child outputs: {string.Join(", ", textio.Children.First().Output)}");
                    }
                    Environment.Exit(1);
                }
                Console.WriteLine();

                // Test 2: Pipeline execution (existing pattern)
                Console.WriteLine("Test 2: Pipeline Execution");
                var textio2 = new TestTextIo();
                await controller.Run("Say Hello | Say", textio2, env);
                
                var pipeOutput = string.Join("", textio2.Output);
                if (pipeOutput.Contains("Hello"))
                {
                    Console.WriteLine("? PASS: Pipeline execution works");
                }
                else
                {
                    Console.WriteLine($"? FAIL: Pipeline did not produce expected output: '{pipeOutput}'");
                    Environment.Exit(1);
                }
                Console.WriteLine();

                // Test 3: New feature - Audit logging (backward compatible)
                Console.WriteLine("Test 3: Audit Logging (New Feature - Backward Compatible)");
                var auditLogger = new TestAuditLogger();
                var controller2 = new CommandController { AuditLogger = auditLogger };
                controller2.EnableDefaultCommands();
                var env2 = new EnvironmentContext();
                env2.SetAuditLogger(auditLogger);
                var textio3 = new TestTextIo();

                await controller2.Run("Say Audit Test", textio3, env2);

                if (auditLogger.CommandExecutions.Count > 0 && auditLogger.CommandExecutions[0].CommandName == "SAY")
                {
                    Console.WriteLine("? PASS: Audit logging works");
                }
                else
                {
                    Console.WriteLine("? FAIL: Audit logging did not capture command execution");
                    Environment.Exit(1);
                }
                Console.WriteLine();

                // Test 4: New feature - Pipeline configuration (backward compatible)
                Console.WriteLine("Test 4: Pipeline Configuration (New Feature - Backward Compatible)");
                var controller3 = new CommandController();
                controller3.EnableDefaultCommands();
                controller3.PipelineConfig = new PipelineConfiguration
                {
                    MaxChannelQueueSize = 1000,
                    BackpressureMode = PipelineBackpressureMode.Block
                };
                var textio4 = new TestTextIo();
                
                await controller3.Run("Say Config Test", textio4, env);
                
                // Check children output (Say command creates child contexts)
                bool hasOutput = textio4.Children.Any() && textio4.Children.First().Output.Any();
                
                if (hasOutput)
                {
                    Console.WriteLine("? PASS: Pipeline configuration works");
                }
                else
                {
                    Console.WriteLine("? FAIL: Pipeline configuration caused failure");
                    Console.WriteLine($"  Children count: {textio4.Children.Count}");
                    Environment.Exit(1);
                }
                Console.WriteLine();

                // Test 5: Environment variables (existing pattern)
                Console.WriteLine("Test 5: Environment Variables");
                env.SetValue("TEST_VAR", "test_value");
                var envValue = env.GetValue("TEST_VAR");
                
                if (envValue == "test_value")
                {
                    Console.WriteLine("? PASS: Environment variables work");
                }
                else
                {
                    Console.WriteLine($"? FAIL: Environment variable incorrect: '{envValue}'");
                    Environment.Exit(1);
                }
                Console.WriteLine();

                Console.WriteLine("=== All Smoke Tests Passed ? ===");
                Console.WriteLine();
                Console.WriteLine("Summary:");
                Console.WriteLine("- Basic command execution: ?");
                Console.WriteLine("- Pipeline execution: ?");
                Console.WriteLine("- Audit logging (new): ?");
                Console.WriteLine("- Pipeline configuration (new): ?");
                Console.WriteLine("- Environment variables: ?");
                Console.WriteLine();
                Console.WriteLine("No exceptions thrown - API is stable and backward compatible");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? FATAL ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }
    }

    /// <summary>
    /// Test implementation of IAuditLogger for smoke testing.
    /// </summary>
    public class TestAuditLogger : IAuditLogger
    {
        public List<CommandExecutionLog> CommandExecutions { get; } = new();
        public List<EnvironmentChangeLog> EnvironmentChanges { get; } = new();

        public void LogCommandExecution(string commandName, string[] parameters, DateTime executedAt, TimeSpan duration, bool success, string? errorMessage = null)
        {
            CommandExecutions.Add(new CommandExecutionLog
            {
                CommandName = commandName,
                Parameters = parameters,
                ExecutedAt = executedAt,
                Duration = duration,
                Success = success,
                ErrorMessage = errorMessage
            });
        }

        public void LogEnvironmentChange(string variableName, string? oldValue, string? newValue, string changedBy, DateTime changedAt)
        {
            EnvironmentChanges.Add(new EnvironmentChangeLog
            {
                VariableName = variableName,
                OldValue = oldValue,
                NewValue = newValue,
                ChangedBy = changedBy,
                ChangedAt = changedAt
            });
        }
    }

    public class CommandExecutionLog
    {
        public string CommandName { get; set; } = string.Empty;
        public string[] Parameters { get; set; } = Array.Empty<string>();
        public DateTime ExecutedAt { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class EnvironmentChangeLog
    {
        public string VariableName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}
