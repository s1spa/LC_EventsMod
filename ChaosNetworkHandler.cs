using Unity.Collections;
using Unity.Netcode;

namespace LCChaosMod
{
    /// <summary>
    /// Orchestrates network handler registration for all modules.
    /// Only Warning stays here — it's used directly by EventManager, not tied to any single event.
    /// </summary>
    internal static class ChaosNetworkHandler
    {
        private const string MsgWarning = "LCChaosMod_Warning";

        public static void Init()
        {
            var mgr = NetworkManager.Singleton.CustomMessagingManager;
            mgr.RegisterNamedMessageHandler(MsgWarning, OnReceiveWarning);

            Utils.TeleportNet.Init();
            Cogs.RandomSound.Net.Init();
            Cogs.InfiniteStamina.Net.Init();

            Plugin.Log.LogInfo("[ChaosNetworkHandler] All handlers registered.");
        }

        // ── Warning ──────────────────────────────────────────────────────────

        public static void BroadcastWarning(string eventName)
        {
            ShowHUD(eventName);

            var writer = new FastBufferWriter(512, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(eventName);
                NetworkManager.Singleton.CustomMessagingManager
                    .SendNamedMessageToAll(MsgWarning, writer);
            }
        }

        private static void OnReceiveWarning(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return;
            reader.ReadValueSafe(out string eventName);
            ShowHUD(eventName);
        }

        private static void ShowHUD(string eventName)
        {
            bool ua    = ChaosSettings.Language.Value == "UA";
            string title = ua ? "ХАОС" : "CHAOS";
            string msg   = ua ? $"{eventName} через 5 секунд!" : $"{eventName} in 5 seconds!";
            HUDManager.Instance?.DisplayTip(title, msg, isWarning: true);
        }
    }
}
