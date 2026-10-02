using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUIHPBar
{
    /// <summary>テクスチャを使わず、単色の矩形と文字だけでUIを組むためのヘルパー。</summary>
    internal static class UiFactory
    {
        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>左下基準で位置とサイズを指定する。</summary>
        public static void Place(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        /// <summary>スプライト無しの Image = 単色の矩形。</summary>
        public static Image Box(string name, Transform parent, Color color)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>
        /// template(バニラのテキスト)のフォントとマテリアルをそのまま使う。
        /// 自前で縁取りを付けると小さい文字が潰れるため、バニラと同じ描画設定に揃える。
        /// </summary>
        public static TMP_Text Text(string name, Transform parent, TMP_Text template, float size, Color color)
        {
            var rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (template != null && template.font != null)
            {
                t.font = template.font;
                t.fontSharedMaterial = template.fontSharedMaterial;
            }

            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.overflowMode = TextOverflowModes.Overflow;
            t.raycastTarget = false;
            return t;
        }

        /// <summary>
        /// バニラのフィールドを名前で取得する。ゲーム更新で名前が変わっても
        /// ビルドや起動が壊れないよう、見つからなければ null を返して警告だけ出す。
        /// </summary>
        public static Transform FindField(object owner, string fieldName)
        {
            var field = AccessTools.Field(owner.GetType(), fieldName);
            if (field == null)
            {
                Plugin.Log.LogWarning($"{owner.GetType().Name}.{fieldName} が見つかりません(ゲーム更新で変わった可能性)");
                return null;
            }

            var value = field.GetValue(owner);
            if (value is GameObject go) return go ? go.transform : null;
            if (value is Component c) return c ? c.transform : null;
            return null;
        }
    }
}
