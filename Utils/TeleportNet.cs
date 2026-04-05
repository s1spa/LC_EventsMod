using GameNetcodeStuff;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Utils
{
    internal static class TeleportNet
    {
        private const string MsgTeleport = "LCChaosMod_Teleport";

        public static void Init()
        {
            NetworkManager.Singleton.CustomMessagingManager
                .RegisterNamedMessageHandler(MsgTeleport, OnReceive);
        }

        /// <summary>
        /// Host: teleport a specific player to dest.
        /// If it's the host's own player — apply locally.
        /// Otherwise — send message to that client.
        /// toShip: also sets isInsideFactory=false, isInHangarShipRoom=true.
        /// </summary>
        public static void Send(PlayerControllerB player, Vector3 dest, bool toShip)
        {
            if (player.actualClientId == NetworkManager.Singleton.LocalClientId)
            {
                Apply(player, dest, toShip);
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

        private static void OnReceive(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return;
            reader.ReadValueSafe(out float x);
            reader.ReadValueSafe(out float y);
            reader.ReadValueSafe(out float z);
            reader.ReadValueSafe(out bool toShip);

            var local = GameNetworkManager.Instance?.localPlayerController;
            if (local == null) return;
            Apply(local, new Vector3(x, y, z), toShip);
        }

        private static void Apply(PlayerControllerB player, Vector3 dest, bool toShip)
        {
            if (toShip)
            {
                player.isInsideFactory    = false;
                player.isInHangarShipRoom = true;
            }
            player.TeleportPlayer(dest);
        }
    }
}
