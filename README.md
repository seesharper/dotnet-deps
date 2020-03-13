# dotnet-deps

A simple command line global tool to list and update NuGet dependencies.



Managing dependencies and making sure they are up to date can be a challenge sometimes. 
`dotnet-deps` a simple tool that makes it easy both to analyze and update our NuGet dependencies.



## Installing

```shell
dotnet tool install -g dotnet-deps
```



## Get Started

Open a terminal window and navigate to the directory that contains the project(s) we want to analyze.

```shell
deps
```



```shell
/Users/bernhardrichter/GitHub/dotnet-deps/src/Dotnet.Deps/Dotnet.Deps.csproj                                            
McMaster.Extensions.CommandLineUtils 2.5.1 => 2.6.0 (nuget.org) 😢
NuGet.Configuration 5.4.0 5.4.0 (nuget.org) 🍺
NuGet.Packaging 5.4.0 5.4.0 (nuget.org) 🍺
NuGet.Packaging.Core 5.4.0 5.4.0 (nuget.org) 🍺
simpleexec 6.2.0 6.2.0 (nuget.org) 🍺
```

The output is quite simple. We get a 🍺  for every dependency that is up-to-date and a 😢 for all dependencies that needs to be updated.

If we should want to update all dependencies we can simply use the `update` option.

```c#
deps --update
```



### Filtering

We can filter the packages to be processed by `dotnet-deps` by passing a `--filter` option.

```shell
deps --filter McMaster
```

> The filter is applied as an regular expression



### Project file types

The following file types are supported by `dotnet-deps`

 * SDK-style csproj files (*.csproj)

 * MsBuild props files (*.props)

 * MsBuild target files (*.target)

 * C# script files (*.csx)

   

> `dotnet-deps` only looks for `<PackageReference>` nodes and WILL NOT try to resolve MSBuild variables.

