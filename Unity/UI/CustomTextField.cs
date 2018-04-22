using System;
using UnityEngine;

namespace EffekseerPlayer.Unity.UI {
    /// <summary>
    /// テキストフィールド コントロール.
    /// hasErrorフラグをtrueにすると背景色をerrorColorに設定する.
    /// </summary>
    public class CustomTextField : GUIControl {
        #region Methods
        public CustomTextField(GUIControl parent) : this(parent, string.Empty) {}

        public CustomTextField(GUIControl parent, string text1) : base(parent) {
            text = text1;
        }

        public override void Awake() {
            if (TextFStyle != null) return;

            TextFStyle = new GUIStyle("textField") {
                alignment = TextAnchor.MiddleLeft,
                normal = {textColor = TextColor},
            };
            if (fontSize > 0) {
                TextFStyle.fontSize = fontSize;
            }
        }

        protected override void DrawGUI() {
            if (hasError) _colorStore.SetColor(ref errorColor, ref errorColor);
            try {
                Text = GUI.TextField(Rect, Text, TextFStyle);
            } finally {
                _colorStore.Restore();
            }
        }
        #endregion

        #region Properties
        public override int FontSize {
            set {
                fontSize = value;
                if (TextFStyle != null) TextFStyle.fontSize = fontSize;
            }
        }
        // テキストフィールドスタイル
        public GUIStyle TextFStyle { get; set; }
        public override string Text {
            get { return text; }
            set {
                if (text == value) return;
                text = value;
                if (Enabled) ValueChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        public bool hasError;
        public Color errorColor = UIParamSet.ErrorColor;
        private readonly GUIColorStore _colorStore = new GUIColorStore();

        #region Events
        public event EventHandler ValueChanged = delegate { };
        #endregion
    }
}
