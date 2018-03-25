using System;
using UnityEngine;

namespace EffekseerPlayerPlugin.Unity.UI {
    /// <summary>
    /// カスタムボタンUIコントロール
    /// </summary>
    public class CustomButton : GUIControl {
        #region Methods
        /// <summary>デフォルトコンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="content">コンテンツ</param>
        public CustomButton(GUIControl parent, GUIContent content=null) : base (parent) {
            // disable once DoNotCallOverridableMethodsInConstructor
            textColor = Color.white;
            Content = content;
        }

        /// <summary>デフォルトコンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="label">ボタンラベル</param>
        public CustomButton(GUIControl parent, string label) : this(parent, new GUIContent(label)) { }

        public override void Awake() {
            if (ButtonStyle == null) {
                ButtonStyle = new GUIStyle("button") {
                    alignment = TextAnchor.MiddleCenter
                };
                if (FontSize > 0) {
                    ButtonStyle.fontSize = fontSize;
                }
            }

            if (Content == null) {
                Content = new GUIContent(Text);
            }
        }

        protected override void DrawGUI() {
            if (GUI.Button(Rect, Content, ButtonStyle)) {
                Click(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Properties
        public GUIStyle ButtonStyle   { get; set; }
        public GUIContent Content     { get; set; }
        public override int FontSize {
            set {
                fontSize = value;
                if (ButtonStyle != null) ButtonStyle.fontSize = fontSize;
            }
        }
        #endregion

        #region Events
        public event EventHandler Click = delegate { };
        #endregion
    }
}
