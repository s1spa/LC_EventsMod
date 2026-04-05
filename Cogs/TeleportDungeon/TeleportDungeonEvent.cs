using System.Collections.Generic;
using GameNetcodeStuff;
using LCChaosMod.Utils;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs
{
    public class TeleportDungeonEvent : IChaosEvent
    {
        public string GetName()   => Loc.Get("event.teleport_dungeon");
        public bool   IsEnabled() => ChaosSettings.EnableTeleportDungeon.Value;

        public void Execute()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Plugin.Log.LogInfo("[TeleportDungeonEvent] Skipped - not host.");
                return;
            }

            var nodes = RoundManager.Instance?.insideAINodes;
            if (nodes == null || nodes.Length == 0)
            {
                Plugin.Log.LogWarning("[TeleportDungeonEvent] No inside AI nodes.");
                return;
            }

            // Телепортуємо всіх гравців які зараз всередині данжу
            var inside = GetInsidePlayers();
            if (inside.Count == 0)
            {
                Plugin.Log.LogInfo("[TeleportDungeonEvent] No players inside factory.");
                return;
            }

            foreach (var player in inside)
            {
                Vector3 dest = nodes[Random.Range(0, nodes.Length)].transform.position;
                Plugin.Log.LogInfo($"[TeleportDungeonEvent] Teleporting {player.playerUsername} to {dest}.");
                ChaosNetworkHandler.SendTeleport(player, dest, toShip: false);
            }
        }

        private static List<PlayerControllerB> GetInsidePlayers()
        {
            var all = StartOfRound.Instance?.allPlayerScripts;
            if (all == null) return new List<PlayerControllerB>();

            var result = new List<PlayerControllerB>();
            foreach (var p in all)
                if (p.isPlayerControlled && !p.isPlayerDead && p.isInsideFactory)
                    result.Add(p);
            return result;
        }
    }
}
