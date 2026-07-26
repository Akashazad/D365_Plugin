# AccountCreatePlugin

This is a Dynamics 365 plugin that sets the `description` field on the `account` entity to a hard-coded string when an account record is created.

How to register
- Build the project (target .NET Framework 4.7.2).
- Use the Plugin Registration Tool or the Dynamics 365 web UI to register the assembly.
- Register the step:
  - Message: `Create`
  - Primary Entity: `account`
  - Stage: `PreOperation` (so the plugin sets the field before the record is persisted)
  - Execution Mode: `Synchronous`

Notes
- The plugin modifies the `Target` entity's `description` attribute; no explicit `Update` call is required when registered in PreOperation.
- If you prefer PostOperation, the plugin would need to call `IOrganizationService.Update` to persist changes.
