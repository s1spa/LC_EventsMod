using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs.FakeMessage
{
    internal static class Net
    {
        private const string MsgFakeMsg = "LCChaosMod_FakeMsg";

        public static void Init()
        {
            NetworkManager.Singleton.CustomMessagingManager
                .RegisterNamedMessageHandler(MsgFakeMsg, OnReceive);
        }

        public static void Broadcast()
        {
            byte idx = (byte)Random.Range(0, FakeMessageOverlay.MessageCount);
            FakeMessageOverlay.Show(idx);

            var writer = new FastBufferWriter(4, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(idx);
                NetworkManager.Singleton.CustomMessagingManager
                    .SendNamedMessageToAll(MsgFakeMsg, writer);
            }
        }

        private static void OnReceive(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return;
            reader.ReadValueSafe(out byte idx);
            FakeMessageOverlay.Show(idx);
        }
    }
}
