# Deep Space Gunsmith / 深空枪匠

A firearm collecting, customization, production and supply mod for **Probably Stolen / 深空当铺**.

- Steam Workshop: https://steamcommunity.com/sharedfiles/filedetails/?id=3829983019
- Bilingual project page: https://share.mork.chat/p/4c435c32-65e4-40b6-af76-58eba4e88856
- Current release: **0.1.1** — [bilingual update notes](docs/RELEASE-0.1.1.md).
- Game API target: **0.46D**; C# target: **.NET Standard 2.1**.
- Mod identity: `local.coldwarmodern`; runtime assembly: `DeepSpaceGunsmith.dll`.

## Features

52 firearms, 36 attachments, dedicated storage, a 64-recipe CNC workbench, a 23-recipe ammunition workbench, a firearms wholesaler and Peace Margin Industries telephone deliveries. Added items include Chinese and English names and descriptions.

See [English update notes](docs/CHANGELOG.en.md) and [中文更新公告](docs/CHANGELOG.zh-CN.md). The [item catalog](docs/items.csv) lists bilingual names and descriptions.

## Build

You need a .NET SDK and your own legally installed copy of the game. **Game, Unity, Harmony and Steam binaries are not included.** References are resolved only from your local game's `Probably Stolen_Data/Managed` directory; they are not copied into the mod package.

PowerShell:

```powershell
.\build.ps1 -GameManaged 'D:\YourGames\Probably Stolen\Probably Stolen_Data\Managed'
```

Or set `PROBABLY_STOLEN_MANAGED` in your shell and run `./build.ps1`. Direct SDK builds can pass `-p:GameManaged=...` or `-p:GameRoot=...`.

The package is written under `dist/DeepSpaceGunsmith-<version>/DeepSpaceGunsmith/`. Copy that final directory to the game's `Mods` folder if installing locally. Do not copy this entire source repository into `Mods`.

## Repository layout

| Directory / file | Contents |
|---|---|
| Root C# files | Firearms, accessories, visuals, trading and localization |
| `Cnc/` | CNC material reserves, recipes, jobs and interface |
| `Ammo/` | Ammunition reserves, recipes and batch production |
| `Suppliers/` | Firearms wholesaler and telephone material deliveries |
| `assets/`, `Paper/` | Locally generated inventory artwork and paper background |
| `Books/`, `Localization/` | Business card definition and supplier language strings |
| `manifest.xml` | Mod identity, version and requested capabilities |

## Material conversion

CNC: metal ingots add **75/85/100/125/150/300** reserve by purity; common electronics and 3D printer material add **25** each; nuclear waste adds **500**. Ammunition workbench: powder-and-primer packs, case piles and projectile piles add **100** each; nuclear waste adds **500**. Each reserve has a separate cap of **5000**.

CNC preserves the first-item 100%, subsequent-item 70% material rule, rounded upward per material for the entire batch. Ammunition batches take one powered night and **8 power per batch**. Nuclear waste is stored but not consumed by current recipes.

## Installation notes

Do not load the standalone CNC, ammunition workbench or supplier mods alongside this integrated mod. Keep machines and containers holding them away from the customer counter. The replaced old workbench has no automatic save migration. Reload a save after changing language to refresh saved item text.

Stable item IDs containing words such as `prototype` or `test` are retained for identity and save references; they are not player-facing development labels.

## Source and artwork provenance

The maintainer states that the mod code and artwork were generated locally, with AI assistance used for artwork and development. Some artwork was guided by public visual references, notably the DOOM BFG 9000. Local generation does not establish ownership of underlying third-party characters, designs, names or trademarks. See [asset provenance](ASSET_PROVENANCE.md).

This is a fan-made mod and is not affiliated with the developers of Probably Stolen, id Software, Bethesda or other referenced rights holders.

## License

- Original mod code and build scripts: **Unlicense** — see [LICENSE](LICENSE).
- Maintainer-controlled original artwork, localization and documentation: **CC0 1.0 Universal** — see [LICENSES/CC0-1.0.txt](LICENSES/CC0-1.0.txt).
- Attribution to this project is **not required** for material covered by these grants. Use, modification and redistribution, including commercial use, are permitted under the applicable terms.

These grants cover only rights held by the maintainer. They do not license third-party game binaries, protected franchise designs or trademarks. See [asset provenance](ASSET_PROVENANCE.md) for the BFG/DOOM reference boundary.

---

## 中文说明

本目录为独立整理的主模组源码与运行所需素材，不包含游戏本体程序集、反编译研究、Steam账号数据、存档、备份和上传工具。构建时请通过参数指定自己电脑上游戏的Managed目录。

代码采用Unlicense；由作者有权授权的原创素材、文本与文档采用CC0 1.0，可自由使用、修改和再分发，无需向本项目署名。AI制作与参考来源在素材声明中列出；第三方名称、设定、外观及商标不因本项目许可而自动获得授权。
