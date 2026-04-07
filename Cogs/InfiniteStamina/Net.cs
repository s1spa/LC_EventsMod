using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs.InfiniteStamina
{
    internal static class Net
    {
        private const string MsgStamina = "LCChaosMod_Stamina";

        public static void Init()
        {
            NetworkManager.Singleton.CustomMessagingManager
                .RegisterNamedMessageHandler(MsgStamina, OnReceive);
        }

        public static void Broadcast(float duration)
        {
            GameNetworkManager.Instance.StartCoroutine(StaminaCoroutine(duration));

            var writer = new FastBufferWriter(8, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(duration);
                NetworkManager.Singleton.CustomMessagingManager
                    .SendNamedMessageToAll(MsgStamina, writer);
            }
        }

        private static void OnReceive(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return;
            reader.ReadValueSafe(out float duration);
            GameNetworkManager.Instance.StartCoroutine(StaminaCoroutine(duration));
        }

        private static IEnumerator StaminaCoroutine(float duration)
        {
            float elapsed = 0f;
            var player = GameNetworkManager.Instance?.localPlayerController;
            if (player == null) yield break;

            Plugin.Log.LogInfo($"[Adrenaline] Active for {duration}s.");
            while (elapsed < duration)
            {
                player.sprintMeter = 1f;
                player.externalForces = player.transform.forward * 8f;
                elapsed += Time.deltaTime;
                yield return null;
            }

            player.externalForces = Vector3.zero;
        }
    }
}
