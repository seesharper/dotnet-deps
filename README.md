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

### Minimum package age

Brand new package versions are sometimes best left to simmer for a little while. The `--min-age` (`-ma`) option makes `dotnet-deps` ignore versions that were published too recently.

```shell
deps --min-age 2d
```

The value is a number followed by an optional suffix.

| Value | Meaning |
| ----- | ------- |
| `2d`  | Ignore versions published less than 2 days ago |
| `12h` | Ignore versions published less than 12 hours ago |
| `2`   | Same as `2d`. Without a suffix we default to days |

When a newer version is held back, the version we *would* have picked is reported like this.

```shell
LightInject 7.0.1 7.0.1 (nuget.org) 🍺 (holding back 7.1.0 ⏳)
```

The minimum age can also be specified in the project file using the `PackagesMinimumAge` property.

```xml
<PropertyGroup>
  <PackagesMinimumAge>2d</PackagesMinimumAge>
</PropertyGroup>
```

The `--min-age` option takes precedence over `PackagesMinimumAge` when both are present.

> `PackagesMinimumAge` must be declared in the same file as the package references. Just like for `<PackageReference>` nodes, `dotnet-deps` does not evaluate MSBuild and will not pick up the property from an imported file such as `Directory.Build.props`.

> Feeds that do not report a publish date for a version (some local and private feeds) will never have that version held back.

### Locked dependencies

If we should want to "lock" a dependency to a specific version, we can do that by adding the `Locked` attribute as shown here.

```xml
<PackageReference Include="FluentAssertions" Version="7.0.0" Locked="true"/>
```



### Exit Code

If all packages are up-to-date, `dotnet-deps` will exit with exit code `0`, otherwise `0xbad`

### Project file types

The following file types are supported by `dotnet-deps`

 * SDK-style csproj files (*.csproj)

 * MsBuild props files (*.props)

 * MsBuild target files (*.target)

 * C# script files (*.csx)

 * NuGet metadata files (*.nuspec)

   

> `dotnet-deps` only looks for `<PackageReference>` nodes and WILL NOT try to resolve MSBuild variables.

