using System;
using EffekseerPlayer.CM3D2.Data;
using EffekseerPlayer.Unity.Data;
using EffekseerPlayer.Unity.UI;
using EffekseerPlayer.Util;
using UnityEngine;

namespace EffekseerPlayer.CM3D2.UI {
    /// <inheritdoc />
    /// <summary>
    /// メインビュー
    /// ファイルからロードされたレシピリストと、プレイ管理を行うビュー.
    /// </summary>
    public class MainPlayView : BaseWindow {
        public MainPlayView(UIParamSet uiParamSet, EditRecipeView editView, 
            RecipeManager recipeMgr, PlayManager playMgr) : base(uiParamSet) {
            uiParamSet.AddListener(WinResized);
            _editView = editView;
            _recipeMgr = recipeMgr;
            _playMgr = playMgr;
        }

        ~MainPlayView() {
            uiParamSet.Remove(WinResized);
        }

        /// <summary>ウィンドウを初期化する</summary>
        public override void Awake() {
            try {
                if (!InitControls()) return;

                AwakeChildren();
                WinResized(uiParamSet);

            } catch (Exception e) {
                Log.Error("failed to awake MainPlayView", e);
                throw;
            }
        }

        internal override void InitPos() {
            Left = Screen.width - uiParamSet.FixPx(420);
            Top = uiParamSet.FixPx(74);
        }

        internal override void InitSize() {
            Width = uiParamSet.FixPx(400);
            Height = Screen.height - uiParamSet.FixPx(200);
            if (Height > 1000) {
                Height = 1000;
            }
#if DEBUG
            DebugLog(Rect, "mainView rect ");
#endif
        }

        private bool InitControls() {
            if (_initialized) return false;

            // ウィンドウスタイル
            if (WinStyle == null) {
                WinStyle = new GUIStyle("box") {
                    alignment = TextAnchor.UpperLeft
                };
                WinStyle.normal.textColor = WinStyle.onNormal.textColor =
                WinStyle.hover.textColor = WinStyle.onHover.textColor =
                WinStyle.active.textColor = WinStyle.onActive.textColor =
                WinStyle.focused.textColor = WinStyle.onFocused.textColor = Color.white;
            }

            if (VerStyle == null) {
                VerStyle = new GUIStyle("label") {
                    alignment = TextAnchor.MiddleRight
                };
            }

            var resHolder = ResourceHolder.Instance;
            closeButton = new CustomButton(this, "×") {
                ButtonStyle = new GUIStyle("button") {
                    alignment = TextAnchor.MiddleCenter,
                    contentOffset = new Vector2(0, 0),
                },
            };

            playButton = new CustomButton(this, new GUIContent(" play", resHolder.PlayImage));
            stopButton = new CustomButton(this, new GUIContent(" stop", resHolder.StopImage));
            stopRButton = new CustomButton(this, new GUIContent(" stopR", resHolder.StopRImage));
            pauseButton = new CustomButton(this, new GUIContent(" pause", resHolder.PauseImage));

            allSelectToggle = new CustomToggle(this) {
                CheckStyle = new GUIStyle("label"),
                Image = ResourceHolder.Instance.CheckoffImage,
                SelectImage = ResourceHolder.Instance.CheckonImage,
//                TextColor = Color.white,
//                SelectTextColor = Color.white,
                BackgroundColor = Color.black,
                SelectBackgroundColor = Color.black,
            };
            expandToggle = new CustomToggle(this) {
                Text = "展開",
                SelectText = "折り畳む",
                TextColor = Color.white,
                SelectTextColor = Color.white,
                SelectBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f),
            };

            deleteButton = new CustomButton(this, new GUIContent(" 削除", resHolder.DeleteImage));
            reloadButton = new CustomButton(this, new GUIContent(" 再読込", resHolder.ReloadImage));

            editViewToggle = new CustomToggle(this) {
                Text = "<",
                SelectText = ">",
                TextColor = Color.white,
                SelectTextColor = Color.white,
                BackgroundColor = Color.green, //new Color(0.33f, 0.35f, 1f),
                SelectBackgroundColor = Color.green,//new Color(0.5f, 0.5f, 1f),
            };

            listView = new RecipeListView(this, _recipeMgr) {
                TextColor = Color.white,
                BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f),
                ButtonBColor = new Color(0.7f, 0.7f, 0.7f, 0.4f),
            };

            //----------------------------------------
            // イベント処理
            //----------------------------------------
            playButton.Click += Play;
            stopButton.Click += Stop;
            stopRButton.Click += StopRoot;
            pauseButton.Click += Pause;

            closeButton.Click += Close;

            deleteButton.Click += DeleteItems;
            reloadButton.Click += Reload;
            allSelectToggle.CheckChanged += SelectItems;
            expandToggle.CheckChanged += ExpandTree;

            editViewToggle.CheckChanged = (obj, args) => { _editView.Visible = ((CustomToggle) obj).Value; };
            listView.clickRecipe     = ClickRecipe;
            listView.clickRecipeSet  = ClickRecipeSet;
            listView.deleteRecipe    = DeleteRecipe;
            listView.deleteRecipeSet = DeleteRecipeSet;

//            filterText = new CustomTextField(this);
//            filterText.ValueChanged += (obj, args) => {
//                // TODO filter list
//            };

            _initialized = true;
            return true;
        }

        public override void Update() {
//            if (!_initialized) return;
        }

        public override void OnGUI() {
            if (!Visible) return;

            try {
                GUI.Box(Rect, Text, WinStyle);
                GUI.Label(titleBarRect, _version, VerStyle);
                closeButton.OnGUI();
                playButton.OnGUI();
                stopButton.OnGUI();
                stopRButton.OnGUI();
                pauseButton.OnGUI();

                allSelectToggle.OnGUI();
                expandToggle.OnGUI();
                editViewToggle.OnGUI();
                deleteButton.OnGUI();
                reloadButton.OnGUI();

                listView.OnGUI();

            } catch (Exception e) {
                Log.Error(e);
            }
        }

        protected override void Layout(UIParamSet uiParams) {
            if (!_initialized) return;
            Margin = uiParams.margin;

            var margin2 = margin * 2;
            var margin6 = margin * 6;

            var itemHeight = uiParamSet.itemHeight;
            var subButtonWidth = uiParamSet.FixPx(20f);
            var topLayout = new AttachData(null, 0f, subButtonWidth);
            var rightLayout = new AttachData(null, 0f, subButtonWidth);
            var formData = new FormData(null, topLayout) { Right = rightLayout };
            closeButton.Align(formData);

            var leftLayout = new AttachData(null, margin, subButtonWidth);
            formData.Left = leftLayout;
            formData.Top.offset = uiParams.unitHeight;
            formData.Top.length = itemHeight * 2;
            formData.Right = null;
            editViewToggle.Align(formData);

            var buttonWidth = (Width - margin6 - subButtonWidth) / 4f;
            formData.Left.Set(editViewToggle, margin, buttonWidth);
            formData.Top.length = itemHeight;
            playButton.Align(formData);
            stopButton.AlignLeft(playButton, formData);
            stopRButton.AlignLeft(stopButton, formData);
            pauseButton.AlignLeft(stopRButton, formData);

            formData.Left.obj = editViewToggle;
            formData.Top.Set(playButton, margin, itemHeight);
            deleteButton.Align(formData);
            reloadButton.AlignLeft(deleteButton, formData);

            formData.Left.Set(null, margin2, subButtonWidth);
            formData.Top.Set(editViewToggle, margin2, subButtonWidth);
            allSelectToggle.Align(formData);

            formData.Left.Set(editViewToggle, margin, buttonWidth);
            expandToggle.Align(formData);

            // filter
            formData.Left.Set(null, margin);
            formData.Right = rightLayout;
            formData.Right.Set(null, margin);
            formData.Top.Set(allSelectToggle, margin);
            formData.Bottom = new AttachData(margin);
            listView.Align(formData);

#if DEBUG
            DebugLog(listView.Rect,  "mainView list ");
#endif
        }

        /// <inheritdoc />
        /// <summary>
        /// 画面サイズが変更された時に呼び出す
        /// 位置変更では呼び出さないこと
        /// </summary>
        internal override void UpdateUISize() {
            if (!_initialized) return;

            Margin = uiParamSet.margin;
            foreach (var child in Children) {
                child.Margin = margin;
            }

            var fontSizeL = uiParamSet.fontSizeL;
            var fontSizeN = uiParamSet.fontSize;
            var fontSizeS = uiParamSet.fontSizeS;
//            var fontSizeSS = uiParams.fontSizeSS;

            WinStyle.fontSize = fontSizeN;
            VerStyle.fontSize = fontSizeN;

            // 子要素のフォント設定
//            filterText.FontSize = fontSizeN;
            closeButton.FontSize = fontSizeL;
            playButton.FontSize = fontSizeN;
            stopButton.FontSize = fontSizeN;
            stopRButton.FontSize = fontSizeN;
            pauseButton.FontSize = fontSizeN;
            allSelectToggle.FontSize = fontSizeN;
            expandToggle.FontSize = fontSizeS;
            editViewToggle.FontSize = fontSizeN;
            deleteButton.FontSize = fontSizeN;
            reloadButton.FontSize = fontSizeN;

            listView.FontSize = fontSizeN;
            listView.ItemHeight = uiParamSet.itemHeight;
        }

        #region Event
        public void SelectItems(object obj, EventArgs args) {
            var selectButton = (CustomToggle)obj;
            var status = (selectButton.Value) ? CheckStatus.Checked : CheckStatus.NoChecked;
            foreach (var set in _recipeMgr.GetRecipeSets()) {
                set.Check = status;
            }
        }

        public void ExpandTree(object obj, EventArgs args) {
//            CustomToggle expandButton = (CustomToggle)obj;
            foreach (var set in _recipeMgr.GetRecipeSets()) {
                set.expand = expandToggle.Value;
            }
            listView.UpdateLayout(uiParamSet);
        }

        public void DeleteItems(object obj, EventArgs args) {
            // TODO 削除確認 ファイル()が削除されます。よろしいですか？
//            var selectButton = (CustomToggle)obj;
            foreach (var set in _recipeMgr.GetRecipeSets()) {
                if (set.Check == CheckStatus.Checked) {
                    _recipeMgr.RemoveSet(set.name, true);
                    
                } else {
                    var deleted = false;
                    foreach (var recipe in set.recipeList) {
                        if (!recipe.selected) continue;

                        _recipeMgr.Remove(set.name, recipe);
                        deleted = true;
                    }
                    if (deleted) {
                        _recipeMgr.Save(set);
                    }
                }
            }

            // 削除後にリストビューのレイアウトを更新
            listView.UpdateLayout(uiParamSet);
        }

        public void Close(object obj, EventArgs args) {
            Visible = false;
        } 

        public void Play(object obj, EventArgs args) {
            SelectedExec(recipe => _playMgr.Play(recipe));
        }

        public void Stop(object obj, EventArgs args) {
            SelectedExec(recipe => _playMgr.Stop(recipe));
        }

        public void StopRoot(object obj, EventArgs args) {
            SelectedExec(recipe => _playMgr.StopRoot(recipe.RecipeId));
        }

        public void Pause(object obj, EventArgs args) {
            SelectedExec(recipe => _playMgr.Pause(recipe.RecipeId));
        }

        private void SelectedExec(Action<PlayRecipe> exec) {
            foreach (var set in _recipeMgr.GetRecipeSets()) {
                foreach (var recipe in set.recipeList) {
                    if (recipe.selected) exec(recipe);
                }
            }
        }

        public void Reload(object obj, EventArgs args) {
            _recipeMgr.Reload();
            listView.UpdateLayout(uiParamSet);
        }

        /// <summary>
        /// クリックしたレシピの情報をエディタにセット
        /// </summary>
        /// <param name="set">レシピセット</param>
        /// <param name="recipe">レシピ</param>
        public void ClickRecipe(RecipeSet set, PlayRecipe recipe) {
            if (!_editView.Visible) return;

            _editView.SetGroupName(set.name);
            _editView.ToEditView(recipe);
        }

        /// <summary>
        /// クリックしたレシピセットの名前をエディタにセット
        /// </summary>
        /// <param name="recipeSet">レシピセット</param>
        public void ClickRecipeSet(RecipeSet recipeSet) {
            if (!_editView.Visible) return;

            _editView.SetGroupName(recipeSet.name);
        }

        public void DeleteRecipe(RecipeSet set, PlayRecipe recipe) {
            _recipeMgr.Remove(set.name, recipe, true);
            listView.UpdateLayout(uiParamSet);
        }

        public void DeleteRecipeSet(RecipeSet recipeSet) {
            _recipeMgr.RemoveSet(recipeSet, true);
            listView.UpdateLayout(uiParamSet);
        }

        #endregion

        #region Properties
        public GUIStyle VerStyle { get; set; }
        private string _version = string.Empty;
        internal string Version {
            set { _version = value; }
        }
        public override bool Visible {
            get { return visible; }
            set {
                visible = value;
                
                if (visible) {
                    if (editViewToggle != null) {
                        _editView.Visible = editViewToggle.Value;
                    }
                    return;
                }

                CloseAction();
            }
        }
        #endregion

        #region Fields
        private bool _initialized;
        private readonly RecipeManager _recipeMgr;
        private readonly PlayManager _playMgr;
        private readonly EditRecipeView _editView;

//        public CustomTextField filterText;
        protected CustomButton closeButton;

        protected CustomButton playButton;
        protected CustomButton stopButton;
        protected CustomButton stopRButton;
        protected CustomButton pauseButton;

        protected CustomToggle allSelectToggle;
        protected CustomToggle expandToggle;
        protected CustomButton reloadButton;
        protected CustomButton deleteButton;
        protected CustomToggle editViewToggle;
        protected RecipeListView listView;

        #endregion
    }
}
