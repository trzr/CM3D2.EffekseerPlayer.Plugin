using System;
using System.Collections.Generic;
using UnityEngine;

namespace EffekseerPlayerPlugin.Unity.UI {
    /// <inheritdoc />
    /// <summary>
    /// コンボボックス
    /// </summary>
    public sealed class CustomComboBox : GUIControl {
        #region Methods
        /// <summary>デフォルトコンストラクタ</summary>
        public CustomComboBox(GUIControl parent):this (parent, new GUIContent[0]) { }

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="items">コンボボックスアイテム</param>
        public CustomComboBox(GUIControl parent, GUIContent[] items) : base (parent) {
            IsShowDropDownList = false;
            if (items == null) throw new NullReferenceException("items is null");

            _id = UIParamSet.NewWindowId();
            AllComboBoxes[_id] = this;
            _items = items;
        }

        public override void Awake() {
            // ボタンスタイル
            if (ButtonStyle == null) {
                ButtonStyle = new GUIStyle("button") {
                    alignment = TextAnchor.MiddleLeft
                };
                if (fontSize > 0) {
                    ButtonStyle.fontSize = fontSize;
                }
                ButtonStyle.normal.textColor  = ButtonStyle.onNormal.textColor  =
                ButtonStyle.hover.textColor   = ButtonStyle.onHover.textColor   =
                ButtonStyle.active.textColor  = ButtonStyle.onActive.textColor  =
                ButtonStyle.focused.textColor = ButtonStyle.onFocused.textColor = Color.white;
            }

            // コンボリストスタイル
            if (ListStyle == null) {
                ListStyle = new GUIStyle {
                    alignment = TextAnchor.MiddleLeft
                };
                if (fontSize > 0) {
                    ListStyle.fontSize = fontSize;
                }
                ListStyle.onHover.background = ListStyle.hover.background = new Texture2D(2, 2);
                ListStyle.padding.left = ListStyle.padding.right = ListStyle.padding.top = ListStyle.padding.bottom = 4;
                ListStyle.normal.textColor  = ListStyle.onNormal.textColor  =
                ListStyle.hover.textColor   = ListStyle.onHover.textColor   =
                ListStyle.active.textColor  = ListStyle.onActive.textColor  =
                ListStyle.focused.textColor = ListStyle.onFocused.textColor = Color.white;
            }

            // ドロップダウンリスト スタイル
            if (DropDownStyle != null) return;

            DropDownStyle = new GUIStyle("box") {
                alignment = TextAnchor.MiddleLeft
            };
            if (fontSize > 0) {
                DropDownStyle.fontSize = fontSize;
            }
        }

        protected override void Layout(UIParamSet uiParams) {
            _dropDownListRect.Set(Left, Top + Height - offset.y, Width, Height);
            _listRect.width = _dropDownListRect.width;     
            _posRect.width  = _dropDownListRect.width;
            _viewRect.width = _dropDownListRect.width - 20;

            //_maxHeight = root.rect.height - root.rect.y;//(float)(Screen.height - Screen.height * 0.15 - uiparams.winRect.y);
            MaxHeight = root.Rect.height - (Top - offset.y - root.Top) - margin;
            if (MaxHeight < 0) MaxHeight = 0;
        }
    
        public override void OnGUI() {
            if (IsShowDropDownList || !Enabled)  {
                enabledStore.SetEnabled(IsShowDropDownList || Enabled);
            }
            if (hasError) _colorSetter.SetBGColor(ref errorColor);
            try {
                // 現在選択中の項目をボタンに表示
                GUIContent comboBoxButton;
                if (0 <= _selectedIdx && _selectedIdx < Count) {
                    comboBoxButton = _items[_selectedIdx];
                } else {
                    comboBoxButton = Empty;
                }

                if (GUI.Button(Rect, comboBoxButton, ButtonStyle)) {
                    // クローズ処理はOnGUI_DropDownList内で行う
                    if (!_closing) {
                        IsShowDropDownList = !IsShowDropDownList;
                    }

                    CloseAllDropDownList(_id);
                    return;
                }

                _closing = false;
                // ドロップダウンリスト表示の場合
                if (!IsShowDropDownList) return;

                var listHeight = ListStyle.CalcHeight(comboBoxButton, 1.0f) * (_items.Length);
                _dropDownListRect.height =  MaxHeight < listHeight ? MaxHeight : listHeight;
                _posRect.height = _dropDownListRect.height;
                _viewRect.height = listHeight;
                _listRect.height = listHeight;

                //                dropDownListRect.x = rect.x + uiParams.winRect.x;
                //                dropDownListRect.y = rect.yMax + uiParams.winRect.y;
                
                GUI.Window(_id, _dropDownListRect, OnGUI_DropDownList, string.Empty, DropDownStyle);
            } catch (Exception e) {
                Log.Debug(e);
            } finally {
                _colorSetter.Restore();
                enabledStore.Restore();
            }
        }

        private void OnGUI_DropDownList(int windowID) {
            try {
                // スクロールビュー開始
                _scrollViewVector = GUI.BeginScrollView(_posRect, _scrollViewVector, _viewRect);
                try {
                    // グリッド表示
                    var newSelectedIdx = GUI.SelectionGrid(_listRect, _selectedIdx, _items, 1, ListStyle);
                    if (newSelectedIdx == _selectedIdx) {
                        // 前回選択したアイテムの選択操作によるクローズ判断のため、マウス左クリック(down->up)の挙動をチェック
                        //  upだけの場合、DropDownListのオープン時にも引っかかるためdownとのセットとして判定
                        if (Input.GetMouseButtonDown(0)) {
                            _mouseDowned = true;
                            return;
                        } else if (!_mouseDowned || !Input.GetMouseButtonUp(0)) return;
                        _closing = true; // 次フレームのGUI.Buttonの反応を抑制するためのフラグ

                    } else {
                        Log.Debug("DropDown Selected:", _selectedIdx, "=>", newSelectedIdx);
                        // グリッド内の項目が選択されたので、選択Indexを更新し、ドロップダウンリストを閉じる
                        SelectedIndex = newSelectedIdx;
                    }
                    IsShowDropDownList = false;
                    _mouseDowned = false;

                } finally {
                    GUI.EndScrollView();
                }
            } catch (Exception e) {
                Log.Error("failed to draw drop-down-list", e);
            }
        }

        /// <summary>選択位置を次の項目に移動。末尾の場合は先頭へ移動</summary>
        /// <returns>移動後のインデックス</returns>
        public int Next() {
            if (0 >= _items.Length) return _selectedIdx;

            if (_selectedIdx + 1 < _items.Length) {
                SelectedIndex++;
            } else {
                SelectedIndex = 0;
            }

            return _selectedIdx;
        }

        /// <summary>選択位置を前の項目に移動。先頭の場合は末尾へ移動</summary>
        /// <returns>移動後のインデックス</returns>
        public int Prev() {
            if (0 >= _items.Length) return _selectedIdx;

            if (1 <= _selectedIdx) {
                SelectedIndex--;
            } else {
                SelectedIndex = _items.Length - 1;
            }

            return _selectedIdx;
        }

        public static void CloseAllDropDownList() {
            try {
                CloseAllDropDownList(-1);
            } catch (Exception e) {
                Log.Error(e);
            }
        }

        public static void CloseAllDropDownList(int ignoreID) {
            try {
                foreach (var combo in AllComboBoxes.Values) {
                    if (combo.ID != ignoreID) {
                        combo.IsShowDropDownList = false;
                    }
                }
            } catch (Exception e) {
                Log.Error(e);
            }
        }
        #endregion

        #region Properties
        private readonly int _id;
        public int ID {
            get { return _id; }
        }
        public override int FontSize {
            set {
                fontSize = value;
                if (ButtonStyle == null) return;

                ButtonStyle.fontSize = fontSize;
                ListStyle.fontSize = fontSize;
                DropDownStyle.fontSize = fontSize;
            }
        }
        private GUIContent[] _items;
        public GUIContent[] Items {
            get { return _items; }
            set {
                var prevItem = SelectedItem;
                var notSelected = _selectedIdx == -1;
                _items = value;
                _selectedIdx = -1;

                if (notSelected || prevItem.Length == 0) return;

                // 既に選択済のアイテムをサーチし再選択
                for (var i = 0; i < _items.Length; i++) {
                    if (_items[i].text != prevItem) continue;

                    _selectedIdx = i;
                    return;
                }
                // 既に選択済のアイテムがなかった場合、未選択状態になるため、変更通知
                if (Enabled) SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        private int _selectedIdx = -1;
        public int SelectedIndex {
            get { return _selectedIdx; }
            set {
                if (_selectedIdx == value) return;
                _selectedIdx = value;

                if (Enabled) SelectedIndexChanged(this, EventArgs.Empty);
            }
        }
        /// <summary>通知なしでset</summary>
        public int Index {
            set { _selectedIdx = value; }
        }

        public string SelectedItem {
            get {
                if (0 <= _selectedIdx && _selectedIdx < _items.Length) {
                    return  _items[_selectedIdx].text;
                }
                return string.Empty;
            }

            set {
                // 指定された項目を検索
                var index = Array.FindIndex(_items, item => item.text == value );
                if (0 <= index) {
                    // 選択位置切り替え
                    SelectedIndex = index;
                }
            }
        }

        public int Count {
            get { return _items.Length; }
        }

        public bool IsShowDropDownList { get; set; }

        private float MaxHeight { get; set; }

        public GUIStyle ButtonStyle   { get; set; }
        public GUIStyle ListStyle     { get; set; }
        public GUIStyle DropDownStyle { get; set; }
        #endregion

        #region Fields
        private static readonly GUIContent Empty = new GUIContent(string.Empty);
        private static readonly Dictionary<int, CustomComboBox> AllComboBoxes = new Dictionary<int, CustomComboBox>();

        public bool hasError;
        public Color errorColor = UIParamSet.ErrorColor;
        private readonly GUIColorStore _colorSetter = new GUIColorStore();

        private Vector2 offset; 
        public Vector2 Offset {
            get { return offset;}
            set {
                offset = value;
                Layout(uiParamSet);
            }
        }
        private Vector2 _scrollViewVector = Vector2.zero;
        private bool _mouseDowned;
        private bool _closing;
        // 各座標
        private Rect _dropDownListRect;
        private Rect _posRect;
        private Rect _viewRect;
        private Rect _listRect;
        #endregion

        public EventHandler SelectedIndexChanged = delegate { };
    }
}