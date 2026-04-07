using System.Collections.Generic;
using GameNetcodeStuff;
using UnityEngine;

namespace LCChaosMod.Cogs.Firefly
{
    /// <summary>
    /// Marker component — placed on the light GameObject under the player.
    /// </summary>
    public class FireflyLight : MonoBehaviour { }

    internal static class FireflyTracker
    {
        private static readonly Color GlowColor = new Color(1f, 0.85f, 0.4f); // warm yellow
        private const float Range     = 8f;
        private const float Intensity = 3f;

        private static readonly List<FireflyLight> _active = new();

        /// <summary>Called when the local player picks up the Apparatus.</summary>
        public static void OnLocalPlayerGrabbed()
        {
            var local = GameNetworkManager.Instance?.localPlayerController;
            if (local == null) return;

            AddLight(local);
            Net.Broadcast(local.actualClientId);
        }

        /// <summary>Called on all clients when any player receives the glow.</summary>
        public static void AddLightToPlayer(ulong clientId)
        {
            var all = StartOfRound.Instance?.allPlayerScripts;
            if (all == null) return;

            foreach (var p in all)
            {
                if (p.actualClientId == clientId)
                {
                    AddLight(p);
                    return;
                }
            }
        }

        private static void AddLight(PlayerControllerB player)
        {
            // Don't add duplicate
            if (player.GetComponentInChildren<FireflyLight>() != null) return;

            var go = new GameObject("FireflyLight");
            go.transform.SetParent(player.transform, false);
            go.transform.localPosition = new Vector3(0f, 1f, 0f);

            var light      = go.AddComponent<Light>();
            light.type      = LightType.Point;
            light.color     = GlowColor;
            light.range     = Range;
            light.intensity = Intensity;
            light.shadows   = LightShadows.None;

            var marker = go.AddComponent<FireflyLight>();
            _active.Add(marker);
            Plugin.Log.LogInfo($"[Firefly] Glow added to {player.playerUsername}.");
        }

        /// <summary>Remove all firefly lights at round end.</summary>
        public static void Cleanup()
        {
            foreach (var marker in _active)
                if (marker != null) Object.Destroy(marker.gameObject);
            _active.Clear();

            Plugin.Log.LogInfo("[Firefly] Lights cleaned up.");
        }
    }
}
