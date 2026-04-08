using Unity.Collections;
using Unity.Netcode;

namespace LCChaosMod.Cogs.Firefly
{
    internal static class Net
    {
        private const string MsgNotify = "LCChaosMod_FireflyNotify"; // client → server
        private const string MsgGlow   = "LCChaosMod_Firefly";       // server → all clients

        public static void Init()
        {
            var mgr = NetworkManager.Singleton.CustomMessagingManager;
            mgr.RegisterNamedMessageHandler(MsgNotify, OnReceiveNotify);
            mgr.RegisterNamedMessageHandler(MsgGlow,   OnReceiveGlow);
        }

        public static void Broadcast(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                BroadcastGlow(clientId);
            }
            else
            {

                var writer = new FastBufferWriter(8, Allocator.Temp);
                using (writer)
                {
                    writer.WriteValueSafe(clientId);
                    NetworkManager.Singleton.CustomMessagingManager
                        .SendNamedMessage(MsgNotify, NetworkManager.ServerClientId, writer);
                }
            }
        }

        private static void OnReceiveNotify(ulong _, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            reader.ReadValueSafe(out ulong clientId);
            FireflyTracker.AddLightToPlayer(clientId); 
            BroadcastGlow(clientId);                   
        }

        private static void BroadcastGlow(ulong clientId)
        {
            var writer = new FastBufferWriter(8, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(clientId);
                NetworkManager.Singleton.CustomMessagingManager
                    .SendNamedMessageToAll(MsgGlow, writer);
            }
        }

        private static void OnReceiveGlow(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return;
            reader.ReadValueSafe(out ulong clientId);
            if (clientId == NetworkManager.Singleton.LocalClientId) return;
            FireflyTracker.AddLightToPlayer(clientId);
        }
    }
}
