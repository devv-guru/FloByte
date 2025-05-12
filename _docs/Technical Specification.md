# FloByte Technical Specification

## Architecture Overview
- Blazor WASM frontend
- ASP.NET Core API (Clean Architecture, CQRS)
- EF Core with MS SQL/SQLite
- Docker host for API nodes
- Private NuGet & Container registries

## Component Details

### Frontend
- **Framework**: Blazor WebAssembly Standalone
- **Styling**: Tailwind CSS v4.1 using the Tailwind CLI tool, CSS variables for themes
- **Workflow Builder**:
  - Drag-and-drop canvas
  - Quick-edit pane (right)
  - Full-screen node editor

### API
- **Layers**: Application, Domain, Infrastructure, Web
- **Mediation**: Mediator (Martin Othamar’s library)
- **Persistence**: EF Core
- **Validation**: FluentValidation
- **Results**: FluentResults

### Node Loader
- Scans NuGet feeds
- Downloads and validates package metadata (nuspec)
- Resolves dependencies and handles version conflicts:
  - Supports semantic versioning rules
  - Resolution strategies: latest stable, pinned version, or version range
  - Logs and alerts on conflicts or missing dependencies
- Loads assemblies via reflection
- Instantiates IWorkflowNode implementations

### Docker Orchestration
- Pull/run images via Docker.DotNet
- Health-check and logging integration

## Data Contracts
- **WorkflowDefinition** (JSON)
- **NodeInput/NodeResult**
- **NuGet metadata schema**

### Parameter Extraction & Type Mapping
When loading a node, FloByte reads the JSON Schema files from the package and maps them into editor parameters:

1. **Locate schemas**: read `inputSchema` path from nuspec metadata and load `schemas/input-schema.json`.
2. **Parse JSON Schema**: use a JSON Schema library (e.g., Newtonsoft.Json.Schema) to deserialize the schema document.
   - **Error Handling**: if parsing fails (invalid JSON or schema mismatch), log the error with context (package ID, schema path) and:
     - Fallback to a safe default: treat all parameters as optional strings, or
     - Mark the node as disabled with an admin alert for remediation.
3. **Extract Properties**:
   - Iterate `schema.properties`:
     - **Name**: property key
     - **Type**: map JSON types (`string`, `number`, `boolean`, `object`, `array`) to .NET types and UI controls (textbox, number input, checkbox, JSON editor).
     - **Description**: from `property.description` for tooltips.
4. **Determine Required Fields**:
   - Read `schema.required` array to flag parameters as required in the UI.
5. **Default Values**:
   - If `property.default` exists, pre-populate the editor field.
6. **Generate Parameter Models**:
   ```csharp
   public class NodeParameter
   {
       public string Name { get; set; }
       public Type ClrType { get; set; }
       public bool IsRequired { get; set; }
       public object DefaultValue { get; set; }
       public string Description { get; set; }
   }
   ```
7. **UI Binding**:
   - Build a collection of `NodeParameter` and bind to the quick-edit pane. The node editor dynamically renders fields based on `ClrType` and `IsRequired`.

These steps ensure consistent interpretation of schema metadata and drive the UI generation of parameter inputs.

## Security
- Role-based access control
- Sandbox AppDomains for code nodes
- **Secret Management**: support multiple providers:
  - Azure Key Vault
  - Environment Variables
  - AWS Secrets Manager

## API Endpoints
| Path       | Method | Description         |
|------------|--------|---------------------|
| /nodes     | GET    | List available nodes |
| /workflows | POST   | Create workflow     |
| /execute   | POST   | Run workflow        |

## Workflow Triggers
...
