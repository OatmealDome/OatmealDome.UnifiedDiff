# OatmealDome.UnifiedDiff

This library allows you to create unified diff files for use with `patch`. Note that this library does not aim to duplicate the exact output from `diff`. 

## Usage

```csharp
string alphaText = File.ReadAllText("a.txt");
string bravoText = File.ReadAllText("b.txt");

UnifiedDiffFile unifiedDiff = UnifiedDiffFile.Create(alphaText, bravoText);

File.WriteAllText("output.patch", unifiedDiff.ToString());
```
