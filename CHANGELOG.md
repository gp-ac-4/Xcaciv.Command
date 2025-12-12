# Changelog

All notable changes to Xcaciv.Command will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.5.18] - 2025-01-XX

### Changed - BREAKING
- **Migrated to Xcaciv.Loader 2.0.1** with instance-based security configuration
  - Replaced static `SetStrictDirectoryRestriction()` with per-instance `AssemblySecurityPolicy`
  - Changed from wildcard (`*`) path restrictions to directory-based security
  - Each `AssemblyContext` now has independent security configuration
  
### Added
- Comprehensive security exception handling for plugin loading
- Detailed trace logging for security violations
- `LoaderMigrationTests` test suite with 8 new security-focused tests
- Security documentation in README files

### Fixed
- CS8602 null reference warnings in `MemoryIoContext` and `TestTextIo` constructors
- Proper path-based security restrictions for plugin loading (replaced TODO comment)
- Enhanced error messages when security violations occur

### Security
- Plugin assemblies now restricted to their actual directory paths
- Security violations are explicitly caught and logged
- Follows SSEM principles: Maintainability, Trustworthiness, and Reliability
- No more wildcard access permissions

### Migration Notes
For users upgrading from versions using Xcaciv.Loader 1.x:
- No API changes required for basic usage
- Plugin security is automatically enhanced
- Security exceptions now propagate with clear error messages
- See [Xcaciv.Loader Migration Guide](https://github.com/Xcaciv/Xcaciv.Loader/blob/main/docs/MIGRATION-v1-to-v2.md)

## Previous Versions

### [1.5.x] - Previous
- Threaded pipeline support
- Sub-command structure
- Auto-generated help
- Built-in commands (SAY, SET, ENV, REGIF)
- Plugin system with Xcaciv.Loader 1.x
