using System;
using EffekseerPlayerPlugin.CM3D2.Data;
using EffekseerPlayerPlugin.Unity.Data;
using EffekseerPlayerPlugin.Unity.UI;
using EffekseerPlayerPlugin.Util;
using UnityEngine;

namespace EffekseerPlayerPlugin.CM3D2.UI {
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
            closeButton.Click += Close;

            playButton = new CustomButton(this, new GUIContent(" play", resHolder.PlayImage));
            stopButton = new CustomButton(this, new GUIContent(" stop", resHolder.StopImage));
            stopRButton = new CustomButton(this, new GUIContent(" stopR", resHolder.StopRImage));
            pauseButton = new CustomButton(this, new GUIContent(" pause", resHolder.PauseImage));
            playButton.Click += Play;
            stopButton.Click += Stop;
            stopRButton.Click += StopRoot;
            pauseButton.Click += Pause;

            allSelectToggle = new CustomToggle(this) {
                CheckStyle = new GUIStyle("label"),
                Image = ResourceHolder.Instance.CheckoffImage,
                SelectImage = ResourceHolder.Instance.CheckonImage,
//                TextColor = Color.white,
//                SelectTextColor = Color.white,
                BackgroundColor = Color.black,
                SelectBackgroundColor = Color.black,
            };
            allSelectToggle.CheckChanged += SelectItems;
            expandToggle = new CustomToggle(this) {
                Text = "展開",
                SelectText = "折り畳む",
                TextColor = Color.white,
                SelectTextColor = Color.white,
                SelectBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f),
            };
            expandToggle.CheckChanged += ExpandTree;

            deleteButton = new CustomButton(this, new GUIContent(" 削除", resHolder.DeleteImage));
            deleteButton.Click += DeleteItems;
            reloadButton = new CustomButton(this, new GUIContent(" 再読込", resHolder.ReloadImage));
            reloadButton.Click += Reload;

            editViewToggle = new CustomToggle(this) {
                Text = "<",
                SelectText = ">",
                TextColor = Color.white,
                SelectTextColor = Color.white,
                BackgroundColor = Color.green, //new Color(0.33f, 0.35f, 1f),
                SelectBackgroundColor = Color.green,//new Color(0.5f, 0.5f, 1f),
                CheckChanged = (obj, args) => { _editView.Visibled = ((CustomToggle) obj).Value; },
            };

            listView = new RecipeListView(this, _recipeMgr) {
                TextColor = Color.white,
                BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f),
                ButtonBColor = new Color(0.7f, 0.7f, 0.7f, 0.4f),
                clickRecipe = ClickRecipe,
                clickRecipeSet = ClickRecipeSet,
                deleteRecipe = DeleteRecipe,
                deleteRecipeSet = DeleteRecipeSet,
            };


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
            if (!Visibled) return;

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

            var margin2 = margin * 2;
            var margin6 = margin * 6;

//            var lineTop = Top + uiParams.unitHeight;
            var itemHeight = uiParamSet.itemHeight;
            var subButtonWidth = uiParamSet.FixPx(20f);
            var align = new Rect(margin, uiParamSet.unitHeight, subButtonWidth, itemHeight*2);
            editViewToggle.Align(ref align);

            align.y = 0f;
            align.x = Width - subButtonWidth;
            align.height = subButtonWidth;
            align.width = subButtonWidth;
            closeButton.Align(ref align);

            var buttonWidth = (Width - margin6 - subButtonWidth) / 4f;
            align.x = margin;
            align.y = uiParamSet.unitHeight;
            align.width = buttonWidth;
            align.height = itemHeight;
            playButton.AlignLeft(editViewToggle, ref align);
            stopButton.AlignLeft(playButton, ref align);
            stopRButton.AlignLeft(stopButton, ref align);
            pauseButton.AlignLeft(stopRButton, ref align);

            align.y = margin;
            deleteButton.AlignLeftTop(editViewToggle, playButton, ref align);
            reloadButton.AlignLeftTop(deleteButton, playButton, ref align);

            align.x = margin2;
            align.y = margin2;
            align.width = subButtonWidth;
            align.height = subButtonWidth;
            allSelectToggle.AlignTop(editViewToggle, ref align);
            align.x = margin;
            align.width = buttonWidth;
            expandToggle.AlignLeftTop(editViewToggle, editViewToggle, ref align);

            // filter

            //var subAreaHeight = (itemHeight+margin) *4;
            //listView.rect.Set(Left, Top + subAreaHeight, Width, Height - subAreaHeight);
            align.x = margin;
            align.y = margin;
            align.width = -margin;
            align.height = - margin;
            listView.AlignTop(allSelectToggle, ref align);
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
#if DEBUG
            DebugLog(titleBarRect, "mainView titleBar ");
#endif

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
                if (set.check == CheckStatus.Checked) {
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
            Visibled = false;
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
            if (!_editView.Visibled) return;

            _editView.SetGroupName(set.name);
            _editView.SelectRecipe(recipe);
        }

        /// <summary>
        /// クリックしたレシピセットの名前をエディタにセット
        /// </summary>
        /// <param name="recipeSet">レシピセット</param>
        public void ClickRecipeSet(RecipeSet recipeSet) {
            if (!_editView.Visibled) return;

            _editView.SetGroupName(recipeSet.name);
        }

        public void DeleteRecipe(RecipeSet set, PlayRecipe recipe) {
            _recipeMgr.Remove(set.name, recipe);
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
        public override bool Visibled {
            get { return visibled; }
            set {
                visibled = value;
                
                if (visibled) {
                    if (editViewToggle != null) {
                        _editView.Visibled = editViewToggle.Value;
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
