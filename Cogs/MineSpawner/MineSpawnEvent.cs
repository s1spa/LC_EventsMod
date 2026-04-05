using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using LCChaosMod.Utils;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs
{
    public class MineSpawnEvent : IChaosEvent
    {
        public string GetName() => Loc.Get("event.mines");
        public bool   IsEnabled() => ChaosSettings.EnableMines.Value;

        public void Execute()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Plugin.Log.LogInfo("[MineSpawnEvent] Skipped - not host.");
                return;
            }
            EventManager.Instance?.StartCoroutine(SpawnLoop());
        }

        // ── Spawn loop: MineCountMin–MineCountMax mines, MineRateMin–MineRateMax s apart ──
        private static IEnumerator SpawnLoop()
        {
            GameObject? prefab = FindMinePrefab();
            if (prefab == null)
            {
                Plugin.Log.LogWarning("[MineSpawnEvent] Mine prefab not found in spawnableMapObjects.");
                yield break;
            }

            int count = Random.Range(ChaosSettings.MineCountMin.Value, ChaosSettings.MineCountMax.Value + 1);
            Plugin.Log.LogInfo($"[MineSpawnEvent] Spawning {count} mines.");

            for (int i = 0; i < count; i++)
            {
                SpawnMine(prefab);
                yield return new WaitForSeconds(
                    Random.Range(ChaosSettings.MineRateMin.Value, ChaosSettings.MineRateMax.Value));
            }
        }

        // ── Find mine prefab via RoundManager.spawnableMapObjects ────
        private static GameObject? FindMinePrefab()
        {
            var objects = StartOfRound.Instance?.currentLevel?.spawnableMapObjects;
            if (objects == null) return null;

            foreach (var entry in objects)
            {
                if (entry?.prefabToSpawn != null &&
                    entry.prefabToSpawn.GetComponentInChildren<Landmine>() != null)
                    return entry.prefabToSpawn;
            }
            return null;
        }

        // ── Spawn single mine near a random non-ship player ──────────
        private static void SpawnMine(GameObject prefab)
        {
            PlayerControllerB? target = GetRandomOutdoorPlayer();
            if (target == null)
            {
                Plugin.Log.LogInfo("[MineSpawnEvent] No outdoor player found, skipping mine.");
                return;
            }

            float ox = Random.Range(-6f, 6f);
            float oz = Random.Range(-6f, 6f);
            Vector3 pos = target.transform.position + new Vector3(ox, 0f, oz);

            Transform parent = RoundManager.Instance.mapPropsContainer.transform;
            GameObject go = Object.Instantiate(prefab, pos, Quaternion.identity, parent);

            var netObj = go.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Spawn(destroyWithScene: true);
            else
                Plugin.Log.LogWarning("[MineSpawnEvent] Mine prefab has no NetworkObject component.");
        }

        // ── Pick a random living player not on ship ──────────────────
        private static PlayerControllerB? GetRandomOutdoorPlayer()
        {
            var all = StartOfRound.Instance?.allPlayerScripts;
            if (all == null) return null;

            var candidates = new List<PlayerControllerB>();
            foreach (var p in all)
            {
                if (p.isPlayerControlled && !p.isPlayerDead && !PlayerUtils.IsOnShip(p))
                    candidates.Add(p);
            }

            if (candidates.Count == 0) return null;
            return candidates[Random.Range(0, candidates.Count)];
        }
    }
}
