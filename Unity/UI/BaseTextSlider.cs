using UnityEngine;

namespace EffekseerPlayerPlugin.Unity.UI {
    public abstract class BaseTextSlider<T> : GUIControl {
        #region Methods
        /// <summary>コンストラクタ.</summary>
        /// <param name="parent">親要素</param>
        protected BaseTextSlider(GUIControl parent) : base(parent) {
            // disable once DoNotCallOverridableMethodsInConstructor
            backgroundColor = Color.white;
        }

        /// <summary>コンストラクタ.</summary>
        /// <param name="uiParams">UIパラメータ</param>
        protected BaseTextSlider(UIParamSet uiParams) : base(uiParams) {
            // disable once DoNotCallOverridableMethodsInConstructor
            backgroundColor = Color.white;
        }

        public override void Awake() {
            if (Text != null) {
                if (LabelStyle == null) {
                    LabelStyle = new GUIStyle("label");
                    if (fontSize > 0) {
                        LabelStyle.fontSize = fontSize;
                    }
                }
            }
            if (listeners != null) {
                if (ButtonStyle == null) {
                    ButtonStyle = new GUIStyle("button") {
                        alignment = TextAnchor.MiddleCenter,
                    };
                    if (fontSizeS > 0) {
                        ButtonStyle.fontSize = fontSizeS;
                    }
                }
            }

            if (TextFStyle == null) {
                TextFStyle = new GUIStyle("textField") {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = fontSize,
                    normal = {textColor = TextColor}
                };
            }
            if (!warnColor.HasValue) {
                warnColor = Color.red;
            }
        }

        #endregion

        #region Fields
        public float indent;
        public Rect labelRect = new Rect();
        public Rect presetRect = new Rect();
        public Rect textRect;
        public Rect sliderRect = new Rect();

        internal readonly GUIColorStore colorStore = new GUIColorStore();
        internal readonly GUITextColorStore txtColorStore = new GUITextColorStore();
        internal Color? warnColor;

        public PresetListener<T>[] listeners;
        #endregion

        #region Properties
        public override int FontSize {
            set {
                fontSize = value;
                if (LabelStyle != null) LabelStyle.fontSize = fontSize;
                if (TextFStyle != null) TextFStyle.fontSize = fontSize;
            }
        }

        protected int fontSizeS;
        public virtual int FontSizeS {
            get { return fontSizeS; }
            set {
                fontSizeS = value;
                if (ButtonStyle != null) ButtonStyle.fontSize = fontSizeS;
            }
        }
        public GUIStyle LabelStyle { get; set; }
        public GUIStyle ButtonStyle { get; set; }
        public GUIStyle TextFStyle { get; set; }

        public virtual float TextWidth {
            get { return textRect.width; }
            set { textRect.width = value; }
        }
        public virtual float TextHeight {
            get { return textRect.height; }
            set { textRect.height = value; }
        }

        #endregion
    }
}
