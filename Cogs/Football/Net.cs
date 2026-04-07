using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs.Football
{
    internal static class Net
    {
        private const string MsgKick = "LCChaosMod_FootballKick";

        public static void Init()
        {
            NetworkManager.Singleton.CustomMessagingManager
                .RegisterNamedMessageHandler(MsgKick, OnReceive);
        }

        /// <summary>Host calls this when a kick is detected.</summary>
        public static void Broadcast(GrabbableObject item, Vector3 dir, Vector3 startPos)
        {
            // Run locally on host
            GameNetworkManager.Instance.StartCoroutine(
                FootballWatcher.KickCoroutineStatic(item, dir, startPos));

            // Send to clients: networkObjectId (8) + dir (12) + startPos (12)
            var writer = new FastBufferWriter(36, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(item.NetworkObjectId);
                writer.WriteValueSafe(dir);
                writer.WriteValueSafe(startPos);
                NetworkManager.Singleton.CustomMessagingManager
                    .SendNamedMessageToAll(MsgKick, writer);
            }
        }

        private static void OnReceive(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return;

            reader.ReadValueSafe(out ulong netObjId);
            reader.ReadValueSafe(out Vector3 dir);
            reader.ReadValueSafe(out Vector3 startPos);

            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
                    .TryGetValue(netObjId, out NetworkObject netObj)) return;
            var item = netObj.GetComponent<GrabbableObject>();
            if (item == null) return;

            GameNetworkManager.Instance.StartCoroutine(
                FootballWatcher.KickCoroutineStatic(item, dir, startPos));
        }
    }
}
