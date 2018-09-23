using System;
using System.Collections.Generic;
using System.IO;
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
                Top  = parentWin.Top + uiParamSet.FixPx(POS_OFFSET_Y);
            } else {
                Left = Screen.width - uiParamSet.FixPx(820);
                Top  = uiParamSet.FixPx(80);
            }
        }

        internal override void InitSize() {
            Width  = uiParamSet.FixPx(460);
            Height = uiParamSet.FixPx(860);
            if (parentWin != null) {
                var deltaY = uiParamSet.FixPx(POS_OFFSET_Y);
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

            groupNameLabel = new CustomLabel(this, "グループ名:");
            groupNameText = new CustomTextField(this);

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

            registButton = new CustomButton(this, "register") {Enabled = false};
            playButton = new CustomButton(this, new GUIContent(" play", resHolder.PlayImage));
            stopButton = new CustomButton(this, new GUIContent(" stop", resHolder.StopImage));
            stopRButton = new CustomButton(this, new GUIContent(" stopRoot", resHolder.StopRImage));
            pauseButton = new CustomButton(this, new GUIContent(" pause", resHolder.PauseImage));
            EnablePlayButtons(false);

            //------------------------------------------
            // イベント処理
            groupNameText.ValueChanged += (obj, args) => {
                var hasError = groupNameText.Text.IndexOfAny(INVALID_PATH_CHARS) != -1;
                groupNameText.hasError = hasError;
            };
            groupNameText.ValueChanged += CheckValidate;

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

            if (!Visible) return;

            subEditView.UpdateSlider();
        }

        public override void OnGUI() {
            if (!Visible) return;

            try {
                GUI.Box(Rect, Text, WinStyle);
                //GUI.Label(titleBarRect, _version, VerStyle);
                groupNameLabel.OnGUI();
                groupNameText.OnGUI();
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

            var formData = new FormData(margin2, uiParamSet.unitHeight) {
                Width = labelWidth, Height = itemHeight
            };
            groupNameLabel.Align(formData);

            formData.Left.obj = groupNameLabel;// margin2, labelWidth
            var rightLayout = new AttachData(margin2);
            formData.Right = rightLayout;
            groupNameText.Align(formData);

            formData.Left.obj = null;  // margin2, labelWidth
            formData.Right = null;
            formData.Top.Set(groupNameLabel, margin2, itemHeight);
            nameLabel.Align(formData);

            formData.Left.obj = nameLabel;
            formData.Right = rightLayout;
            nameText.Align(formData);

            formData.Left.obj = null; // margin2, labelWidth
            formData.Top.obj = nameLabel;
            formData.Right = null;
            efkLabel.Align(formData);

            formData.Left.obj = efkLabel;
            formData.Top.obj = nameLabel;
            formData.Right = rightLayout;
            efkCombo.Align(formData);

            var buttonWidth = (Width - margin6) / 4f;
            formData.Left.Set(null, margin, buttonWidth);
            formData.Right = null;
            formData.Top.obj = efkLabel;
            repeatToggle.Align(formData);

            var leftLayout = formData.Left;
            formData.Left = null;
            formData.Right = rightLayout;
            formData.Right.length = buttonWidth;
            formData.Top.obj = efkLabel;
            registButton.Align(formData);

            formData.Left = leftLayout;
            formData.Right = null;
            formData.Top.obj = repeatToggle;
            playButton.Align(formData);
            stopButton.AlignLeft(playButton, formData);
            stopRButton.AlignLeft(stopButton, formData);
            pauseButton.AlignLeft(stopRButton, formData);

            // スクロールビュー
            formData.Left.Set(null, 0);
            formData.Right = rightLayout;
            formData.Right.Set(null, 0);
            formData.Top.Set(playButton, margin, 0);
            formData.Bottom = new AttachData(0);
            subEditView.Align(formData);

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

            groupNameLabel.FontSize = fontSizeN;
            groupNameText.FontSize = fontSizeN;
            nameLabel.FontSize = fontSizeN;
            nameText.FontSize = fontSizeN;

            efkLabel.FontSize = fontSizeN;
            efkCombo.FontSize = fontSizeN;
            repeatToggle.FontSize = fontSizeN;

            subEditView.UpdateUISize();
        }

        #region Fields
        private static readonly char[] INVALID_PATH_CHARS = Path.GetInvalidFileNameChars();
        private const int POS_OFFSET_Y = 10;

//        private int _frameCount;
        internal readonly RecipeManager recipeMgr;
        internal readonly EfkManager efkMgr;

        private bool _initialized;

        public CustomLabel groupNameLabel;
        public CustomTextField groupNameText;
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
