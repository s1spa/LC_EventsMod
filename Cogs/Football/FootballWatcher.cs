using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using UnityEngine;

namespace LCChaosMod.Cogs.Football
{
    /// <summary>
    /// Runs on the host only for the duration of the Football event.
    /// Kicks items near running players using a smooth parabolic arc.
    /// </summary>
    public class FootballWatcher : MonoBehaviour
    {
        private const float KickRadius   = 1.5f;
        private const float KickCooldown = 0.8f;
        private const float MinSpeed     = 0.3f;

        // kick physics
        private const float HSpeed  = 9f;   // horizontal m/s
        private const float VSpeed  = 4f;   // initial upward m/s
        private const float Gravity = 18f;  // m/s²

        // collision masks (same as used by LC internals)
        private const int WallMask  = 369101057; // SoccerBallProp wall mask
        private const int FloorMask = 268437760; // GrabbableObject floor mask

        private float _timeLeft;
        private readonly Dictionary<GrabbableObject, float> _cooldowns = new();
        private readonly Dictionary<ulong, Vector3>         _prevPos   = new();
        private readonly HashSet<GrabbableObject>            _flying    = new();

        private GrabbableObject[] _itemCache = System.Array.Empty<GrabbableObject>();
        private float _cacheTimer = 0f;
        private const float CacheInterval = 1f;

        public void Setup(float duration) => _timeLeft = duration;

        private void Update()
        {
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f) { Destroy(gameObject); return; }

            // Tick cooldowns
            var keys = new List<GrabbableObject>(_cooldowns.Keys);
            foreach (var k in keys)
            {
                _cooldowns[k] -= Time.deltaTime;
                if (_cooldowns[k] <= 0f) _cooldowns.Remove(k);
            }

            var all = StartOfRound.Instance?.allPlayerScripts;
            if (all == null) return;

            _cacheTimer -= Time.deltaTime;
            if (_cacheTimer <= 0f)
            {
                _itemCache  = Object.FindObjectsOfType<GrabbableObject>();
                _cacheTimer = CacheInterval;
            }
            var items = _itemCache;

            foreach (var player in all)
            {
                if (!player.isPlayerControlled || player.isPlayerDead) continue;

                Vector3 pos = player.transform.position;

                bool moving = false;
                if (_prevPos.TryGetValue(player.actualClientId, out Vector3 prev))
                    moving = Vector3.Distance(pos, prev) / Time.deltaTime >= MinSpeed;
                _prevPos[player.actualClientId] = pos;

                if (!moving) continue;

                foreach (var item in items)
                {
                    if (item == null) continue;
                    if (item.playerHeldBy != null) continue;
                    if (_flying.Contains(item)) continue;
                    if (_cooldowns.ContainsKey(item)) continue;
                    if (Vector3.Distance(pos, item.transform.position) > KickRadius) continue;

                    StartCoroutine(KickCoroutine(item, player));
                    _cooldowns[item] = KickCooldown;
                    Plugin.Log.LogInfo($"[Football] {player.playerUsername} kicked {item.itemProperties?.itemName ?? item.name}.");
                }
            }
        }

        private IEnumerator KickCoroutine(GrabbableObject item, PlayerControllerB player)
        {
            _flying.Add(item);
            item.hasHitGround = true; // stop GrabbableObject from running FallWithCurve

            // Horizontal direction away from player
            Vector3 dir = item.transform.position - player.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) dir = player.transform.forward;
            dir = dir.normalized;

            Vector3 pos = item.transform.position;
            float vy = VSpeed;
            float elapsed = 0f;

            while (elapsed < 3f)
            {
                if (item == null || item.playerHeldBy != null) break;

                float dt = Time.deltaTime;
                elapsed += dt;
                vy -= Gravity * dt;

                Vector3 step = (dir * HSpeed + Vector3.up * vy) * dt;

                // Wall / obstacle check — stop item at surface instead of clipping
                if (Physics.Raycast(pos, step.normalized, out RaycastHit wallHit,
                    step.magnitude + 0.08f, WallMask, QueryTriggerInteraction.Ignore))
                {
                    // Reflect off wall, kill most of the speed
                    Vector3 flatNormal = wallHit.normal;
                    flatNormal.y = 0f;
                    dir = Vector3.Reflect(dir, flatNormal.normalized);
                    dir.y = 0f;
                    dir = dir.normalized;
                    vy *= 0.2f;
                    pos = wallHit.point + wallHit.normal * 0.05f;
                }
                else
                {
                    pos += step;
                }

                item.transform.position = pos;

                // Land when falling and close to floor
                if (vy < 0f)
                {
                    float checkDist = Mathf.Abs(vy) * dt + 0.15f;
                    if (Physics.Raycast(pos + Vector3.up * 0.1f, Vector3.down, out RaycastHit floorHit,
                        checkDist, FloorMask, QueryTriggerInteraction.Ignore))
                    {
                        float landY = floorHit.point.y + (item.itemProperties?.verticalOffset ?? 0f);
                        pos.y = landY;
                        item.transform.position = pos;
                        break;
                    }
                }

                yield return null;
            }

            // Settle — update GrabbableObject state so it stays put
            if (item != null)
            {
                item.hasHitGround    = true;
                item.fallTime        = 1f;
                item.targetFloorPosition = item.transform.parent != null
                    ? item.transform.parent.InverseTransformPoint(item.transform.position)
                    : item.transform.position;

                _flying.Remove(item);
            }
        }

        private void OnDestroy()
        {
            _flying.Clear();
            Plugin.Log.LogInfo("[Football] Event ended.");
        }
    }
}
