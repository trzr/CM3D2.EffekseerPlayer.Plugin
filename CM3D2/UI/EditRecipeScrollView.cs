using System;
using EffekseerPlayer.CM3D2.Render;
using EffekseerPlayer.CM3D2.Util;
using EffekseerPlayer.Unity.Data;
using EffekseerPlayer.Unity.UI;
using EffekseerPlayer.Unity.Util;
using EffekseerPlayer.Util;
using UnityEngine;

namespace EffekseerPlayer.CM3D2.UI {
    /// <summary>
    /// レシピエディットウィンドウ用のスクロールビュークラス.
    /// レシピの各種パラメータを設定するUIを定義する.
    /// 
    /// </summary>
    public partial class EditRecipeScrollView : BaseScrollView {

        public EditRecipeScrollView(GUIControl parent, EventHandler checkValidate) : base(parent) {
            CheckValidate = checkValidate;
        }

        public void Dispose() {
            if (currentEmitter != null) {
                currentEmitter.Stop();
                UnityEngine.Object.Destroy(currentEmitter);
                currentEmitter = null;
            }

            if (posGizmo != null) {
                posGizmo.Visible = false;
                UnityEngine.Object.Destroy(posGizmo);
                posGizmo  = null;
            }

            if (rotGizmo != null) {
                rotGizmo.Visible = false;
                UnityEngine.Object.Destroy(rotGizmo);
                rotGizmo  = null;
            }

            if (gobj != null) {
                UnityEngine.Object.Destroy(gobj);
                gobj = null;
            }
        }

        public override void Awake() {
            gobj = new GameObject(EMITTER_NAME);
            UnityEngine.Object.DontDestroyOnLoad(gobj);

            _boneRenderer = new CustomBoneRenderer();

            var settings = Settings.Instance;
            var scaleRange = new EditRange(5, 0.00001f, settings.maxScale);
            var speedRange = new EditRange(5, 0.0001f, settings.maxSpeed);
            var fixFrameRange = new EditRange(0, 0f, settings.maxFrame);
            var frameRange = new EditRange(0, 0f, settings.maxFrame);
            var posRange = new EditRange(5, -settings.maxLocation, settings.maxLocation);

            scaleSlider = new CustomTextLogSlider(this, 1.0f, scaleRange) {
                Text = "◆スケール",
                listeners = new[] {
                    new PresetListener<EditTextLogValue>("0.01", 40, val => val.Set(0.01f, true)),
                    new PresetListener<EditTextLogValue>("0.05", 40, val => val.Set(0.05f, true)),
                    new PresetListener<EditTextLogValue>("0.1", 40, val => val.Set(0.1f, true)),
                    new PresetListener<EditTextLogValue>("0.5", 40, val => val.Set(0.5f, true)),
                    new PresetListener<EditTextLogValue>("1", 20, val => val.Set(1, true)),
                },
                prevListeners = new[] {
                    new PresetListener<EditTextLogValue>("<<", 30, val => val.Multiply(0.1f, true)),
                    new PresetListener<EditTextLogValue>("<", 20, val => val.Multiply(0.5f, true)),
                },
                nextListeners = new[] {
                    new PresetListener<EditTextLogValue>(">", 20, val => val.Multiply(2f, true)),
                    new PresetListener<EditTextLogValue>(">>", 30, val => val.Multiply(10f, true)),
                },
            };
            speedSlider = new CustomTextLogSlider(this, 1.0f, speedRange) {
                Text = "◆再生速度",
                listeners = new[] {
                    new PresetListener<EditTextLogValue>("0.05", 40, val => val.Set(0.05f, true)),
                    new PresetListener<EditTextLogValue>("0.1", 40, val => val.Set(0.1f, true)),
                    new PresetListener<EditTextLogValue>("0.5", 40, val => val.Set(0.5f, true)),
                    new PresetListener<EditTextLogValue>("1", 20, val => val.Set(1f, true)),
                    new PresetListener<EditTextLogValue>("2", 20, val => val.Set(2f, true)),
                },
                prevListeners = new[] {
                    new PresetListener<EditTextLogValue>("<<", 30, val => val.Multiply(0.1f, true)),
                    new PresetListener<EditTextLogValue>("<", 20, val => val.Multiply(0.5f, true)),
                },
                nextListeners = new[] {
                    new PresetListener<EditTextLogValue>(">", 20, val => val.Multiply(2f, true)),
                    new PresetListener<EditTextLogValue>(">>", 30, val => val.Multiply(10f, true)),
                },
            };
            endFrameSlider = new CustomTextSlider(this, 0f, fixFrameRange) {
                Text = "◆エンドフレーム",
                listeners = new[] {
                    new PresetListener<EditTextValue>("0", 20, val => val.Set(0, true)),
                },
                prevListeners = new[] {
                    new PresetListener<EditTextValue>("<<", 30, val => val.Add(-120, true)),
                    new PresetListener<EditTextValue>("<", 20, val => val.Add(-60, true)),
                },
                nextListeners = new[] {
                    new PresetListener<EditTextValue>(">", 20, val => val.Add(+60, true)),
                    new PresetListener<EditTextValue>(">>", 30, val => val.Add(+120, true)),
                },
            };
            frameSlider = new CustomTextSlider(this, 0f, frameRange) {
                Text = "◇フレーム",
                Enabled = false,
            };
            delaySlider = new CustomTextSlider(this, 0f, fixFrameRange) {
                Text = "◇ディレイ",
                listeners = new[] {
                    new PresetListener<EditTextValue>("0", 20, val => val.Set(0, true)),
                },
                prevListeners = new[] {
                    new PresetListener<EditTextValue>("<<", 30, val => val.Add(-120, true)),
                    new PresetListener<EditTextValue>("<", 20, val => val.Add(-60, true)),
                },
                nextListeners = new[] {
                    new PresetListener<EditTextValue>(">", 20, val => val.Add(+60, true)),
                    new PresetListener<EditTextValue>(">>", 30, val => val.Add(+120, true)),
                },
            };
            postDelaySlider = new CustomTextSlider(this, 0f, fixFrameRange) {
                Text = "◇ポストディレイ",
                listeners = new[] {
                    new PresetListener<EditTextValue>("0", 20, val => val.Set(0, true)),
                },
                prevListeners = new[] {
                    new PresetListener<EditTextValue>("<<", 30, val => val.Add(-120, true)),
                    new PresetListener<EditTextValue>("<", 20, val => val.Add(-60, true)),
                },
                nextListeners = new[] {
                    new PresetListener<EditTextValue>(">", 20, val => val.Add(+60, true)),
                    new PresetListener<EditTextValue>(">>", 30, val => val.Add(+120, true)),
                },
            };
            var defaultColor = Color.white;
            var presetMgr = ColorPresetManager.Instance;
            var rgbMapTex = ColorUtils.CreateRGBMapTex(257, 257);
            var borderWidth = 1;
            var lightTex = ColorUtils.CreateLightTex(16, 256, borderWidth);
            var editColor = new EditRGBATex(ref defaultColor, rgbMapTex, lightTex) {
                colorIconBorder = borderWidth,
                colorIconHeight = (int) uiParamSet.itemHeight
            };
            var btnStyle = new GUIStyle("button") {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = uiParamSet.fontSizeS,
                };
            colorSlider = new CustomColorSlider(this, editColor, presetMgr) {
                Text = "◆色",
                ButtonStyle = btnStyle,
                listeners = new[] {
                    new PresetListener<EditColorTex>("<<", btnStyle, val => val.Add(-0.25f, true)),
                    new PresetListener<EditColorTex>("<", btnStyle, val => val.Add(-0.1f, true)),
                    new PresetListener<EditColorTex>(">", btnStyle, val => val.Add(0.1f,　true)),
                    new PresetListener<EditColorTex>(">>", btnStyle, val => val.Add(0.25f, true)),
                    new PresetListener<EditColorTex>("reset", btnStyle, val => val.SetColor(ref defaultColor)),
                },
                prevListeners = new[] {
                    new SubPresetListener<int, EditColorTex>("0", btnStyle, (i, val) => val.SetColorValue(i, 0f, true)),
                    new SubPresetListener<int, EditColorTex>("<", btnStyle, (i, val) => val.Add(i, -0.1f, true)),
                },
                nextListeners = new[] {
                    new SubPresetListener<int, EditColorTex>(">", btnStyle, (i, val) => val.Add(i, 0.1f, true)),
                    new SubPresetListener<int, EditColorTex>("1", btnStyle, (i, val) => val.SetColorValue(i, 1f, true)),
                },
            };
            attachLabel = new CustomLabel(this, "◆アタッチ");

            attachToggle = new CustomToggle(this) {
                Text = "off",
                SelectText = "on",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };

            fixOffsetToggle = new CustomToggle(this) {
                Text = "位置 (local)",
                SelectText = "位置 (offset)",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };
            fixPosToggle = new CustomToggle(this) {
                Text = "位置追従",
                SelectText = "位置固定",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };
            fixRotToggle = new CustomToggle(this) {
                Text = "回転追従",
                SelectText = "回転固定",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };
            rotScopeToggle = new CustomToggle(this) {
                Text = "回転 (local)",
                SelectText = "回転 (global)",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };

            // maidComboは常にEnabled
            maidCombo = new CustomComboBox(this, EMPTY_CONTS) { Enabled = true };

            maidRefreshButton = new CustomButton(this, new GUIContent(ResourceHolder.Instance.ReloadImage));
            prevMaidButton = new CustomButton(this, "<") {Enabled = false};
            nextMaidButton = new CustomButton(this, ">") {Enabled = false};
            boneToggle = new CustomToggle(this) {
                Text = "bone 非表示",
                SelectText = "bone 表示",
                TextColor = Color.white,
                SelectTextColor = Color.white
            };

            slotLabel = new CustomLabel(this, "アタッチスロット");
            var filterLabelStyle = new GUIStyle("label") {
                alignment = TextAnchor.MiddleLeft,
                normal = {textColor = Color.white},
            };
            slotLabel.LabelStyle = filterLabelStyle;
            slotFilterText = new CustomTextField(this);

            // slotコンボボックス
            slotCombo = new CustomComboBox(this, EMPTY_CONTS);

            boneLabel = new CustomLabel(this, "アタッチボーン") {LabelStyle = filterLabelStyle};
            boneFilterText = new CustomTextField(this);
            // boneコンボボックス
            boneCombo = new CustomComboBox(this, EMPTY_CONTS) {
                Enabled = false
            };


            var btnStyle2 = new GUIStyle("button") {
                alignment = TextAnchor.MiddleCenter,
                fontSize = uiParamSet.fontSizeS,
            };
            posSlider = new CustomTextSliders(this,
                new[] {"X", "Y", "Z"}, new[] {0f, 0f, 0f}, posRange) {
                Text = "◆位置",
                ButtonStyle = btnStyle2,
                listeners = new[] {
                    new PresetListener<EditTextValues>("reset", btnStyle2, vals => vals.SetWithNotify(0, 0, 0)),
                },
                prevListeners = new[] {
                    new SubPresetListener<int, EditTextValues>("<", btnStyle2, (i, val) => val[i].Add(-0.1f, true)),
                },
                nextListeners = new[] {
                    new SubPresetListener<int, EditTextValues>(">", btnStyle2, (i, val) => val[i].Add(0.1f, true)),
                    new SubPresetListener<int, EditTextValues>("0", btnStyle2, (i, val) => val.Set(i, 0, true)),
                },
            };

            rotToggle = new CustomToggle(this) {
                Text = "ToEuler",
                SelectText = "ToQuaternion",
                TextColor = Color.white,
                SelectTextColor = Color.white,
                //SelectBackgroundColor =  Color.green,
            };
            posGizmoToggle = new CustomToggle(this) {
                Text = "Gizmo off",
                SelectText = "Gizmo on",
                TextColor = Color.white,
                SelectTextColor = Color.white,
            };
            rotGizmoToggle = new CustomToggle(this) {
                Text = "Gizmo off",
                SelectText = "Gizmo on",
                TextColor = Color.white,
                SelectTextColor = Color.white,
            };

            var rotBtnStyle = new GUIStyle("button") {
                alignment = TextAnchor.MiddleCenter,
                fontSize = uiParamSet.fontSizeS,
            };

            eulerSlider = new CustomTextSliders(this,
                new[] {"X", "Y", "Z"}, new[] {0f, 0f, 0f}, EULER_RANGE) {
                Text = "◆回転(Euler)",
                Enabled = false,
                ButtonStyle = rotBtnStyle,
                listeners = new[] {
                    new PresetListener<EditTextValues>("reset", rotBtnStyle, vals => vals.SetWithNotify(0, 0, 0)),
                },
                prevListeners = new[] {
                    new SubPresetListener<int, EditTextValues>("-90", rotBtnStyle, (i, val) => val[i].Add(-90, true)),
                },
                nextListeners = new[] {
                    new SubPresetListener<int, EditTextValues>("+90", rotBtnStyle, (i, val) => val[i].Add(+90, true)),
                    new SubPresetListener<int, EditTextValues>("0", rotBtnStyle, (i, val) => val.Set(i, 0, true)),
                }
            };

            quatSlider = new CustomTextSliders(this,
                new[] {"X", "Y", "Z", "W"},
                new[] {0f, 0f, 0f, 1f},
                new[] {ROT_RANGE, ROT_RANGE, ROT_RANGE, ROT_RANGEW}) {
                Text = "◆回転(Quaternion)",
                Enabled = true,
                ButtonStyle = rotBtnStyle,
                listeners = new[] {
                    new PresetListener<EditTextValues>("reset", rotBtnStyle, vals => vals.SetWithNotify(0f, 0f, 0f, 1f)),
                },
                prevListeners = new[] {
                    new SubPresetListener<int, EditTextValues>("-0.5", rotBtnStyle, (i, val) => val[i].Add(-0.5f, true)),
                },
                nextListeners = new[] {
                    new SubPresetListener<int, EditTextValues>("+0.5", rotBtnStyle, (i, val) => val[i].Add(+0.5f, true)),
                    new SubPresetListener<int, EditTextValues>("0", rotBtnStyle, (i, val) => val.Set(i, 0, true)),
                },
            };

            //------------------------------------------
            // イベント処理
            //------------------------------------------
            scaleSlider.Value.ValueChanged += ScaleChanged;
            speedSlider.Value.ValueChanged += SpeedChanged;
            endFrameSlider.Value.ValueChanged += EndFrameChanged;
            delaySlider.Value.ValueChanged += DelayChanged;
            postDelaySlider.Value.ValueChanged += PostDelayChanged;
            colorSlider.EditVal.ValueChanged += ColorsChanged;
            colorSlider.ExpandChanged += (obj, args) => { UpdateLayout(uiParamSet); };

            attachToggle.CheckChanged += (obj, args) => {
                UpdateLayout(uiParamSet);
                if (attachToggle.Value) {
                    ReloadMaidCombo();
                }
                UpdateAttach(attachToggle.Value);

                //var tggle = (CustomToggle)obj;
                //blendEdit.blendCombo.IsShowDropDownList &= tggle.Value;
                CheckValidate(obj, args);
            };

            fixOffsetToggle.CheckChanged += OffsetPosChanged;
            fixPosToggle.CheckChanged += FixPosToggleChanged;
            fixRotToggle.CheckChanged += SkipRotChanged;
            rotScopeToggle.CheckChanged += RotScopeChanged;
            maidCombo.SelectedIndexChanged += (obj, args) => {
                var maidSelected = maidCombo.SelectedIndex != -1;
                boneToggle.Enabled = maidSelected && (slotCombo.SelectedIndex != -1);
                prevMaidButton.Enabled = (maidCombo.SelectedIndex > 0);
                nextMaidButton.Enabled = (maidCombo.Count-1 > maidCombo.SelectedIndex);
                _currentMaid = GetMaid();

                ReloadSlotCombo();
                // スロットに対して、ボーンが選択可能かを確認する
                if (_currentMaid != null && slotCombo.SelectedIndex != -1) {
                    ChangeCurrentSlot(SelectedSlot(_currentMaid));
                }
                UpdateAttach(attachToggle.Value);
            };
            maidRefreshButton.Click += (obj, args) => {
                if (!ReloadMaidCombo()) {
                    if (_currentMaid == null || _currentSlot == null) return;

                    ReloadSlotCombo();
                }
            };
            prevMaidButton.Click += (obj, args) => { maidCombo.Prev(); };
            nextMaidButton.Click += (obj, args) => { maidCombo.Next(); };
            boneToggle.CheckChanged += (obj, args) => {
//                if (boneToggle.Value) _boneRenderer.Update();
                if (boneToggle.Value && _currentMaid != null) {
                    var slot = SelectedSlot();
                    if (slot != null && slot.obj != null) {
                        _boneRenderer.Setup(slot.obj, slot.RID);
                    }
                }
                _boneRenderer.SetVisible(boneToggle.Value);
            };

            slotFilterText.ValueChanged += (obj, args) => {
                FilterSlotCombo();
            };
            slotCombo.SelectedIndexChanged += SlotChanged;
            boneFilterText.ValueChanged += (obj, args) => {
                FilterBoneCombo();
            };
            boneCombo.SelectedIndexChanged += (obj, args) => {
                boneCombo.hasError = boneCombo.SelectedIndex == -1;
                CheckValidate(obj, args);
                if (!boneCombo.hasError) BoneChanged(obj, args); 
            };
            posSlider.Value[0].ValueChanged += PosXChanged;
            posSlider.Value[1].ValueChanged += PosYChanged;
            posSlider.Value[2].ValueChanged += PosZChanged;
            posSlider.Value.ValueChanged += PosChanged;
            posGizmoToggle.CheckChanged += (obj, args) => {
                if (posGizmo == null) {
                    posGizmo = CreateGizmo();
                    posGizmo.PosChanged += (trans0, emp) => {
                        var pos = ((Transform)trans0).localPosition;
                        _location = pos;
                        ToLocationSlider(ref pos);
                        ApplyLocation();
                        if (rotGizmo != null && rotGizmo.Visible) {
                            rotGizmo.transform.localPosition = pos;
                        }
                    };
                }
                ToggleGizmo(ref posGizmo, posGizmoToggle.Value, true, false);
                if (posGizmoToggle.Value) {
                    posGizmo.transform.localPosition = _location;
                    posGizmo.transform.localRotation = GetQuat();
                }
            };
            rotGizmoToggle.CheckChanged += (obj, args) => {
                if (rotGizmo == null) {
                    rotGizmo = CreateGizmo();
                    rotGizmo.RotChanged += (trans0, emp) => {
                        var trans = (Transform) trans0;
                        ToRotationSlider(trans.localRotation);
                        if (currentEmitter != null) {
                            currentEmitter.transform.localRotation = trans.localRotation;
                            currentEmitter.UpdateRotation();
                        }
                        if (posGizmo != null && posGizmo.Visible) {
                            posGizmo.transform.localRotation = trans.localRotation;
                        }
                    };
                }

                ToggleGizmo(ref rotGizmo, rotGizmoToggle.Value, false, true);
                if (rotGizmoToggle.Value) {
                    rotGizmo.transform.localPosition = _location;
                    rotGizmo.transform.localRotation = GetQuat();
                }
            };
            rotToggle.CheckChanged += (obj, args) => {
                // EulerとQuaternion間の変換
                if (eulerSlider.Enabled) {
                    var rot = Quaternion.Euler(_euler);
                    quatSlider.Value.SetWithNotify(rot.x, rot.y, rot.z, rot.w);
                } else {
                    var rot = new Quaternion(quatSlider.Value[0].Value,
                        quatSlider.Value[1].Value,
                        quatSlider.Value[2].Value,
                        quatSlider.Value[3].Value);
                    SetEulerSlider(ref rot, true);
                }
                eulerSlider.Enabled = !eulerSlider.Enabled;
                quatSlider.Enabled = !quatSlider.Enabled;
                UpdateLayout(uiParamSet);
            };
            eulerSlider.Value[0].ValueChanged += EulerXChanged;
            eulerSlider.Value[1].ValueChanged += EulerYChanged;
            eulerSlider.Value[2].ValueChanged += EulerZChanged;
            eulerSlider.Value.ValueChanged += EulerChanged;
            quatSlider.Value[0].ValueChanged += RotXChanged;
            quatSlider.Value[1].ValueChanged += RotYChanged;
            quatSlider.Value[2].ValueChanged += RotZChanged;
            quatSlider.Value[3].ValueChanged += RotWChanged;
            quatSlider.Value.ValueChanged += RotChanged;

            base.Awake();

            // スクロールビューの位置をコンボボックスに伝達する
            ScrollChanged += (pos) => {
                maidCombo.Offset = pos;
                boneCombo.Offset = pos;
                slotCombo.Offset = pos;
            };
        }

        private int updateFrame;
        public override void Update() {
            // メイド情報更新
            CheckItemChanged();

            if (_boneRenderer != null) _boneRenderer.Update();
            if (currentEmitter != null && currentEmitter.Exists) {
                if (++updateFrame > 1) {
                    frameSlider.Num = currentEmitter.Frame;
                    updateFrame = 0;
                }
            }
        }

        public void UpdateSlider() {
            scaleSlider.Update();
            speedSlider.Update();
 
            //posSlider.Update();
            //eulerSlider.Update();
            //quatSlider.Update();
        }

        public override float GetViewHeight() {
            return viewHeight;
        }

        internal void CalcViewHeight() {
            viewHeight = quatSlider.yMax - Top + Margin*2;
        }

        protected override void OnContentView() {
            scaleSlider.OnGUI();
            speedSlider.OnGUI();
            endFrameSlider.OnGUI();
            frameSlider.OnGUI();
            delaySlider.OnGUI();
            postDelaySlider.OnGUI();
            colorSlider.OnGUI();

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
            rotToggle.OnGUI();
            posGizmoToggle.OnGUI();
            rotGizmoToggle.OnGUI();
            posSlider.OnGUI();
            if (eulerSlider.Enabled) {
                eulerSlider.OnGUI();
            } else {
                quatSlider.OnGUI();
            }
        }

        public bool IsShowDropdown() {
            return maidCombo.IsShowDropDownList || boneCombo.IsShowDropDownList || slotCombo.IsShowDropDownList;
        }

        protected override void Layout(UIParamSet uiParams) {
            var margin2 = margin * 2;
            var margin6 = margin * 6;

            var viewWidth = Width - WIDTH_SCROLLBAR;
            var buttonWidth = (viewWidth - margin6) / 4f;
            var itemHeight = uiParamSet.itemHeight;
            var textWidth = viewWidth * 0.20f;

            var formData = new FormData(margin, 0);
            var rightLayout = new AttachData(margin + WIDTH_SCROLLBAR);
            formData.Right = rightLayout;
            formData.Height = itemHeight;

            scaleSlider.Align(formData);
            scaleSlider.indent = margin*4;
            scaleSlider.TextWidth = textWidth;
            scaleSlider.TextHeight = itemHeight;

            formData.Top = new AttachData(scaleSlider, margin2);
            speedSlider.Align(formData);
            speedSlider.indent = margin*4;
            speedSlider.TextWidth = textWidth;
            speedSlider.TextHeight = itemHeight;

            formData.Top.obj = speedSlider;
            endFrameSlider.Align(formData);
            endFrameSlider.indent = margin*4;
            endFrameSlider.TextWidth = textWidth;
            endFrameSlider.TextHeight = itemHeight;

            formData.Top.obj = endFrameSlider;
            frameSlider.Align(formData);
            frameSlider.indent = margin*4;
            frameSlider.TextWidth = textWidth;
            frameSlider.TextHeight = itemHeight;

            formData.Top.obj = frameSlider;
            delaySlider.Align(formData);
            delaySlider.indent = margin*4;
            delaySlider.TextWidth = textWidth;
            delaySlider.TextHeight = itemHeight;

            formData.Top.obj = delaySlider;
            postDelaySlider.Align(formData);
            postDelaySlider.indent = margin*4;
            postDelaySlider.TextWidth = textWidth;
            postDelaySlider.TextHeight = itemHeight;

            formData.Left.offset = margin;
            formData.Top.Set(postDelaySlider, margin, 0);
            formData.Right = rightLayout;
            var indent = uiParamSet.FixPx(14);
            colorSlider.subLabelIndent = indent;
//            colorSlider.TextWidth  = textWidth;
            colorSlider.RowHeight = itemHeight;
            colorSlider.Align(formData);

            formData.Top.Set(colorSlider, margin2, itemHeight);
            formData.Right = null;
            formData.Width = buttonWidth;
            attachLabel.Align(formData);
            formData.Left.obj = attachLabel;
            formData.Width = buttonWidth * 0.5f;
            attachToggle.Align(formData);

            GUIControl baseObj = attachToggle;
            if (attachToggle.Value) {
                formData.Left.offset = margin2;
                formData.Width = buttonWidth;

                formData.Left.obj = attachToggle;
                fixOffsetToggle.Align(formData);

                formData.Left.obj = fixOffsetToggle;
                fixPosToggle.Align(formData);

                formData.Left.obj = null;
                formData.Top.obj = attachToggle;
                boneToggle.Align(formData);

                formData.Left.obj = attachToggle;
                rotScopeToggle.Align(formData);
                formData.Left.obj = rotScopeToggle;
                fixRotToggle.Align(formData);

                var subButtonWidth = uiParamSet.FixPx(20f);
                formData.Left.Set(null, margin2, subButtonWidth);
                formData.Top.Set(fixRotToggle, margin2, itemHeight * 2);
                prevMaidButton.Align(formData);

                var leftLayout = formData.Left;
                // 右から順に配置 ( combo <- next <- refresh )
                formData.Left = null;
                formData.Right = new AttachData(null, margin2 + WIDTH_SCROLLBAR, subButtonWidth+margin2);
                formData.Top.obj = fixRotToggle;
                maidRefreshButton.AlignRev(formData);

                formData.Right.Set(maidRefreshButton, margin, subButtonWidth);
                nextMaidButton.AlignRev(formData);

                formData.Left = leftLayout;
                formData.Left.Set(prevMaidButton, margin, 0);
                formData.Right.Set(nextMaidButton, margin, 0);
                maidCombo.Align(formData);

                formData.Top.Set(maidCombo, 0, itemHeight);
                formData.Left.Set(null, margin2, viewWidth * 0.5f - margin6);
                formData.Right = null;
                slotLabel.Align(formData);
                formData.Top.obj = slotLabel;
                slotFilterText.Align(formData);
                formData.Top.obj = slotFilterText;
                slotCombo.Align(formData);

                formData.Left.obj = null;
                formData.Left.offset = margin2 + viewWidth * 0.5f;
                formData.Top.obj = maidCombo;
                boneLabel.Align(formData);
                formData.Top.obj = boneLabel;
                boneFilterText.Align(formData);
                formData.Top.obj = boneFilterText;
                boneCombo.Align(formData);

                baseObj = boneCombo;
            }

            formData.Left.Set(null, margin);
            formData.Right = new AttachData(null, margin + WIDTH_SCROLLBAR);
            formData.Top.obj = baseObj;
            formData.Top.offset = margin;
            posSlider.indent = indent;
            posSlider.subLabelWidth = indent;
            posSlider.TextWidth  = textWidth;
            posSlider.TextHeight = itemHeight;
            posSlider.Align(formData);

            var leftBackup = formData.Left;
            formData.Left = null;
            var gizmoWidth = posGizmoToggle.CalcWidth();
            formData.Right.Set(null, margin2 + WIDTH_RESET + WIDTH_SCROLLBAR, gizmoWidth);
            posGizmoToggle.Align(formData);

            formData.Left = leftBackup;
            formData.Left.Set(null, margin);
            formData.Top.obj = posSlider;
            formData.Top.offset = margin2;
            formData.Right.Set(null, margin + WIDTH_SCROLLBAR);
            eulerSlider.indent = indent;
            eulerSlider.subLabelWidth = indent;
            eulerSlider.TextWidth = textWidth;
            eulerSlider.TextHeight = itemHeight;
            eulerSlider.Align(formData);

            quatSlider.indent = indent;
            quatSlider.subLabelWidth = indent;
            quatSlider.TextWidth = textWidth;
            quatSlider.TextHeight = itemHeight;
            quatSlider.Align(formData);

            formData.Top.obj = posSlider;
            formData.Left = null;
            formData.Right.Set(null, margin2 + WIDTH_RESET + WIDTH_SCROLLBAR, gizmoWidth);
            rotGizmoToggle.Align(formData);

            var rotToggleWidth = rotToggle.CalcWidth();
            formData.Left = leftBackup;
            formData.Left.Set(null, margin2 + textWidth + indent * 3, rotToggleWidth);
            formData.Right = null;
            rotToggle.Align(formData);

            CalcViewHeight();
            base.Layout(uiParamSet);
        }

        internal void UpdateUISize() {
            Margin = uiParamSet.margin;
            foreach (var child in Children) {
                child.Margin = margin;
            }
            var fontSizeN = uiParamSet.fontSize;
            var fontSizeS = uiParamSet.fontSizeS;
            scaleSlider.FontSize = fontSizeN;
            scaleSlider.FontSizeS = fontSizeS;
            speedSlider.FontSize = fontSizeN;
            speedSlider.FontSizeS = fontSizeS;
            endFrameSlider.FontSize = fontSizeN;
            endFrameSlider.FontSizeS = fontSizeS;
            frameSlider.FontSize = fontSizeN;
            frameSlider.FontSizeS = fontSizeS;
            delaySlider.FontSize = fontSizeN;
            delaySlider.FontSizeS = fontSizeS;
            postDelaySlider.FontSize = fontSizeN;
            postDelaySlider.FontSizeS = fontSizeS;
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
            rotToggle.FontSize = fontSizeN;
            posGizmoToggle.FontSize = fontSizeN;
            rotGizmoToggle.FontSize = fontSizeN;
        }

        #region Fields
        private const string EMITTER_NAME = "___CurrentEmitter";
        private const int WIDTH_RESET = 50;
        private readonly MaidHolder _maidHolder = new MaidHolder();
        private readonly EventHandler CheckValidate;
        protected float viewHeight;

        // scale slider
        public CustomTextLogSlider scaleSlider;
        public CustomTextLogSlider speedSlider;
        public CustomTextSlider endFrameSlider;
        public CustomTextSlider frameSlider;
        public CustomTextSlider delaySlider;
        public CustomTextSlider postDelaySlider;
        public CustomColorSlider colorSlider;

        public CustomLabel attachLabel;
        public CustomToggle attachToggle;
        public CustomToggle boneToggle;

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
        private CustomToggle rotToggle;

        public CustomToggle posGizmoToggle;
        public CustomToggle rotGizmoToggle;
        public CustomGizmoRender posGizmo;
        public CustomGizmoRender rotGizmo;

        #endregion
    }
}
