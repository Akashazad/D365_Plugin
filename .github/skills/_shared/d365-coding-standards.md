# Shared Dynamics 365 / Dataverse Coding Standards

Reference this file from any d365-*-review skill instead of duplicating it.

## Naming conventions
- C# namespaces: `{Org}.{Module}.Plugins`, `{Org}.{Module}.CustomApis`
- Plugin classes: PascalCase, verb-first where possible (e.g. `AccountPreValidateOnCreate`)
- JS web resources: `{org}_{entity}_{purpose}.js` (e.g. `contoso_account_formscripts.js`)
- Custom API unique names: `{prefix}_{Verb}{Noun}` (e.g. `contoso_CalculateDiscount`)

## Error handling philosophy
- Never swallow exceptions silently.
- Server-side (C#): throw `InvalidPluginExecutionException` with a user-safe message;
  log full technical detail via `ITracingService` before throwing.
- Client-side (JS): never let a form script fail silently — show
  `Xrm.Navigation.openAlertDialog` or `formContext.ui.setFormNotification` on error.

## Logging
- Plugins: use `ITracingService.Trace()` at entry, on key branches, and before any throw.
  Never trace PII or full record payloads in production.
- JS: use `console.log`/`console.error` only behind a debug flag; don't leave raw
  console output in production web resources.

## General
- No hardcoded GUIDs, environment URLs, or connection strings — use configuration
  (environment variables, custom config entities, or Custom API params).
- Prefer early-bound entities/context where the project has generated them;
  otherwise be consistent with late-bound and document why.
