using UnityEngine;

namespace EffekseerPlayerPlugin.Unity.UI {
    /// <summary>
    /// スクロールビュー抽象クラス
    /// </summary>
    public abstract class BaseScrollView : GUIControl {
        public BaseScrollView(UIParamSet uiParamSet) : base(uiParamSet) { }

        public BaseScrollView(GUIControl parent) : base(parent) { }

        //public override void Update() {
        //    viewRect.height = GetContentHeight();
        //}
        public override void Awake() {
            if (HScrollStyle == null) {
                HScrollStyle = new GUIStyle("horizontalScrollbar");
            }
            if (VScrollStyle == null) {
                VScrollStyle = new GUIStyle("verticalScrollbar");
            }
        }

        public override void OnGUI() {
            scrollViewPosition = GUI.BeginScrollView(rect, scrollViewPosition, viewRect, 
                AlwaysHScroll, AlwaysVScroll, HScrollStyle, VScrollStyle);

            colorStore.SetColor(ref textColor, ref backgroundColor);
            try {
                OnContentView();
            } finally {
                colorStore.Restore();
                GUI.EndScrollView();
            }
        }
        /// <summary>
        /// スクロールの内容ビュー
        /// </summary>
        protected abstract void OnContentView();
            //if (Children != null) base.OnGUIChildren();

        internal override void Relayout(UIParamSet uiparams) {
            viewRect.Set(rect.x, rect.y, GetViewWidth(), GetViewHeight());
        }

        public abstract float GetViewHeight();

        public virtual float GetViewWidth() {
            return rect.width - WIDTH_SCROLLBAR;
        }

        #region Fields/Properties
        protected Vector2 scrollViewPosition = Vector2.zero;
        protected Rect viewRect = new Rect();
        public bool AlwaysHScroll { get; set; }
        public bool AlwaysVScroll { get; set; }
        public GUIStyle HScrollStyle { get; set; }
        public GUIStyle VScrollStyle { get; set; }
        //protected virtual float GetContentHeight() {
        //    return ContentHeightFunc();
        //}

        internal readonly GUIColorStore colorStore = new GUIColorStore();

        //public Func<float> ContentHeightFunc = () => 0;
        #endregion
    }
}
