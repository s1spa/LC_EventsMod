using System.Collections;
using UnityEngine;

namespace LCChaosMod.Cogs.FakeMessage
{
    public class FakeMessageOverlay : MonoBehaviour
    {
        private const float Duration    = 5f;
        private const float RefreshRate = 5f;

        private static readonly (string hEN, string bEN, string hUA, string bUA)[] Messages =
        {
            (
                "CRITICAL ERROR",
                "SHIP LEAVING IN {0}s",
                "КРИТИЧНА ПОМИЛКА",
                "КОРАБЕЛЬ ВІДЛІТАЄ ЧЕРЕЗ {0}с"
            ),
            (
                "COMPANY NOTICE",
                "QUOTA DOUBLED EFFECTIVE IMMEDIATELY",
                "ПОВІДОМЛЕННЯ КОМПАНІЇ",
                "КВОТА ЗБІЛЬШЕНА ВДВІЧІ"
            ),
            (
                "WARNING",
                "ALL ASSETS WILL BE CONFISCATED",
                "УВАГА",
                "ВЕШЕ МАЙНО КОНФІСКУЮТЬ ЧЕРЕЗ"
            ),
            (
                "COMPANY ANNOUNCEMENT",
                "NEW QUOTA TARGET: $99,999",
                "ОГОЛОШЕННЯ КОМПАНІЇ",
                "НОВА КВОТА: $99,999"
            ),
            (
                "FACILITY ALERT",
                "SELF-DESTRUCT SEQUENCE INITIATED",
                "СИГНАЛ ОБ'ЄКТУ",
                "ЗАПУЩЕНО САМОЗНИЩЕННЯ"
            ),
            (
                "HR NOTICE",
                "YOUR INSURANCE HAS BEEN CANCELLED",
                "ВІДДІЛ КАДРІВ",
                "ВАШУ СТРАХОВКУ СКАСОВАНО"
            ),
            (
                "POLICY UPDATE",
                "UNPAID OVERTIME NOW MANDATORY",
                "ОНОВЛЕННЯ ПРАВИЛ",
                "ПОНАДНОРМОВА РОБОТА ТЕПЕР БЕЗ ОПЛАТИ"
            ),
            (
                "SECURITY BREACH",
                "UNIDENTIFIED ENTITY IN YOUR LOCATION",
                "ПОРУШЕННЯ БЕЗПЕКИ",
                "НЕВІДОМА СУТНІСТЬ У ВАШОМУ МІСЦІ"
            ),
            (
                "EMPLOYEE REVIEW",
                "PERFORMANCE EVALUATION IN {0}s",
                "ПЕРЕВІРКА ПЕРСОНАЛУ",
                "ОЦІНКА ЕФЕКТИВНОСТІ ЧЕРЕЗ {0}с"
            ),
            (
                "COMPANY MEMO",
                "EMPLOYEE SALARIES REDUCED TO $2",
                "МЕМОРАНДУМ КОМПАНІЇ",
                "ЗАРПЛАТУ ЗНИЖЕНО ДО $2"
            ),
            (
                "SYSTEM ERROR",
                "ALL PROGRESS RESETS IN {0}s",
                "СИСТЕМНА ПОМИЛКА",
                "ВЕСЬ ПРОГРЕС СКИНЕТЬСЯ ЧЕРЕЗ {0}с"
            ),
            (
                "OXYGEN ALERT",
                "OXYGEN SUPPLY CRITICAL — {0}s REMAINING",
                "КИСНЕВИЙ СИГНАЛ",
                "КРИТИЧНИЙ РІВЕНЬ КИСНЮ — {0}с ЗАЛИШИЛОСЬ"
            ),
        };

        public static int MessageCount => Messages.Length;

        private int   _msgIdx;
        private float _timeLeft;

        public static void Show(byte idx)
        {
            if (FindObjectOfType<FakeMessageOverlay>() != null) return;

            var go = new GameObject("FakeMessageOverlay");
            DontDestroyOnLoad(go);
            var o = go.AddComponent<FakeMessageOverlay>();
            o._msgIdx   = Mathf.Clamp(idx, 0, Messages.Length - 1);
            o._timeLeft = Duration;
            Plugin.Log.LogInfo($"[FakeMessage] Showing message #{idx}.");
        }

        private IEnumerator Start()
        {
            while (_timeLeft > 0f)
            {
                if (StartOfRound.Instance != null && StartOfRound.Instance.inShipPhase)
                    break;

                ShowTip();
                yield return new WaitForSeconds(RefreshRate);
                _timeLeft -= RefreshRate;
            }

            Destroy(gameObject);
        }

        private void ShowTip()
        {
            var hud = HUDManager.Instance;
            if (hud == null) return;

            var    msg  = Messages[_msgIdx];
            string body = msg.bEN.Replace("{0}s", "30s").Replace("{0}с", "30с");

            // isWarning: false → blue TriggerHint panel
            hud.DisplayTip(msg.hEN, body, isWarning: false);
        }

        private void OnDestroy()
        {
            Plugin.Log.LogInfo("[FakeMessage] Overlay destroyed.");
        }
    }
}
