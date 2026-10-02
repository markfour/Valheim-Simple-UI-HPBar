# Simple-UI-HPBar (Valheim)

見た目だけを変えるクライアントサイドMOD。ゲームの値は読むだけで書き換えない。

## 現在の範囲 (v0.1.0)

- HP / スタミナ / エイトル: 画面下中央の単色バー + 数値。HPは緑で固定
- 食べ物: アイコン3枠 + 残り時間バー
- バニラの該当表示は透明化して隠す(設定で戻せる)

未着手: インベントリ、チェスト、スキル画面。

## 必要なもの

- Valheim (Steam) + BepInExPack Valheim
- .NET SDK 6 以降(`dotnet build` 用)。Visual Studio / Rider でも可

## ビルド

Valheim と BepInEx の場所が既定(Steam 既定のフォルダ + ゲームフォルダ直下の `BepInEx`)と違う場合は、
プロジェクトと同じフォルダに `Simple-UI-HPBar.csproj.user` を作ってパスを指定する。
このファイルは `.gitignore` で除外される。

```xml
<Project>
  <PropertyGroup>
    <ValheimPath>D:\SteamLibrary\steamapps\common\Valheim</ValheimPath>
    <!-- r2modman を使う場合はプロファイル内の BepInEx -->
    <BepInExPath>C:\Users\(ユーザー名)\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\(プロファイル名)\BepInEx</BepInExPath>
  </PropertyGroup>
</Project>
```

```
dotnet build -c Release
```

- DLL は `BepInEx\plugins\Simple-UI-HPBar\` に自動コピーされる。
- Thunderstore 用の zip が `dist\Simple_UI_HPBar-(バージョン).zip` に作られる。
- バージョンを上げるときは、csproj の `Version`、`Plugin.cs` の `Version`、`thunderstore\manifest.json` の `version_number` を揃える。

## 設定

初回起動後に `BepInEx\config\markfour.simple-ui-hpbar.cfg` が生成される。

| セクション | キー | 内容 |
|---|---|---|
| General | AutoScale | 画面の高さに比例して拡大(1080p基準) |
| HUD | Enabled / HideVanilla | 自前HUDの表示 / バニラ表示を隠す |
| HUD | Scale | 倍率 (0.5〜3) |
| HUD | PositionX / PositionY | 食べ物表示の位置(画面左下から) |
| HUD | BarsOffsetX / BarsPositionY | HP・スタミナバーの位置(画面下中央から) |
| HUD | BarWidth / BarHeight / EitrHeight / ShowNumbers | バーの長さ / HP・スタミナの高さ / エイトルの高さ / 数値表示 |
| HUD | FontName | 数値のフォント(名前の一部) |
| HUD | MoveGuardianPower / GuardianOffsetX / GuardianOffsetY | 守護者の力を食べ物の上へ移動 / 位置の微調整 |
| Colors | Health / Stamina / Eitr / Food / FoodLow / Track / Text | 各色 |

## 構成

- `Plugin.cs` — エントリポイントと `Hud.Awake` へのパッチ
- `ModConfig.cs` — 設定項目
- `UiFactory.cs` — 単色矩形・文字を作るヘルパー
- `MinimalHud.cs` — HUD本体
