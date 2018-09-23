using System;
using System.Collections.Generic;
using UnityEngine;

namespace EffekseerPlayer.Unity.UI {
    /// <summary>
    /// カスタムコントロールクラス.
    /// コントロールの抽象クラス.
    /// </summary>
    public abstract class GUIControl {
        #region Methods
        protected GUIControl(UIParamSet uiParamSet) {
            this.uiParamSet = uiParamSet;
            margin = uiParamSet.margin;
            root = this;
        }

        protected GUIControl(GUIControl parent) {
            uiParamSet = parent.uiParamSet;
            margin = uiParamSet.margin;
            parent.Add(this);
        }

        protected virtual void AwakeChildren() {
            if (Children == null) return;

            try {
                foreach (var child in Children) {
                    child.Awake();
                }
            } catch (Exception e) {
                Log.Error(e);
            }
        }

        protected virtual void OnGUIChildren() {
            if (Children == null) return;
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
//                Log.Error("Failed to Draw GUI", e);
            } finally {
                enabledStore.Restore();
            }
        }

        protected virtual void DrawGUI() { }

        /// <summary>
        /// 画面サイズ等のUIパラメータの変更に対して、レイアウト情報を更新する.
        /// </summary>
        /// <param name="uiParams">UIパラメータ</param>
        protected virtual void Layout(UIParamSet uiParams) { }

        /// <summary>
        /// レイアウト情報を更新する.
        /// 画面サイズ等のUIパラメータが変更された後に呼び出す必要がある.
        /// </summary>
        /// <param name="uiParams">UIパラメータ</param>
        internal void UpdateLayout(UIParamSet uiParams) {
            Layout(uiParams);

            LayoutChildren(uiParams);
        }

        protected virtual void LayoutChildren(UIParamSet uiParams) {
            if (Children == null) return;

            foreach (var child in Children) {
                child.UpdateLayout(uiParams);
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

        public void AlignLeft(GUIControl obj, FormData fd) {
            fd.Left.obj = obj;
            Align(fd);
        }

        public void AlignTop(GUIControl obj, FormData fd) {
            fd.Top.obj = obj;
            Align(fd);
        }

        /// <summary>
        /// 左端・上端の位置から優先的に配置する.
        ///
        /// ただし、Left=nullの場合は右端から配置、 Top=nullの場合は下端から配置
        /// </summary>
        /// <param name="fd">フォームデータ</param>
        public void Align(FormData fd) {
            if (fd.Left != null) AlignLeft(fd.Left, fd.Right);
            else AlignRight(fd.Left, fd.Right);

            if (fd.Top != null) AlignTop(fd.Top, fd.Bottom);
            else AlignBottom(fd.Top, fd.Bottom);
        }

        /// <summary>
        /// 右端・下端の位置から配置する.
        /// Widthが指定されていれば、offsetを用いて右端からLeftを特定する.
        /// Heightが指定されていれば、offsetを用いて下端からTopを特定する.
        ///
        /// </summary>
        /// <param name="fd">フォームデータ</param>
        public void AlignRev(FormData fd) {
            if (fd.Right != null) AlignRight(fd.Left, fd.Right);
            else AlignLeft(fd.Left, fd.Right);

            if (fd.Bottom != null) AlignBottom(fd.Top, fd.Bottom);
            else AlignTop(fd.Top, fd.Bottom);
        }

        /// <summary>
        /// 左から詰めて配置する.
        ///
        /// * 幅の算出優先度
        /// right配置情報が指定された場合は、そこからWidthを算出する.
        /// rが指定されていなければ、l.lengthをWidthとする.
        /// l.lengthが指定されていない場合は、Widthは設定しない
        /// </summary>
        /// <param name="l">left配置情報</param>
        /// <param name="r">right配置情報</param>
        protected void AlignLeft(AttachData l, AttachData r) {
            if (l == null) return;

            if (l.obj != null) {
                Left = l.obj.xMax + l.offset;
            } else {
                Left = parent.Left + l.offset;
            }

            if (r != null) {
                var rightPos = (r.obj != null) ? r.obj.Left - r.offset : parent.xMax - r.offset;
                Width = rightPos - Left;
            } else if (l.length > 0) {
                Width = l.length;
            }
        }

        /// <summary>
        /// 右から順に配置する
        /// </summary>
        /// <param name="l">left配置情報</param>
        /// <param name="r">right配置情報</param>
        protected void AlignRight(AttachData l, AttachData r) {
            if (r == null) return;

            if (r.length > 0) {
                Width = r.length;

                var rightPos = (r.obj != null) ? r.obj.Left - r.offset : parent.xMax - r.offset;
                var left  = rightPos - Width;
                if (l != null) left -= l.offset;

                if (left < parent.Left) {
                    if (Width > rightPos - parent.Left) {
                        Width = rightPos - parent.Left;
                    }
                    left = parent.Left;
                }
                Left = left;
            } else {
                if (l != null) {
                    var rightPos = (r.obj != null) ? r.obj.Left - r.offset : parent.xMax - r.offset;
                    var leftPos = (l.obj != null) ? l.obj.xMax + l.offset : parent.Left + l.offset;
                    Width = rightPos - leftPos;
                    if (Width < 0) {
                        Width = 0;
                        return;
                    }
                    Left = leftPos;

                } else {
                    var rightPos = (r.obj != null) ? r.obj.Left - r.offset : parent.xMax - r.offset;
                    // 親の左端から、ad.objの左端まで
                    Width = rightPos  - parent.Left;
                    Left = parent.Left;
                }
            }
        }

        protected void AlignTop(AttachData t, AttachData b) {
            if (t == null) return;
            if (t.obj != null) {
                Top = t.obj.yMax + t.offset;
            } else {
                Top = parent.Top + t.offset;
            }

            if (b != null) {
                var topPos = (b.obj != null) ? b.obj.Top - b.offset: parent.yMax - b.offset;
                Height = topPos - Top;
            } else if (t.length > 0) {
                Height = t.length;
            }
        }

        /// <summary>
        /// 下から順に配置する
        /// </summary>
        /// <param name="t">top配置情報</param>
        /// <param name="b">bottom配置情報</param>
        protected void AlignBottom(AttachData t, AttachData b) {
            if (b == null) return;

            if (b.length > 0) {
                Height = b.length;

                var bottomPos = (b.obj != null) ? b.obj.Top - b.offset : parent.yMax - b.offset;
                var top  = bottomPos - Height;
                if (t != null) top -= t.offset;

                if (top < parent.Top) {
                    if (Height > bottomPos - parent.Top) {
                        Height = bottomPos - parent.Top;
                    }
                    top = parent.Top;
                }
                Top = top;
            } else {
                if (t != null) {
                    var bottomPos = (b.obj != null) ? b.obj.Top - b.offset : parent.yMax - b.offset;
                    var topPos = (t.obj != null) ? t.obj.yMax + t.offset : parent.Top + t.offset;
                    Height = bottomPos - topPos;
                    if (Height < 0) {
                        Height = 0;
                        return;
                    }
                    Top = topPos;

                } else {
                    var bottomPos = (b.obj != null) ? b.obj.Top - b.offset : parent.yMax - b.offset;
                    // 親の上端から、b.objの下端まで
                    Height = bottomPos  - parent.Top;
                    Top = parent.Top;
                }
            }
        }


        // Leftを先に設定する場合
        protected void AlignRight(AttachData ad) {
            if (ad == null) return;
            if (ad.obj != null) {
                Width = ad.obj.Left - Left - ad.offset;
            } else {
                Width = parent.xMax - Left - ad.offset;
            }
        }

        // Topを先に設定する
        protected void AlignBottom(AttachData ad) {
            if (ad == null) return;
            if (ad.obj != null) {
                Height = ad.obj.Top - Top - ad.offset;
            } else {
                Height = parent.yMax - Top - ad.offset;
            }
        }

        #endregion

        #region Properties
        protected virtual List<GUIControl> Children { set; get; }
    
        private bool _enabled = true;
        public virtual bool Enabled {
            get { return _enabled;  }
            set { _enabled = value; }
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
        protected readonly UIParamSet uiParamSet;
        protected readonly GUIEnabledStore enabledStore = new GUIEnabledStore();


        protected const float WIDTH_SCROLLBAR = 16f;

        internal static readonly Action<Rect, string> DebugLog = (r, key) => Log.DebugF("{0}, ({1}, {2}, {3}, {4})", key, r.x, r.y, r.width, r.height);
    }

    ///-------------------------------------------------------------------------
    /// <summary>GUI色設定</summary>
    public class GUIColor : IDisposable {
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
    public class GUIColorStore {
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
    public class GUITextColor : IDisposable {
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
    public class GUITextColorStore {
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
    public class GUIEnabledStore {
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
