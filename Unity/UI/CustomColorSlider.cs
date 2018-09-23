using System.Linq;
using EffekseerPlayer.Unity.Data;
using UnityEngine;

namespace EffekseerPlayer.Unity.UI {
    /// <summary>
    /// カラー専用スライダー.
    /// カラーピッカーも内包される.
    ///
    /// expandによりheightが変化する.
    /// 設定パラメータ/Properties
    ///  subLabelIndent
    ///  RowHeight
    ///
    /// rectについては、
    ///  x, y, width
    /// を外部から設定される想定
    /// </summary>
    public class CustomColorSlider : CustomColorPicker {
        public CustomColorSlider(GUIControl parent, EditColorTex editColorTex, ColorPresetManager presetMgr = null)
            : base(parent, editColorTex, presetMgr) { }

        #region Methods
        public override void Awake() {
            base.Awake();

            if (SubLabelStyle == null) {
                SubLabelStyle = new GUIStyle("label");
                if (fontSize > 0) {
                    SubLabelStyle.fontSize = fontSize;
                }
            }
            if (!warnColor.HasValue) {
                warnColor = Color.red;
            }
        }

        /// <summary>
        /// x, y, widthは外から設定されることを想定
        /// heightは状況に応じて自動設定
        /// 
        /// </summary>
        /// <param name="uiParams">UIパラメータ</param>
        protected override void Layout(UIParamSet uiParams) {
            base.Layout(uiParams);
            var margin1 = uiParams.margin;

            var xPos = Left + subLabelIndent;
            var subLabelWidth = SubLabelStyle.CalcSize(new GUIContent("R")).x;
            subLabelRect.Set(xPos, 0, subLabelWidth, RowHeight);
            var tfWidth = TextFStyle.CalcSize(new GUIContent("0.8888")).x; // 下3桁で十分
            textRect.x = subLabelRect.xMax + uiParams.margin;
            textRect.width = tfWidth;
            textRect.height = RowHeight;

            var prevWidth = 0f;
            var nextWidth = 0f;
            if (prevListeners != null) {
                prevWidth += prevListeners.Sum(prev => prev.width + margin);
            }
            if (nextListeners != null) {
                nextWidth += nextListeners.Sum(next => next.width + margin);
            }
            sliderRect.x = textRect.xMax + margin1 + prevWidth;
            sliderRect.width  = xMax - textRect.xMax - margin1 * 2 - prevWidth - nextWidth;
            sliderRect.height = RowHeight - margin1 * 2;

            subPresetRect.height = RowHeight; // ほぼ描画時に設定
        }

        protected override float GetMapMargin() {
            return EditVal.Size * RowHeight + uiParamSet.margin;
        }

        protected override void DrawGUI() {
            if (!initialized) return;
            if (GUI.Button(colorIconRect, EditVal.ColorTex, ColorIconStyle)) Expand = !expand;
            if (Text != null) {
                if (GUI.Button(labelRect, Text, LabelStyle)) Expand = !expand;
            }

            DrawColorCode();
            DrawPresetButtons();
            if (!expand) return;

            var yPos = DrawSliders();

            var offset = circleIcon.width * 0.5f;
            DrawMapTex(ref mapTexRect, offset);
            DrawLightTex(ref lightTexRect, offset);
            if (presetMgr != null) {
                var xPos = lightTexRect.xMax + uiParamSet.margin;
                yPos += uiParamSet.margin;
                DrawPreset(xPos, yPos);
            }
        }

        protected virtual float DrawSliders() {
            var yPos = labelRect.yMax + margin;
            var margin2 = margin * 2;
            for (var i=0; i<EditVal.Size; i++) {
                subLabelRect.y  = yPos;
                textRect.y      = yPos;
                subPresetRect.y = yPos;
                sliderRect.y    = yPos + margin2;

                GUI.Label(subLabelRect, EditTextColor.LABELS[i], SubLabelStyle);
                // テキストフィールド
                if (!EditVal.Synched[i] && warnColor.HasValue) txtColorStore.SetColor(TextFStyle, warnColor.Value);
                try {
                    var editText = GUI.TextField(textRect, EditVal.Texts[i], TextFStyle);
                    EditVal.SetText(i, editText, true);
                    
                } finally {
                    txtColorStore.Restore();
                }
                if (prevListeners != null) {
                    var xPos = textRect.xMax;
                    foreach (var listener in prevListeners) {
                        subPresetRect.x = xPos + margin;
                        subPresetRect.width = listener.width;
                        if (GUI.Button(subPresetRect, listener.label, ButtonStyle)) {
                            listener.action(i, EditVal);
                        }
                        xPos += listener.width + margin;
                    }
                }
                if (nextListeners != null) {
                    var xPos = sliderRect.xMax;
                    foreach (var listener in nextListeners) {
                        subPresetRect.x = xPos + margin;
                        subPresetRect.width = listener.width;
                        xPos += (listener.width + margin);
                        if (GUI.Button(subPresetRect, listener.label, ButtonStyle)) {
                            listener.action(i, EditVal);
                        }
                    }
                }
                // スライダー
                colorStore.SetColor(ref textColor, ref backgroundColor);
                try {
                    var range = EditVal.GetRange(i);
                    var editVal = GUI.HorizontalSlider(sliderRect, EditVal[i], range.SoftMin, range.SoftMax);
                    EditVal.SetColorValue(i, editVal, true);

//                    var val = EditVal[i];
//                    var editVal = GUI.HorizontalSlider(sliderRect, val, range.SoftMin, range.SoftMax);
//                    if (Mathf.Abs(editVal - val) < EditTextColor.EPSILON) {
//                        EditVal.SetColorValue(i, editVal, true);
//                    }

                } finally {
                    colorStore.Restore();
                }
                yPos += rowHeight;
            }

            return yPos;
        }

        protected override void UpdateHeight() {
            var height = rowHeight;
            if (expand) height += uiParamSet.margin + rowHeight*EditVal.Size + texHeight;

            Log.Debug("height updated: ", Height, " -> ", height, ", expand:", expand);
            Height = height;
        }

        #endregion

        #region Fields
        public float subLabelIndent = 10;
        public Rect subLabelRect;
        public Rect subPresetRect;
        public Rect textRect;
        public Rect sliderRect;

        public SubPresetListener<int, EditColorTex>[] prevListeners;
        public SubPresetListener<int, EditColorTex>[] nextListeners;

        internal readonly GUIColorStore colorStore = new GUIColorStore();
        internal readonly GUITextColorStore txtColorStore = new GUITextColorStore();
        internal Color? warnColor;

        #endregion

        #region Properties
        public override int FontSize {
            set {
                base.FontSize = value;
                if (SubLabelStyle != null) SubLabelStyle.fontSize = fontSize;
            }
        }

        public GUIStyle SubLabelStyle { get; set; }

        #endregion
    }
}
