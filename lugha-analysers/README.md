# Lugha.Analysers

Roslyn diagnostic analysers that enforce text scope correctness at compile time. Packed into the `Lugha` NuGet package - not published independently.

## Diagnostics

| ID | Severity | Default | Description |
|---|---|---|---|
| LGH001 | Error | Enabled | Text scope member does not return `string`. |
| LGH003 | Error | Enabled | Text scope implementation returns `null`, `null!`, `default`, `default!`, `string.Empty`, or `""`. |
| LGH004 | Warning | Enabled | Parameterised text scope method does not use all parameters in its return expression. |
| LGH005 | Info | Opt-in | Text scope interface has no implementations in the assembly. |
| LGH006 | Info | Opt-in | `PluralForms` initialiser sets only `Other` and `One` for a language needing more CLDR categories. |
| LGH007 | Info | Opt-in | Text scope member defined but unreferenced in the assembly. |
| LGH008 | Warning | Enabled | Text scope implementation body contains side-effecting calls (heuristic). |

### Enabling opt-in diagnostics

Add an `.editorconfig` entry:

```ini
[*.cs]
dotnet_diagnostic.LGH005.severity = suggestion
dotnet_diagnostic.LGH006.severity = suggestion
dotnet_diagnostic.LGH007.severity = suggestion
```

## Architecture

All analysers share a common `TextScopeHelper` that resolves `ITextScope`-derived interfaces via `ToDisplayString()` comparison against the fully-qualified `Lugha.ITextScope` metadata name.

### LGH001 - `TextScopeReturnTypeAnalyser`

Registers on `SymbolKind.NamedType`. For every type implementing an `ITextScope`-derived interface, inspects each property and method member. Reports if the return type is not `string`.

### LGH003 - `NullOrEmptyReturnAnalyser`

Registers on `ReturnStatement` and `ArrowExpressionClause` syntax nodes. Unwraps `SuppressNullableWarningExpression` (`null!`, `default!`). Reports `null`, `default`, `string.Empty`, and `""` returns from text scope implementation members.

### LGH004 - `ParameterUsageAnalyser`

Registers on `SymbolKind.Method`. For each method that implements a text scope interface member, collects all `IdentifierNameSyntax` tokens in the return expression and reports any parameter whose name does not appear.

### LGH005 - `OrphanedScopeAnalyser`

Registers a compilation-end action. Scans all named types in the compilation: collects `ITextScope`-derived interfaces and classes that implement them. Reports any scope interface with zero concrete implementations.

### LGH006 - `PluralCategoryCompletenessAnalyser`

Registers on `ObjectCreationExpression` and `ImplicitObjectCreationExpression`. When the created type is `PluralForms`, attempts to detect the language from the containing class's `Culture` member (property or field) by locating a `CultureInfo.GetCultureInfo("...")` invocation. Compares the number of initialised category slots against the expected count for that language's CLDR cardinal rule.

### LGH007 - `UnreferencedMemberAnalyser`

Registers a compilation-start action that installs syntax-node and compilation-end callbacks. During compilation, collects all member-access references to `ITextScope` members. At compilation end, reports any declared scope member that was never referenced.

### LGH008 - `SideEffectAnalyser`

Registers on `MethodDeclaration` and `PropertyDeclaration`. Scans the body/expression for known side-effect patterns: `Console`, `File`, `HttpClient`, `Debug`, `Trace` member access; `await` expressions; field/property assignments outside the method; and `throw` of non-argument exceptions.

## Build

Targets `netstandard2.0` with `Microsoft.CodeAnalysis.CSharp` 5.0.0 and `PolySharp` 1.15.0 for C# 10+ polyfills.

```
dotnet build lugha-analysers
```

## Licence

[Apache License 2.0](../LICENSE)
