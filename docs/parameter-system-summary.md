# Parameter System – Consolidated Summary

## Scope
This note consolidates the prior PARAMETER_* documents into a single reference for the parameter system design, behavior, and recent refactors.

## Design Goals
- Strongly-typed parameter values with early conversion/validation.
- Case-insensitive lookup for command parameters.
- Clear diagnostics for validation and type mismatch failures.
- Safe handling of invalid values via an explicit sentinel (`InvalidParameterValue`).
- No nulls as converted values; empty strings are preserved as provided.

## Key Components
- **IParameterConverter / DefaultParameterConverter**: Performs invariant-culture conversions for string, numeric primitives, bool (with smart boolean literals), Guid, DateTime, and JsonElement. Rejects unsupported types, null target types, and invalid formats, returning the invalid sentinel and a validation message when conversion fails.
- **ParameterValue<T>**: Strongly-typed value object storing the boxed value and metadata (`Name`, `RawValue`, `DataType`, `ValidationError`, `IsValid`, `UntypedValue`). Access through `GetValue<T>()`/`TryGetValue<T>()` enforces validation, type checks, and detailed mismatch diagnostics. Invalid values keep `UntypedValue = InvalidParameterValue`.
- **ParameterCollection**: Case-insensitive dictionary of `IParameterValue`. Provides typed getters (`GetValue<T>()`, `GetAsValueType<T>()`, `GetValueOrDefault<T>()`) that enforce validation and rely on the new accessors.
- **ParameterCollectionBuilder / CommandParameters**: Convert and validate as early as possible using the converter, then materialize `ParameterValue<T>` instances through the runtime factory to retain declared types and sentinel semantics.

## Behavioral Notes
- Conversion errors surface immediately with validation errors; access to invalid parameters throws with context (name, raw value, expected type, and stored type).
- Empty strings remain empty for string targets; other types treat empty strings as invalid input.
- Type safety checks ensure the converter’s output matches the requested target type; mismatches throw.

## Breaking Changes (intentional)
- `ParameterValue` is now generic (`ParameterValue<T>`) with runtime factory creation; the old `As<T>()` and `Value` members are replaced by `GetValue<T>()`/`TryGetValue<T>()`.
- `IParameterValue` adds `DataType`, `UntypedValue`, and typed accessors; consumers must update to the new API.

## Migration Tips
- Replace `param.As<T>()` or `param.Value` access with `param.GetValue<T>()` (or `TryGetValue<T>(out var v)` for tolerant paths).
- When constructing parameters dynamically, use `ParameterValue.Create(name, raw, convertedValue, targetType, isValid, validationError)` to obtain the correct closed generic instance.
- Handle invalid parameters via `IsValid` or by checking `UntypedValue is InvalidParameterValue`.

## Source Documents Mapped
This summary supersedes:
- PARAMETER_CONVERTER_SEPARATION.md
- PARAMETER_SYSTEM_IMPLEMENTATION.md
- PARAMETER_SYSTEM_SIMPLIFIED.md
- PARAMETER_TESTS_ANALYSIS.md
- PARAMETER_TESTS_FINAL_ANALYSIS.md
- PARAMETER_TESTS_QUICK_REFERENCE.md
- PARAMETER_TESTS_SUMMARY.md
- PARAMETER_VALUE_PURE_OBJECT.md
