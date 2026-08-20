---
name: d365-plugin-unittest-generator
description: >-
  Generates xUnit + FakeXrmEasy unit tests for Dynamics 365 / Dataverse
  plugins. Use when the user asks to "generate unit tests for this plugin",
  "create test cases for this plugin", "write tests for this plugin class",
  or shares a C# class implementing IPlugin and wants test coverage.
---

## Reference

Before generating tests, apply the correctness/context checks from
`../d365-plugin-review/SKILL.md` to understand the plugin's behavior first
(execution context usage, message/entity/stage, branching logic) — tests
should exercise the plugin as it actually behaves, not an assumed version
of it.

## Framework & conventions

- Test runner: **xUnit** (`[Fact]` for single scenarios, `[Theory]` +
  `[InlineData]` for parameterized variants).
- Mocking Dataverse: **FakeXrmEasy** (`XrmFakedContext`,
  `ExecutePluginWith<T>`).
- Mocking non-Dataverse dependencies (HTTP clients, config, external
  services): **Moq**.
- One test class per plugin class, named `{PluginClassName}Tests`.
- This project's test project is `src/D365Plugins.Tests`, referencing
  `src/D365Plugins/D365Plugins.csproj`. New generated test files go into
  `src/D365Plugins.Tests/{PluginClassName}Tests.cs` — do not create a new
  test project or place tests elsewhere.
- Every test follows explicit **Arrange / Act / Assert** structure, marked
  with comments.

## Procedure

1. **Analyze the plugin under test**
   - Identify the message (Create/Update/Delete/custom), target entity,
     and stage the plugin is registered for (infer from code/comments if
     not explicit).
   - Identify every branch in `Execute()`: conditionals, early returns,
     exception paths, and any loop logic.
   - Identify all reads (`Target`, `PreImage`, `PostImage`,
     `InputParameters`) and all writes (`Update`/`Create`/`OutputParameters`)
     the plugin performs.
   - Identify external dependencies beyond `IOrganizationService`
     (HTTP calls, other services) that need separate mocking.

2. **Determine test scenarios to cover**
   For each identified branch, generate at minimum:
   - **Happy path**: valid input, expected side effect occurs exactly as
     coded (e.g. correct field updated with correct value, correct
     related record created).
   - **Boundary/edge cases**: empty/null optional fields, minimum/maximum
     values for any numeric or string-length logic, empty collections.
   - **Missing/null required data**: a required attribute absent from
     Target or a required PreImage attribute missing — assert the plugin
     either handles it gracefully or throws the expected
     `InvalidPluginExecutionException`.
   - **Exception path**: force a downstream `IOrganizationService` call to
     throw, assert the plugin surfaces an `InvalidPluginExecutionException`
     with a meaningful message rather than an unhandled exception.
   - **Depth/recursion guard** (if the plugin updates its own trigger
     entity): assert it exits early when `context.Depth` exceeds the
     expected threshold, to prove the recursion guard actually works.
   - **Message-specific variants**: if the plugin behaves differently for
     Create vs. Update (e.g. checks `PreImage` only on Update), generate
     a separate test per message type.

3. **Generate the FakeXrmEasy scaffold for each test**
   Standard shape:
   ```csharp
   [Fact]
   public void Should_<expected behavior>_When_<condition>()
   {
       // Arrange
       var context = new XrmFakedContext();
       var target = new Entity("entityname")
       {
           Id = Guid.NewGuid(),
           ["attribute"] = value
       };
       // Register pre-existing data / pre-image if needed
       context.Initialize(new[] { /* related records if needed */ });

       var pluginContext = context.GetDefaultPluginContext();
       pluginContext.MessageName = "Update"; // or Create/Delete/custom
       pluginContext.PrimaryEntityName = "entityname";
       pluginContext.InputParameters["Target"] = target;
       // pluginContext.PreEntityImages.Add("PreImage", preImageEntity);

       // Act
       context.ExecutePluginWith<YourPluginClass>(pluginContext);

       // Assert
       var updated = context.CreateQuery("entityname")
           .FirstOrDefault(e => e.Id == target.Id);
       Assert.Equal(expectedValue, updated["attribute"]);
   }
   ```
   - For exception-path tests, wrap the Act line in
     `Assert.Throws<InvalidPluginExecutionException>(() => ...)`.
   - For external dependency mocking (non-Dataverse), inject a `Moq`
     mock through the plugin's constructor if it supports dependency
     injection, or note in a comment that the plugin needs a testable
     constructor if it currently doesn't (flag this as a prerequisite
     rather than silently working around it).

4. **Naming convention for test methods**
   `Should_<ExpectedResult>_When_<Condition>` — e.g.
   `Should_UpdateAccountRating_When_RevenueExceedsThreshold`,
   `Should_ThrowInvalidPluginException_When_RequiredFieldMissing`.

5. **Flag untestable code**
   If the plugin's constructor doesn't accept a way to inject/mock
   dependencies (e.g. it directly `new`s up an `HttpClient` inside
   `Execute()`), do not silently generate a broken test — call this out
   explicitly as a refactor needed before proper unit testing is possible,
   and suggest the minimal constructor injection change required.

6. **Output format**

   ```
   ## Unit Test Generation Summary
   Plugin: {ClassName} | Message: {message} | Stage: {stage} | Entity: {entity}

   ### Test scenarios generated
   - [list of test method names with one-line description each]

   ### ⚠️ Testability concerns
   - [any refactor needed before tests can be written, e.g. missing DI]

   ### Generated test file
   ```csharp
   [full test class]
   ```
   ```
   - Write/save the generated test class directly into
     `src/D365Plugins.Tests/{PluginClassName}Tests.cs`. If a test file for
     this plugin already exists there, append new scenarios rather than
     overwriting existing tests, unless the user asks for a full
     regeneration.
