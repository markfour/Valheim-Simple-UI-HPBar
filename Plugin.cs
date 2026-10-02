using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SimpleUIHPBar
{
    /// <summary>
    /// 見た目だけを変えるクライアントサイドMOD。
    /// ゲームのデータや挙動には一切書き込まない(読み取りのみ)。
    /// </summary>
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "markfour.simple-ui-hpbar";
        public const string Name = "Simple-UI-HPBar";
        public const string Version = "0.1.0";

        internal static ManualLogSource Log;
        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            ModConfig.Bind(Config);
            Config.SettingChanged += (_, __) => MinimalHud.RequestRebuild();

            _harmony = new Harmony(Guid);
            _harmony.PatchAll();
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }

    /// <summary>HUD が生成されたタイミングで自前のHUDを差し込む。</summary>
    [HarmonyPatch(typeof(Hud), "Awake")]
    internal static class Hud_Awake_Patch
    {
        private static void Postfix(Hud __instance)
        {
            try
            {
                MinimalHud.Attach(__instance);
            }
            catch (Exception e)
            {
                // UI MOD が原因でゲームが止まらないよう、失敗してもバニラUIのまま続行する
                Plugin.Log.LogError($"MinimalHud の生成に失敗: {e}");
            }
        }
    }
}
