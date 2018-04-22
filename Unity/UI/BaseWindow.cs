using UnityEngine;

namespace EffekseerPlayer.Unity.UI　{
    /// <summary>
    /// ウィンドウを実現する抽象クラス.
    /// ただし、DropDownを実現するため、GUI.WindowではなくあくまでBoxとして疑似的なウィンドウを実現する.
    /// 
    /// </summary>
    public abstract class BaseWindow : GUIControl　{
        protected BaseWindow(UIParamSet uiParamSet) : base(uiParamSet) { }

        /// <summary>
        /// カーソル位置が描画中にウィンドウ内に入っているか判定し、
        /// 状態を更新する
        /// </summary>
        /// <param name="cursor"></param>
        public void UpdateCursor(ref Vector2 cursor) {
            cursorContains = false;
            cursorTitleContains = false;

            if (!Visibled) return;

            titleBarRect.x = Rect.x;
            titleBarRect.y = Rect.y;

            cursorContains = Rect.Contains(cursor);
            if (cursorContains) {
                cursorTitleContains = titleBarRect.Contains(cursor);
            }
        }

        /// <summary>
        /// ドラッグ操作で位置を更新する
        /// </summary>
        public void UpdateDrag() {
            if (!Visibled) return;

            // Drag動作
            // http://answers.unity3d.com/questions/258048/editor-gui-drag-selection-box.html
            switch (Event.current.type) {
            case EventType.MouseDrag:
                if (!dragging) {
                    if (cursorTitleContains) {
                        dragging = true;
                        startPosition = Event.current.mousePosition;
                    }
                }
                break;
            case EventType.MouseUp:
                if (dragging) {
                    MoveWin();
                    dragging = false;
                }
                break;
            }

            if (!dragging) return;

            var boxRect = new Rect(rect.x + Event.current.mousePosition.x - startPosition.x,
                rect.y + Event.current.mousePosition.y - startPosition.y,
                rect.width, rect.height);

            GUIColorStore.Default.SetColor(ref transColor, ref transColor);
            try {
                GUI.Box(boxRect, string.Empty);//, uiParams.frameStyle);
            } finally {
                GUIColorStore.Default.Restore();
            }
        }

        protected virtual void MoveWin() {
            attached = false;
            var currentPos = Event.current.mousePosition;
            MoveWin(currentPos.x - startPosition.x, currentPos.y - startPosition.y);
        }

        protected virtual void MoveWin(float deltaX, float deltaY) {
            rect.x += deltaX;
            rect.y += deltaY;

            // 吸着判定
            if (parentWin != null) {
                if (parentWin.Left - DELTA <= rect.xMax && rect.xMax <= parentWin.Left + DELTA) {
                    rect.x = parentWin.Left - rect.width;
                    attached = true;
                } 
            }

            UpdateLayout(uiParamSet);
            if (_followedWin != null && _followedWin.attached) {
                _followedWin.MoveWin(deltaX, deltaY);
            }
        }

        /// <summary>
        /// スクリーンサイズが変更されたときに呼びされるメソッド.
        /// ウィンドウのサイズを更新し、リレイアウトを行う
        /// </summary>
        /// <param name="uiParams">UIパラメータセット</param>
        internal virtual void WinResized(UIParamSet uiParams) {
            InitSize();
            CheckWinPosition();

            titleBarRect.Set(Left+4, Top, Width-8-20, uiParams.FixPx(titleBarHeight));

            UpdateUISize();
            UpdateLayout(uiParams);
        }

        /// <summary>
        /// 未初期化か画面外であるかをチェックし、その場合に位置を初期化する
        /// </summary>
        protected void CheckWinPosition() {
            if (Left <= 0 || Screen.width <= Left || Top <= 0 || Screen.height <= Top) {
                InitPos();
            }
        }

        /// <summary>
        /// ウィンドウの位置を更新する.
        /// </summary>
        internal abstract void InitPos();

        /// <summary>
        /// ウィンドウのサイズを更新する.
        /// </summary>
        internal abstract void InitSize();

        /// <summary>
        /// スクリーンサイズが変更時に呼び出されるUIのサイズが変更された場合に呼び出される.
        /// フォントサイズやマージンなどを更新する.
        /// </summary>
        internal abstract void UpdateUISize();

        public void InitStatus() {
            cursorContains = false;
            cursorTitleContains = false;
            visibled = false;
            dragging = false;
        }

        /// <summary>
        /// ドラッグ時の透明カラー
        /// </summary>
        public Color transColor = new Color(1f, 1f, 1f, 0.34f);
        protected bool cursorTitleContains;
        protected bool dragging;
        protected Vector2 startPosition;

        private const int DELTA = 16;
        public float titleBarHeight = 24f;
        protected Rect titleBarRect;
        internal bool attached;
        protected BaseWindow parentWin;
        private BaseWindow _followedWin;
        /// <summary>
        /// 追従ウィンドウ.
        /// attached=trueであれば、ウィンドウ移動時に追従する
        /// </summary>
        internal BaseWindow FollowedWin {
            get { return _followedWin; }
            set {
                _followedWin = value;
                _followedWin.attached = true;
                _followedWin.parentWin = this;
            }
        }

        protected void CloseAction() {
            if (_followedWin != null) _followedWin.Visibled = false;
            cursorContains = false;
            cursorTitleContains = false;
        }

        #region Properties
        public GUIStyle WinStyle { get; set; }
        protected bool cursorContains;
        public bool CursorContains {
            get {
                return Visibled && cursorContains;
            }
        }

        protected bool visibled;
        public virtual bool Visibled {
            get { return visibled; }
            set {
                visibled = value;
                if (visibled) {
                    CheckWinPosition();
                    return;
                }

                CloseAction();
            }
        }
        #endregion

    }
}
