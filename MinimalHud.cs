using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUIHPBar
{
    /// <summary>
    /// HP・スタミナ・エイトル・食べ物を、背景パネル無しの細いバーとアイコンだけで描くHUD。
    /// Player から値を読むだけで、ゲーム側の状態は変更しない。
    /// </summary>
    internal sealed class MinimalHud : MonoBehaviour
    {
        // バニラ側で隠す対象(Hud のフィールド名)
        private static readonly string[] VanillaRoots = { "m_healthPanel", "m_staminaBar2Root", "m_eitrBarRoot" };

        // レイアウトの寸法は 4 / 8 / 16 / 32 を基準にする(4 の倍数)
        private const float Gap = 8f;
        private const float BarGap = 8f;
        private const float FoodSize = 32f;
        private const float FoodTimerHeight = 4f;
        private const float FoodTimerGap = 4f;
        private const float LabelFontSize = 12f;
        private const int FoodSlots = 3;

        private static bool _rebuildRequested;

        private TMP_Text _textTemplate;
        private RectTransform _gpRoot;
        private Vector2 _gpOriginalPos;
        private bool _gpMoved;
        private float _foodTopY;
        private int _frame;
        private readonly List<CanvasGroup> _vanillaGroups = new List<CanvasGroup>();

        private RectTransform _content;
        private RectTransform _left;   // 画面左下: 食べ物
        private RectTransform _center; // 画面下中央: HP・スタミナ・エイトル
        private Bar _health, _stamina, _eitr;
        private RectTransform _foodRow;
        private readonly FoodSlot[] _foods = new FoodSlot[FoodSlots];
        private bool _eitrVisible;

        private int _lastScreenHeight;
        private float _lastCanvasScale;

        public static void Attach(Hud hud)
        {
            var parent = UiFactory.FindField(hud, "m_rootObject") ?? hud.transform;
            var rt = UiFactory.Rect("SimpleUIHPBar_Hud", parent);
            rt.gameObject.AddComponent<MinimalHud>().Init(hud);
        }

        public static void RequestRebuild() => _rebuildRequested = true;

        private void Init(Hud hud)
        {
            _textTemplate = FindTextTemplate(hud);
            _gpRoot = UiFactory.FindField(hud, "m_gpRoot") as RectTransform;
            if (_gpRoot != null) _gpOriginalPos = _gpRoot.anchoredPosition;

            foreach (var name in VanillaRoots)
            {
                var t = UiFactory.FindField(hud, name);
                if (t == null) continue;
                var group = t.GetComponent<CanvasGroup>();
                if (group == null) group = t.gameObject.AddComponent<CanvasGroup>();
                _vanillaGroups.Add(group);
            }

            Build();
        }

        private void Build()
        {
            if (_content != null) Destroy(_content.gameObject);

            // ルートは画面全体に広げ、左下と下中央の2グループをそれぞれの基準点で拡大縮小する
            var root = (RectTransform)transform;
            UiFactory.Stretch(root);
            root.localScale = Vector3.one;

            _content = UiFactory.Rect("Content", root);
            UiFactory.Stretch(_content);

            _left = UiFactory.Rect("Left", _content);
            UiFactory.Place(_left, ModConfig.HudPosX.Value, ModConfig.HudPosY.Value, 0f, 0f);

            float width = ModConfig.BarWidth.Value;
            float height = ModConfig.BarHeight.Value;
            _center = UiFactory.Rect("Center", _content);
            _center.anchorMin = _center.anchorMax = new Vector2(0.5f, 0f);
            _center.pivot = new Vector2(0.5f, 0f);
            _center.anchoredPosition = new Vector2(ModConfig.BarsOffsetX.Value, ModConfig.BarsPosY.Value);
            _center.sizeDelta = new Vector2(width, 0f);

            _eitr = new Bar("Eitr", _center, width, ModConfig.EitrHeight.Value, ModConfig.EitrColor.Value, _textTemplate);
            _stamina = new Bar("Stamina", _center, width, height, ModConfig.StaminaColor.Value, _textTemplate);
            _health = new Bar("Health", _center, width, height, ModConfig.HealthColor.Value, _textTemplate);

            _foodRow = UiFactory.Rect("Food", _left);
            for (int i = 0; i < FoodSlots; i++)
            {
                _foods[i] = new FoodSlot(_foodRow, i * (FoodSize + Gap));
            }

            _eitrVisible = false;
            Layout();

            _lastScreenHeight = 0; // 次の Update でスケールを再計算させる
        }

        /// <summary>下中央は下から エイトル(任意) → スタミナ → HP の順に積む。食べ物は左下。</summary>
        private void Layout()
        {
            float barHeight = ModConfig.BarHeight.Value;
            float y = 0f;
            _eitr.Root.gameObject.SetActive(_eitrVisible);
            if (_eitrVisible)
            {
                _eitr.Root.anchoredPosition = new Vector2(0f, y);
                y += ModConfig.EitrHeight.Value + BarGap;
            }

            _stamina.Root.anchoredPosition = new Vector2(0f, y);
            y += barHeight + BarGap;
            _health.Root.anchoredPosition = new Vector2(0f, y);

            float foodHeight = FoodSize + FoodTimerHeight + FoodTimerGap;
            UiFactory.Place(_foodRow, 0f, 0f, FoodSlots * (FoodSize + Gap), foodHeight);
            _foodTopY = foodHeight;
            _frame = 0; // 守護者の力の位置を次のフレームで合わせ直す
        }

        private void Update()
        {
            if (_rebuildRequested)
            {
                _rebuildRequested = false;
                Build();
            }

            UpdateScale();

            var player = Player.m_localPlayer;
            bool show = ModConfig.HudEnabled.Value && player != null;
            if (_content.gameObject.activeSelf != show) _content.gameObject.SetActive(show);
            if (!show) return;

            float dt = Time.deltaTime;
            _health.Set(player.GetHealth(), player.GetMaxHealth(), dt);
            _stamina.Set(player.GetStamina(), player.GetMaxStamina(), dt);

            float maxEitr = player.GetMaxEitr();
            bool eitr = maxEitr > 0.5f;
            if (eitr != _eitrVisible)
            {
                _eitrVisible = eitr;
                Layout();
            }

            if (eitr) _eitr.Set(player.GetEitr(), maxEitr, dt);

            var foods = player.GetFoods();
            for (int i = 0; i < FoodSlots; i++)
            {
                _foods[i].Set(i < foods.Count ? foods[i] : null);
            }
        }

        /// <summary>
        /// バニラのアニメーターが表示状態を書き戻しても負けないよう、
        /// アニメーション更新後の LateUpdate で毎フレーム透明度を指定する。
        /// SetActive は使わない(バニラ側の表示ロジックと競合させないため)。
        /// </summary>
        private void LateUpdate()
        {
            bool hide = ModConfig.HudEnabled.Value && ModConfig.HideVanillaHud.Value;
            float alpha = hide ? 0f : 1f;
            for (int i = 0; i < _vanillaGroups.Count; i++)
            {
                var g = _vanillaGroups[i];
                if (g == null) continue;
                if (!hide && g.alpha > 0f) continue; // 表示中はバニラの制御に任せる
                g.alpha = alpha;
            }

            PositionGuardianPower();
        }

        /// <summary>数値用のフォントを持つバニラのテキストを探す。見つからなければ最初のテキストを使う。</summary>
        private static TMP_Text FindTextTemplate(Hud hud)
        {
            var texts = hud.GetComponentsInChildren<TMP_Text>(true);
            string wanted = ModConfig.FontName.Value;
            if (!string.IsNullOrEmpty(wanted))
            {
                foreach (var t in texts)
                {
                    if (t.font != null && t.font.name.IndexOf(wanted, System.StringComparison.OrdinalIgnoreCase) >= 0) return t;
                }

                Plugin.Log.LogWarning($"フォント '{wanted}' を使うテキストがHUDに見つかりません。既定のフォントを使います");
            }

            return texts.Length > 0 ? texts[0] : null;
        }

        /// <summary>
        /// 守護者の力(Moder など)の表示を食べ物の行のすぐ上、左端揃えに移動する。
        /// 名前・アイコン・状態テキストを含む全体の左下を基準に合わせる。
        /// </summary>
        private void PositionGuardianPower()
        {
            if (_gpRoot == null) return;

            bool move = ModConfig.HudEnabled.Value && ModConfig.MoveGuardianPower.Value;
            if (!move)
            {
                if (_gpMoved)
                {
                    _gpRoot.anchoredPosition = _gpOriginalPos;
                    _gpMoved = false;
                }

                return;
            }

            // 子要素の走査が要るので毎フレームは行わない
            if (_frame++ % 20 != 0) return;
            if (!_content.gameObject.activeSelf || !_gpRoot.gameObject.activeInHierarchy) return;

            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(_gpRoot);
            Vector3 current = _gpRoot.TransformPoint(new Vector3(bounds.min.x, bounds.min.y, 0f));
            Vector3 target = _left.TransformPoint(new Vector3(
                ModConfig.GuardianOffsetX.Value,
                _foodTopY + Gap + ModConfig.GuardianOffsetY.Value,
                0f));
            Vector3 delta = target - current;
            delta.z = 0f;
            if (delta.sqrMagnitude < 0.25f) return;

            _gpRoot.position += delta;
            _gpMoved = true;
        }

        private void UpdateScale()
        {
            var canvas = GetComponentInParent<Canvas>();
            float canvasScale = canvas != null ? canvas.rootCanvas.scaleFactor : 1f;
            if (canvasScale <= 0f) canvasScale = 1f;
            if (Screen.height == _lastScreenHeight && Mathf.Approximately(canvasScale, _lastCanvasScale)) return;

            _lastScreenHeight = Screen.height;
            _lastCanvasScale = canvasScale;

            float scale = ModConfig.HudScale.Value;
            if (ModConfig.AutoScale.Value)
            {
                // キャンバス側の拡大率を打ち消し、画面の高さに対して常に同じ比率になるようにする
                scale *= (Screen.height / 1080f) / canvasScale;
            }

            _left.localScale = Vector3.one * scale;
            _center.localScale = Vector3.one * scale;
        }

        /// <summary>単色バー。減少時は薄い残像が少し遅れて追従する。</summary>
        private sealed class Bar
        {
            public readonly RectTransform Root;
            private readonly RectTransform _fill;
            private readonly RectTransform _trail;
            private readonly TMP_Text _label;
            private float _trailValue = 1f;
            private int _shownCurrent = -1, _shownMax = -1;

            public Bar(string name, Transform parent, float width, float height, Color color, TMP_Text template)
            {
                var track = UiFactory.Box(name, parent, ModConfig.TrackColor.Value);
                Root = track.rectTransform;
                UiFactory.Place(Root, 0f, 0f, width, height);

                var trailColor = color;
                trailColor.a *= 0.35f;
                _trail = UiFactory.Box("Trail", Root, trailColor).rectTransform;
                _fill = UiFactory.Box("Fill", Root, color).rectTransform;
                UiFactory.Stretch(_trail);
                UiFactory.Stretch(_fill);

                if (ModConfig.ShowNumbers.Value)
                {
                    _label = UiFactory.Text("Label", Root, template, LabelFontSize, ModConfig.TextColor.Value);
                    var rt = _label.rectTransform;
                    rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
                    rt.pivot = new Vector2(0f, 0.5f);
                    rt.anchoredPosition = new Vector2(8f, 0f);
                    rt.sizeDelta = new Vector2(200f, 16f);
                }
            }

            public void Set(float current, float max, float dt)
            {
                float f = max > 0f ? Mathf.Clamp01(current / max) : 0f;
                _fill.anchorMax = new Vector2(f, 1f);

                _trailValue = f >= _trailValue ? f : Mathf.MoveTowards(_trailValue, f, dt * 0.6f);
                _trail.anchorMax = new Vector2(_trailValue, 1f);

                if (_label == null) return;
                int c = Mathf.CeilToInt(current), m = Mathf.CeilToInt(max);
                if (c == _shownCurrent && m == _shownMax) return; // 値が変わった時だけ文字列を作る
                _shownCurrent = c;
                _shownMax = m;
                _label.text = c + " / " + m;
            }
        }

        /// <summary>食べ物1枠。アイコンと、その下の残り時間バーだけ。</summary>
        private sealed class FoodSlot
        {
            private readonly Image _empty;
            private readonly Image _icon;
            private readonly GameObject _timer;
            private readonly Image _timerFill;

            public FoodSlot(Transform parent, float x)
            {
                float iconY = FoodTimerHeight + FoodTimerGap;
                var root = UiFactory.Rect("Slot", parent);
                UiFactory.Place(root, x, 0f, FoodSize, FoodSize + iconY);

                var track = UiFactory.Box("Timer", root, ModConfig.TrackColor.Value);
                UiFactory.Place(track.rectTransform, 0f, 0f, FoodSize, FoodTimerHeight);
                _timer = track.gameObject;
                _timerFill = UiFactory.Box("Fill", track.transform, ModConfig.FoodColor.Value);
                UiFactory.Stretch(_timerFill.rectTransform);

                var emptyColor = ModConfig.TrackColor.Value;
                emptyColor.a *= 0.5f;
                _empty = UiFactory.Box("Empty", root, emptyColor);
                UiFactory.Place(_empty.rectTransform, 0f, iconY, FoodSize, FoodSize);

                _icon = UiFactory.Box("Icon", root, Color.white);
                UiFactory.Place(_icon.rectTransform, 0f, iconY, FoodSize, FoodSize);
                _icon.preserveAspect = true;
            }

            public void Set(Player.Food food)
            {
                bool has = food != null && food.m_item != null;
                _icon.enabled = has;
                _empty.enabled = !has;
                if (_timer.activeSelf != has) _timer.SetActive(has);
                if (!has) return;

                var sprite = food.m_item.GetIcon();
                if (_icon.sprite != sprite) _icon.sprite = sprite;

                float burn = food.m_item.m_shared.m_foodBurnTime;
                float f = burn > 0f ? Mathf.Clamp01(food.m_time / burn) : 0f;
                _timerFill.rectTransform.anchorMax = new Vector2(f, 1f);

                // 残り半分を切る(=食べ直せる)とバーを黄色にする
                _timerFill.color = f < 0.5f ? ModConfig.FoodLowColor.Value : ModConfig.FoodColor.Value;
            }
        }
    }
}
