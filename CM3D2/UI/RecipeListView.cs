using System;
using EffekseerPlayer.CM3D2.Data;
using EffekseerPlayer.Effekseer;
using EffekseerPlayer.Unity.Data;
using EffekseerPlayer.Unity.UI;
using EffekseerPlayer.Util;
using UnityEngine;

namespace EffekseerPlayer.CM3D2.UI {
    /// <inheritdoc />
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class RecipeListView : BaseScrollView {

        public RecipeListView(GUIControl parent, RecipeManager recipeMgr) : base(parent) {
            _recipeMgr = recipeMgr;
        }

        public override void Awake() {
            if (CheckStyle == null) {
                CheckStyle = new GUIStyle("label") {
                    alignment = TextAnchor.MiddleCenter,
                };
            }
            if (LabelStyle == null) {
                LabelStyle = new GUIStyle("label") {
                    alignment = TextAnchor.MiddleLeft,
                };
                if (fontSize > 0) {
                    LabelStyle.fontSize = fontSize;
                }
                LabelStyle.hover.background = new Texture2D(2, 2);
            }
            if (ButtonAStyle == null) {
                ButtonAStyle = new GUIStyle("button") {
                    alignment = TextAnchor.MiddleCenter,
                };
                if (fontSize > 0) {
                    ButtonAStyle.fontSize = fontSize;
                }
            }
            if (ButtonBStyle == null) {
                ButtonBStyle = new GUIStyle("button") {
                    alignment = TextAnchor.MiddleCenter,
                };
                if (fontSize > 0) {
                    ButtonBStyle.fontSize = fontSize;
                }
            }
            if (_noChecked == null) {
                _noChecked = new GUIContent(ResourceHolder.Instance.CheckoffImage);
            }
            if (_checked == null) {
                _checked = new GUIContent(ResourceHolder.Instance.CheckonImage);
            }
            if (_partChecked == null) {
                _partChecked = new GUIContent(ResourceHolder.Instance.CheckpartImage);
            }

            _checks = new[] {_noChecked, _checked, _partChecked};
            if (_plus == null) {
                _plus = new GUIContent(ResourceHolder.Instance.PlusImage);
            }
            if (_minus == null) {
                _minus = new GUIContent(ResourceHolder.Instance.MinusImage);
            }
            if (_empty == null) {
                _empty = new GUIContent(ResourceHolder.Instance.FrameImage);
            }
            if (_repeat == null) {
                _repeat = new GUIContent(ResourceHolder.Instance.RepeatImage);
            }
            if (_repeatOff == null) {
                _repeatOff = new GUIContent(ResourceHolder.Instance.RepeatOffImage);
            }
            if (_delete == null) {
                _delete = new GUIContent("削除", ResourceHolder.Instance.DeleteImage);
            }
            base.Awake();
        }

        protected override void OnContentView() {
            if (!_recipeMgr.Any()) {
                labelRect.y = Top;
                GUI.Label(labelRect, "データが1件もありません", LabelStyle);
                return;
            }

            var yPos = Top;
            var space = (ItemHeight - chkRect.height) / 2;
            foreach (var recipeSet in _recipeMgr.GetRecipeSets()) {
                chkRect.y = yPos + space;
                var chk = _checks[(int) recipeSet.Check];
                if (GUI.Button(chkRect, chk, CheckStyle)) {
                    switch (recipeSet.Check) {
                    case CheckStatus.NoChecked:
                        recipeSet.Check = CheckStatus.Checked;
                        break;
                    case CheckStatus.Checked:
                        recipeSet.Check = CheckStatus.NoChecked;
                        break;
                    case CheckStatus.PartChecked:
                        recipeSet.Check = CheckStatus.NoChecked;
                        break;
                    }
                }
                expandRect.y = yPos + space;
                var expand = recipeSet.expand ? _minus : _plus;
                if (GUI.Button(expandRect, expand, CheckStyle)) {
                    recipeSet.expand = !recipeSet.expand;

                    UpdateLayout(uiParamSet);
                }
                labelRect.y = yPos;
                if (GUI.Button(labelRect, recipeSet.name, LabelStyle)) {
                    clickRecipeSet(recipeSet);
                    recipeSet.expand = !recipeSet.expand;

                    UpdateLayout(uiParamSet);
                }
                buttonRect.y = yPos;
                if (GUI.Button(buttonRect, _delete, ButtonAStyle)) {
                    deleteRecipeSet(recipeSet);

                    UpdateLayout(uiParamSet);
                    break;
                }

                yPos += ItemHeight;
                if (!recipeSet.expand) continue;

                foreach (var recipe in recipeSet.recipeList) {
                    chkRect.y = yPos + space;
                    iconRect.y = yPos + space;
                    sublabelRect.y = yPos;
                    statusRect.y = yPos;
                    buttonRect.y = yPos;

                    chk = recipe.selected ? _checked : _noChecked;
                    if (GUI.Button(chkRect, chk, CheckStyle)) {
                        CheckChanged(recipeSet, recipe);
                    }

                    var repeatCont = (recipe.Repeat) ? _repeat : _repeatOff;
                    if (GUI.Button(iconRect, repeatCont, CheckStyle)) {
                        recipe.Repeat = !recipe.Repeat;
                        // Set.dirty = true;
                        _recipeMgr.Save(recipeSet);
                    }
                    if (GUI.Button(sublabelRect, recipe.name, LabelStyle)) {
                        clickRecipe(recipeSet, recipe);
                    }

                    var cont = Status(recipe);
                    GUI.Label(statusRect, cont, LabelStyle);

                    _colorStore.SetBGColor(ref _buttonBColor);
                    try {
                        if (GUI.Button(buttonRect, _delete, ButtonBStyle)) {
                            deleteRecipe(recipeSet, recipe);

                            UpdateLayout(uiParamSet);
                            break;
                        }
                    } finally {
                        _colorStore.Restore();
                    }

                    yPos += ItemHeight;
                }
            }
        }

        private static GUIContent Status(PlayRecipe recipe) {
            switch (recipe.Status) {
            case EffekseerEmitter.EmitterStatus.Playing:
                return Playing;
            case EffekseerEmitter.EmitterStatus.Paused:
                return Paused;
            case EffekseerEmitter.EmitterStatus.Stopped:
                return Stopped;
            case EffekseerEmitter.EmitterStatus.Stopping:
                return Stopping;
            }
            return Empty;
        }

        private static readonly GUIContent Empty    = new GUIContent("Empty");
        private static readonly GUIContent Playing  = new GUIContent("Playing");
        private static readonly GUIContent Paused   = new GUIContent("Paused");
        private static readonly GUIContent Stopped  = new GUIContent("Stopped");
        private static readonly GUIContent Stopping = new GUIContent("Stopping");

        protected override void Layout(UIParamSet uiParams) {
            CalcViewHeight();

            base.Layout(uiParamSet);
            var unitSize = uiParamSet.FixPx(20);
            chkRect.width = unitSize;
            chkRect.height = unitSize;
            chkRect.x = Left;
            expandRect.width = unitSize;
            expandRect.height = unitSize;
            expandRect.x = Left + chkRect.width + margin;

            labelRect.width = Width * 0.6f;
            labelRect.height = ItemHeight;
            labelRect.x = expandRect.xMax + margin;

            iconRect.x = expandRect.xMax - margin;
            iconRect.width = unitSize;
            iconRect.height = unitSize;

            sublabelRect.width = Width * 0.6f;
            sublabelRect.height = ItemHeight;
            sublabelRect.x = iconRect.xMax + margin;

            buttonRect.width = uiParamSet.FixPx(70f);
            buttonRect.height = ItemHeight;
            buttonRect.x = xMax - buttonRect.width - margin * 2 - WIDTH_SCROLLBAR;

            statusRect.width = uiParamSet.FixPx(80f);
            statusRect.height = ItemHeight;
            statusRect.x = buttonRect.x - margin * 2 - statusRect.width;
        }

        // ビューの高さを計算して更新する.
        // 要素数が変更された時やツリーの展開状態を変更した時に、呼び出す必要がある.
        internal void CalcViewHeight() {
            var recipeSets = _recipeMgr.GetRecipeSets();
            var count = recipeSets.Count;

            foreach (var recipeSet in recipeSets) {
                if (recipeSet.expand) {
                    count += recipeSet.Size();
                }
            }
            viewHeight = count * ItemHeight;
            Log.Debug("CalcViewHeight:", viewHeight, ", count:", count);
        }

        public override float GetViewHeight() {
            return viewHeight;
        }

        private readonly RecipeManager _recipeMgr;
        protected float viewHeight;

        private GUIContent[] _checks;
        private GUIContent _checked;
        private GUIContent _noChecked;
        private GUIContent _partChecked;
        private GUIContent _plus;
        private GUIContent _minus;
        private GUIContent _empty;
        private GUIContent _repeat;
        private GUIContent _repeatOff;
        private GUIContent _delete;

        protected Rect chkRect;
        protected Rect expandRect;
        protected Rect labelRect;
        protected Rect iconRect;
        protected Rect sublabelRect;
        protected Rect statusRect;
        protected Rect buttonRect;
        private readonly GUIColorStore _colorStore = new GUIColorStore();

        private Color? _buttonBColor;
        public Color? ButtonBColor {
            get { return _buttonBColor;}
            set { _buttonBColor = value; }
        }
        public GUIStyle CheckStyle { get; set; }
        public GUIStyle ButtonAStyle { get; set; }
        public GUIStyle ButtonBStyle { get; set; }
        public GUIStyle LabelStyle { get; set; }
        public override int FontSize {
            set {
                fontSize = value;
                if (CheckStyle != null) CheckStyle.fontSize = fontSize;
                if (ButtonAStyle != null) ButtonAStyle.fontSize = fontSize;
                if (ButtonBStyle != null) ButtonBStyle.fontSize = fontSize;
                if (LabelStyle != null) LabelStyle.fontSize = fontSize;
            }
        }
        public virtual float ItemHeight { get; set; }

        private void CheckChanged(RecipeSet set, PlayRecipe recipe) {
            recipe.selected = !recipe.selected;
            switch (set.Check) {
            case CheckStatus.NoChecked:
                if (recipe.selected) set.Check = CheckStatus.PartChecked;
                break;
            case CheckStatus.Checked:
                if (!recipe.selected) set.Check = CheckStatus.PartChecked;
                break;
            case CheckStatus.PartChecked:
                if (set.recipeList.FindIndex(rcp => rcp.selected != recipe.selected) == -1) {
                    set.Check = recipe.selected ? CheckStatus.Checked : CheckStatus.NoChecked;
                }
                break;
            }
        }
        public EventHandler SelectChanged = delegate { };
        public Action<RecipeSet> clickRecipeSet = set => { };
        public Action<RecipeSet, PlayRecipe> clickRecipe = (set, recipe) => { };
        public Action<RecipeSet> deleteRecipeSet = set => { };
        public Action<RecipeSet, PlayRecipe> deleteRecipe = (set, recipe) => { };
    }
}
