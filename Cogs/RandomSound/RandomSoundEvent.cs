using System.Collections.Generic;
using GameNetcodeStuff;
using static LCChaosMod.Utils.PlayerUtils;
using UnityEngine;

namespace LCChaosMod.Cogs
{
    public class RandomSoundEvent : IChaosEvent
    {
        public string GetName()   => Loc.Get("event.random_sound");
        public bool   IsEnabled() => ChaosSettings.EnableRandomSound.Value;

        public void Execute()
        {
            var inside = GetInsidePlayers();
            if (inside.Count == 0)
            {
                Plugin.Log.LogInfo("[RandomSoundEvent] No players inside factory.");
                return;
            }

            var enemies = RoundManager.Instance?.currentLevel?.Enemies;
            if (enemies == null || enemies.Count == 0)
            {
                Plugin.Log.LogWarning("[RandomSoundEvent] No indoor enemies on this level.");
                return;
            }

            // Збираємо тільки тих у кого є audioClips
            var pool = new System.Collections.Generic.List<AudioClip>();
            foreach (var entry in enemies)
            {
                var clips = entry?.enemyType?.audioClips;
                if (clips == null) continue;
                foreach (var c in clips)
                    if (c != null) pool.Add(c);
            }

            if (pool.Count == 0)
            {
                Plugin.Log.LogWarning("[RandomSoundEvent] No enemies with audioClips on this level.");
                return;
            }

            AudioClip clip = pool[Random.Range(0, pool.Count)];
            if (clip == null) return;

            // Перемішуємо список і беремо рандомну кількість гравців (від 1 до всіх)
            for (int i = inside.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (inside[i], inside[j]) = (inside[j], inside[i]);
            }
            int maxTargets  = Mathf.Max(1, inside.Count / 2);
            int targetCount = Random.Range(1, maxTargets + 1);

            for (int i = 0; i < targetCount; i++)
            {
                var target = inside[i];
                float ox = Random.Range(-6f, 6f);
                float oz = Random.Range(-6f, 6f);
                Vector3 pos = target.transform.position + new Vector3(ox, 1f, oz);

                Plugin.Log.LogInfo($"[RandomSoundEvent] Playing '{clip.name}' near {target.playerUsername}.");
                RandomSound.Net.Broadcast(clip.name, pos);
            }
        }

        private static List<PlayerControllerB> GetInsidePlayers()
        {
            var all = StartOfRound.Instance?.allPlayerScripts;
            if (all == null) return new List<PlayerControllerB>();

            var result = new List<PlayerControllerB>();
            foreach (var p in all)
                if (p.isPlayerControlled && !p.isPlayerDead && IsInDungeon(p))
                    result.Add(p);
            return result;
        }
    }
}
