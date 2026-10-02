using BepInEx.Configuration;
using UnityEngine;

namespace SimpleUIHPBar
{
    internal static class ModConfig
    {
        // General
        public static ConfigEntry<bool> AutoScale;

        // HUD
        public static ConfigEntry<bool> HudEnabled;
        public static ConfigEntry<bool> HideVanillaHud;
        public static ConfigEntry<float> HudScale;
        public static ConfigEntry<float> HudPosX;
        public static ConfigEntry<float> HudPosY;
        public static ConfigEntry<float> BarsOffsetX;
        public static ConfigEntry<float> BarsPosY;
        public static ConfigEntry<float> BarWidth;
        public static ConfigEntry<float> BarHeight;
        public static ConfigEntry<float> EitrHeight;
        public static ConfigEntry<bool> ShowNumbers;
        public static ConfigEntry<string> FontName;
        public static ConfigEntry<bool> MoveGuardianPower;
        public static ConfigEntry<float> GuardianOffsetX;
        public static ConfigEntry<float> GuardianOffsetY;

        // Colors
        public static ConfigEntry<Color> HealthColor;
        public static ConfigEntry<Color> StaminaColor;
        public static ConfigEntry<Color> EitrColor;
        public static ConfigEntry<Color> FoodColor;
        public static ConfigEntry<Color> FoodLowColor;
        public static ConfigEntry<Color> TrackColor;
        public static ConfigEntry<Color> TextColor;

        // サイズ・余白の既定値は 4 / 8 / 16 / 32 を基準にする(4 の倍数)
        public static void Bind(ConfigFile cfg)
        {
            AutoScale = cfg.Bind("General", "AutoScale", true,
                "true: 画面の高さ(1080p基準)に比例して拡大する。false: ゲーム内のUIスケール設定に従う。");

            HudEnabled = cfg.Bind("HUD", "Enabled", true, "シンプルHUDを表示する。");
            HideVanillaHud = cfg.Bind("HUD", "HideVanilla", true, "バニラのHP・食べ物・スタミナ・エイトル表示を隠す。");
            HudScale = cfg.Bind("HUD", "Scale", 1f,
                new ConfigDescription("HUDの倍率。", new AcceptableValueRange<float>(0.5f, 3f)));
            HudPosX = cfg.Bind("HUD", "PositionX", 40f, "食べ物表示の、画面左端からの距離。");
            HudPosY = cfg.Bind("HUD", "PositionY", 40f, "食べ物表示の、画面下端からの距離。");
            BarsOffsetX = cfg.Bind("HUD", "BarsOffsetX", 0f, "HP・スタミナバーの、画面中央からの横ずれ。");
            BarsPosY = cfg.Bind("HUD", "BarsPositionY", 88f, "HP・スタミナバーの、画面下端からの距離(操作ヒントの上)。");
            BarWidth = cfg.Bind("HUD", "BarWidth", 256f,
                new ConfigDescription("バーの長さ。", new AcceptableValueRange<float>(80f, 800f)));
            BarHeight = cfg.Bind("HUD", "BarHeight", 8f,
                new ConfigDescription("HP・スタミナバーの高さ。", new AcceptableValueRange<float>(2f, 64f)));
            EitrHeight = cfg.Bind("HUD", "EitrHeight", 36f,
                new ConfigDescription("エイトルバーの高さ。", new AcceptableValueRange<float>(2f, 64f)));
            FontName = cfg.Bind("HUD", "FontName", "AveriaSans",
                "数値に使うフォント名の一部。ゲーム内のフォントから名前が一致するものを使う。");
            MoveGuardianPower = cfg.Bind("HUD", "MoveGuardianPower", true, "守護者の力(Moder など)の表示を食べ物の上に移動する。");
            GuardianOffsetX = cfg.Bind("HUD", "GuardianOffsetX", 0f, "守護者の力の表示位置の微調整(横)。");
            GuardianOffsetY = cfg.Bind("HUD", "GuardianOffsetY", 0f, "守護者の力の表示位置の微調整(縦)。");
            ShowNumbers = cfg.Bind("HUD", "ShowNumbers", true, "バーの右に「現在値 / 最大値」を表示する。");

            HealthColor = cfg.Bind("Colors", "Health", new Color(0.30f, 0.78f, 0.35f, 1f), "HPバーの色(残量によらず固定)。");
            StaminaColor = cfg.Bind("Colors", "Stamina", new Color(0.93f, 0.78f, 0.30f, 1f), "スタミナバーの色。");
            EitrColor = cfg.Bind("Colors", "Eitr", new Color(0.45f, 0.55f, 0.95f, 1f), "エイトルバーの色。");
            FoodColor = cfg.Bind("Colors", "Food", new Color(1f, 1f, 1f, 0.9f), "食べ物の残り時間バーの色。");
            FoodLowColor = cfg.Bind("Colors", "FoodLow", new Color(0.95f, 0.80f, 0.20f, 1f), "残り時間が50%を切った時のバーの色。");
            TrackColor = cfg.Bind("Colors", "Track", new Color(0f, 0f, 0f, 0.35f), "バーの空き部分の色。");
            TextColor = cfg.Bind("Colors", "Text", new Color(1f, 1f, 1f, 0.9f), "数値の色。");
        }
    }
}
