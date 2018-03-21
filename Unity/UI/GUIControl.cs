using System;
using System.Collections.Generic;
using UnityEngine;

namespace EffekseerPlayerPlugin.Unity.UI
{
    /// <summary>
    /// カスタムコントロールクラス.
    /// コントロールの抽象クラス.
    /// </summary>
    public abstract class GUIControl
    {
        #region Methods
        protected GUIControl(UIParamSet uiParams) {
            this.uiParams = uiParams;
            root = this;
        }

        protected GUIControl(GUIControl parent) {
            uiParams = parent.uiParams;
            parent.Add(this);
        }

        protected virtual void AwakeChildren() {
            try {
                foreach (var child in Children) {
                    child.Awake();
                }
            } catch (Exception e) {
                Log.Error(e);
            }
        }

        protected virtual void OnGUIChildren() {
            try {
                foreach (var child in Children) {
                    child.OnGUI();
                }
            } catch (Exception e) {
                Log.Error(e);
            }
        }

        public virtual void Awake() { }
        public virtual void OnLevelWasLoaded(int level) { }
        public virtual void Update() { }

        public virtual void OnGUI() {
            if (!Enabled) enabledStore.SetEnabled(false);

            try {
                DrawGUI();
//            } catch(Exception e) {
//                Log.Error("Failed to Drawa GUI", e);
            } finally {
                enabledStore.Restore();
            }
        }

        protected virtual void DrawGUI() { }

        /// <summary>
        /// レイアウト情報を更新する.
        /// 画面サイズ等のUIパラメータが変更された後に呼び出されることとする.
        /// (Awake後でなければ呼んではいけない)
        /// </summary>
        /// <param name="uiparams">UIパラメータ</param>
        internal virtual void Relayout(UIParamSet uiparams) { }

        internal virtual void RelayoutChildren() {
            foreach (var child in Children) {
                child.Relayout(uiParams);
            }
        }

        /// <summary>
        /// Childrenに追加する.
        /// root要素が先に決定している想定とする.
        /// </summary>
        /// <param name="child">子要素</param>
        protected virtual void Add(GUIControl child) {
            if (Children == null) Children = new List<GUIControl>();

            Children.Add(child);
            child.parent = this;
            child.root = root;
        }

        /// <summary>
        /// 親コントロールの位置と、指定されたalignを元に配置する.
        /// widthとheightは、0以下の値の場合 親コントロールの位置とサイズから
        /// 最大サイズからの差分としてサイズを決定する.
        /// ex) 0の場合:最大値,
        ///    -1の場合:最大値-1のサイズとなる
        /// </summary>
        /// <param name="align"></param>
        public virtual void Align(ref Rect align) {
            Align(align.x, align.y, align.width, align.height);
        }

        public virtual void Align(float left, float top, float width, float height) {
            Left = parent.Left + left;
            Top = parent.Top + top;
            AlignSize(width, height);
        }

        public virtual void AlignLeft(GUIControl ctrl, ref Rect align) {
            AlignLeft(ctrl, align.x, align.y, align.width, align.height);
        }

        /// ctrl を左側に並べて配置する
        public virtual void AlignLeft(GUIControl ctrl, float left, float top, float width, float height) {
            Left = ctrl.xMax + left;
            Top = parent.Top + top;
            AlignSize(width, height);
        }

        public virtual void AlignTop(GUIControl ctrl, ref Rect align) {
            AlignTop(ctrl, align.x, align.y, align.width, align.height);
        }

        public virtual void AlignTop(GUIControl ctrl, float left, float top, float width, float height) {
            Left = parent.Left + left;
            Top = ctrl.yMax + top;
            AlignSize(width, height);
        }

        public virtual void AlignLeftTop(GUIControl ctrlL, GUIControl ctrlT, ref Rect align) {
            AlignLeftTop(ctrlL, ctrlT, align.x, align.y, align.width, align.height);
        }

        public virtual void AlignLeftTop(GUIControl ctrlL, GUIControl ctrlT, float left, float top, float width, float height) {
            Left = ctrlL.xMax + left;
            Top = ctrlT.yMax + top;
            AlignSize(width, height);
        }

        //private void AlignSize(float width, float height) {
        //    Width = (width >= 0) ? width : parent.Width + width;
        //    Height = (height >= 0) ? height : parent.Height + height;
        //}
        private void AlignSize(float width, float height) {
            Width = (width > 0) ? width : parent.Width - (Left - parent.Left) + width;
            Height = (height > 0) ? height : parent.Height - (Top - parent.Top) + height;
        }

        #endregion

        #region Properties
        
        protected virtual List<GUIControl> Children { set; get; }
    
        private bool _enabled = true;
        public virtual bool Enabled {
            get {  return _enabled;  }
            set {  _enabled = value; }
        }

        protected string text;
        public virtual string Text {
            get { return text; }
            set { text = value; }
        }

        protected Color textColor = Color.white;
        public virtual Color TextColor {
            get { return textColor; }
            set { textColor = value; }
        }

        protected Color? backgroundColor;
        public virtual Color? BackgroundColor {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        protected Rect rect;
        public virtual Rect Rect {
            get { return rect; }
            set { rect = value; }
        }

        public virtual float Top { 
            get { return rect.y;  } 
            set { rect.y = value; }
        }

        public virtual float Left { 
            get { return rect.x;  } 
            set { rect.x = value; }
        }

        public virtual float Width { 
            get { return rect.width; }
            set { rect.width = value; }
        }

        public virtual float Height {
            get { return rect.height; }
            set { rect.height = value; }
        }

        // ReSharper disable once InconsistentNaming
        public virtual float xMax { get { return rect.xMax; } }
        // ReSharper disable once InconsistentNaming
        public virtual float yMax { get { return rect.yMax; } }

        public virtual void SetPos(float x, float y) {
            rect.x = x;
            rect.y = y;
        }

        public virtual void SetSize(float width, float height) {
            rect.width = width;
            rect.height = height;
        }

        protected int fontSize;
        public virtual int FontSize {
            get { return fontSize; }
            set { fontSize = value; }
        }

        protected float margin;
        public virtual float Margin {
            get { return margin;  }
            set { margin = value; }
        }
        #endregion

        protected bool modal;
        protected GUIControl root;
        protected GUIControl parent;
        protected readonly UIParamSet uiParams;
        protected readonly GUIEnabledStore enabledStore = new GUIEnabledStore();


        protected const float WIDTH_SCROLLBAR = 20f;

        internal static readonly Action<Rect, string> DebugLog = (r, key) => Log.DebugF("{0}, ({1}, {2}, {3}, {4})", key, r.x, r.y, r.width, r.height);
    }

    ///-------------------------------------------------------------------------
    /// <summary>GUI色設定</summary>
    public class GUIColor : IDisposable
    {
        #region Methods
    
        /// <summary>コンストラクタ</summary>
        /// <param name="backgroundColor">背景色</param>
        /// <param name="contentColor">コンテンツ色</param>
        public GUIColor(Color backgroundColor, Color contentColor) {
            try {
                // 元の色退避
                _oldBackgroundColor = GUI.backgroundColor;
                _oldcontentColor = GUI.contentColor;

                // 色設定
                GUI.backgroundColor = backgroundColor;
                GUI.contentColor = contentColor;
            } catch (Exception e) {
                Log.Error(e);
            }
        }

        /// <summary>元に戻す</summary>
        public void Dispose() {
            try {
                // 色を戻す
                GUI.backgroundColor = _oldBackgroundColor;
                GUI.contentColor = _oldcontentColor;
            } catch (Exception e) {
                Log.Error(e);
            }
        }
        #endregion

        #region Fileds
        /// <summary>元の色</summary>
        private readonly Color _oldBackgroundColor;

        /// <summary>元の色</summary>
        private readonly Color _oldcontentColor;
        #endregion
    }

    ///-------------------------------------------------------------------------
    /// <summary>GUI色設定</summary>
    public class GUIColorStore
    {
        #region Methods
        /// <summary>背景色とコンテンツを設定する</summary>
        /// <param name="contentColor">コンテンツ色</param>
        /// <param name="backgroundColor">背景色</param>
        public void SetColor(Color contentColor, Color? backgroundColor) {
            _backgroundColor = GUI.backgroundColor;
            _contentColor    = GUI.contentColor;

            if (backgroundColor.HasValue) {
                GUI.backgroundColor = backgroundColor.Value;
            }
            GUI.contentColor    = contentColor;
        }

        /// <summary>背景色とコンテンツを設定する</summary>
        /// <param name="contentColor">コンテンツ色</param>
        /// <param name="backgroundColor">背景色</param>
        public void SetColor(ref Color contentColor, ref Color? backgroundColor) {
            _backgroundColor = GUI.backgroundColor;
            _contentColor    = GUI.contentColor;

            if (backgroundColor.HasValue) {
                GUI.backgroundColor = backgroundColor.Value;
            }
            GUI.contentColor    = contentColor;
        }

        /// <summary>背景色とコンテンツを設定する</summary>
        /// <param name="contentColor">コンテンツ色</param>
        /// <param name="backgroundColor">背景色</param>
        public void SetColor(ref Color contentColor, ref Color backgroundColor) {
            _backgroundColor = GUI.backgroundColor;
            _contentColor    = GUI.contentColor;

            GUI.backgroundColor = backgroundColor;
            GUI.contentColor    = contentColor;
        }

        /// <summary>背景色を設定する</summary>
        /// <param name="backgroundColor">背景色</param>
        public void SetBGColor(ref Color? backgroundColor) {
            if (!backgroundColor.HasValue) return;

            _backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor.Value;
        }

        /// <summary>背景色を設定する</summary>
        /// <param name="backgroundColor">背景色</param>
        public void SetBGColor(ref Color backgroundColor) {
            _backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
        }

        /// <summary>コンテンツ色を設定する</summary>
        /// <param name="contentColor">コンテンツ色</param>
        public void SetContentColor(ref Color contentColor) {
            _contentColor = GUI.contentColor;
            GUI.contentColor = contentColor;
        }

        /// <summary>元に戻す</summary>
        public void Restore() {
            if (_backgroundColor.HasValue) {
                GUI.backgroundColor = _backgroundColor.Value;
                _backgroundColor = null;
            }

            if (!_contentColor.HasValue) return;
            GUI.contentColor = _contentColor.Value;
            _contentColor = null;
        }
        #endregion

        #region Fileds
        private Color? _backgroundColor;
        private Color? _contentColor;
    
        private static readonly GUIColorStore INSTANCE = new GUIColorStore();
        public static GUIColorStore Default { get { return INSTANCE; } }
        #endregion
    }

    ///-------------------------------------------------------------------------
    /// <summary>GUIテキスト色設定</summary>
    public class GUITextColor : IDisposable
    {
        #region Methods
    
        /// <summary>コンストラクタ.</summary>
        /// <param name="style">スタイル</param>
        /// <param name="normal">通常色</param>
        /// <param name="focused">フォーカス色</param>
        /// <param name="active">アクティブ時の色</param>
        /// <param name="hover">ホバー時の色</param>
        public GUITextColor(GUIStyle style, ref Color normal, ref Color focused, ref Color active, ref Color hover) {
            if (style == null)
                throw new ArgumentNullException("style");

            try {
                _style = style;
                // 元の色退避
                _normal  = _style.normal.textColor;
                _focused = _style.focused.textColor;
                _active  = _style.active.textColor;
                _hover   = _style.hover.textColor;

                // 色設定
                _style.normal.textColor  = normal;
                _style.focused.textColor = focused;
                _style.active.textColor  = active;
                _style.hover.textColor   = hover;
            } catch (Exception e) {
                Log.Error(e);
            }
        }

        public GUITextColor(GUIStyle style, ref Color color)
            : this(style, ref color, ref color, ref color, ref color) { }

        /// <summary>元に戻す</summary>
        public void Dispose() {
            try {
                // 色を戻す
                _style.normal.textColor  = _normal;
                _style.focused.textColor = _focused;
                _style.active.textColor  = _active;
                _style.hover.textColor   = _hover;
            } catch (Exception e) {
                Log.Error(e);
            }
        }
        #endregion

        #region Fileds
        private readonly GUIStyle _style;
        private readonly Color _normal;
        private readonly Color _focused;
        private readonly Color _active;
        private readonly Color _hover;
        #endregion
    }

    ///-------------------------------------------------------------------------
    /// <summary>GUIテキスト色設定</summary>
    public class GUITextColorStore 
    {
        #region Methods
        /// <summary>テキスト色をスタイルに設定する.</summary>
        /// <param name="style">スタイル</param>
        /// <param name="normal">通常色</param>
        /// <param name="focused">フォーカス色</param>
        /// <param name="active">アクティブ時の色</param>
        /// <param name="hover">ホバー時の色</param>
        public void SetColor(GUIStyle style, ref Color normal, ref Color focused, ref Color active, ref Color hover) {
            if (style == null) return;

            try {
                _style = style;
                // 元の色退避
                _normal  = _style.normal.textColor;
                _focused = _style.focused.textColor;
                _active  = _style.active.textColor;
                _hover   = _style.hover.textColor;

                // 色設定
                _style.normal.textColor  = normal;
                _style.focused.textColor = focused;
                _style.active.textColor  = active;
                _style.hover.textColor   = hover;
            } catch (Exception e) {
                Log.Error(e);
            }
        }

        public void SetColor(GUIStyle style, Color color) {
            SetColor(style, ref color, ref color, ref color, ref color);
        }

        public void SetColor(GUIStyle style, ref Color color) {
            SetColor(style, ref color, ref color, ref color, ref color);
        }

        /// <summary>元に戻す</summary>
        public void Restore() {
            if (_style == null) return;

            // 色を戻す
            _style.normal.textColor  = _normal;
            _style.focused.textColor = _focused;
            _style.active.textColor  = _active;
            _style.hover.textColor   = _hover;
            _style = null;
        }
        #endregion

        #region Fileds
        private GUIStyle _style;

        private Color _normal;
        private Color _focused;
        private Color _active;
        private Color _hover;
        #endregion
    }

    ///-------------------------------------------------------------------------
    /// <summary>GUI有効設定</summary>
    public class GUIEnabledStore 
    {
        #region Methods
        /// <summary>
        /// 有効状態を設定する.
        /// 現在と同じ状態が指定された場合は、変更しない.
        /// その場合は、リストアでは何もしない.
        /// </summary>
        /// <param name="enabled">有効状態</param>
        public void SetEnabled(bool enabled) {
            if (_enabled == GUI.enabled) return;

            _enabled = GUI.enabled;
            GUI.enabled = enabled;
        }

        /// <summary>元に戻す</summary>
        public void Restore() {
            if (!_enabled.HasValue) return;

            GUI.enabled = _enabled.Value;
            _enabled = null;
        }
        #endregion

        #region Fileds
        private bool? _enabled;

        private static readonly GUIEnabledStore INSTANCE = new GUIEnabledStore();
        public static GUIEnabledStore Default { get { return INSTANCE; } }
        #endregion
    }
}
