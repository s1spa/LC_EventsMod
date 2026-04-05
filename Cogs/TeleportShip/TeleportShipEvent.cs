using System.Collections.Generic;
using GameNetcodeStuff;
using LCChaosMod.Utils;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs
{
    public class TeleportShipEvent : IChaosEvent
    {
        public string GetName()   => Loc.Get("event.teleport_ship");
        public bool   IsEnabled() => ChaosSettings.EnableTeleportShip.Value;

        public void Execute()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Plugin.Log.LogInfo("[TeleportShipEvent] Skipped - not host.");
                return;
            }

            Vector3 shipPos = GetShipPosition();

            var players = GetPlayersNotOnShip();
            if (players.Count == 0)
            {
                Plugin.Log.LogInfo("[TeleportShipEvent] All players already on ship.");
                return;
            }

            var player = players[Random.Range(0, players.Count)];
            Plugin.Log.LogInfo($"[TeleportShipEvent] Teleporting {player.playerUsername} to ship.");
            ChaosNetworkHandler.SendTeleport(player, shipPos, toShip: true);
        }

        private static Vector3 GetShipPosition()
        {
            // middleOfShipNode — стандартний anchor для корабля
            var mid = StartOfRound.Instance?.middleOfShipNode;
            if (mid != null) return mid.position;

            // fallback: перша spawn позиція
            var spawns = StartOfRound.Instance?.playerSpawnPositions;
            if (spawns != null && spawns.Length > 0)
                return spawns[0].position;

            return Vector3.zero;
        }

        private static List<PlayerControllerB> GetPlayersNotOnShip()
        {
            var all = StartOfRound.Instance?.allPlayerScripts;
            if (all == null) return new List<PlayerControllerB>();

            var result = new List<PlayerControllerB>();
            foreach (var p in all)
                if (p.isPlayerControlled && !p.isPlayerDead && !PlayerUtils.IsOnShip(p))
                    result.Add(p);
            return result;
        }
    }
}
