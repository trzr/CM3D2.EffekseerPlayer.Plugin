using System;
using System.Collections.Generic;
using EffekseerPlayerPlugin.CM3D2.Data;
using EffekseerPlayerPlugin.CM3D2.Render;
using EffekseerPlayerPlugin.CM3D2.Util;
using EffekseerPlayerPlugin.Unity.Data;
using EffekseerPlayerPlugin.Unity.UI;
using EffekseerPlayerPlugin.Util;
using UnityEngine;

namespace EffekseerPlayerPlugin.CM3D2.UI {
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
            if (_currentEmitter == null) return;

            _currentEmitter.Stop();
            UnityEngine.Object.Destroy(_currentEmitter);
            _currentEmitter = null;
        }

        /// <summary>ウィンドウを初期化する</summary>
        public override void Awake() {
            try {
                if (!InitControls()) return;
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
                Top = parentWin.Top + uiParamSet.FixPx(20);
            } else {
                Left = Screen.width - uiParamSet.FixPx(820);
                Top = uiParamSet.FixPx(80);
            }
        }

        internal override void InitSize() {
            Width = uiParamSet.FixPx(460);
//            Height = Screen.height - uiParamSet.FixPx(200);
            Height = uiParamSet.FixPx(840);
            if (Height > 900) {
                Height = 900;
            }
#if DEBUG
            DebugLog(Rect, "editView rect  ");
#endif
        }

        private bool InitControls() {
            if (_initialized) return false;

            var settings = Settings.Instance;
            var scaleRange = new EditRange(5, 0.00001f, settings.maxScale);
            var speedRange = new EditRange(5, 0.0001f, settings.maxSpeed);
            var posRange = new EditRange(5, -settings.maxLocation, settings.maxLocation);

            _boneRenderer = new CustomBoneRenderer();

            Children = new List<GUIControl>(30);
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
            groupnameText.ValueChanged += (obj, args) => {
                var hasError = groupnameText.Text.IndexOfAny(INVALID_PATHCHARS) != -1;
                groupnameText.hasError = hasError;
            };
            groupnameText.ValueChanged += CheckValidate;

            nameLabel = new CustomLabel(this, "名前:");
            nameText = new CustomTextField(this);
            nameText.ValueChanged += CheckValidate;
            efkLabel = new CustomLabel(this, "effekseer:");

            // efkコンボボックス
            efkCombo = new CustomComboBox(this, CreateEfkItems());
            efkCombo.SelectedIndexChanged += CheckValidate;

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
            registButton.Click += Register;

            playButton = new CustomButton(this, new GUIContent(" play", resHolder.PlayImage));
            playButton.Click += Play;

            stopButton = new CustomButton(this, new GUIContent(" stop", resHolder.StopImage));
            stopButton.Click += Stop;

            stopRButton = new CustomButton(this, new GUIContent(" stopRoot", resHolder.StopRImage));
            stopRButton.Click += StopRoot;

            pauseButton = new CustomButton(this, new GUIContent(" pause", resHolder.PauseImage));
            pauseButton.Click += Pause;
            EnablePlayButtons(false);

            scaleSlider = new CustomTextLogSlider(this, 1.0f, scaleRange) {
                Text = "◆スケール",
            };
            scaleSlider.Value.ValueChanged += ScaleChanged;
            scaleSlider.listeners = new[] {
                new PresetListener<EditTextLogValue>("0.01", 40, val => val.Set(0.01f, true)),
                new PresetListener<EditTextLogValue>("0.05", 40, val => val.Set(0.05f, true)),
                new PresetListener<EditTextLogValue>("0.1", 40, val => val.Set(0.1f, true)),
                new PresetListener<EditTextLogValue>("0.5", 40, val => val.Set(0.5f, true)),
                new PresetListener<EditTextLogValue>("1", 20, val => val.Set(1, true)),
            };
            scaleSlider.prevListeners = new[] {
                new PresetListener<EditTextLogValue>("<<", 30, val => val.Multiply(0.1f, true)),
                new PresetListener<EditTextLogValue>("<", 20, val => val.Multiply(0.5f, true)),
            };
            scaleSlider.nextListeners = new[] {
                new PresetListener<EditTextLogValue>(">", 20, val => val.Multiply(2f, true)),
                new PresetListener<EditTextLogValue>(">>", 30, val => val.Multiply(10f, true)),
            };
            speedSlider = new CustomTextLogSlider(this, 1.0f, speedRange) {
                Text = "◆再生速度",
            };
            speedSlider.Value.ValueChanged += SpeedChanged;
            speedSlider.listeners = new[] {
                new PresetListener<EditTextLogValue>("0.05", 40, val => val.Set(0.05f, true)),
                new PresetListener<EditTextLogValue>("0.1", 40, val => val.Set(0.1f, true)),
                new PresetListener<EditTextLogValue>("0.5", 40, val => val.Set(0.5f, true)),
                new PresetListener<EditTextLogValue>("1", 20, val => val.Set(1f, true)),
                new PresetListener<EditTextLogValue>("2", 20, val => val.Set(2f, true)),
            };
            speedSlider.prevListeners = new[] {
                new PresetListener<EditTextLogValue>("<<", 30, val => val.Multiply(0.1f, true)),
                new PresetListener<EditTextLogValue>("<", 20, val => val.Multiply(0.5f, true)),
            };
            speedSlider.nextListeners = new[] {
                new PresetListener<EditTextLogValue>(">", 20, val => val.Multiply(2f, true)),
                new PresetListener<EditTextLogValue>(">>", 30, val => val.Multiply(10f, true)),
            };
            colorSlider = new CustomTextSliders(this,
                new[] { "R", "G", "B", "A" }, 
                new[] { 1f, 1f, 1f, 1f }, 
                new[] { RGB_RANGE,RGB_RANGE,RGB_RANGE,RGB_RANGE,}) {
                Text = "◆色",
            };
            colorSlider.Value[0].ValueChanged += ColorRChanged;
            colorSlider.Value[1].ValueChanged += ColorGChanged;
            colorSlider.Value[2].ValueChanged += ColorBChanged;
            colorSlider.Value[3].ValueChanged += ColorAChanged;
            colorSlider.Value.ValueChanged += ColorsChanged;
            colorSlider.listeners = new[] {
                new PresetListener<EditTextValues>("<<", 30, vals => vals.Add(new []{-0.25f, -0.25f, -0.25f, 0f}, true)),
                new PresetListener<EditTextValues>("<", 20, vals => vals.Add(new []{-0.1f, -0.1f, -0.1f, 0f}, true)),
                new PresetListener<EditTextValues>(">", 20, vals => vals.Add(new []{0.1f, 0.1f, 0.1f, 0f}, true)),
                new PresetListener<EditTextValues>(">>", 30, vals => vals.Add(new []{0.25f, 0.25f, 0.25f, 0f}, true)),
                new PresetListener<EditTextValues>("reset", 50, vals => vals.SetWithNotify(1, 1, 1, 1)),
            };
            colorSlider.prevListeners = new[] {
                new PresetListener<EditTextValue>("0", 20, val => val.Set(0, true)),
                new PresetListener<EditTextValue>("<", 20, val => val.Add(-0.1f, true)),
            };
            colorSlider.nextListeners = new[] {
                new PresetListener<EditTextValue>(">", 20, val => val.Add(0.1f, true)),
                new PresetListener<EditTextValue>("1", 20, val => val.Set(1, true)),
            };
            attachLabel = new CustomLabel(this, "◆アタッチ");

            attachToggle = new CustomToggle(this) {
                Text = "off",
                SelectText = "on",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };
            attachToggle.CheckChanged += (obj, args) => {
                UpdateLayout(uiParamSet);
                if (attachToggle.Value) {
                    ReloadMaidCombo();
                }
                //var tggle = (CustomToggle)obj;
                //blendEdit.blendCombo.IsShowDropDownList &= tggle.Value;
                CheckValidate(obj, args);
            };

            fixOffsetToggle = new CustomToggle(this) {
                Text = "位置 (local)",
                SelectText = "位置 (offset)",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };
            fixOffsetToggle.CheckChanged += OffsetPosChanged;
            fixPosToggle = new CustomToggle(this) {
                Text = "位置追従",
                SelectText = "位置固定",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };
            fixPosToggle.CheckChanged += FixPosToggleChanged;
            fixRotToggle = new CustomToggle(this) {
                Text = "回転追従",
                SelectText = "回転固定",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };
            fixRotToggle.CheckChanged += SkipRotChanged;
            rotScopeToggle = new CustomToggle(this) {
                Text = "回転 (local)",
                SelectText = "回転 (global)",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };
            rotScopeToggle.CheckChanged += RotScopeChanged;
            
            // maidComboは常にEnabled
            maidCombo = new CustomComboBox(this, EMPTY_CONTS) { Enabled = true };
            maidCombo.SelectedIndexChanged += (obj, args) => {
                var maidSelected = maidCombo.SelectedIndex != -1;
                boneToggle.Enabled = maidSelected && (slotCombo.SelectedIndex != -1);
                prevMaidButton.Enabled = (maidCombo.SelectedIndex > 0);
                nextMaidButton.Enabled = (maidCombo.Count-1 > maidCombo.SelectedIndex);
                _currentMaid = GetMaid();

                ReloadSlotCombo();
                if (CanPlay()) MaidChanged(obj, args);
            };

            maidRefreshButton = new CustomButton(this, new GUIContent(resHolder.ReloadImage));
            maidRefreshButton.Click += (obj, args) => { ReloadMaidCombo(); };
            prevMaidButton = new CustomButton(this, "<") {Enabled = false};
            nextMaidButton = new CustomButton(this, ">") {Enabled = false};
            prevMaidButton.Click += (obj, args) => { maidCombo.Prev(); };
            nextMaidButton.Click += (obj, args) => { maidCombo.Next(); };
            boneToggle = new CustomToggle(this) {
                Text = "bone 非表示",
                SelectText = "bone 表示",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };
            boneToggle.CheckChanged += (obj, args) => {
//                if (boneToggle.Value) _boneRenderer.Update();
                if (boneToggle.Value && _currentMaid != null) {
                    var slot = SelectedSlot();
                    if (slot != null && slot.obj != null) {
                        _boneRenderer.Setup(slot.obj.transform);
                    }
                }
                _boneRenderer.SetVisible(boneToggle.Value);
            };

            slotLabel = new CustomLabel(this, "アタッチスロット");
            var filterLabelStyle = new GUIStyle("label") {
                alignment = TextAnchor.MiddleLeft,
                normal = {textColor = Color.white},
            };
            slotLabel.LabelStyle = filterLabelStyle;
            slotFilterText = new CustomTextField(this);
            slotFilterText.ValueChanged += (obj, args) => {
                FilterSlotCombo();
            };

            // slotコンボボックス
            slotCombo = new CustomComboBox(this, EMPTY_CONTS);
            slotCombo.SelectedIndexChanged += SlotChanged;

            boneLabel = new CustomLabel(this, "アタッチボーン") {LabelStyle = filterLabelStyle};
            boneFilterText = new CustomTextField(this);
            boneFilterText.ValueChanged += (obj, args) => {
                FilterBoneCombo();
            };
            // boneコンボボックス
            boneCombo = new CustomComboBox(this, EMPTY_CONTS) {
                Enabled = false
            };
            boneCombo.SelectedIndexChanged += (obj, args) => {
                boneCombo.hasError = boneCombo.SelectedIndex == -1;
                CheckValidate(obj, args);
                if (!boneCombo.hasError) BoneChanged(obj, args); 
            };

            posSlider = new CustomTextSliders(this,
                new[] { "X", "Y", "Z" }, new[] { 0f, 0f, 0f }, posRange) {
                    Text = "◆位置",
            };
            posSlider.Value[0].ValueChanged += PosXChanged;
            posSlider.Value[1].ValueChanged += PosYChanged;
            posSlider.Value[2].ValueChanged += PosZChanged;
            posSlider.Value.ValueChanged += PosChanged;
            posSlider.listeners = new[] {
                new PresetListener<EditTextValues>("reset", 50, vals=> vals.SetWithNotify(0, 0, 0)),
            };
            posSlider.prevListeners = new[] {
                new PresetListener<EditTextValue>("<", 20, val=> val.Add(-0.1f, true)),
            };
            posSlider.nextListeners = new[] {
                new PresetListener<EditTextValue>(">", 20, val=> val.Add(0.1f, true)),
                new PresetListener<EditTextValue>("0", 20, val=> val.Set(0, true)),
            };

            _rotToggle = new CustomToggle(this) {
                Text = "ToEuler",
                SelectText = "ToQuatenion",
                TextColor = Color.white,
                SelectTextColor = Color.white,
                //SelectBackgroundColor =  Color.green,
            };
            _rotToggle.CheckChanged += (obj, args) => {
                // EulerとQuaternion間の変換
                if (eulerSlider.Enabled) {
                    var rot = Quaternion.Euler(eulerSlider.Value[0].Value,
                                    eulerSlider.Value[1].Value,
                                    eulerSlider.Value[2].Value);
                    quatSlider.Value.SetWithNotify(rot.x, rot.y, rot.z, rot.w);
                } else {
                    var rot = new Quaternion(quatSlider.Value[0].Value,
                                    quatSlider.Value[1].Value,
                                    quatSlider.Value[2].Value,
                                    quatSlider.Value[3].Value);
                    var euler = rot.eulerAngles;
                    // 範囲を [0, 360]から[-180, 180]に補正
                    if (euler.x > 180) euler.x -=360;
                    if (euler.y > 180) euler.y -=360;
                    if (euler.z > 180) euler.z -=360;
                    eulerSlider.Value.SetWithNotify(euler.x, euler.y, euler.z);
                }
                eulerSlider.Enabled = !eulerSlider.Enabled;
                quatSlider.Enabled = !quatSlider.Enabled;
                UpdateLayout(uiParamSet);
            };

            eulerSlider = new CustomTextSliders(this,
                new[] { "X", "Y", "Z" }, new[] { 0f, 0f, 0f }, EULER_RANGE) {
                Text = "◆回転(Euler)",
                Enabled = false
            };
            eulerSlider.Value[0].ValueChanged += EulerXChanged;
            eulerSlider.Value[1].ValueChanged += EulerYChanged;
            eulerSlider.Value[2].ValueChanged += EulerZChanged;
            eulerSlider.Value.ValueChanged += EulerChanged;
            eulerSlider.listeners = new[] {
                new PresetListener<EditTextValues>("reset", 50, vals => vals.SetWithNotify(0, 0, 0)),
            };
            eulerSlider.prevListeners = new[] {
                new PresetListener<EditTextValue>("-90", 40, val => val.Add(-90, true)),
            };
            eulerSlider.nextListeners = new[] {
                new PresetListener<EditTextValue>("+90", 40, val => val.Add(+90, true)),
                new PresetListener<EditTextValue>("0", 20, val => val.Set(0, true)),
            };

            quatSlider = new CustomTextSliders(this,
                new[] { "X", "Y", "Z", "W" }, 
                new[] { 0f, 0f, 0f, 1f }, 
                new[] {ROT_RANGE,ROT_RANGE,ROT_RANGE,ROT_RANGEW}) {
                Text = "◆回転(Quaternion)",
                Enabled = true,
            };
            quatSlider.Value[0].ValueChanged += RotXChanged;
            quatSlider.Value[1].ValueChanged += RotYChanged;
            quatSlider.Value[2].ValueChanged += RotZChanged;
            quatSlider.Value[3].ValueChanged += RotWChanged;
            quatSlider.Value.ValueChanged += RotChanged;
            quatSlider.listeners = new[] {
                new PresetListener<EditTextValues>("reset", 50, vals => vals.SetWithNotify(0f, 0f, 0f, 1f)),
            };
            quatSlider.prevListeners = new[] {
                new PresetListener<EditTextValue>("-0.5", 40, val => val.Add(-0.5f, true)),
            };
            quatSlider.nextListeners = new[] {
                new PresetListener<EditTextValue>("+0.5", 40, val => val.Add(+0.5f, true)),
                new PresetListener<EditTextValue>("0", 20, val => val.Set(0, true)),
            };

            _initialized = true;
            return true;
        }

        public override void Update() {
            if (!_initialized) return;

            // メイド情報更新
            CheckItemChanged();

            if (_boneRenderer != null) _boneRenderer.Update();
            if (!Visibled) return;

            scaleSlider.Update();
            speedSlider.Update();

            //posSlider.Update();
            //eulerSlider.Update();
            //quatSlider.Update();
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

                    scaleSlider.OnGUI();
                    speedSlider.OnGUI();
                    colorSlider.OnGUI();
                    registButton.OnGUI();

                } finally {
                    enabledStore.Restore();
                }

                attachLabel.OnGUI();
                attachToggle.OnGUI();
                if (attachToggle.Value) {
                    maidCombo.OnGUI();
                    nextMaidButton.OnGUI();
                    prevMaidButton.OnGUI();
                    boneToggle.OnGUI();
                    maidRefreshButton.OnGUI();

                    slotLabel.OnGUI();
                    slotCombo.OnGUI();
                    boneLabel.OnGUI();
                    boneCombo.OnGUI();
                    slotFilterText.OnGUI();
                    boneFilterText.OnGUI();
                    fixOffsetToggle.OnGUI();
                    fixPosToggle.OnGUI();
                    rotScopeToggle.OnGUI();
                    fixRotToggle.OnGUI();
                }
                _rotToggle.OnGUI();
                posSlider.OnGUI();
                if (eulerSlider.Enabled) {
                    eulerSlider.OnGUI();
                } else {
                    quatSlider.OnGUI();
                }

            } catch (Exception e) {
                Log.Error(e);
            }
        }

        private bool IsShowDropdown() {
            return efkCombo.IsShowDropDownList || maidCombo.IsShowDropDownList;
        }

        protected override void Layout(UIParamSet uiParams) {
            if (!_initialized) return;

            var margin2 = margin * 2;
            var margin6 = margin * 6;

            var itemHeight = uiParams.itemHeight;
            var labelWidth = uiParams.FixPx(100);
            var align = new Rect(margin2, uiParams.unitHeight, labelWidth, itemHeight);

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

            var textWidth = Width * 0.20f;
            align.y = margin2;
            align.width = -margin * 2;
            scaleSlider.AlignTop(playButton, ref align);
            scaleSlider.indent = margin*4;
            scaleSlider.TextWidth = textWidth;
            scaleSlider.TextHeight = itemHeight;

            speedSlider.AlignTop(scaleSlider, ref align);
            speedSlider.indent = margin*4;
            speedSlider.TextWidth = textWidth;
            speedSlider.TextHeight = itemHeight;
            
            align.x = margin;
            align.y = margin;
            align.width = Width - margin2;
            var indent = uiParams.FixPx(14);
            colorSlider.indent = indent;
            colorSlider.subLabelWidth = indent;
            colorSlider.TextWidth  = textWidth;
            colorSlider.TextHeight = itemHeight;
            colorSlider.AlignTop(speedSlider, ref align);

            align.x = margin;
            align.y = margin2;
            align.width = buttonWidth;
            attachLabel.AlignTop(colorSlider, ref align);
            align.x = margin;
            align.width = buttonWidth *0.5f;
            attachToggle.AlignLeftTop(attachLabel, colorSlider, ref align);

            GUIControl baseObj = attachToggle;
            if (attachToggle.Value) {
                align.x = margin2;
                align.width = buttonWidth;
                fixOffsetToggle.AlignLeftTop(attachToggle, colorSlider, ref align);
                fixPosToggle.AlignLeftTop(fixOffsetToggle, colorSlider, ref align);

                boneToggle.AlignTop(attachToggle, ref align);
                rotScopeToggle.AlignLeftTop(attachToggle, attachToggle, ref align);
                fixRotToggle.AlignLeftTop(rotScopeToggle, attachToggle, ref align);

                var subButtonWidth = uiParams.FixPx(20f);
                align.x = margin2;
                align.width = subButtonWidth;
                align.height = itemHeight * 2;
                prevMaidButton.AlignTop(fixRotToggle, ref align);
                align.x = margin;
                align.width = Width - margin * 10 - subButtonWidth*3;
                maidCombo.AlignLeftTop(prevMaidButton, fixRotToggle, ref align);
                
                align.width = subButtonWidth;
                nextMaidButton.AlignLeftTop(maidCombo, fixRotToggle, ref align);
                align.width = -margin2;
                maidRefreshButton.AlignLeftTop(nextMaidButton, fixRotToggle, ref align);

                align.x = margin2;
                align.height = itemHeight;
                align.y = 0;
                align.width = Width * 0.5f - margin6;
                slotLabel.AlignTop(maidCombo, ref align);
                slotFilterText.AlignTop(slotLabel, ref align);
                slotCombo.AlignTop(slotFilterText, ref align);

                align.x = margin2 + Width * 0.5f;
                boneLabel.AlignTop(maidCombo, ref align);
                boneFilterText.AlignTop(boneLabel, ref align);
                boneCombo.AlignTop(boneFilterText, ref align);

                baseObj = boneCombo;
            }

            align.x = margin;
            align.y = margin;
            align.width = Width - margin2;
            posSlider.indent = indent;
            posSlider.subLabelWidth = indent;
            posSlider.TextWidth  = textWidth;
            posSlider.TextHeight = itemHeight;
            posSlider.AlignTop(baseObj, ref align);

            align.y = margin2;
            align.width = Width - margin2;
            eulerSlider.indent = indent;
            eulerSlider.subLabelWidth = indent;
            eulerSlider.TextWidth = textWidth;
            eulerSlider.TextHeight = itemHeight;
            eulerSlider.AlignTop(posSlider, ref align);
            quatSlider.indent = indent;
            quatSlider.subLabelWidth = indent;
            quatSlider.TextWidth = textWidth;
            quatSlider.TextHeight = itemHeight;
            quatSlider.AlignTop(posSlider, ref align);

            align.x = margin2 + textWidth + indent * 3;
            align.width = uiParams.FixPx(100);
            _rotToggle.AlignTop(posSlider, ref align);

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
            var fontSizeS = uiParamSet.fontSizeS;
//            var fontSizeSS = uiParamSet.fontSizeSS;

            WinStyle.fontSize = fontSizeN;
//            VerStyle.fontSize = fontSizeN;

            // 子要素のフォント設定
            playButton.FontSize = fontSizeN;
            stopButton.FontSize = fontSizeN;
            stopRButton.FontSize = fontSizeN;
            pauseButton.FontSize = fontSizeN;
            registButton.FontSize = fontSizeN;
            //allPlayButton.FontSize = fontSizeS;
            //allStopButton.FontSize = fontSizeS;
            groupnameLabel.FontSize = fontSizeN;
            groupnameText.FontSize = fontSizeN;
            nameLabel.FontSize = fontSizeN;
            nameText.FontSize = fontSizeN;

            efkLabel.FontSize = fontSizeN;
            efkCombo.FontSize = fontSizeN;
            repeatToggle.FontSize = fontSizeN;

            scaleSlider.FontSize = fontSizeN;
            scaleSlider.FontSizeS = fontSizeS;
            speedSlider.FontSize = fontSizeN;
            speedSlider.FontSizeS = fontSizeS;
            colorSlider.FontSize = fontSizeN;
            colorSlider.FontSizeS = fontSizeS;

            attachLabel.FontSize = fontSizeN;
            attachToggle.FontSize = fontSizeN;
            maidCombo.FontSize = fontSizeN;
            nextMaidButton.FontSize = fontSizeS;
            prevMaidButton.FontSize = fontSizeS;

            boneToggle.FontSize = fontSizeN;
            maidRefreshButton.FontSize = fontSizeN;

            slotLabel.FontSize = fontSizeS;
            boneLabel.FontSize = fontSizeS;
            fixOffsetToggle.FontSize = fontSizeS;
            fixPosToggle.FontSize = fontSizeS;
            fixRotToggle.FontSize = fontSizeS;
            rotScopeToggle.FontSize = fontSizeS;

            slotFilterText.FontSize = fontSizeN;
            boneFilterText.FontSize = fontSizeN;
            slotCombo.FontSize = fontSizeN;
            boneCombo.FontSize = fontSizeN;

            posSlider.FontSize = fontSizeN;
            posSlider.FontSizeS = fontSizeS;

            eulerSlider.FontSize = fontSizeN;
            eulerSlider.FontSizeS = fontSizeS;

            quatSlider.FontSize = fontSizeN;
            quatSlider.FontSizeS = fontSizeS;
            _rotToggle.FontSize = fontSizeN;
        }
        #region Properties

        #endregion

        #region Fields
        private readonly MaidHolder _maidHolder = new MaidHolder();
//        private int _frameCount;
        internal readonly RecipeManager recipeMgr;
        internal readonly EfkManager efkMgr;

        public CustomLabel groupnameLabel;
        public CustomTextField groupnameText;
        public CustomLabel nameLabel;
        public CustomTextField nameText;

        public CustomLabel efkLabel;
        public CustomComboBox efkCombo;

        public CustomToggle repeatToggle;
        public CustomToggle boneToggle;

        public CustomButton playButton;
        public CustomButton stopButton;
        public CustomButton stopRButton;
        public CustomButton pauseButton;

        public CustomButton registButton;

        // scale slider
        public CustomTextLogSlider scaleSlider;
        public CustomTextLogSlider speedSlider;
        public CustomTextSliders colorSlider;

        public CustomLabel attachLabel;
        public CustomToggle attachToggle;
        // FIXME 対象指定:
        //    メイド選択,
        //    有効なメイドすべて
        //    NPC/Man選択
        //    有効なメイド/Man/NPCすべて
        // 
        public CustomComboBox maidCombo;
        public CustomButton nextMaidButton;
        public CustomButton prevMaidButton;
        public CustomButton maidRefreshButton;

        public CustomLabel slotLabel;
        public CustomLabel boneLabel;
        public CustomToggle fixOffsetToggle;
        public CustomToggle fixPosToggle;
        public CustomToggle fixRotToggle;
        public CustomToggle rotScopeToggle;

        public CustomTextField slotFilterText;
        public CustomTextField boneFilterText;
        public CustomComboBox slotCombo;
        public CustomComboBox boneCombo;

        public CustomTextSliders posSlider;
        public CustomTextSliders eulerSlider;
        public CustomTextSliders quatSlider;
        private CustomToggle _rotToggle;

        #endregion
    }
}
