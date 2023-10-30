# InjectPRXBuildPS4
This is a command line app is for loading sprx into process with post build events on Visual Studio


## Installation

### Requirements
* [Orbis-Suite](https://github.com/OSM-Made/Orbis-Suite/releases) on your PC and Console
* [make_fself_rust](https://github.com/TheRouletteBoi/make_fself_rust/releases) for converting a .prx into .sprx

### Visual Studio 

1. Go to Project Properties -> Build Events -> Commnad Line
Paste this code
    ```
    cd "$(SolutionDir)vendor\make_fself\bin\"
    make_fself.exe "$(TargetDir)$(TargetName)$(TargetExt)" "$(TargetDir)$(TargetName).sprx"
    cd "$(SolutionDir)vendor\InjectPRXBuildPS4\bin\"
    InjectPRXBuildPS4.exe
    ```
2. TODO

## Download
[Releases](https://github.com/TheRouletteBoi/InjectPRXBuildPS4/releases)

## Building 
In Visual Studio navigate to Tools -> NuGet Package Manager -> Package Manager Console and install each NuGet Package individually

```bash
install-package YamlDotNet
install-package sqlite-net-pcl
install-package Ftp.dll
install-package Google.Protobuf
install-package H.Pipes
install-package Microsoft.Extensions.Logging.Abstractions
install-package System.Data.SQLite
install-package System.Json
```