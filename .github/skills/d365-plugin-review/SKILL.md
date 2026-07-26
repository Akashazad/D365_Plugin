---
name: d365-plugin-review
description: >-
  Reviews Dynamics 365 / Dataverse plugin C# code for correctness, performance,
  security, and adherence to plugin execution best practices. Use when the
  user asks to "review this plugin", "check this plugin code", "code review
  for plugin", or shares a C# class implementing IPlugin.
---

## Reference

Before reviewing, apply the shared conventions in
`../_shared/d365-coding-standards.md` (naming, error handling, logging).

## Procedure

1. **Identify plugin context**
   - Confirm the class implements `IPlugin` and note the likely registration
     (message, entity, stage: pre-validation/pre-op/post-op, sync/async) from
     context clues in the code (comments, class name, logic).

2. **Execution context & pipeline correctness**
   - Is `IPluginExecutionContext` retrieved correctly via
     `serviceProvider.GetService(typeof(IPluginExecutionContext))`?
   - Does the plugin check `context.Depth` to avoid infinite recursion when
     it updates the same entity that triggered it?
   - Does the plugin correctly distinguish `PreImage`/`PostImage` usage vs.
     re-querying the entity unnecessarily?
   - Is `InputParameters`/`OutputParameters` accessed defensively
     (`Contains` checks before indexing)?

3. **Sandbox & threading safety**
   - No static/instance mutable state that isn't thread-safe — plugins are
     multi-threaded and can be reused across requests. Static readonly
     immutable data is fine; static mutable fields are a bug.
   - No `Thread.Sleep`, no direct file/registry/network access outside
     `IOrganizationService`/approved HTTP endpoints (sandbox restrictions).

4. **Performance**
   - No `RetrieveMultiple` or `Retrieve` calls inside a loop (N+1 pattern) —
     should batch via `ExecuteMultipleRequest` or a single query with the
     right filters.
   - Only requested columns retrieved (`ColumnSet` explicit, not
     `AllColumns` in production code) — no over-fetching.
   - Update calls only include changed attributes (avoid "overposting" the
     full entity back, which can cause unnecessary trigger cascades).
   - No unnecessary synchronous heavy logic that should be async
     (e.g. external API calls in a sync pre-op plugin blocking the UI).

5. **Error handling & tracing**
   - Exceptions surfaced as `InvalidPluginExecutionException` with a
     user-safe message, not raw stack traces.
   - `ITracingService.Trace()` used at entry and before any throw, without
     logging PII/full payloads.
   - Try/catch doesn't swallow exceptions silently.

6. **Security**
   - No hardcoded credentials, connection strings, or environment-specific
     GUIDs.
   - Impersonation (`context.UserId` vs `InitiatingUserId`) used correctly —
     flag if a plugin runs as SYSTEM when it shouldn't.
   - Input parameters validated before use (avoid trusting client-supplied
     data blindly, especially in Custom API-invoked plugins).

7. **Registration hygiene** (if registration XML/step config is included)
   - Filtering attributes set to minimize unnecessary triggering.
   - Correct stage/mode (sync vs async) for the operation's latency needs.

8. **Output format**

   ```
   ## Plugin Review Summary
   [1-2 sentence overall assessment, including inferred message/stage/entity]

   ### 🔴 Must Fix (correctness, security, sandbox violations)
   - [file:line] Issue — why it matters — suggested fix

   ### 🟡 Should Fix (performance, error handling)
   - [file:line] Issue — why it matters — suggested fix

   ### 🔵 Style/Convention
   - [file:line] Issue — suggested fix

   ### ✅ What's Good
   - [notable good patterns]
   ```
   - Omit empty sections.
   - Reference exact line numbers where possible.
