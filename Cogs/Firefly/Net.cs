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

        /// <summary>Called on the client that grabbed the apparatus.</summary>
        public static void Broadcast(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                // Host grabbed it — add locally and broadcast to all clients
                BroadcastGlow(clientId);
            }
            else
            {
                // Client grabbed it — notify server, server will broadcast
                var writer = new FastBufferWriter(8, Allocator.Temp);
                using (writer)
                {
                    writer.WriteValueSafe(clientId);
                    NetworkManager.Singleton.CustomMessagingManager
                        .SendNamedMessage(MsgNotify, NetworkManager.ServerClientId, writer);
                }
            }
        }

        // Server receives notification from a client
        private static void OnReceiveNotify(ulong _, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            reader.ReadValueSafe(out ulong clientId);
            FireflyTracker.AddLightToPlayer(clientId); // host sees it
            BroadcastGlow(clientId);                   // tell all clients
        }

        // Server → all clients
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

        // Client receives glow notification from server
        private static void OnReceiveGlow(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return; // host already applied in OnReceiveNotify
            reader.ReadValueSafe(out ulong clientId);
            if (clientId == NetworkManager.Singleton.LocalClientId) return; // already applied locally
            FireflyTracker.AddLightToPlayer(clientId);
        }
    }
}
