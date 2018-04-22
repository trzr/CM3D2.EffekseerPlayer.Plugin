using System;
using System.Collections.Generic;
using System.IO;
using EffekseerPlayer.CM3D2.Data;
using EffekseerPlayer.Unity.UI;
using EffekseerPlayer.Util;
using UnityEngine;

namespace EffekseerPlayer.CM3D2.UI {
    /// <inheritdoc />
    /// <summary>
    /// レシピ編集用ウィンドウ
    /// </summary>
    public partial class EditRecipeView : BaseWindow {
        public EditRecipeView(UIParamSet uiParamSet, RecipeManager recipeMgr, EfkManager efkMgr) : base(uiParamSet) {
            uiParamSet.AddListener(WinResized);
            this.recipeMgr = recipeMgr;
            this.efkMgr = efkMgr;
        }

        ~EditRecipeView() {
            uiParamSet.Remove(WinResized);

            if (subEditView != null) subEditView.Dispose();
        }

        /// <summary>ウィンドウを初期化する</summary>
        public override void Awake() {
            try {
                if (!InitControls()) return;
                Log.Debug("EditRecipe Awake");
                AwakeChildren();

                WinResized(uiParamSet);

            } catch (Exception e) {
                Log.Error("Failed to awake EditRecipeView", e);
                throw;
            }
        }

        internal override void InitPos() {
            if (parentWin != null) {
                Left = parentWin.Left - Width;
                Top  = parentWin.Top + uiParamSet.FixPx(POS_OFFSETY);
            } else {
                Left = Screen.width - uiParamSet.FixPx(820);
                Top  = uiParamSet.FixPx(80);
            }
        }

        internal override void InitSize() {
            Width  = uiParamSet.FixPx(460);
            Height = uiParamSet.FixPx(860);
            if (parentWin != null) {
                var deltaY = uiParamSet.FixPx(POS_OFFSETY);
                if (Height > parentWin.Height - deltaY) {
                    Height = parentWin.Height - deltaY;
                }  
            } else if (Height > 900) {
                Height = 900;
            }
#if DEBUG
            DebugLog(Rect, "editView rect  ");
#endif
        }

        private bool InitControls() {
            if (_initialized) return false;

            Children = new List<GUIControl>(12);
            // ウィンドウスタイル
            if (WinStyle == null) {
                WinStyle = new GUIStyle("box") {
                    alignment = TextAnchor.UpperLeft
                };
                WinStyle.normal.textColor = WinStyle.onNormal.textColor =
                WinStyle.hover.textColor = WinStyle.onHover.textColor =
                WinStyle.active.textColor = WinStyle.onActive.textColor =
                WinStyle.focused.textColor = WinStyle.onFocused.textColor = Color.white;

                WinStyle.border = new RectOffset(5, 5, 5, 5);
            }

            groupnameLabel = new CustomLabel(this, "グループ名:");
            groupnameText = new CustomTextField(this);

            nameLabel = new CustomLabel(this, "名前:");
            nameText = new CustomTextField(this);
            efkLabel = new CustomLabel(this, "effekseer:");

            // efkコンボボックス
            efkCombo = new CustomComboBox(this, CreateEfkItems());

            var resHolder = ResourceHolder.Instance;
            repeatToggle = new CustomToggle(this) {
                Text = " repeat",
                Image = resHolder.RepeatOffImage,
                SelectImage = resHolder.RepeatImage,
                SelectText = " repeat",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };

            registButton = new CustomButton(this, "regist") {Enabled = false};
            playButton = new CustomButton(this, new GUIContent(" play", resHolder.PlayImage));
            stopButton = new CustomButton(this, new GUIContent(" stop", resHolder.StopImage));
            stopRButton = new CustomButton(this, new GUIContent(" stopRoot", resHolder.StopRImage));
            pauseButton = new CustomButton(this, new GUIContent(" pause", resHolder.PauseImage));
            EnablePlayButtons(false);

            //------------------------------------------
            // イベント処理
            groupnameText.ValueChanged += (obj, args) => {
                var hasError = groupnameText.Text.IndexOfAny(INVALID_PATHCHARS) != -1;
                groupnameText.hasError = hasError;
            };
            groupnameText.ValueChanged += CheckValidate;

            nameText.ValueChanged += CheckValidate;
            efkCombo.SelectedIndexChanged += CheckValidate;
            registButton.Click += Register;
            playButton.Click   += Play;
            stopButton.Click   += Stop;
            stopRButton.Click  += StopRoot;
            pauseButton.Click  += Pause;

            subEditView = new EditRecipeScrollView(this, CheckValidate);
            
            _initialized = true;
            return true;
        }

        public override void Update() {
            if (!_initialized) return;

            subEditView.Update();

            if (!Visibled) return;

            subEditView.UpdateSlider();
        }

        public override void OnGUI() {
            if (!Visibled) return;

            try {
                GUI.Box(Rect, Text, WinStyle);
                //GUI.Label(titleBarRect, _version, VerStyle);
                groupnameLabel.OnGUI();
                groupnameText.OnGUI();
                nameLabel.OnGUI();
                nameText.OnGUI();

                efkLabel.OnGUI();
                efkCombo.OnGUI();
                repeatToggle.OnGUI();
                if (IsShowDropdown()) enabledStore.SetEnabled(false);
                // OnGUIChildren();
                try {
                    playButton.OnGUI();
                    stopButton.OnGUI();
                    stopRButton.OnGUI();
                    pauseButton.OnGUI();

                    registButton.OnGUI();
                } finally {
                    enabledStore.Restore();
                }
                subEditView.OnGUI();

            } catch (Exception e) {
                Log.Error(e);
            }
        }

        private bool IsShowDropdown() {
            return efkCombo.IsShowDropDownList || subEditView.IsShowDropdown();
        }

        protected override void Layout(UIParamSet uiParams) {
            if (!_initialized) return;

            var margin2 = margin * 2;
            var margin6 = margin * 6;

            var itemHeight = uiParamSet.itemHeight;
            var labelWidth = uiParamSet.FixPx(100);
            var align = new Rect(margin2, uiParamSet.unitHeight, labelWidth, itemHeight);

            groupnameLabel.Align(ref align);

            align.x = margin2;
            align.width = - margin2;
            groupnameText.AlignLeft(groupnameLabel, ref align);

            align.y = margin2;
            align.width = labelWidth;
            nameLabel.AlignTop(groupnameLabel, ref align);

            align.width = - margin2;
            nameText.AlignLeftTop(nameLabel, groupnameLabel, ref align);

            align.x = margin2;
            align.width = labelWidth;
            efkLabel.AlignTop(nameLabel, ref align);

            align.width = - margin2;
            efkCombo.AlignLeftTop(efkLabel, nameLabel, ref align);

            var buttonWidth = (Width - margin6) / 4f;
            align.x = margin;
            align.width = buttonWidth;
            repeatToggle.AlignTop(efkLabel, ref align);

            align.width = buttonWidth;
            align.x = Width - buttonWidth - margin2;
            registButton.AlignTop(efkLabel, ref align);

            align.x = margin;
            playButton.AlignTop(repeatToggle, ref align);
            stopButton.AlignLeftTop(playButton, repeatToggle, ref align);
            stopRButton.AlignLeftTop(stopButton, repeatToggle, ref align);
            pauseButton.AlignLeftTop(stopRButton, repeatToggle, ref align);

            // スクロールビュー
            align.Set(0, 0, 0, 0);
            subEditView.AlignTop(playButton, ref align);

#if DEBUG
//            DebugLog(scaleSlider.Rect,  "editView slider");
#endif
        }

        /// <summary>
        /// 画面サイズが変更された時に呼び出す
        /// 位置変更では呼び出さないこと
        /// </summary>
        internal override void UpdateUISize() {
            if (!_initialized) return;
#if DEBUG
            DebugLog(titleBarRect, "editView titleBar ");
#endif
            Margin = uiParamSet.margin;
            foreach (var child in Children) {
                child.Margin = margin;
            }

            var fontSizeN = uiParamSet.fontSize;
            // var fontSizeS = uiParamSet.fontSizeS;
            // var fontSizeSS = uiParams.fontSizeSS;

            WinStyle.fontSize = fontSizeN;

            // 子要素のフォント設定
            playButton.FontSize = fontSizeN;
            stopButton.FontSize = fontSizeN;
            stopRButton.FontSize = fontSizeN;
            pauseButton.FontSize = fontSizeN;
            registButton.FontSize = fontSizeN;

            groupnameLabel.FontSize = fontSizeN;
            groupnameText.FontSize = fontSizeN;
            nameLabel.FontSize = fontSizeN;
            nameText.FontSize = fontSizeN;

            efkLabel.FontSize = fontSizeN;
            efkCombo.FontSize = fontSizeN;
            repeatToggle.FontSize = fontSizeN;

            subEditView.UpdateUISize();
        }

        #region Fields
        private static readonly char[] INVALID_PATHCHARS = Path.GetInvalidFileNameChars();
        private const int POS_OFFSETY = 10;

//        private int _frameCount;
        internal readonly RecipeManager recipeMgr;
        internal readonly EfkManager efkMgr;

        private bool _initialized;

        public CustomLabel groupnameLabel;
        public CustomTextField groupnameText;
        public CustomLabel nameLabel;
        public CustomTextField nameText;

        public CustomLabel efkLabel;
        public CustomComboBox efkCombo;

        public CustomToggle repeatToggle;

        public CustomButton playButton;
        public CustomButton stopButton;
        public CustomButton stopRButton;
        public CustomButton pauseButton;

        public CustomButton registButton;

        private EditRecipeScrollView subEditView;
        #endregion
    }
}
