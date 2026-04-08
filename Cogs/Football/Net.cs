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

        public static void Broadcast(GrabbableObject item, Vector3 dir, Vector3 startPos)
        {
            GameNetworkManager.Instance.StartCoroutine(
                FootballWatcher.KickCoroutineStatic(item, dir, startPos));

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
