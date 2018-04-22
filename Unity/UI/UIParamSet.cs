using System;
using System.Collections.Generic;
using UnityEngine;

namespace EffekseerPlayer.Unity.UI {
    /// <summary>
    /// UIパーツの共通パラメータ管理クラス.
    /// 基本となるパラメータセットのみとする
    /// </summary>
    public class UIParamSet {
        public static readonly UIParamSet Instance = new UIParamSet();

        public static readonly int WINID_MAIN = NewWindowId();
        public static readonly int WINID_DIALOG = NewWindowId();

        private static int _windowID = 20310;
        /// <summary>次に作成するウィンドウID</summary>
        public static int NewWindowId() {
            return _windowID++;
        }

        #region Constants
        public static readonly Color ErrorColor = new Color(0.75f, 0f, 0.1f, 0.9f);
        public const float MARGIN_PX = 3;
        public const float MARGIN_L_PX = 12;
        public const float ITEM_HEIGHT_PX = 24;
        public const int FONT_PX = 16;
        public const int FONT_S_PX = 14;
        public const int FONT_SS_PX = 12;
        public const int FONT_L_PX = 24;
        #endregion

        private readonly List<Action<UIParamSet>> _updaters = new List<Action<UIParamSet>>();

        private Func<float> _calcRate;
        private float _sizeRate;
        public float SizeRate {
            get { return _sizeRate;  }
            set {
                _sizeRate = value;
                _calcRate = null;
            }
        }

        private int _width;
        private int _height;
        private float _ratio;

        public float margin;
        public float marginL; //
        public int fontSize;
        public int fontSizeS;
        public int fontSizeSS;
        public int fontSizeL;
        public float itemHeight;
        public float unitHeight;

        public readonly GUIStyle listStyle = new GUIStyle();
        public readonly GUIStyle frameStyle = new GUIStyle("box");

        public readonly Color textColor = new Color(1f, 1f, 1f, 0.98f);
        //public readonly Color transColor = new Color(1f, 1f, 1f, 0.34f);

        public UIParamSet() {
            listStyle.onHover.background = listStyle.hover.background = new Texture2D(2, 2);
            listStyle.padding.left = listStyle.padding.right = 4;
            listStyle.padding.top = listStyle.padding.bottom = 1;
            listStyle.normal.textColor = listStyle.onNormal.textColor =
                listStyle.hover.textColor = listStyle.onHover.textColor =
                listStyle.active.textColor = listStyle.onActive.textColor = Color.white;
            listStyle.focused.textColor = listStyle.onFocused.textColor = Color.blue;

            frameStyle.border = new RectOffset(5, 5, 5, 5);
            //new RectOffset(
            // frameStyle.contentOffset = new Vector2(2, 2);
        }

        private bool ScreenSizeChanged() {
            var screenSizeChanged = false;

            if (Screen.height != _height) {
                _height = Screen.height;
                screenSizeChanged = true;
            }
            if (Screen.width == _width) return screenSizeChanged;

            _width = Screen.width;
            return true;
        }

        protected void InitRate() {
            if (_sizeRate <= 0) {
                _calcRate = () => (1.0f + ((float)_width / 1920 - 1.0f) * 0.5f);
            } else {
                _calcRate = () => _sizeRate;
            }
        }

        public void Update() {
            if (!ScreenSizeChanged()) return;

            if (_calcRate == null) InitRate();
            _ratio = _calcRate();
#if DEBUG
            Debug.LogError("width:" + _width + ", ratio=" + _ratio);
#endif

            // 画面サイズが変更された場合にのみ更新
            fontSize   = FixPx(FONT_PX);
            fontSizeS  = FixPx(FONT_S_PX);
            fontSizeSS = FixPx(FONT_SS_PX);
            fontSizeL  = FixPx(FONT_L_PX);
            margin  = FixPx(MARGIN_PX);
            marginL = FixPx(MARGIN_L_PX);
            itemHeight = FixPx(ITEM_HEIGHT_PX);
            unitHeight = margin + itemHeight;

            listStyle.fontSize = fontSizeS;

            Log.DebugF("screen=({0},{1}),margin={2},height={3},ratio={4})", _width, _height, margin, itemHeight, _ratio);
            //InitWinRect();

            foreach (var func in _updaters) func(this);
        }

        public int FixPx(int px) {
            return (int)(_ratio * px);
        }

        public float FixPx(float px) {
            return (_ratio * px);
        }

        public void AddListener(Action<UIParamSet> action) {
            _updaters.Add(action);
        }

        public bool Remove(Action<UIParamSet> action) {
            return _updaters.Remove(action);
        }
    }
}