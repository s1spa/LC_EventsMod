using System.Collections.Generic;
using GameNetcodeStuff;
using LCChaosMod.Utils;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs
{
    public class MobSpawnEvent : IChaosEvent
    {
        public string GetName()   => Loc.Get("event.mob");
        public bool   IsEnabled() => ChaosSettings.EnableMobSpawn.Value;

        public void Execute()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Plugin.Log.LogInfo("[MobSpawnEvent] Skipped - not host.");
                return;
            }

            PlayerControllerB? target = GetRandomLivingPlayer();
            if (target == null)
            {
                Plugin.Log.LogInfo("[MobSpawnEvent] No living player found.");
                return;
            }

            if (target.isInsideFactory)
                SpawnIndoor(target);
            else
                SpawnOutdoor(target);
        }

        // ── Indoor: використовуємо офіційний серверний API ───────────
        private static void SpawnIndoor(PlayerControllerB target)
        {
            var enemies = RoundManager.Instance.currentLevel?.Enemies;
            if (enemies == null || enemies.Count == 0)
            {
                Plugin.Log.LogWarning("[MobSpawnEvent] No indoor enemies on this level.");
                return;
            }

            int idx = Random.Range(0, enemies.Count);
            string name = enemies[idx].enemyType.enemyName;

            // Знаходимо AI-node найближчий до гравця всередині
            Vector3 spawnPos = GetNearestInsideNode(target.transform.position);

            Plugin.Log.LogInfo($"[MobSpawnEvent] Spawning indoor '{name}' near {target.playerUsername}.");
            RoundManager.Instance.SpawnEnemyOnServer(spawnPos, Random.Range(0f, 360f), idx);
        }

        // ── Outdoor: інстанціюємо prefab напряму ────────────────────
        private static void SpawnOutdoor(PlayerControllerB target)
        {
            var enemies = RoundManager.Instance.currentLevel?.OutsideEnemies;
            if (enemies == null || enemies.Count == 0)
            {
                Plugin.Log.LogWarning("[MobSpawnEvent] No outdoor enemies on this level.");
                return;
            }

            int idx = Random.Range(0, enemies.Count);
            var enemyType = enemies[idx].enemyType;
            string name = enemyType.enemyName;

            float ox = Random.Range(-10f, 10f);
            float oz = Random.Range(-10f, 10f);
            Vector3 pos = target.transform.position + new Vector3(ox, 0f, oz);
            float yRot = Random.Range(0f, 360f);

            Plugin.Log.LogInfo($"[MobSpawnEvent] Spawning outdoor '{name}' near {target.playerUsername}.");

            GameObject go = Object.Instantiate(enemyType.enemyPrefab, pos, Quaternion.Euler(0f, yRot, 0f));
            var netObj = go.GetComponentInChildren<NetworkObject>();
            if (netObj == null) { Object.Destroy(go); return; }
            netObj.Spawn(destroyWithScene: true);

            var ai = go.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.isOutside = true;
                ai.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                RoundManager.Instance.SpawnedEnemies.Add(ai);
            }
        }

        // ── Найближчий indoor AI-node ────────────────────────────────
        private static Vector3 GetNearestInsideNode(Vector3 origin)
        {
            var nodes = RoundManager.Instance.insideAINodes;
            if (nodes == null || nodes.Length == 0) return origin;

            float best = float.MaxValue;
            Vector3 result = origin;
            foreach (var n in nodes)
            {
                float d = Vector3.SqrMagnitude(n.transform.position - origin);
                if (d < best) { best = d; result = n.transform.position; }
            }
            return result;
        }

        private static PlayerControllerB? GetRandomLivingPlayer()
        {
            var all = StartOfRound.Instance?.allPlayerScripts;
            if (all == null) return null;

            var alive = new List<PlayerControllerB>();
            foreach (var p in all)
                if (p.isPlayerControlled && !p.isPlayerDead && !PlayerUtils.IsOnShip(p))
                    alive.Add(p);

            if (alive.Count == 0) return null;
            return alive[Random.Range(0, alive.Count)];
        }
    }
}
