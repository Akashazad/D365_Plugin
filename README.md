# D365_Plugin
Store all the Plugins for Dynamics 365.

## Structure
- The plugins are consolidated into one class library project at [src/D365Plugins/D365Plugins.csproj](src/D365Plugins/D365Plugins.csproj).
- This project contains multiple plugin classes in a single assembly: [src/D365Plugins/AccountCreatePlugin.cs](src/D365Plugins/AccountCreatePlugin.cs) and [src/D365Plugins/ContactUpdatePlugin.cs](src/D365Plugins/ContactUpdatePlugin.cs).
- The solution file [D365_Plugins.sln](D365_Plugins.sln) builds that single DLL.

## Build
Run the following from the repository root:

```bash
dotnet build D365_Plugins.sln
```
