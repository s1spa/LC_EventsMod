using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LCChaosMod.UI
{
    // Harmony Postfix на MenuManager.Start — UI вже ініціалізований, корутини не потрібні.
    [HarmonyPatch(typeof(MenuManager), "Start")]
    internal static class MainMenuInjector
    {
        // * Finalizer запускається навіть якщо інші патчі (LethalCasino) кинули виняток
        [HarmonyFinalizer]
        static void Finalizer(System.Exception? __exception)
        {
            // ! Не дублюємо — якщо кнопка вже є, виходимо
            if (GameObject.Find("ChaosModButton") != null) return;

            var allBtns = Object.FindObjectsOfType<Button>(includeInactive: true);
            Plugin.Log.LogInfo($"[MainMenuInjector] Buttons found: {allBtns.Length}");

            Button src = null!;
            float topY    = float.MinValue;
            float spacing = 60f;
            var siblingRts = new List<RectTransform>();

            foreach (var b in allBtns)
            {
                var rt  = b.GetComponent<RectTransform>();
                var pos = rt != null ? rt.anchoredPosition : Vector2.zero;
                Plugin.Log.LogInfo($"  '{b.name}' | parent: '{b.transform.parent?.name}' | anchoredPos: {pos}");

                string n = b.name.ToLower();
                if (n.Contains("host") || n.Contains("settings") || n.Contains("quit"))
                    src = src == null ? b : src;
            }
            if (src == null) src = allBtns.Length > 0 ? allBtns[0] : null!;
            if (src == null) return;

            // Збираємо позиції братів (однаковий батько) щоб вирахувати spacing
            foreach (Transform child in src.transform.parent)
            {
                var rt = child.GetComponent<RectTransform>();
                if (rt != null) siblingRts.Add(rt);
            }

            if (siblingRts.Count >= 2)
            {
                siblingRts.Sort((a, b) => b.anchoredPosition.y.CompareTo(a.anchoredPosition.y));
                spacing = siblingRts[1].anchoredPosition.y - siblingRts[0].anchoredPosition.y;
                topY    = siblingRts[0].anchoredPosition.y;
            }
            else if (siblingRts.Count == 1)
            {
                topY = siblingRts[0].anchoredPosition.y;
            }

            Plugin.Log.LogInfo($"[MainMenuInjector] topY={topY}, spacing={spacing}");

            if (SettingsOverlay.Instance == null)
            {
                var go = new GameObject("ChaosModOverlay");
                Object.DontDestroyOnLoad(go);
                go.AddComponent<SettingsOverlay>();
            }

            var btn = Object.Instantiate(src, src.transform.parent);
            btn.name = "ChaosModButton";

            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = "> Chaos Mod";

            // * Розміщуємо ВИЩЕ всіх інших кнопок (spacing від'ємний → мінус від'ємного = вище)
            var btnRt = btn.GetComponent<RectTransform>();
            if (btnRt != null)
                btnRt.anchoredPosition = new Vector2(btnRt.anchoredPosition.x, topY - spacing);

            btn.transform.SetAsFirstSibling();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SettingsOverlay.Instance?.Show());

            Plugin.Log.LogInfo("[MainMenuInjector] Button injected!");
        }
    }
}
