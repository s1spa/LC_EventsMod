using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using LCChaosMod.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace LCChaosMod.Cogs
{
    public class TurretSpawnEvent : IChaosEvent
    {
        public string GetName()    => Loc.Get("event.turrets");
        public bool   IsEnabled()  => ChaosSettings.EnableTurrets.Value;

        public void Execute()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Plugin.Log.LogInfo("[TurretSpawnEvent] Skipped - not host.");
                return;
            }
            EventManager.Instance?.StartCoroutine(SpawnLoop());
        }

        private static IEnumerator SpawnLoop()
        {
            GameObject? prefab = FindTurretPrefab();
            if (prefab == null)
            {
                Plugin.Log.LogWarning("[TurretSpawnEvent] Turret prefab not found in spawnableMapObjects.");
                yield break;
            }

            int count = Random.Range(ChaosSettings.TurretCountMin.Value, ChaosSettings.TurretCountMax.Value + 1);
            Plugin.Log.LogInfo($"[TurretSpawnEvent] Spawning {count} turrets.");

            for (int i = 0; i < count; i++)
            {
                SpawnTurret(prefab);
                yield return new WaitForSeconds(
                    Random.Range(ChaosSettings.TurretRateMin.Value, ChaosSettings.TurretRateMax.Value));
            }
        }

        private static GameObject? FindTurretPrefab()
        {
            var objects = StartOfRound.Instance?.currentLevel?.spawnableMapObjects;
            if (objects == null) return null;

            foreach (var entry in objects)
            {
                if (entry?.prefabToSpawn != null &&
                    entry.prefabToSpawn.GetComponentInChildren<Turret>() != null)
                    return entry.prefabToSpawn;
            }
            return null;
        }

        private static void SpawnTurret(GameObject prefab)
        {
            PlayerControllerB? target = GetRandomOutdoorPlayer();
            if (target == null)
            {
                Plugin.Log.LogInfo("[TurretSpawnEvent] No outdoor player found, skipping turret.");
                return;
            }

            float ox = Random.Range(-8f, 8f);
            float oz = Random.Range(-8f, 8f);
            Vector3 pos = target.transform.position + new Vector3(ox, 0f, oz);

            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                pos = hit.position;

            Transform parent = RoundManager.Instance.mapPropsContainer.transform;
            GameObject go = Object.Instantiate(prefab, pos, Quaternion.identity, parent);

            var netObj = go.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Spawn(destroyWithScene: true);
            else
                Plugin.Log.LogWarning("[TurretSpawnEvent] Turret prefab has no NetworkObject component.");
        }

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
