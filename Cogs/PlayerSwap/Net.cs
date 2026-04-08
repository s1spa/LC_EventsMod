using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs.PlayerSwap
{

    internal static class Net
    {
        private const string MsgSwap = "LCChaosMod_PlayerSwap";

        public static void Init()
        {
            NetworkManager.Singleton.CustomMessagingManager
                .RegisterNamedMessageHandler(MsgSwap, OnReceive);
        }

        public static void Send(GameNetcodeStuff.PlayerControllerB player, Vector3 dest, bool isInsideFactory)
        {
            if (player.actualClientId == NetworkManager.Singleton.LocalClientId)
            {
                Apply(player, dest, isInsideFactory);
                return;
            }

            var writer = new FastBufferWriter(32, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(dest.x);
                writer.WriteValueSafe(dest.y);
                writer.WriteValueSafe(dest.z);
                writer.WriteValueSafe(isInsideFactory);
                NetworkManager.Singleton.CustomMessagingManager
                    .SendNamedMessage(MsgSwap, player.actualClientId, writer);
            }
        }

        private static void OnReceive(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return;
            reader.ReadValueSafe(out float x);
            reader.ReadValueSafe(out float y);
            reader.ReadValueSafe(out float z);
            reader.ReadValueSafe(out bool isInsideFactory);

            var local = GameNetworkManager.Instance?.localPlayerController;
            if (local == null) return;
            Apply(local, new Vector3(x, y, z), isInsideFactory);
        }

        private static void Apply(GameNetcodeStuff.PlayerControllerB player, Vector3 dest, bool isInsideFactory)
        {
            player.isInsideFactory = isInsideFactory;
            player.TeleportPlayer(dest);
        }
    }
}
