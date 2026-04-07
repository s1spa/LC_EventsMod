using System.Collections;
using GameNetcodeStuff;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs.RagdollParty
{
    internal static class Net
    {
        private const string MsgTrip = "LCChaosMod_Trip";

        public static void Init()
        {
            NetworkManager.Singleton.CustomMessagingManager
                .RegisterNamedMessageHandler(MsgTrip, OnReceive);
        }

        public static void Broadcast(ulong targetClientId)
        {
            ApplyTrip(targetClientId);

            var writer = new FastBufferWriter(8, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(targetClientId);
                NetworkManager.Singleton.CustomMessagingManager
                    .SendNamedMessageToAll(MsgTrip, writer);
            }
        }

        private static void OnReceive(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return;
            reader.ReadValueSafe(out ulong clientId);
            ApplyTrip(clientId);
        }

        private static void ApplyTrip(ulong clientId)
        {
            var player = FindPlayer(clientId);
            if (player == null) return;

            float duration = Random.Range(ChaosSettings.RagdollDurationMin.Value, ChaosSettings.RagdollDurationMax.Value);
            Plugin.Log.LogInfo($"[RagdollParty] Tripping {player.playerUsername} for {duration:F1}s.");
            GameNetworkManager.Instance.StartCoroutine(TripCoroutine(player, duration));
        }

        private static IEnumerator TripCoroutine(PlayerControllerB player, float duration)
        {
            bool isLocal = player == GameNetworkManager.Instance?.localPlayerController;

            // ── АНІМАТОР ─────────────────────────────────────────────────
            player.playerBodyAnimator.enabled = false;

            // ── CharacterController + тимчасовий колайдер ────────────────
            // CC є і контролером, і колайдером. Додаємо CapsuleCollider з тими
            // ж розмірами, щоб Rigidbody не провалювався крізь геометрію.
            var cc = player.GetComponent<CharacterController>();
            CapsuleCollider? tempCol = null;
            if (cc != null)
            {
                tempCol = player.gameObject.AddComponent<CapsuleCollider>();
                tempCol.center = cc.center;
                tempCol.radius = cc.radius;
                tempCol.height = cc.height;
                cc.enabled = false;
            }

            // ── RIGIDBODY — гравець вже має його, просто конфігуруємо ────
            var rb = player.GetComponent<Rigidbody>();
            bool hadRb = rb != null;
            bool wasKinematic = hadRb && rb.isKinematic;
            RigidbodyInterpolation origInterp = hadRb ? rb.interpolation : RigidbodyInterpolation.None;
            CollisionDetectionMode origCDM = hadRb ? rb.collisionDetectionMode : CollisionDetectionMode.Discrete;
            RigidbodyConstraints origConstraints = hadRb ? rb.constraints : RigidbodyConstraints.None;

            if (!hadRb) rb = player.gameObject.AddComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.mass = 70f;
            rb.drag = 1f;
            rb.angularDrag = 2f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            rb.AddForce(player.transform.forward * 5f + Vector3.up * 4f, ForceMode.Impulse);
            rb.AddTorque(new Vector3(Random.Range(3f, 5f), Random.Range(-1f, 1f), 0f), ForceMode.Impulse);

            // Вимикаємо PlayerControllerB щоб його Update() не перекривав фізику
            player.enabled = false;

            yield return new WaitForSeconds(duration);

            // ── ВІДНОВЛЕННЯ ───────────────────────────────────────────────
            if (rb != null)
            {
                if (!hadRb) Object.Destroy(rb);
                else
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.collisionDetectionMode = origCDM;
                    rb.constraints = origConstraints;
                    rb.isKinematic = wasKinematic;
                    rb.interpolation = origInterp;
                }
            }

            if (tempCol != null) Object.Destroy(tempCol);
            if (cc != null) cc.enabled = true;

            if (player != null)
            {
                player.playerBodyAnimator.enabled = true;
                player.enabled = true;
            }
        }

        private static PlayerControllerB FindPlayer(ulong clientId)
        {
            foreach (var p in StartOfRound.Instance.allPlayerScripts)
                if (p.actualClientId == clientId && p.isPlayerControlled && !p.isPlayerDead)
                    return p;
            return null;
        }
    }
}
