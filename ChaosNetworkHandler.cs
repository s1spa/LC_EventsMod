using GameNetcodeStuff;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod
{
    /// <summary>
    /// Handles all cross-client communication via CustomMessagingManager.
    /// Init() must be called on ALL players when a level loads.
    /// </summary>
    internal static class ChaosNetworkHandler
    {
        private const string MsgWarning  = "LCChaosMod_Warning";
        private const string MsgTeleport = "LCChaosMod_Teleport";
        private const string MsgSound    = "LCChaosMod_Sound";

        public static void Init()
        {
            var mgr = NetworkManager.Singleton.CustomMessagingManager;
            mgr.RegisterNamedMessageHandler(MsgWarning,  OnReceiveWarning);
            mgr.RegisterNamedMessageHandler(MsgTeleport, OnReceiveTeleport);
            mgr.RegisterNamedMessageHandler(MsgSound,    OnReceiveSound);
            Plugin.Log.LogInfo("[ChaosNetworkHandler] Handlers registered.");
        }

        // ── Warning ──────────────────────────────────────────────────────────

        /// <summary>Host: show HUD locally and broadcast to all clients.</summary>
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
            if (NetworkManager.Singleton.IsServer) return; // host already handled locally
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

        // ── Teleport ─────────────────────────────────────────────────────────

        /// <summary>
        /// Host: teleport a specific player to dest.
        /// If it's the host's own player — apply locally.
        /// Otherwise — send message to that client.
        /// toShip: also sets isInsideFactory=false, isInHangarShipRoom=true.
        /// </summary>
        public static void SendTeleport(PlayerControllerB player, Vector3 dest, bool toShip)
        {
            if (player.actualClientId == NetworkManager.Singleton.LocalClientId)
            {
                ApplyTeleport(player, dest, toShip);
                return;
            }

            var writer = new FastBufferWriter(32, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(dest.x);
                writer.WriteValueSafe(dest.y);
                writer.WriteValueSafe(dest.z);
                writer.WriteValueSafe(toShip);
                NetworkManager.Singleton.CustomMessagingManager
                    .SendNamedMessage(MsgTeleport, player.actualClientId, writer);
            }
        }

        private static void OnReceiveTeleport(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return; // host already handled locally
            reader.ReadValueSafe(out float x);
            reader.ReadValueSafe(out float y);
            reader.ReadValueSafe(out float z);
            reader.ReadValueSafe(out bool toShip);

            var local = GameNetworkManager.Instance?.localPlayerController;
            if (local == null) return;
            ApplyTeleport(local, new Vector3(x, y, z), toShip);
        }

        private static void ApplyTeleport(PlayerControllerB player, Vector3 dest, bool toShip)
        {
            if (toShip)
            {
                player.isInsideFactory    = false;
                player.isInHangarShipRoom = true;
            }
            player.TeleportPlayer(dest);
        }

        // ── Sound ─────────────────────────────────────────────────────────────

        /// <summary>Host: play sound locally and broadcast clip name + position to all clients.</summary>
        public static void BroadcastSound(string clipName, Vector3 pos)
        {
            PlaySoundLocal(clipName, pos);

            var writer = new FastBufferWriter(512, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(clipName);
                writer.WriteValueSafe(pos.x);
                writer.WriteValueSafe(pos.y);
                writer.WriteValueSafe(pos.z);
                NetworkManager.Singleton.CustomMessagingManager
                    .SendNamedMessageToAll(MsgSound, writer);
            }
        }

        private static void OnReceiveSound(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return; // host already handled locally
            reader.ReadValueSafe(out string clipName);
            reader.ReadValueSafe(out float x);
            reader.ReadValueSafe(out float y);
            reader.ReadValueSafe(out float z);
            PlaySoundLocal(clipName, new Vector3(x, y, z));
        }

        private static void PlaySoundLocal(string clipName, Vector3 pos)
        {
            var enemies = RoundManager.Instance?.currentLevel?.Enemies;
            if (enemies == null) return;
            foreach (var entry in enemies)
            {
                var clips = entry?.enemyType?.audioClips;
                if (clips == null) continue;
                foreach (var c in clips)
                {
                    if (c != null && c.name == clipName)
                    {
                        AudioSource.PlayClipAtPoint(c, pos);
                        return;
                    }
                }
            }
            Plugin.Log.LogWarning($"[ChaosNetworkHandler] Clip '{clipName}' not found locally.");
        }
    }
}
