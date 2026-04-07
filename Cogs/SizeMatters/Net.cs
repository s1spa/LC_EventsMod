using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs.SizeMatters
{
    internal static class Net
    {
        private const string MsgShrink  = "LCChaosMod_Shrink";
        private const string MsgStretch = "LCChaosMod_Stretch";

        // Використовуємо playerClientId (0, 1, 2, 3), а не actualClientId
        private static readonly HashSet<ulong> _active = new();
        private static readonly Dictionary<int, int> _shrinkGen  = new();
        private static readonly Dictionary<int, int> _stretchGen = new();

        public static bool IsActive(ulong pClientId) => _active.Contains(pClientId);

        public static void Init()
        {
            var mgr = NetworkManager.Singleton.CustomMessagingManager;
            mgr.RegisterNamedMessageHandler(MsgShrink,  OnReceiveShrink);
            mgr.RegisterNamedMessageHandler(MsgStretch, OnReceiveStretch);
        }

        // ── Shrink ────────────────────────────────────────────────────────────

        public static void Broadcast(ulong targetPlayerClientId, float scale, float duration)
        {
            ApplyShrink(targetPlayerClientId, scale, duration);
            var writer = new FastBufferWriter(32, Allocator.Temp); // Збільшено буфер для безпеки
            using (writer)
            {
                writer.WriteValueSafe(targetPlayerClientId);
                writer.WriteValueSafe(scale);
                writer.WriteValueSafe(duration);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll(MsgShrink, writer);
            }
        }

        private static void OnReceiveShrink(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return;
            reader.ReadValueSafe(out ulong pClientId);
            reader.ReadValueSafe(out float scale);
            reader.ReadValueSafe(out float duration);
            ApplyShrink(pClientId, scale, duration);
        }

        private static void ApplyShrink(ulong pClientId, float scale, float duration)
        {
            var player = FindPlayer(pClientId);
            if (player == null) return;
            GameNetworkManager.Instance.StartCoroutine(ShrinkCoroutine(player, scale, duration));
        }

        private static IEnumerator ShrinkCoroutine(PlayerControllerB player, float scale, float duration)
        {
            int pidx = (int)player.playerClientId;
            int gen  = (_shrinkGen.TryGetValue(pidx, out int g) ? g : 0) + 1;
            _shrinkGen[pidx] = gen;
            _active.Add(player.playerClientId);

            Plugin.Log.LogInfo($"[SizeMatters] {player.playerUsername} → shrink {scale} for {duration}s (gen {gen}).");

            bool isLocal = player == GameNetworkManager.Instance?.localPlayerController;

            Transform? cam = isLocal ? player.gameplayCamera?.transform : null;
            Vector3 origCamPos = cam != null ? cam.localPosition : Vector3.zero;
            if (cam != null)
                cam.localPosition = new Vector3(origCamPos.x, origCamPos.y * scale, origCamPos.z);

            CharacterController? cc = isLocal ? player.GetComponent<CharacterController>() : null;
            float origHeight = 0f, origRadius = 0f;
            Vector3 origCenter = Vector3.zero;
            if (cc != null)
            {
                origHeight = cc.height; origRadius = cc.radius; origCenter = cc.center;
                cc.height = origHeight * scale; cc.radius = origRadius * scale; cc.center = origCenter * scale;
            }

            float origSpeed = 0f;
            if (isLocal) { origSpeed = player.movementSpeed; player.movementSpeed = origSpeed * (2f - scale); }

            var enforcer = ScaleEnforcer.Attach(player, new Vector3(scale, scale, scale));

            float elapsed = 0f;
            // Додано перевірку player.isPlayerControlled на випадок, якщо гравець вийде з гри
            while (elapsed < duration
                   && player != null && player.isPlayerControlled && !player.isPlayerDead
                   && !StartOfRound.Instance.inShipPhase
                   && _shrinkGen.GetValueOrDefault(pidx) == gen)
            {
                SetPitch(pidx, player, 2f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            bool isCurrent = _shrinkGen.GetValueOrDefault(pidx) == gen;

            if (enforcer != null) Object.Destroy(enforcer.gameObject);
            if (player != null) player.thisPlayerBody.localScale = Vector3.one;

            if (player != null && !player.isPlayerDead && isCurrent)
            {
                if (cam != null) cam.localPosition = origCamPos;
                if (cc  != null)
                {
                    cc.enabled = false;
                    player.transform.position += Vector3.up * (origHeight - cc.height);
                    cc.height = origHeight; cc.radius = origRadius; cc.center = origCenter;
                    cc.enabled = true;
                }
                if (isLocal) player.movementSpeed = origSpeed;
            }

            if (isCurrent)
            {
                _active.Remove(player.playerClientId);
                ResetPitch(pidx, player); // Жорстко повертаємо голос до норми
            }
            Plugin.Log.LogInfo($"[SizeMatters] {player?.playerUsername} (small) done gen={gen}.");
        }

        // ── Stretch ───────────────────────────────────────────────────────────

        public static void BroadcastStretch(ulong targetPlayerClientId, float scaleY, float duration)
        {
            ApplyStretch(targetPlayerClientId, scaleY, duration);
            var writer = new FastBufferWriter(32, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(targetPlayerClientId);
                writer.WriteValueSafe(scaleY);
                writer.WriteValueSafe(duration);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll(MsgStretch, writer);
            }
        }

        private static void OnReceiveStretch(ulong _, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsServer) return;
            reader.ReadValueSafe(out ulong pClientId);
            reader.ReadValueSafe(out float scaleY);
            reader.ReadValueSafe(out float duration);
            ApplyStretch(pClientId, scaleY, duration);
        }

        private static void ApplyStretch(ulong pClientId, float scaleY, float duration)
        {
            var player = FindPlayer(pClientId);
            if (player == null) return;
            GameNetworkManager.Instance.StartCoroutine(StretchCoroutine(player, scaleY, duration));
        }

        private static IEnumerator StretchCoroutine(PlayerControllerB player, float scaleY, float duration)
        {
            int pidx = (int)player.playerClientId;
            int gen  = (_stretchGen.TryGetValue(pidx, out int g) ? g : 0) + 1;
            _stretchGen[pidx] = gen;
            _active.Add(player.playerClientId);

            Plugin.Log.LogInfo($"[SizeMatters] {player.playerUsername} → stretch Y={scaleY} for {duration}s (gen {gen}).");

            bool isLocal = player == GameNetworkManager.Instance?.localPlayerController;

            Transform? cam = isLocal ? player.gameplayCamera?.transform : null;
            Vector3 origCamPos = cam != null ? cam.localPosition : Vector3.zero;
            if (cam != null)
                cam.localPosition = new Vector3(origCamPos.x, origCamPos.y * scaleY, origCamPos.z);

            CharacterController? cc = isLocal ? player.GetComponent<CharacterController>() : null;
            float origHeight = 0f;
            Vector3 origCenter = Vector3.zero;
            if (cc != null)
            {
                origHeight = cc.height; origCenter = cc.center;
                cc.height = origHeight * scaleY; cc.center = origCenter * scaleY;
            }

            var enforcer = ScaleEnforcer.Attach(player, new Vector3(1f, scaleY, 1f));

            float elapsed = 0f;
            while (elapsed < duration
                   && player != null && player.isPlayerControlled && !player.isPlayerDead
                   && !StartOfRound.Instance.inShipPhase
                   && _stretchGen.GetValueOrDefault(pidx) == gen)
            {
                SetPitch(pidx, player, 0.7f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            bool isCurrent = _stretchGen.GetValueOrDefault(pidx) == gen;

            if (enforcer != null) Object.Destroy(enforcer.gameObject);
            if (player != null) player.thisPlayerBody.localScale = Vector3.one;

            if (player != null && !player.isPlayerDead && isCurrent)
            {
                if (cam != null) cam.localPosition = origCamPos;
                if (cc  != null)
                {
                    cc.enabled = false;
                    // Для великого гравця МИ НЕ рухаємо його позицію вниз, щоб він не провалився під текстури!
                    cc.height = origHeight; cc.center = origCenter;
                    cc.enabled = true;
                }
            }

            if (isCurrent)
            {
                _active.Remove(player.playerClientId);
                ResetPitch(pidx, player);
            }
            Plugin.Log.LogInfo($"[SizeMatters] {player?.playerUsername} (tall) done gen={gen}.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void SetPitch(int pidx, PlayerControllerB player, float pitch)
        {
            if (SoundManager.Instance != null)
            {
                if (pidx < SoundManager.Instance.playerVoicePitchTargets.Length)
                    SoundManager.Instance.playerVoicePitchTargets[pidx] = pitch;
                if (pidx < SoundManager.Instance.playerVoicePitches.Length)
                    SoundManager.Instance.playerVoicePitches[pidx] = pitch;
            }
            if (player != null && player.currentVoiceChatAudioSource != null)
                player.currentVoiceChatAudioSource.pitch = pitch;
        }

        private static void ResetPitch(int pidx, PlayerControllerB player)
        {
            if (SoundManager.Instance != null)
            {
                if (pidx < SoundManager.Instance.playerVoicePitchTargets.Length)
                    SoundManager.Instance.playerVoicePitchTargets[pidx] = 1f;
                if (pidx < SoundManager.Instance.playerVoicePitches.Length)
                    SoundManager.Instance.playerVoicePitches[pidx] = 1f;
            }
            if (player != null && player.currentVoiceChatAudioSource != null)
                player.currentVoiceChatAudioSource.pitch = 1f;
        }

        // Тепер шукаємо гравців за їхнім стабільним індексом
        private static PlayerControllerB? FindPlayer(ulong pClientId)
        {
            foreach (var p in StartOfRound.Instance.allPlayerScripts)
                if (p.playerClientId == pClientId && p.isPlayerControlled && !p.isPlayerDead)
                    return p;
            return null;
        }
    }

    internal class ScaleEnforcer : MonoBehaviour
    {
        private PlayerControllerB? _player;
        private Vector3 _scale;

        internal static ScaleEnforcer Attach(PlayerControllerB player, Vector3 scale)
        {
            var go   = new GameObject($"SizeEnforcer_{player.playerClientId}");
            var comp = go.AddComponent<ScaleEnforcer>();
            comp._player = player;
            comp._scale  = scale;
            return comp;
        }

        private void LateUpdate()
        {
            if (_player != null && _player.thisPlayerBody != null)
                _player.thisPlayerBody.localScale = _scale;
        }
    }
}