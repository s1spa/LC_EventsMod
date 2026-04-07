using UnityEngine;

namespace LCChaosMod.UI
{
    public class SettingsOverlay : MonoBehaviour
    {
        public static SettingsOverlay? Instance { get; private set; }

        private bool  _visible;
        private Rect  _winRect;
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

        private Vector2 _evtScroll;

        // яка подія зараз розгорнута (-1 = жодна)
        private int   _expandedEvt = -1;

        // ── Кольори (#FF5000 як основний) ───────────────────────
        private static readonly Color CMain   = new Color(1.00f, 0.314f, 0.00f); // #FF5000
        private static readonly Color CMainDim= new Color(0.55f, 0.17f,  0.00f); // тьмяний
        private static readonly Color CBg     = new Color(0.03f, 0.008f, 0.00f, 0.95f); // темний фон з помаранчевим відтінком
        private static readonly Color CActTxt = new Color(0.20f, 0.06f,  0.00f); // темний текст на active кнопці

        private static readonly string[] Langs = { "EN", "UA" };
        private static readonly string[] Diffs = { "НУБ", "СЛАБАК", "НОРМАЛЬНО", "ВАЖКОВАТО", "МЕГА ЛЕГКО" };

        // ── Стилі ───────────────────────────────────────────────
        private GUIStyle? _sWin, _sTitle, _sLang, _sLbl,
                          _sBtnOff, _sBtnOn,
                          _sSliderBg, _sSliderThumb, _sFooter;
        private Texture2D? _txBg, _txMain, _txDim, _txBtnOff, _txBtnOn;

        // ── Lifecycle ────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromConfig();
        }
        private void OnDestroy()
        {
            Instance = null;
            if (_txBg     != null) Object.Destroy(_txBg);
            if (_txMain   != null) Object.Destroy(_txMain);
            if (_txDim    != null) Object.Destroy(_txDim);
            if (_txBtnOff != null) Object.Destroy(_txBtnOff);
            if (_txBtnOn  != null) Object.Destroy(_txBtnOn);
            // Circle texture is stored only in _sSliderThumb.normal.background
            if (_sSliderThumb?.normal.background != null) Object.Destroy(_sSliderThumb.normal.background);
            // Hover texture stored locally in EnsureStyles
            if (_sBtnOn?.hover.background != null) Object.Destroy(_sBtnOn.hover.background);
        }

        public void Show()
        {
            LoadFromConfig();
            _visible = true;
            _winRect = new Rect(Screen.width / 2f - 420f, Screen.height / 2f - 270f, 840f, 540f);
            Cursor.visible   = true;
            Cursor.lockState = CursorLockMode.None;
        }
        public void Hide() => _visible = false;

        // ── OnGUI ────────────────────────────────────────────────
        private void OnGUI()
        {
            if (!_visible) return;
            EnsureStyles();

            var old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.88f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = old;

            _winRect = GUI.Window(9999, _winRect, Draw, GUIContent.none, _sWin!);
        }

        private void Draw(int id)
        {
            float W = _winRect.width;
            float H = _winRect.height;
            float pad = 20f;

            GUILayout.Space(12);

            // ── Заголовок ───────────────────────────────────────
            GUILayout.Label(Loc.Get("ui.title"), _sTitle!);
            GUILayout.Space(6);

            // ── Мова ────────────────────────────────────────────
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button($"> {Loc.Get("ui.lang")}: {Langs[_langIdx]}", _sLang!, GUILayout.Width(220)))
            {
                _langIdx = (_langIdx + 1) % Langs.Length;
                Loc.SetLang(Langs[_langIdx]);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(16);
            DrawHLine(W, pad);
            GUILayout.Space(10);

            // ── Слайдери ─────────────────────────────────────────
            SliderRow(Loc.Get("ui.min_interval"), ref _minInt, 5f, 300f);
            GUILayout.Space(8);
            SliderRow(Loc.Get("ui.max_interval"), ref _maxInt, 10f, 600f);
            GUILayout.Space(2);

            GUILayout.Space(10);
            DrawHLine(W, pad);
            GUILayout.Space(8);

            // ── Евенти: акордеон ────────────────────────────────────
            GUILayout.BeginHorizontal();
            GUILayout.Space(pad);
            GUILayout.Label(Loc.Get("ui.events"), _sLbl!, GUILayout.Width(120));
            GUILayout.Space(pad);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            _evtScroll = GUILayout.BeginScrollView(_evtScroll, GUILayout.Height(260f));

            DrawEventRow(0, Loc.Get("event.mines"), ref _evtMines);
            if (_expandedEvt == 0)
            {
                GUILayout.Space(4);
                IntSliderRow(Loc.Get("ui.mine_count_min"), ref _mineCountMin, 1, 20);
                GUILayout.Space(2);
                IntSliderRow(Loc.Get("ui.mine_count_max"), ref _mineCountMax, 1, 20);
                GUILayout.Space(2);
                SliderRow(Loc.Get("ui.mine_rate_min"), ref _mineRateMin, 0.5f, 10f);
                GUILayout.Space(2);
                SliderRow(Loc.Get("ui.mine_rate_max"), ref _mineRateMax, 0.5f, 10f);
            }

            GUILayout.Space(4);
            DrawEventRow(1, Loc.Get("event.turrets"), ref _evtTurrets);
            if (_expandedEvt == 1)
            {
                GUILayout.Space(4);
                IntSliderRow(Loc.Get("ui.turret_count_min"), ref _turretCountMin, 1, 10);
                GUILayout.Space(2);
                IntSliderRow(Loc.Get("ui.turret_count_max"), ref _turretCountMax, 1, 10);
                GUILayout.Space(2);
                SliderRow(Loc.Get("ui.turret_rate_min"), ref _turretRateMin, 0.5f, 10f);
                GUILayout.Space(2);
                SliderRow(Loc.Get("ui.turret_rate_max"), ref _turretRateMax, 0.5f, 10f);
            }

            GUILayout.Space(4);
            DrawEventRow(2, Loc.Get("event.mob"), ref _evtMob);
            GUILayout.Space(4);
            DrawEventRow(3, Loc.Get("event.teleport_dungeon"), ref _evtTeleportDungeon);
            GUILayout.Space(4);
            DrawEventRow(4, Loc.Get("event.teleport_ship"), ref _evtTeleportShip);
            GUILayout.Space(4);
            DrawEventRow(5, Loc.Get("event.random_sound"), ref _evtRandomSound);
            GUILayout.Space(4);
            DrawEventRow(6, Loc.Get("event.adrenaline"), ref _evtInfiniteStamina);
            if (_expandedEvt == 6)
            {
                GUILayout.Space(4);
                SliderRow(Loc.Get("ui.stamina_duration"), ref _staminaDuration, 5f, 120f);
            }

            GUILayout.Space(4);
            DrawEventRow(7, Loc.Get("event.firefly"), ref _evtFirefly);
            GUILayout.Space(4);
            DrawEventRow(8, Loc.Get("event.player_swap"), ref _evtPlayerSwap);
            GUILayout.Space(4);
            DrawEventRow(9, Loc.Get("event.berserk_turret"), ref _evtBerserkTurret);
            if (_expandedEvt == 9)
            {
                GUILayout.Space(4);
                SliderRow(Loc.Get("ui.berserk_duration"), ref _berserkDuration, 5f, 60f);
            }

            GUILayout.Space(4);
            DrawEventRow(10, Loc.Get("event.football"), ref _evtFootball);
            if (_expandedEvt == 10)
            {
                GUILayout.Space(4);
                SliderRow(Loc.Get("ui.football_duration"), ref _footballDuration, 10f, 120f);
            }

            GUILayout.Space(4);
            DrawEventRow(11, Loc.Get("event.fake_message"), ref _evtFakeMessage);

            GUILayout.Space(4);
            DrawEventRow(12, Loc.Get("event.size_matters"), ref _evtSizeMatters);
            if (_expandedEvt == 12)
            {
                GUILayout.Space(4);
                SliderRow(Loc.Get("ui.size_scale"),         ref _sizeScale,        0.1f, 1.0f);
                GUILayout.Space(2);
                SliderRow(Loc.Get("ui.size_stretch_scale"), ref _sizeStretchScale, 1.0f, 3.0f);
                GUILayout.Space(2);
                SliderRow(Loc.Get("ui.size_duration"),      ref _sizeDuration,     5f,   60f);
            }

            GUILayout.Space(4);
            GUILayout.EndScrollView();

            GUILayout.Space(4);

            // ── Absolute bottom: Save, Cancel, BIOS, Company ────
            float btnY    = H - 36f;
            float biosY   = H - 18f;
            float btnH    = 26f;
            float footH   = 16f;

            GUI.Label(new Rect(pad, biosY, 120f, footH), "BIOS VER 0.9",        _sFooter!);
            GUI.Label(new Rect(W - pad - 175f, biosY, 175f, footH), "COMPANY DIRECTIVE  ✦", _sFooter!);

            float saveW = 140f, cancelW = 90f, gap = 8f;
            float totalBtns = saveW + gap + cancelW;
            float bx = (W - totalBtns) / 2f;
            if (GUI.Button(new Rect(bx,               btnY, saveW,   btnH), Loc.Get("ui.save"),   _sBtnOn!))
                { SaveToConfig(); Hide(); }
            if (GUI.Button(new Rect(bx + saveW + gap, btnY, cancelW, btnH), Loc.Get("ui.cancel"), _sBtnOff!))
                Hide();

            GUI.DragWindow();
        }

        // ── Рядок події (акордеон) ───────────────────────────────
        private void DrawEventRow(int idx, string label, ref bool enabled)
        {
            bool expanded = _expandedEvt == idx;

            // Хедер — тільки розгортання/згортання
            GUILayout.BeginHorizontal();
            GUILayout.Space(40f);
            var headerStyle = expanded ? _sBtnOn! : _sBtnOff!;
            string arrow = expanded ? "▼" : "▶";
            if (GUILayout.Button($"{arrow}  {label}", headerStyle, GUILayout.Width(260)))
                _expandedEvt = expanded ? -1 : idx;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Тогл — окремий рядок всередині розгорнутого блоку
            if (expanded)
            {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(60f);
                GUILayout.Label(enabled ? Loc.Get("ui.enabled") : Loc.Get("ui.disabled"), _sLbl!, GUILayout.Width(80));
                var togStyle = enabled ? _sBtnOn! : _sBtnOff!;
                if (GUILayout.Button(enabled ? "[ON]" : "[OFF]", togStyle, GUILayout.Width(60)))
                    enabled = !enabled;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        // ── Горизонтальна лінія ──────────────────────────────────
        private void DrawHLine(float w, float pad)
        {
            var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none,
                GUILayout.ExpandWidth(true), GUILayout.Height(2f));
            if (Event.current.type == EventType.Repaint)
            {
                var old = GUI.color;
                GUI.color = CMainDim;
                GUI.DrawTexture(new Rect(pad, r.y, w - pad * 2f, 1f), Texture2D.whiteTexture);
                GUI.color = old;
            }
        }

        // ── Слайдер (float) ──────────────────────────────────────
        private void SliderRow(string label, ref float val, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label($"{label}  {val:F1}s", _sLbl!, GUILayout.Width(310f));
            val = GUILayout.HorizontalSlider(val, min, max, _sSliderBg!, _sSliderThumb!);
            GUILayout.Space(20f);
            GUILayout.EndHorizontal();
        }

        // ── Слайдер (int) ────────────────────────────────────────
        private void IntSliderRow(string label, ref int val, int min, int max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label($"{label}  {val}", _sLbl!, GUILayout.Width(310f));
            val = Mathf.RoundToInt(GUILayout.HorizontalSlider(val, min, max, _sSliderBg!, _sSliderThumb!));
            GUILayout.Space(20f);
            GUILayout.EndHorizontal();
        }

        // ── Стилі ────────────────────────────────────────────────
        private void EnsureStyles()
        {
            if (_sWin != null) return;

            _txBg     = Tex(CBg);
            _txMain   = Tex(CMain);
            _txDim    = Tex(CMainDim);
            _txBtnOff = MakeBorder(CMain, CBg, 20, 2);
            _txBtnOn  = Tex(CMain);

            var txBtnOnHov = Tex(new Color(1.00f, 0.45f, 0.05f));

            _sWin = new GUIStyle(GUI.skin.box)
            {
                normal  = { background = _txBg, textColor = CMain },
                padding = new RectOffset(0, 0, 0, 0),
                border  = new RectOffset(1, 1, 1, 1),
            };

            _sTitle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 22,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = CMain },
            };

            _sLang = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 13,
                fontStyle = FontStyle.Bold,
                normal    = { background = _txBtnOff, textColor = CMain },
                hover     = { background = _txBtnOn,  textColor = CActTxt },
                active    = { background = _txBtnOn,  textColor = CActTxt },
                border    = new RectOffset(2, 2, 2, 2),
                padding   = new RectOffset(8, 8, 5, 5),
            };

            _sLbl = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal   = { textColor = CMain },
            };

            // Outline кнопка (невибрана)
            _sBtnOff = new GUIStyle(GUI.skin.button)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal    = { background = _txBtnOff,  textColor = CMain },
                hover     = { background = _txBtnOn,   textColor = CActTxt },
                active    = { background = _txBtnOn,   textColor = CActTxt },
                border    = new RectOffset(2, 2, 2, 2),
                padding   = new RectOffset(6, 6, 4, 4),
            };

            // Filled кнопка (вибрана / save) — як "Join a crew"
            _sBtnOn = new GUIStyle(_sBtnOff)
            {
                normal = { background = _txBtnOn,    textColor = CActTxt },
                hover  = { background = txBtnOnHov,  textColor = CActTxt },
                active = { background = _txBtnOn,    textColor = CActTxt },
            };

            _sFooter = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                normal   = { textColor = CMainDim },
            };

            // Slider track: тонка помаранчева лінія
            _sSliderBg = new GUIStyle(GUI.skin.horizontalSlider)
            {
                normal      = { background = _txMain },
                border      = new RectOffset(0, 0, 0, 0),
                fixedHeight = 3f,
                margin      = new RectOffset(0, 0, 10, 10),
            };

            // Slider thumb: білий круг
            _sSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb)
            {
                normal      = { background = MakeCircle(16, Color.white) },
                fixedWidth  = 16f,
                fixedHeight = 16f,
            };
        }

        private static Texture2D Tex(Color c)
        {
            var t = new Texture2D(1, 1); t.SetPixel(0, 0, c); t.Apply(); return t;
        }

        private static Texture2D MakeBorder(Color border, Color fill, int size = 20, int bw = 2)
        {
            var t = new Texture2D(size, size);
            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                t.SetPixel(x, y, (x < bw || x >= size - bw || y < bw || y >= size - bw) ? border : fill);
            t.Apply(); return t;
        }

        private static Texture2D MakeCircle(int size, Color col)
        {
            var t = new Texture2D(size, size); t.filterMode = FilterMode.Bilinear;
            float r = size / 2f;
            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            { float dx = x - r + .5f, dy = y - r + .5f; t.SetPixel(x, y, dx*dx+dy*dy <= r*r ? col : Color.clear); }
            t.Apply(); return t;
        }

        // ── Config ───────────────────────────────────────────────
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
}
