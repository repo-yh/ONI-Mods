# xyr's Mods for Oxygen Not Included</p>

This is a collection of mods for Oxygen Not Included, written by Xyr.

---

## [Steam Workshop](https://steamcommunity.com/profiles/xyr531)

|**Name**|**Description**|**下载**
|---|---|---|
|[多倍功率发电](https://steamcommunity.com/sharedfiles/filedetails/?id=2380984533)|增加发电倍率.|[Multiple_Power_Generator.zip](https://github.com/laz-yh/ONI-Mods/releases/latest/download/Multiple_Power_Generator.zip)|
|[最后的补给包-Fix](https://steamcommunity.com/sharedfiles/filedetails/?id=3280056840)|最后的补给包修复版.|[SelectLastCarePackage.zip](https://github.com/laz-yh/ONI-Mods/releases/latest/download/SelectLastCarePackage.zip)|
|[调谐仪(多合一)](https://steamcommunity.com/sharedfiles/filedetails/?id=3519403656)|调谐仪(多合一)|[GeoTuner_mod.zip](https://github.com/laz-yh/ONI-Mods/releases/latest/download/GeoTuner_mod.zip)|
|[大容量材料研究终端](https://steamcommunity.com/sharedfiles/filedetails/?id=3749047120)|大容量材料研究终端|[NuclearResearch.zip](https://github.com/laz-yh/ONI-Mods/releases/latest/download/NuclearResearch.zip)|
|[储物隔绝](https://steamcommunity.com/sharedfiles/filedetails/?id=3749021783)|储物隔绝|[Storage_Isolation.zip](https://github.com/laz-yh/ONI-Mods/releases/latest/download/Storage_Isolation.zip)|

---

## 本地开发构建（Debug）

### 环境依赖

- [.NET SDK 10.x](https://dotnet.microsoft.com/download)（命令行构建）或 Visual Studio 2022（勾选“.NET 桌面开发”工作负载）。
- 如需完整合包（ILRepack 将依赖 DLL 合入主 DLL），需安装 [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48)。
- 游戏引用程序集已随仓库 `lib/` 目录分发，clone 后可直接编译。
- Debug 重新生成时，构建脚本会自动从 `GameFolder`（游戏 `OxygenNotIncluded_Data\Managed\`）同步游戏 DLL 到 `lib/`——游戏版本更新后重新生成一次即可，无需手动拷贝（需保证 `GameFolder` 配置正确）。

### 路径配置

构建路径统一由仓库根的 `Directory.Build.props.default` 定义：

|属性|默认值|说明|
|---|---|---|
|`SteamFolder`|`F:\SteamLibrary`|Steam 库根目录|
|`GameFolder`|`$(SteamFolder)\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed`|游戏托管程序集目录|
|`ModFolder`|`%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods`|游戏本地 mods 目录|

路径与默认值不一致时，在仓库根新建 `Directory.Build.props.user` 覆盖同名属性即可（该文件不随 git 分发，注意不要提交）：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <SteamFolder>D:\SteamLibrary</SteamFolder>
  </PropertyGroup>
</Project>
```

### 构建与调试

1. Visual Studio 打开 `ONI-Mods.sln`，以 **Debug** 配置生成目标 mod 项目（或整个解决方案）；也可用命令行快速编译：

   ```
   dotnet build SelectLastCarePackage\SelectLastCarePackage.csproj -c Debug
   ```

2. Debug 构建完成后，产物自动部署到游戏 Dev 目录 `$(ModFolder)\Dev\DEV_<项目名>`，游戏内 mod 列表会直接出现对应条目（标题带 `DEV:` 前缀），启用即可调试。
3. 改代码后重新生成即完成更新，无需手动拷贝文件。

版本号：每次构建自动递增项目根 `revision.txt`（`yyyyMMdd|N`，跨天重置），生成 `yyyy.MM.dd.N` 写入 `mod_info.yaml` 的 `version` 字段。

### 项目常用开关（各 mod 的 .csproj）

|属性|作用|
|---|---|
|`UseCommons`|引用共享代码库 `Commons` 项目|
|`UsesPLib`|引入 PLib 依赖包|
|`UsesAzeLib`|改用 `lib/*_public.dll` 公共化游戏程序集|
|`Title` / `Description`|mod 标题与描述，写入 `mod.yaml`|
