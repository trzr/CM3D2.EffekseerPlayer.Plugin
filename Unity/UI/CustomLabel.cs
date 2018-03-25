using UnityEngine;

namespace EffekseerPlayerPlugin.Unity.UI {
    /// <summary>
    /// ラベル コントロール
    /// </summary>
    public class CustomLabel : GUIControl {
        #region Methods
        public CustomLabel(GUIControl parent) : base(parent) {
            // disable once DoNotCallOverridableMethodsInConstructor
            backgroundColor = Color.clear;
        }

        public CustomLabel(GUIControl parent, string label) : this(parent) {
            text = label;
        }

        public override void Awake() {
            if (LabelStyle != null) return;

            LabelStyle = new GUIStyle("label") {
                alignment = TextAnchor.MiddleLeft,
                normal = {textColor = TextColor},
            };
            if (fontSize > 0) {
                LabelStyle.fontSize = fontSize;
            }
        }

        public override void OnGUI() {
            GUI.Label(Rect, Text, LabelStyle);
        }

        public void OnGUI(ref Rect rect1) {
            GUI.Label(rect1, Text, LabelStyle);
        }

        #endregion

        public override int FontSize {
            set {
                fontSize = value;
                if (LabelStyle != null) LabelStyle.fontSize = fontSize;
            }
        }
        public GUIStyle LabelStyle { set; get; }
    }
}