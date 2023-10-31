# InjectPRXBuildPS4
This is a command line app is for loading sprx into process with post build events on Visual Studio

Your YAML configuration file should look like this
```yaml
target:
  target_name: PS4
  console_ip: 192.168.2.252
  source_path: 'C:\Users\USER\Desktop\livermorium\ORBIS_Debug\gta-5.sprx'
  destination_path: '/data/Orbis Suite/'

```

## Installation

### Requirements
* [Orbis-Suite](https://github.com/OSM-Made/Orbis-Suite/releases) on your PC and Console
* [make_fself_rust](https://github.com/TheRouletteBoi/make_fself_rust/releases) for converting a .prx into .sprx

### Visual Studio 

1. Go to Project Properties -> Build Events -> Commnad Line

   - Paste this code
    ```
    cd "$(SolutionDir)vendor\make_fself\bin\"
    make_fself.exe "$(TargetDir)$(TargetName)$(TargetExt)" "$(TargetDir)$(TargetName).sprx"
    cd "$(SolutionDir)vendor\InjectPRXBuildPS4\bin\"
    InjectPRXBuildPS4.exe
    ```
    ![1  postBuildEvent](https://github.com/TheRouletteBoi/InjectPRXBuildPS4/assets/9206290/17e71568-0d72-404c-b231-c3553ba2dc95)
2. In your project directory create a folder named vendor
   - vendor folder
    ![2  vendorFolder](https://github.com/TheRouletteBoi/InjectPRXBuildPS4/assets/9206290/4047a77a-46d3-4edb-a369-1a047ccddb97) 
3. Inside vendor folder create 2 folders named make_fself and InjectPRXBuildPS4
   - 2 folders
   ![3  create2Folders](https://github.com/TheRouletteBoi/InjectPRXBuildPS4/assets/9206290/46144fb7-d8ce-4882-879d-9772d81631a9)
4. Inside make_fself create a folder named bin and inside InjectPRXBuildPS4 create a folder named bin
   - bin folder
   ![4  createBinFolder](https://github.com/TheRouletteBoi/InjectPRXBuildPS4/assets/9206290/8affd60f-14bd-4636-a4bb-a10015b35f21)
5. Copy the make_fself application files from [make_fself_rust](https://github.com/TheRouletteBoi/make_fself_rust/releases) and place them inside bin folder.
   - inside bin folder
   ![5  copyApplicationIntoBinFolder](https://github.com/TheRouletteBoi/InjectPRXBuildPS4/assets/9206290/80c6822b-53d6-410c-9e1c-4ee40f21a7d1)
6. Copy InjectPRXBuildPS4 application files from [InjectPRXBuildPS4](https://github.com/TheRouletteBoi/InjectPRXBuildPS4/releases) and place them inside bin folder
   - inside bin folder
   ![6  copyInjectPrxBuildFiles](https://github.com/TheRouletteBoi/InjectPRXBuildPS4/assets/9206290/c374433d-0bd9-4eea-be91-d971052901b3)



## Download
[Releases](https://github.com/TheRouletteBoi/InjectPRXBuildPS4/releases)

## Building 
In Visual Studio navigate to Tools -> NuGet Package Manager -> Package Manager Console 
 
install each NuGet Package individually
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


## Contributors
- OSM-Made for Orbis-Suite and code assistance 
