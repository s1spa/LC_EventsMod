using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LCChaosMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; } = null!;
        internal static ManualLogSource Log { get; private set; } = null!;

        private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            // Реєструємо локалізацію кожного Cog
            Cogs.MineSpawner.Lang.Init();

            ChaosSettings.Init(Config);
            _harmony.PatchAll();
            UI.MainMenuInjector.Init();
            Patches.RoundLifecycle.Init();

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} loaded!");
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
}
