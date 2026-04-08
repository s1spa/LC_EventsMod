using System;
using UnityEngine;
using UnityEngine.UI;

namespace LCChaosMod.UI
{
    public class SettingsOverlay : MonoBehaviour
    {
        public static SettingsOverlay? Instance { get; private set; }

        private float _minInt;
        private float _maxInt;
        private int   _diff;    // 1-5
        private int   _langIdx; // 0=EN 1=UA

        // ── Евенти ──────────────────────────────────────────────────
        private bool  _evtMines;
        private int   _mineCountMin;
        private int   _mineCountMax;
        private float _mineRateMin;
        private float _mineRateMax;

        private bool  _evtTurrets;
        private int   _turretCountMin;
        private int   _turretCountMax;
        private float _turretRateMin;
        private float _turretRateMax;

        private bool  _evtMob;
        private bool  _evtTeleportDungeon;
        private bool  _evtTeleportShip;
        private bool  _evtRandomSound;
        private bool  _evtInfiniteStamina;
        private float _staminaDuration;
        private bool  _evtFirefly;
        private bool  _evtPlayerSwap;
        private bool  _evtBerserkTurret;
        private float _berserkDuration;
        private bool  _evtFootball;
        private float _footballDuration;
        private bool  _evtFakeMessage;
        private bool  _evtSizeMatters;
        private float _sizeScale;
        private float _sizeStretchScale;
        private float _sizeDuration;

        private Font?       _gameFont;
        private GameObject? _canvasRoot;
        private GameObject? _contentPanel;

        private static readonly string[] Langs = { "EN", "UA" };

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadFromConfig();
            FindGameFont();
            BuildModernCanvas();
            Hide();
        }

        public void Show()
        {
            LoadFromConfig();
            _canvasRoot?.SetActive(true);
            Canvas.ForceUpdateCanvases(); // прораховуємо ContentSizeFitter до першого скролу
            UpdateUIValues();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void Hide()
        {
            _canvasRoot?.SetActive(false);
        }

        // ── Ловимо ігровий шрифт ──────────────────────────────────────
        private void FindGameFont()
        {
            _gameFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Fallback
            foreach (var f in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (f.name.Contains("3270") || f.name.Contains("edunline")) 
                { 
                    _gameFont = f; 
                    break; 
                }
            }
        }

        // ── БУДУЄМО UGUI ЧЕРЕЗ КОД ────────────────────────────────────
        private void BuildModernCanvas()
        {
            // 1. Root Canvas
            _canvasRoot = new GameObject("ChaosCanvas_UGUI");
            DontDestroyOnLoad(_canvasRoot);
            Canvas canvas = _canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Поверх усього
            _canvasRoot.AddComponent<GraphicRaycaster>();
            
            CanvasScaler scaler = _canvasRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // 2. Затемнення фону
            GameObject bg = UIBuilder.CreatePanel(_canvasRoot.transform, new Color(0, 0, 0, 0.85f));
            UIBuilder.StretchToFill(bg.GetComponent<RectTransform>());

            // 3. Головне вікно (Термінал)
            GameObject win = UIBuilder.CreatePanel(bg.transform, new Color(0.03f, 0.05f, 0.03f, 1f));
            RectTransform winRect = win.GetComponent<RectTransform>();
            winRect.sizeDelta = new Vector2(900, 750);
            
            // Додаємо зелену рамку
            Outline outline = win.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.8f, 0.2f);
            outline.effectDistance = new Vector2(2, -2);

            VerticalLayoutGroup winVlg = win.AddComponent<VerticalLayoutGroup>();
            winVlg.padding = new RectOffset(30, 30, 30, 30);
            winVlg.spacing = 15;
            winVlg.childControlHeight = false; 
            winVlg.childForceExpandHeight = false;

            // Заголовок
            UIBuilder.CreateText(win.transform, "> LC CHAOS MOD OS_v0.9", _gameFont!, 36, new Color(0.2f, 0.9f, 0.2f), TextAnchor.MiddleCenter, 50);

            // 4. Створюємо ScrollView для налаштувань
            _contentPanel = UIBuilder.CreateScrollView(win.transform, new Vector2(840, 520));

            // 5. Заповнюємо контент (Наші івенти)
            PopulateSettings();

            // 6. Нижня панель кнопок (Save / Cancel)
            GameObject footer = UIBuilder.CreatePanel(win.transform, Color.clear);
            HorizontalLayoutGroup hlg = footer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30; 
            hlg.childAlignment = TextAnchor.MiddleCenter;
            footer.GetComponent<RectTransform>().sizeDelta = new Vector2(840, 60);

            UIBuilder.CreateButton(footer.transform, "[ SAVE ]", _gameFont!, () => { SaveToConfig(); Hide(); }, 200, 50);
            UIBuilder.CreateButton(footer.transform, "[ CANCEL ]", _gameFont!, Hide, 200, 50);

            // 7. Підписи по кутках (поза VLG)
            var dimGreen = new Color(0.2f, 0.5f, 0.2f);

            var biosT = UIBuilder.CreateText(win.transform, $"BIOS ver. {MyPluginInfo.PLUGIN_VERSION} by s1spa", _gameFont!, 12, dimGreen, TextAnchor.LowerLeft, 18);
            biosT.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var biosRT = biosT.GetComponent<RectTransform>();
            biosRT.anchorMin = biosRT.anchorMax = new Vector2(0, 0);
            biosRT.pivot     = new Vector2(0, 0);
            biosRT.anchoredPosition = new Vector2(10, 8);
            biosRT.sizeDelta = new Vector2(340, 18);

            var dirT = UIBuilder.CreateText(win.transform, "COMPANY DIRECTIVE", _gameFont!, 12, dimGreen, TextAnchor.LowerRight, 18);
            dirT.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var dirRT = dirT.GetComponent<RectTransform>();
            dirRT.anchorMin = dirRT.anchorMax = new Vector2(1, 0);
            dirRT.pivot     = new Vector2(1, 0);
            dirRT.anchoredPosition = new Vector2(-10, 8);
            dirRT.sizeDelta = new Vector2(280, 18);
        }

        // ── Хелпер: акордеон (кнопка → тіло, стрілка + переклад) ──────
        private void Accordion(Transform parent, Func<string> getTitle, Action<Transform> fill)
        {
            GameObject body  = null!;
            Button     btn   = null!;
            bool       isOpen = false;
            btn = UIBuilder.CreateAccordionBtn(parent, () => "> " + getTitle(), _gameFont!, () =>
            {
                isOpen = !isOpen;
                body.SetActive(isOpen);
                var lbl = btn.GetComponentInChildren<Text>();
                if (lbl != null) lbl.text = "  " + (isOpen ? "v " : "> ") + getTitle();
            });
            // UIUpdater для заголовку акордеону (оновлює переклад)
            btn.gameObject.AddComponent<UIUpdater>().OnUpdate = () =>
            {
                var lbl = btn.GetComponentInChildren<Text>();
                if (lbl != null) lbl.text = "  " + (isOpen ? "v " : "> ") + getTitle();
            };
            body = UIBuilder.CreateAccordionBody(parent);
            fill(body.transform);
        }

        // ── ЗАПОВНЕННЯ МЕНЮ НАЛАШТУВАННЯМИ ────────────────────────────
        private void PopulateSettings()
        {
            Transform c = _contentPanel!.transform;
            static Func<string> L(string key) => () => Loc.Get(key); // скорочення

            // Мова + таймінги
            UIBuilder.CreateCycleButton(c, L("ui.s.lang"), _gameFont!, () => Langs[_langIdx], () =>
            {
                _langIdx = (_langIdx + 1) % Langs.Length;
                Loc.SetLang(Langs[_langIdx]);
                UpdateUIValues(); // оновити всі лейбли одразу
            });
            UIBuilder.CreateStepper(c, L("ui.s.min_interval"), _gameFont!, () => _minInt, v => _minInt = v, 5f);
            UIBuilder.CreateStepper(c, L("ui.s.max_interval"), _gameFont!, () => _maxInt, v => _maxInt = v, 5f);
            UIBuilder.CreateSpacer(c, 15);

            // Акордеони
            Accordion(c, L("ui.s.mines"), body =>
            {
                UIBuilder.CreateToggle(body, L("ui.s.enable"),   _gameFont!, () => _evtMines,     v => _evtMines     = v);
                UIBuilder.CreateStepper(body, L("ui.s.min_count"), _gameFont!, () => _mineCountMin,  v => _mineCountMin  = (int)v, 1f);
                UIBuilder.CreateStepper(body, L("ui.s.max_count"), _gameFont!, () => _mineCountMax,  v => _mineCountMax  = (int)v, 1f);
                UIBuilder.CreateStepper(body, L("ui.s.rate_min"),  _gameFont!, () => _mineRateMin,   v => _mineRateMin   = v, 0.5f);
                UIBuilder.CreateStepper(body, L("ui.s.rate_max"),  _gameFont!, () => _mineRateMax,   v => _mineRateMax   = v, 0.5f);
            });
            Accordion(c, L("ui.s.turrets"), body =>
            {
                UIBuilder.CreateToggle(body, L("ui.s.enable"),   _gameFont!, () => _evtTurrets,    v => _evtTurrets    = v);
                UIBuilder.CreateStepper(body, L("ui.s.min_count"), _gameFont!, () => _turretCountMin, v => _turretCountMin = (int)v, 1f);
                UIBuilder.CreateStepper(body, L("ui.s.max_count"), _gameFont!, () => _turretCountMax, v => _turretCountMax = (int)v, 1f);
                UIBuilder.CreateStepper(body, L("ui.s.rate_min"),  _gameFont!, () => _turretRateMin,  v => _turretRateMin  = v, 0.5f);
                UIBuilder.CreateStepper(body, L("ui.s.rate_max"),  _gameFont!, () => _turretRateMax,  v => _turretRateMax  = v, 0.5f);
            });
            Accordion(c, L("ui.s.size_matters"), body =>
            {
                UIBuilder.CreateToggle(body, L("ui.s.enable"),        _gameFont!, () => _evtSizeMatters,  v => _evtSizeMatters  = v);
                UIBuilder.CreateStepper(body, L("ui.s.shrink_scale"), _gameFont!, () => _sizeScale,        v => _sizeScale        = v, 0.1f);
                UIBuilder.CreateStepper(body, L("ui.s.stretch_scale"),_gameFont!, () => _sizeStretchScale, v => _sizeStretchScale = v, 0.1f);
                UIBuilder.CreateStepper(body, L("ui.s.duration"),     _gameFont!, () => _sizeDuration,     v => _sizeDuration     = v, 5f);
            });
            Accordion(c, L("ui.s.berserk_turret"), body =>
            {
                UIBuilder.CreateToggle(body, L("ui.s.enable"),    _gameFont!, () => _evtBerserkTurret, v => _evtBerserkTurret = v);
                UIBuilder.CreateStepper(body, L("ui.s.duration"), _gameFont!, () => _berserkDuration,  v => _berserkDuration  = v, 5f);
            });
            Accordion(c, L("ui.s.football"), body =>
            {
                UIBuilder.CreateToggle(body, L("ui.s.enable"),    _gameFont!, () => _evtFootball,      v => _evtFootball      = v);
                UIBuilder.CreateStepper(body, L("ui.s.duration"), _gameFont!, () => _footballDuration, v => _footballDuration = v, 5f);
            });
            Accordion(c, L("ui.s.stamina"), body =>
            {
                UIBuilder.CreateToggle(body, L("ui.s.enable"),    _gameFont!, () => _evtInfiniteStamina, v => _evtInfiniteStamina = v);
                UIBuilder.CreateStepper(body, L("ui.s.duration"), _gameFont!, () => _staminaDuration,    v => _staminaDuration    = v, 5f);
            });

            // Прості перемикачі
            UIBuilder.CreateSpacer(c, 10);
            UIBuilder.CreateToggle(c, L("ui.s.mob_spawn"),        _gameFont!, () => _evtMob,            v => _evtMob            = v);
            UIBuilder.CreateToggle(c, L("ui.s.teleport_dungeon"), _gameFont!, () => _evtTeleportDungeon, v => _evtTeleportDungeon = v);
            UIBuilder.CreateToggle(c, L("ui.s.teleport_ship"),    _gameFont!, () => _evtTeleportShip,    v => _evtTeleportShip   = v);
            UIBuilder.CreateToggle(c, L("ui.s.random_sound"),     _gameFont!, () => _evtRandomSound,     v => _evtRandomSound    = v);
            UIBuilder.CreateToggle(c, L("ui.s.firefly"),          _gameFont!, () => _evtFirefly,         v => _evtFirefly        = v);
            UIBuilder.CreateToggle(c, L("ui.s.player_swap"),      _gameFont!, () => _evtPlayerSwap,      v => _evtPlayerSwap     = v);
            UIBuilder.CreateToggle(c, L("ui.s.fake_message"),     _gameFont!, () => _evtFakeMessage,     v => _evtFakeMessage    = v);
        }

        // Оновлює текст на кнопках при відкритті меню / зміні мови
        private void UpdateUIValues()
        {
            if (_canvasRoot == null) return;
            foreach (var updater in _canvasRoot.GetComponentsInChildren<IUIUpdater>(true))
                updater.UpdateVisuals();
        }

        // ── Config логіка (твоя стара залишається) ───────────────────
       private void LoadFromConfig()
        {
            _minInt  = ChaosSettings.MinInterval.Value;
            _maxInt  = ChaosSettings.MaxInterval.Value;
            _diff    = Mathf.Clamp(ChaosSettings.Difficulty.Value, 1, 5);
            _langIdx = ChaosSettings.Language.Value == "UA" ? 1 : 0;
            Loc.SetLang(Langs[_langIdx]);

            _evtMines     = ChaosSettings.EnableMines.Value;
            _mineCountMin = ChaosSettings.MineCountMin.Value;
            _mineCountMax = ChaosSettings.MineCountMax.Value;
            _mineRateMin  = ChaosSettings.MineRateMin.Value;
            _mineRateMax  = ChaosSettings.MineRateMax.Value;

            _evtMob              = ChaosSettings.EnableMobSpawn.Value;
            _evtTeleportDungeon  = ChaosSettings.EnableTeleportDungeon.Value;
            _evtTeleportShip     = ChaosSettings.EnableTeleportShip.Value;
            _evtRandomSound       = ChaosSettings.EnableRandomSound.Value;
            _evtInfiniteStamina   = ChaosSettings.EnableInfiniteStamina.Value;
            _staminaDuration      = ChaosSettings.StaminaDuration.Value;
            _evtFirefly           = ChaosSettings.EnableGlowstick.Value;
            _evtPlayerSwap        = ChaosSettings.EnablePlayerSwap.Value;
            _evtBerserkTurret     = ChaosSettings.EnableBerserkTurret.Value;
            _berserkDuration      = ChaosSettings.BerserkDuration.Value;
            _evtFootball          = ChaosSettings.EnableFootball.Value;
            _footballDuration     = ChaosSettings.FootballDuration.Value;
            _evtFakeMessage       = ChaosSettings.EnableFakeMessage.Value;
            _evtSizeMatters       = ChaosSettings.EnableSizeMatters.Value;
            _sizeScale            = ChaosSettings.SizeScale.Value;
            _sizeStretchScale     = ChaosSettings.SizeStretchScale.Value;
            _sizeDuration         = ChaosSettings.SizeDuration.Value;
            _evtTurrets     = ChaosSettings.EnableTurrets.Value;
            _turretCountMin = ChaosSettings.TurretCountMin.Value;
            _turretCountMax = ChaosSettings.TurretCountMax.Value;
            _turretRateMin  = ChaosSettings.TurretRateMin.Value;
            _turretRateMax  = ChaosSettings.TurretRateMax.Value;
        }

        private void SaveToConfig()
        {
            ChaosSettings.MinInterval.Value = _minInt;
            ChaosSettings.MaxInterval.Value = _maxInt;
            ChaosSettings.Difficulty.Value  = _diff;
            ChaosSettings.Language.Value    = Langs[_langIdx];

            ChaosSettings.EnableMines.Value  = _evtMines;
            ChaosSettings.MineCountMin.Value = _mineCountMin;
            ChaosSettings.MineCountMax.Value = _mineCountMax;
            ChaosSettings.MineRateMin.Value  = _mineRateMin;
            ChaosSettings.MineRateMax.Value  = _mineRateMax;

            ChaosSettings.EnableMobSpawn.Value        = _evtMob;
            ChaosSettings.EnableTeleportDungeon.Value = _evtTeleportDungeon;
            ChaosSettings.EnableTeleportShip.Value    = _evtTeleportShip;
            ChaosSettings.EnableRandomSound.Value     = _evtRandomSound;
            ChaosSettings.EnableInfiniteStamina.Value = _evtInfiniteStamina;
            ChaosSettings.StaminaDuration.Value       = _staminaDuration;
            ChaosSettings.EnableGlowstick.Value       = _evtFirefly;
            ChaosSettings.EnablePlayerSwap.Value      = _evtPlayerSwap;
            ChaosSettings.EnableBerserkTurret.Value   = _evtBerserkTurret;
            ChaosSettings.BerserkDuration.Value       = _berserkDuration;
            ChaosSettings.EnableFootball.Value        = _evtFootball;
            ChaosSettings.FootballDuration.Value      = _footballDuration;
            ChaosSettings.EnableFakeMessage.Value     = _evtFakeMessage;
            ChaosSettings.EnableSizeMatters.Value     = _evtSizeMatters;
            ChaosSettings.SizeScale.Value             = _sizeScale;
            ChaosSettings.SizeStretchScale.Value      = _sizeStretchScale;
            ChaosSettings.SizeDuration.Value          = _sizeDuration;
            ChaosSettings.EnableTurrets.Value  = _evtTurrets;
            ChaosSettings.TurretCountMin.Value = _turretCountMin;
            ChaosSettings.TurretCountMax.Value = _turretCountMax;
            ChaosSettings.TurretRateMin.Value  = _turretRateMin;
            ChaosSettings.TurretRateMax.Value  = _turretRateMax;
        }
    }

    // =====================================================================
    // ================= UGUI BUILDER ENGINE (Міні-рушій) ==================
    // =====================================================================
    public static class UIBuilder
    {
        public static GameObject CreatePanel(Transform parent, Color color)
        {
            GameObject go = new GameObject("Panel");
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = color;
            return go;
        }

        public static void StretchToFill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        public static Text CreateText(Transform parent, string text, Font font, int size, Color color, TextAnchor align, float height)
        {
            GameObject go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, height);
            Text t = go.AddComponent<Text>();
            t.font = font; t.text = text; t.fontSize = size; t.color = color; t.alignment = align;
            return t;
        }

        public static GameObject CreateSpacer(Transform parent, float height)
        {
            GameObject go = new GameObject("Spacer");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0, height);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height; le.minHeight = height;
            return go;
        }

        public static Button CreateButton(Transform parent, string text, Font font, Action onClick, float width, float height)
        {
            GameObject go = CreatePanel(parent, new Color(0.1f, 0.3f, 0.1f));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);
            go.AddComponent<LayoutElement>().preferredHeight = height;
            Button btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick());
            Text t = CreateText(go.transform, text, font, 24, Color.white, TextAnchor.MiddleCenter, height);
            StretchToFill(t.GetComponent<RectTransform>());
            return btn;
        }

        public static Button CreateAccordionBtn(Transform parent, Func<string> getText, Font font, Action onClick)
        {
            GameObject go = CreatePanel(parent, new Color(0.05f, 0.15f, 0.05f));
            go.AddComponent<LayoutElement>().preferredHeight = 40;
            Button btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick());
            Text t = CreateText(go.transform, "  " + getText(), font, 22, new Color(0.4f, 1f, 0.4f), TextAnchor.MiddleLeft, 40);
            StretchToFill(t.GetComponent<RectTransform>());
            t.alignment = TextAnchor.MiddleLeft;
            return btn;
        }

        public static GameObject CreateAccordionBody(Transform parent)
        {
            GameObject body = CreatePanel(parent, Color.clear);
            body.SetActive(false);
            VerticalLayoutGroup vlg = body.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 0, 5, 5);
            vlg.spacing = 8; vlg.childControlHeight = false; vlg.childForceExpandHeight = false;
            body.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return body;
        }

        // Замість складних слайдерів - термінальний "Stepper" VALUE +/-
        public static void CreateStepper(Transform parent, Func<string> getLabel, Font font, Func<float> getVal, Action<float> setVal, float step)
        {
            GameObject row = CreatePanel(parent, Color.clear);
            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false; hlg.spacing = 15;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 35);

            Text labelText = CreateText(row.transform, getLabel(), font, 20, Color.white, TextAnchor.MiddleLeft, 35);
            labelText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 35);

            Text valText = CreateText(row.transform, getVal().ToString("F1"), font, 22, Color.yellow, TextAnchor.MiddleCenter, 35);
            valText.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 35);

            CreateButton(row.transform, " - ", font, () => { setVal(getVal() - step); valText.text = getVal().ToString("F1"); }, 50, 35);
            valText.transform.SetAsLastSibling();
            CreateButton(row.transform, " + ", font, () => { setVal(getVal() + step); valText.text = getVal().ToString("F1"); }, 50, 35);

            row.AddComponent<UIUpdater>().OnUpdate = () => {
                labelText.text = getLabel();
                valText.text = getVal().ToString("F1");
            };
        }

        // Кнопка що циклічно перемикає між значеннями (напр. мова EN/UA)
        public static void CreateCycleButton(Transform parent, Func<string> getLabel, Font font, Func<string> getVal, Action onCycle)
        {
            GameObject row = CreatePanel(parent, Color.clear);
            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false; hlg.spacing = 20;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 35);
            row.AddComponent<LayoutElement>().preferredHeight = 35;

            Text labelText = CreateText(row.transform, getLabel(), font, 20, Color.white, TextAnchor.MiddleLeft, 35);
            labelText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 35);

            Text valText = null!;
            Button btn = CreateButton(row.transform, getVal(), font, () => { onCycle(); valText.text = getVal(); }, 120, 35);
            valText = btn.GetComponentInChildren<Text>();

            row.AddComponent<UIUpdater>().OnUpdate = () => {
                labelText.text = getLabel();
                valText.text = getVal();
            };
        }

        // Термінальний перемикач [ ON ] / [ OFF ]
        public static void CreateToggle(Transform parent, Func<string> getLabel, Font font, Func<bool> getVal, Action<bool> setVal)
        {
            GameObject row = CreatePanel(parent, Color.clear);
            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false; hlg.spacing = 20;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 35);

            Text labelText = CreateText(row.transform, getLabel(), font, 20, Color.white, TextAnchor.MiddleLeft, 35);
            labelText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 35);

            Text btnText = null!;
            Button btn = CreateButton(row.transform, getVal() ? "[ ON ]" : "[ OFF ]", font, () =>
            {
                setVal(!getVal());
                btnText.text = getVal() ? "[ ON ]" : "[ OFF ]";
                btnText.color = getVal() ? Color.green : Color.red;
            }, 100, 35);
            btnText = btn.GetComponentInChildren<Text>();
            btnText.color = getVal() ? Color.green : Color.red;

            row.AddComponent<UIUpdater>().OnUpdate = () => {
                labelText.text = getLabel();
                btnText.text = getVal() ? "[ ON ]" : "[ OFF ]";
                btnText.color = getVal() ? Color.green : Color.red;
            };
        }

        // Створює магію прокрутки
        public static GameObject CreateScrollView(Transform parent, Vector2 size)
        {
            GameObject scrollRoot = CreatePanel(parent, new Color(0, 0, 0, 0.2f));
            scrollRoot.GetComponent<RectTransform>().sizeDelta = size;
            ScrollRect sr = scrollRoot.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.scrollSensitivity = 5f;
            sr.movementType = ScrollRect.MovementType.Clamped; // без пружного відскоку

            GameObject viewport = CreatePanel(scrollRoot.transform, Color.clear);
            StretchToFill(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            sr.viewport = viewport.GetComponent<RectTransform>();

            GameObject content = CreatePanel(viewport.transform, Color.clear);
            RectTransform cRect = content.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0, 1); cRect.anchorMax = new Vector2(1, 1);
            cRect.pivot = new Vector2(0.5f, 1);
            cRect.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 10;
            vlg.childControlHeight = true; vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;

            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = cRect;

            return content;
        }
    }

    public interface IUIUpdater { void UpdateVisuals(); }
    public class UIUpdater : MonoBehaviour, IUIUpdater { public Action OnUpdate; public void UpdateVisuals() => OnUpdate?.Invoke(); }
}