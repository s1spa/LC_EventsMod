using System.Collections;
using LCChaosMod.Utils;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs
{
    public class BerserkTurretEvent : IChaosEvent
    {
        public string GetName()   => Loc.Get("event.berserk_turret");
        public bool   IsEnabled() => ChaosSettings.EnableBerserkTurret.Value;

        public void Execute()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Plugin.Log.LogInfo("[BerserkTurretEvent] Skipped - not host.");
                return;
            }

            var turrets = Object.FindObjectsOfType<Turret>();
            if (turrets.Length == 0)
            {
                Plugin.Log.LogInfo("[BerserkTurretEvent] No turrets found.");
                return;
            }

            float duration = ChaosSettings.BerserkDuration.Value;
            Plugin.Log.LogInfo($"[BerserkTurretEvent] {turrets.Length} turret(s) going berserk for {duration}s.");

            foreach (var turret in turrets)
                EventManager.Instance?.StartCoroutine(BerserkCoroutine(turret, duration));
        }

        private static IEnumerator BerserkCoroutine(Turret turret, float duration)
        {
            if (turret == null) yield break;

            turret.turretMode = TurretMode.Berserk;

            yield return new WaitForSeconds(duration);

            if (turret != null)
                turret.turretMode = TurretMode.Detection;
        }
    }
}
