using EffekseerPlayer.Unity.Data;
using UnityEngine;

namespace EffekseerPlayer.Unity.UI {
    /// <inheritdoc />
    /// <summary>
    /// 数値型テキストフィールド コントロール.
    /// 指定された範囲を超えた場合に色を変える動作を行う.
    /// 変更時のイベントハンドラはEditTextValueのハンドラを利用する.
    /// </summary>
    public class CustomNumTextField : GUIControl {
        /// <inheritdoc />
        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="value1">初期値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="dec">小数点桁数</param>
        public CustomNumTextField(GUIControl parent, float value1, float min, float max, int dec) 
            : this (parent, new EditTextValue(string.Empty, value1, dec, min, max)) { }

        /// <inheritdoc />
        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="value1">初期値</param>
        /// <param name="range">値範囲</param>
        public CustomNumTextField(GUIControl parent, float value1, EditRange range)
            : this(parent, new EditTextValue(string.Empty, value1, range)) { }

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="value1">編集値</param>
        public CustomNumTextField(GUIControl parent, EditTextValue value1) : base(parent) {
            Value = value1;
        }

        #region Methods
        public override void Awake() {
            if (TextFStyle != null) return;

            // スタイル
            TextFStyle = new GUIStyle("textField") {
                alignment = TextAnchor.MiddleCenter,
                normal = {textColor = TextColor}
            };

            if (fontSize > 0) {
                TextFStyle.fontSize = fontSize;
            }
        }

        protected override void DrawGUI() {

            if (!Value.isSynched) _txtColorSetter.SetColor(TextFStyle, ref errorColor);
            try {
                Value.Text = GUI.TextField(Rect, Value.Text, TextFStyle);
            } finally {
                _txtColorSetter.Restore();
            }
        }
        #endregion

        #region Fields
        public override int FontSize {
            set {
                fontSize = value;
                if (TextFStyle != null) TextFStyle.fontSize = fontSize;
            }
        }
        public Color errorColor = UIParamSet.ErrorColor;
        private readonly GUITextColorStore _txtColorSetter = new GUITextColorStore();
        public GUIStyle TextFStyle { get; set; }
        public EditTextValue Value { get; private set;  }
        public override string Text {
            get { return Value.Text;  }
            set { Value.Text = value; }
        }
        #endregion

    }
}