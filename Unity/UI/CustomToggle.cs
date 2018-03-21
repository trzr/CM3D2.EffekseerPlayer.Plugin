using System;
using UnityEngine;

namespace EffekseerPlayerPlugin.Unity.UI
{
    /// <inheritdoc />
    /// <summary>
    /// トグルUIコントロール.
    /// </summary>
    public class CustomToggle : GUIControl
    {
        #region Methods
        /// <summary>コンストラクタ</summary>
        /// <param name="uiParams">UIパラメータ</param>
        /// <param name="value">トグル初期状態</param>
        public CustomToggle(UIParamSet uiParams, bool value=false) : base(uiParams) {
            _value = value;
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="value">トグル初期状態</param>
        public CustomToggle(GUIControl parent, bool value = false) : base(parent) {
            _value = value;
        }

        public override void Awake() {
            if (CheckStyle != null) return;
            CheckStyle = new GUIStyle("button") {
                alignment = TextAnchor.MiddleCenter,
            };
            if (fontSize > 0) {
                CheckStyle.fontSize = fontSize;
            }
        }

        public override void OnGUI() {
            OnGUI(ref rect, Enabled);
        }

        public void OnGUI(ref Rect viewRect, bool enabled=true) {
            // トグル表示
            bool retVal;
            if (!enabled) enabledStore.SetEnabled(false);
            _colorStore.SetColor(CurrentTextColor, CurrentBackgroundColor);
            try {
                retVal = GUI.Toggle(viewRect, _value, CurrentContent, CheckStyle);

            } finally {
                _colorStore.Restore();
                enabledStore.Restore();
            }

            if (retVal == _value) return;
            if (_pair != null) {
                if (retVal == _pair.Value) {
                    // TODO ペアの場合は片方の変更通知のみにする
                    _pair.Value = !retVal;
                }
            }
            Value = retVal;
        }

        public override void Update() {
//            if (ValFunc != null) _value = ValFunc();
        }

        /// <summary>通知無しで値を設定する</summary>
        /// <param name="value">値</param>
        /// <param name="notify">通知指定</param>
        internal void Set(bool value, bool notify = false) {
            if (_value == value) return;

            _value = value;
            if (notify && Enabled) CheckChanged(this, EventArgs.Empty);
        }

        /// <summary>ボタンペアリング</summary>
        public static void SetPairButton(CustomToggle button1, CustomToggle button2) {
            button1._pair = button2;
            button2._pair = button1;
        }

        #endregion

        #region Properties
        /// <summary>現在の状態</summary>
        private bool _value;
        public bool Value {
            get { return _value; }
            set {
                Set(value, true);
            }
        }

        public GUIStyle CheckStyle { set; get; }
        public override int FontSize {
            set {
                fontSize = value;
                if (CheckStyle != null) CheckStyle.fontSize = fontSize;
            }
        }
        /// <summary>現在の背景色</summary>
        public Color? CurrentBackgroundColor {
            get {
                return _value ? _selectBackgroundColor : BackgroundColor;
            }
        }

        /// <summary>現在のテキスト色</summary>
        public Color CurrentTextColor {
            get {
                return _value ? _selectTextColor : TextColor;
            }
        }

        public GUIContent CurrentContent {
            get {
                return _value ? _selectedContent : _content;
            }
        }

        /// <summary>選択色</summary>
        private Color _selectBackgroundColor = Color.green;
        public Color SelectBackgroundColor {
            get { return _selectBackgroundColor; }
            set { _selectBackgroundColor = value; }
        }

        /// <summary>選択テキスト色</summary>
        private Color _selectTextColor = Color.white;
        public Color SelectTextColor {
            get { return _selectTextColor; }
            set { _selectTextColor = value; }
        }

        private readonly GUIContent _selectedContent = new GUIContent();
        public string SelectText {
            get { return _selectedContent.text; }
            set { _selectedContent.text = value; }
        }
        public Texture SelectImage {
            get { return _selectedContent.image; }
            set { _selectedContent.image = value; }
        }
        private readonly GUIContent _content = new GUIContent();
        public override string Text {
            get { return _content.text; }
            set { _content.text = value; }
        }

        public Texture Image {
            get { return _content.image; }
            set { _content.image = value; }
        }

        #endregion

        #region Fields
//        public Func<bool> ValFunc;
        private CustomToggle _pair;
        private readonly GUIColorStore _colorStore = new GUIColorStore();
        #endregion

        #region Events
        public EventHandler CheckChanged = delegate { };
        #endregion
    }
}
