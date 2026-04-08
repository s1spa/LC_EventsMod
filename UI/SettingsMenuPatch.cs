using System.Collections;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LCChaosMod.UI
{

    // Вішає кнопку "Chaos Mod" в головному меню через SceneManager.sceneLoaded.
    // Надійніше ніж патчити MenuManager, бо не залежить від назви методу.
    internal static class MainMenuInjector
    {
        public static void Init()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Plugin.Log.LogInfo("[MainMenuInjector] Registered sceneLoaded.");
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Plugin.Log.LogInfo($"[MainMenuInjector] Scene loaded: '{scene.name}'");

            if (scene.name != "MainMenu") return;

            // Використовуємо окремий GameObject для корутини, бо Plugin.Instance може бути null
            var runner = new GameObject("__ChaosInjectorRunner");
            Object.DontDestroyOnLoad(runner);
            runner.AddComponent<CoroutineHelper>().Run(TryInject(runner));
        }

        private sealed class CoroutineHelper : MonoBehaviour
        {
            public void Run(IEnumerator routine) => StartCoroutine(routine);
        }

        private static IEnumerator TryInject(GameObject runner)
        {
            yield return new WaitUntil(() => Object.FindObjectOfType<Button>(true) != null);

            if (GameObject.Find("ChaosModButton") != null) { Object.Destroy(runner); yield break; }

            var allBtns = Object.FindObjectsOfType<Button>(includeInactive: true);
            Plugin.Log.LogInfo($"[MainMenuInjector] Buttons found: {allBtns.Length}");

            // Знаходимо кнопки з одного батька (головне меню)
            Button src = null!;
            float topY = float.MinValue;
            float spacing = 60f;
            var siblingRts = new System.Collections.Generic.List<RectTransform>();

            foreach (var b in allBtns)
            {
                var rt = b.GetComponent<RectTransform>();
                var pos = rt != null ? rt.anchoredPosition : Vector2.zero;
                Plugin.Log.LogInfo($"  '{b.name}' | parent: '{b.transform.parent?.name}' | anchoredPos: {pos}");

                string n = b.name.ToLower();
                if (n.Contains("host") || n.Contains("settings") || n.Contains("quit"))
                    src = src == null ? b : src; // берем перший знайдений
            }
            if (src == null) src = allBtns.Length > 0 ? allBtns[0] : null!;
            if (src == null) { Object.Destroy(runner); yield break; }

            // Збираємо позиції братів (однаковий батько)
            foreach (Transform child in src.transform.parent)
            {
                var rt = child.GetComponent<RectTransform>();
                if (rt != null) siblingRts.Add(rt);
            }

            if (siblingRts.Count >= 2)
            {
                siblingRts.Sort((a, b2) => b2.anchoredPosition.y.CompareTo(a.anchoredPosition.y));
                // spacing = різниця між першою і другою кнопкою (від'ємна — вниз)
                spacing = siblingRts[1].anchoredPosition.y - siblingRts[0].anchoredPosition.y;
                topY = siblingRts[0].anchoredPosition.y;
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

            // Розміщуємо ВИЩЕ всіх інших кнопок
            var btnRt = btn.GetComponent<RectTransform>();
            if (btnRt != null)
                btnRt.anchoredPosition = new Vector2(btnRt.anchoredPosition.x, topY - spacing); // вище топу (spacing від'ємний → мінус від'ємного = вище)

            btn.transform.SetAsFirstSibling();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SettingsOverlay.Instance?.Show());

            Plugin.Log.LogInfo("[MainMenuInjector] Button injected!");
            Object.Destroy(runner);
        }
    }
}
