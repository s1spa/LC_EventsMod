using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs.RandomSound
{
    internal static class Net
    {
        private const string MsgSound = "LCChaosMod_Sound";

        public static void Init()
        {
            NetworkManager.Singleton.CustomMessagingManager
                .RegisterNamedMessageHandler(MsgSound, OnReceive);
        }

        public static void Broadcast(string clipName, Vector3 pos)
        {
            PlayLocal(clipName, pos);

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

        private static void OnReceive(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return;
            reader.ReadValueSafe(out string clipName);
            reader.ReadValueSafe(out float x);
            reader.ReadValueSafe(out float y);
            reader.ReadValueSafe(out float z);
            PlayLocal(clipName, new Vector3(x, y, z));
        }

        internal static void PlayLocal(string clipName, Vector3 pos)
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
            Plugin.Log.LogWarning($"[RandomSound] Clip '{clipName}' not found locally.");
        }
    }
}
