using System.Collections.Generic;
using System.Linq;
using EffekseerPlayer.Unity.Data;
using UnityEngine;

namespace EffekseerPlayer.Unity.UI {
    /// <summary>
    /// 連動するテキストフィールドとスライダー（水平）を並べたコントロールの複数セット.
    /// </summary>
    public class CustomTextSliders : BaseTextSlider<EditTextValues> {
        #region Methods
        /// <inheritdoc />
        /// <summary>コンストラクタ.</summary>
        /// <param name="parent">親要素</param>
        protected CustomTextSliders(GUIControl parent) : base(parent) { }

        /// <inheritdoc />
        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="names">名前一覧</param>
        /// <param name="values">初期値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="dec">小数点桁数</param>
        public CustomTextSliders(GUIControl parent, IList<string> names, IList<float> values, float min, float max, int dec) : this(parent) {
            var range = new EditRange(dec, min, max);
            Value = new EditTextValues(names, values, range);
        }

        /// <inheritdoc />
        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="names">名前一覧</param>
        /// <param name="values">初期値</param>
        /// <param name="range">値の範囲</param>
        public CustomTextSliders(GUIControl parent, IList<string> names, IList<float> values, EditRange range) : this(parent) {
            Value = new EditTextValues(names, values, range);
        }
        /// <inheritdoc />
        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="names">名前一覧</param>
        /// <param name="values">初期値</param>
        /// <param name="ranges">値の範囲</param>
        public CustomTextSliders(GUIControl parent, IList<string> names, IList<float> values, IList<EditRange> ranges) : this(parent) {
            Value = new EditTextValues(names, values, ranges);
        }

        /// <inheritdoc />
        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="vals">編集値オブジェクト</param>
        public CustomTextSliders(GUIControl parent, EditTextValues vals) : this(parent) {
            Value = vals;
        }

        public override void Awake() {
            base.Awake();

            if (SubLabelStyle != null) return;

            SubLabelStyle = new GUIStyle("label");
            if (fontSize > 0) {
                SubLabelStyle.fontSize = fontSize;
            }
        }

        public override void OnGUI() {
            if (Text != null) GUI.Label(labelRect, Text, LabelStyle);
            if (listeners != null) {
                var xPos = xMax;
                for (var i=listeners.Length-1; i>=0; i--) {
                    var listener = listeners[i];
                    presetRect.x = xPos - listener.width - margin;
                    presetRect.width = listener.width;
                    xPos -= (listener.width + margin);
                    if (GUI.Button(presetRect, listener.label, ButtonStyle)) {
                        listener.action(Value);
                    }
                }
            }

            var yPos = labelRect.yMax + margin;
            var margin2 = margin * 2;
            for (var i=0; i<Value.Size; i++) {
                var editVal = Value[i];
                subLabelRect.y  = yPos;
                textRect.y      = yPos;
                subPresetRect.y = yPos;
                sliderRect.y    = yPos + margin2;

                GUI.Label(subLabelRect, editVal.name, SubLabelStyle);
                // テキストフィールド
                if (!editVal.isSynched && warnColor.HasValue) txtColorStore.SetColor(TextFStyle, warnColor.Value);
                try {
                    editVal.Text = GUI.TextField(textRect, editVal.Text, TextFStyle);
                } finally {
                    txtColorStore.Restore();
                }
                if (prevListeners != null) {
                    var xPos = textRect.xMax;
                    foreach (var listener in prevListeners) {
                        subPresetRect.x = xPos + margin;
                        subPresetRect.width = listener.width;
                        if (GUI.Button(subPresetRect, listener.label, ButtonStyle)) {
                            listener.action(editVal);
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
                            listener.action(editVal);
                        }
                    }
                }
                // スライダー
                colorStore.SetColor(ref textColor, ref backgroundColor);
                try {
                    editVal.Value = GUI.HorizontalSlider(sliderRect, editVal.Value, editVal.SoftMin, editVal.SoftMax);
                } finally {
                    colorStore.Restore();
                }
                yPos += TextHeight;
            }
        }

        protected override void Layout(UIParamSet uiParams) {

            // 各コントロールの位置調整
            if (Text != null) {
                labelRect.Set(Left, Top, Text.Length * fontSize, TextHeight);
//                Log.Debug("MultiSlider.label:(w,h)=(", labelRect.width, ",", labelRect.height, ")");

                presetRect.y = labelRect.y;
                presetRect.height = TextHeight;
            }

            // indent: 外から指定
            subLabelRect.x = Left + indent;
            subLabelRect.width  = uiParamSet.FixPx(subLabelWidth);
            subLabelRect.height = TextHeight;

            textRect.x = subLabelRect.xMax + margin;
            //textRect.y = Top; // 自動整列
            // width,height: 外から指定
            var prevWidth = 0f;
            var nextWidth = 0f;
            if (prevListeners != null) {
                prevWidth += prevListeners.Sum(prev => prev.width + margin);
            }
            if (nextListeners != null) {
                nextWidth += nextListeners.Sum(next => next.width + margin);
            }
            subPresetRect.height = TextHeight;

            //sliderRect.y = Top + Margin * 2;
            sliderRect.x = textRect.xMax + margin + prevWidth;
            sliderRect.width  = Width - textRect.width - subLabelRect.width - indent - margin * 2 - prevWidth - nextWidth;
            sliderRect.height = TextHeight - margin * 2;
        }
        #endregion

        #region Fields
        public float subLabelWidth;
        public Rect subLabelRect;
        public Rect subPresetRect;
        public PresetListener<EditTextValue>[] prevListeners;
        public PresetListener<EditTextValue>[] nextListeners;

        #endregion

        #region Properties
        public override float Height {
            get { return (Value.Size + 1) * TextHeight; }
            set { rect.height = value; }
        }
        public override float yMax {
            get { return rect.y + Height; }
        }

        public GUIStyle SubLabelStyle { get; set; }

        public EditTextValues Value { get; private set; }
        #endregion

        #region Events
        #endregion
    }
}
