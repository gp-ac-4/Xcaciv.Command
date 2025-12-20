# API Reference: Attributes

Complete reference for parameter and command definition attributes.

## CommandRegisterAttribute

Registers a command class with the framework.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class CommandRegisterAttribute : Attribute
{
    public CommandRegisterAttribute(
        string command,
        string description,
        string prototype = "todo");
    
    public string Command { get; }
    public string Description { get; }
    public string Prototype { get; }
}
```

### Properties

#### Command
The command name users type.

**Type:** string

**Requirements:**
- Must be unique
- Case-insensitive
- Alphanumeric (conventionally UPPERCASE)

#### Description
Short description for help text.

**Type:** string

#### Prototype
Usage prototype for help. Default "todo" = auto-generate from parameters.

**Type:** string

### Example

```csharp
[CommandRegister("GREET", "Greet a user")]
public class GreetCommand : AbstractCommand { }

// With custom prototype
[CommandRegister(
    "GREET",
    "Greet a user",
    prototype: "GREET <name> [--formal]")]
public class GreetCommand : AbstractCommand { }
```

---

## CommandRootAttribute

Marks a command as a sub-command under a root.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class CommandRootAttribute : Attribute
{
    public CommandRootAttribute(string command);
    
    public string Command { get; }
}
```

### Properties

#### Command
The root command name.

**Type:** string

### Example

```csharp
[CommandRoot("DATABASE")]
[CommandRegister("CREATE", "Create a new database")]
public class DatabaseCreateCommand : AbstractCommand { }

[CommandRoot("DATABASE")]
[CommandRegister("DELETE", "Delete a database")]
public class DatabaseDeleteCommand : AbstractCommand { }

// Usage: DATABASE CREATE mydb
```

---

## CommandHelpRemarksAttribute

Adds extended remarks to command help.

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CommandHelpRemarksAttribute : Attribute
{
    public CommandHelpRemarksAttribute(string remarks);
    
    public string Remarks { get; }
}
```

### Properties

#### Remarks
Extended help text.

**Type:** string

### Example

```csharp
[CommandRegister("MYCOMMAND", "My command")]
[CommandHelpRemarks("This command does X.\n\nExamples:\n  MYCOMMAND arg1\n  MYCOMMAND arg1 arg2")]
[CommandHelpRemarks("For more info, see https://example.com/docs")]
public class MyCommand : AbstractCommand { }
```

---

## CommandParameterOrderedAttribute

Defines a positional parameter.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class CommandParameterOrderedAttribute : AbstractCommandParameter
{
    public CommandParameterOrderedAttribute(
        string name,
        string description,
        bool required = true,
        bool usePipe = false,
        string[] allowedValues = null);
    
    public string Name { get; }
    public string Description { get; }
    public bool Required { get; }
    public bool UsePipe { get; }
    public string[] AllowedValues { get; }
}
```

### Properties

#### Name
Parameter name for help text.

**Type:** string

#### Description
Parameter description.

**Type:** string

#### Required
Whether parameter must be provided.

**Type:** bool (default: true)

#### UsePipe
Whether parameter can accept piped input.

**Type:** bool (default: false)

#### AllowedValues
Restricts parameter to these values (optional).

**Type:** string[] (default: null = any value)

### Example

```csharp
[CommandRegister("GREET", "Greet a user")]
public class GreetCommand : AbstractCommand
{
    [CommandParameterOrdered("name", "Person's name")]
    public string Name { get; set; }
    
    [CommandParameterOrdered("greeting", "Greeting text", required: false)]
    public string Greeting { get; set; }
    
    [CommandParameterOrdered("format", "Output format", allowedValues: new[] { "plain", "json", "xml" })]
    public string Format { get; set; }
}

// Usage: GREET John "Hello there" json
```

---

## CommandParameterNamedAttribute

Defines a named parameter (key-value argument).

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class CommandParameterNamedAttribute : AbstractCommandParameter
{
    public CommandParameterNamedAttribute(
        string name,
        string description,
        bool required = false,
        bool usePipe = false,
        string[] allowedValues = null);
    
    public string Name { get; }
    public string Description { get; }
    public bool Required { get; }
    public bool UsePipe { get; }
    public string[] AllowedValues { get; }
}
```

### Properties

Same as CommandParameterOrderedAttribute.

### Example

```csharp
[CommandRegister("GREET", "Greet a user")]
public class GreetCommand : AbstractCommand
{
    [CommandParameterNamed("name", "Person's name", required: true)]
    public string Name { get; set; }
    
    [CommandParameterNamed("greeting", "Greeting text")]
    public string Greeting { get; set; }
    
    [CommandParameterNamed("format", "Output format", allowedValues: new[] { "plain", "json" })]
    public string Format { get; set; }
}

// Usage: GREET --name John --greeting "Hello" --format json
```

---

## CommandFlagAttribute

Defines a boolean flag parameter.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class CommandFlagAttribute : AbstractCommandParameter
{
    public CommandFlagAttribute(string name, string description);
    
    public string Name { get; }
    public string Description { get; }
}
```

### Properties

#### Name
Flag name.

**Type:** string

#### Description
Flag description.

**Type:** string

### Example

```csharp
[CommandRegister("GREET", "Greet a user")]
public class GreetCommand : AbstractCommand
{
    [CommandParameterOrdered("name", "Person's name")]
    public string Name { get; set; }
    
    [CommandFlag("verbose", "Show detailed output")]
    public bool Verbose { get; set; }
    
    [CommandFlag("quiet", "Suppress output")]
    public bool Quiet { get; set; }
}

// Usage: GREET John --verbose
// Usage: GREET John --verbose --quiet
```

---

## CommandParameterSuffixAttribute

Captures remaining arguments as an array.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class CommandParameterSuffixAttribute : AbstractCommandParameter
{
    public CommandParameterSuffixAttribute(
        string name,
        string description,
        bool required = false,
        bool usePipe = false);
    
    public string Name { get; }
    public string Description { get; }
    public bool Required { get; }
    public bool UsePipe { get; }
}
```

### Properties

#### Name
Parameter name.

**Type:** string

#### Description
Parameter description.

**Type:** string

#### Required
Whether at least one value must be provided.

**Type:** bool (default: false)

#### UsePipe
Whether parameter can accept piped input.

**Type:** bool (default: false)

### Example

```csharp
[CommandRegister("RUN", "Run a command with arguments")]
public class RunCommand : AbstractCommand
{
    [CommandParameterOrdered("program", "Program to run")]
    public string Program { get; set; }
    
    [CommandParameterSuffix("args", "Program arguments")]
    public string[] Args { get; set; }
}

// Usage: RUN myapp.exe arg1 arg2 arg3
// Program = "myapp.exe", Args = ["arg1", "arg2", "arg3"]
```

---

## AbstractCommandParameter

Base class for all parameter attributes.

```csharp
public abstract class AbstractCommandParameter : Attribute
{
    public bool UsePipe { get; protected set; }
    public string[] AllowedValues { get; protected set; }
    
    public abstract string GetIndicator();
}
```

### Methods

#### GetIndicator()
Returns the parameter indicator for help text.

**Returns:** (string) Formatted parameter indicator

---

## Parameter Order

When combining different parameter types, they must appear in this order:

1. **Ordered parameters** (positional)
2. **Flag parameters** (boolean switches)
3. **Named parameters** (key-value)
4. **Suffix parameters** (trailing arguments)

Example:

```csharp
[CommandRegister("CMD", "Complex command")]
public class ComplexCommand : AbstractCommand
{
    // 1. Ordered (positional)
    [CommandParameterOrdered("input", "Input file")]
    public string Input { get; set; }
    
    // 2. Flags
    [CommandFlag("verbose", "Verbose output")]
    public bool Verbose { get; set; }
    
    // 3. Named
    [CommandParameterNamed("output", "Output file")]
    public string Output { get; set; }
    
    // 4. Suffix
    [CommandParameterSuffix("extra", "Extra arguments")]
    public string[] Extra { get; set; }
}

// Usage: CMD input.txt --verbose --output result.txt extra1 extra2
```

---

## Piped Input Parameters

Parameters can accept piped input from previous pipeline stage:

```csharp
[CommandRegister("UPPERCASE", "Convert to uppercase")]
public class UppercaseCommand : AbstractCommand
{
    // This parameter can receive piped input
    [CommandParameterOrdered("text", "Text to convert", usePipe: true)]
    public string Text { get; set; }
}

// Usage: SAY hello | UPPERCASE
```

---

## See Also

- [Create a Command](getting-started-create-command.md)
- [API Interfaces](api-interfaces.md)
- [Core Types](api-core.md)
