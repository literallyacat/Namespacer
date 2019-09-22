# Namespacer
Netcore CLI tool to automate replacing namespaces in c# source files. Useful for Unity projects where you've created a ton of `*.cs` files in various folders but want to switch to using Assembly Definitions, or reduce name collisions with other plugins/libraries.

```
$ dotnet run --namespace "MyExample.Root.Namespace" --searchPath "/path/to/my/unity/assets/folder"
```

