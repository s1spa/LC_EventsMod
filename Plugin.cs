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
            Cogs.TurretSpawner.Lang.Init();
            Cogs.MobSpawner.Lang.Init();
            Cogs.TeleportDungeon.Lang.Init();
            Cogs.TeleportShip.Lang.Init();
            Cogs.RandomSound.Lang.Init();
            Cogs.InfiniteStamina.Lang.Init();
            Cogs.Firefly.Lang.Init();
            Cogs.PlayerSwap.Lang.Init();
            Cogs.BerserkTurret.Lang.Init();
            Cogs.Football.Lang.Init();
            Cogs.FakeMessage.Lang.Init();
            Cogs.SizeMatters.Lang.Init();

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
