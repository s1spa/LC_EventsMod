using UnityEngine;
using UnityEngine.SceneManagement;

namespace LCChaosMod.Patches
{
    /// <summary>
    /// Starts/stops EventManager based on scene transitions.
    /// Uses SceneManager.sceneLoaded — same approach as MainMenuInjector (confirmed working).
    /// Level* scenes = in-game round. SampleSceneRelay = lobby/ship.
    /// </summary>
    internal static class RoundLifecycle
    {
        public static void Init()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Plugin.Log.LogInfo("[RoundLifecycle] Registered sceneLoaded.");
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Plugin.Log.LogInfo($"[RoundLifecycle] Scene loaded: '{scene.name}'");

            if (scene.name.StartsWith("Level"))
            {
                ChaosNetworkHandler.Init();
                StartRound();
            }
            else if (scene.name == "SampleSceneRelay")
            {
                StopRound();
            }
        }

        private static void StartRound()
        {
            if (!ChaosSettings.ModEnabled.Value) return;
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer) return;
            if (EventManager.Instance != null) return;
            // Skip Gordion (company moon) — it's "LevelGordion" but just in case check name too
            if (StartOfRound.Instance != null && StartOfRound.Instance.currentLevelID == 3) return;

            var go = new GameObject("ChaosEventManager");
            go.AddComponent<EventManager>();
            Plugin.Log.LogInfo("[RoundLifecycle] EventManager started.");
        }

        private static void StopRound()
        {
            if (EventManager.Instance != null)
            {
                Object.Destroy(EventManager.Instance.gameObject);
                Plugin.Log.LogInfo("[RoundLifecycle] EventManager stopped.");
            }
        }
    }
}
