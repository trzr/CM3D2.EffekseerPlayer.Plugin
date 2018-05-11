using System;
using System.Collections.Generic;
using System.Linq;
using EffekseerPlayer.CM3D2.Data;
using EffekseerPlayer.CM3D2.Render;
using EffekseerPlayer.CM3D2.Util;
using EffekseerPlayer.Effekseer;
using EffekseerPlayer.Unity.Data;
using UnityEngine;

namespace EffekseerPlayer.CM3D2.UI {
    public partial class EditRecipeScrollView {

        /// <summary>
        /// 対象メイドの無効化や、スロットの変更をチェックする.
        /// 
        /// </summary>
        private void CheckItemChanged() {
            if (_currentMaid == null) return;

            // 対象のメイドが無効化された場合
            if (!_currentMaid.enabled) {
                Log.Debug("maid disabled.", _currentMaid.name);
                _currentMaid = null;
                ReloadMaidCombo();
                return;
            }

            if (_currentSlot == null || _currentSlot.RID == _currentRID) return;

            // 衣装アイテムが変更された場合
            Log.Debug("item changed.", _currentSlot.Category);
            // スロット情報が変更された可能性があるため、スロットコンボを更新
            var slotObj = _currentSlot.obj;
            ReloadSlotCombo();

            // 対象スロットが無効になった場合：コンボの選択状態が変わりイベント通知にて伝わる
            // スロットアイテム変更の場合　　：イベント通知されないため、手動で更新
            if (slotObj != null) {
                ChangeCurrentSlot(_currentSlot);
            }
        }

        public void SetupRecipe(PlayRecipe recipe) {
            recipe.scale = scaleSlider.Num;
            recipe.speed = speedSlider.Num;
            recipe.endFrame = endFrameSlider.Num;
            recipe.delayFrame = delaySlider.Num;
            recipe.postDelayFrame = postDelaySlider.Num;
            recipe.color = _color;
            if (attachToggle.Value) {
                recipe.attach = true;
                var slotNo = SelectedSlotIndex();
                if (slotNo != -1) {
                    recipe.attachSlot = (TBody.SlotID)slotNo;
                }
                var maid = GetMaid();
                if (maid != null) {
                    recipe.maid = MaidHelper.GetGuid(maid);
                }
                
                recipe.attachBone = boneCombo.SelectedItem;
                recipe.fixOffset = fixOffsetToggle.Value;
                recipe.fixLocation = fixPosToggle.Value;
                recipe.useLocalRotation = rotScopeToggle.Value;
                recipe.fixRotation = fixRotToggle.Value;
            }
            var vals = posSlider.Value;
            //recipe.location = new Vector3(posXSlider.Num, posYSlider.Num, posZSlider.Num);
            recipe.location = new Vector3(vals[0].Value, vals[1].Value, vals[2].Value);
            recipe.rotation = GetQuat();
        }

        /// <summary>
        /// レシピ情報をエディットの各パラメータに反映する.
        /// </summary>
        /// <param name="recipe">レシピ情報</param>
        public void ToEditView(PlayRecipe recipe) {
            scaleSlider.Num = recipe.scale;
            speedSlider.Num = recipe.speed;
            endFrameSlider.Num  = recipe.endFrame;
            delaySlider.Num     = recipe.delayFrame;
            postDelaySlider.Num = recipe.postDelayFrame;
            _color = recipe.color;
            var cols = new[]{_color.r, _color.g, _color.b, _color.a };
            colorSlider.Value.Set(cols, true);
            attachToggle.Value = recipe.attach;
            if (recipe.attach) {
                fixOffsetToggle.Value = recipe.fixOffset;
                fixPosToggle.Value = recipe.fixLocation;
                fixRotToggle.Value = recipe.fixRotation;
                rotScopeToggle.Value = recipe.useLocalRotation;

                if (recipe.maid != null) {
                    var maidIndex = _maidHolder.GetMaidIndex(recipe.maid);
                    maidCombo.SelectedIndex = maidIndex;
                }
                SelectSlot(recipe.attachSlot ?? TBody.SlotID.body);
                boneCombo.SelectedItem = recipe.attachBone;
            }

            var vec3 = recipe.location;
            var vals = new[]{vec3[0], vec3[1], vec3[2] };
            posSlider.Value.Set(vals, true);
            var rot = recipe.rotation;

            if (rotToggle.Value) {
                SetEulerSlider(ref rot, true);
            } else {
                var rotVals = new[] {rot[0], rot[1], rot[2], rot[3]};
                quatSlider.Value.Set(rotVals, true);
            }
        }

        public EffekseerEmitter LoadEmitter(string effectName, bool repeat) {
            if (currentEmitter == null) {
                var go = GetGameObject();
                currentEmitter = go.AddComponent<EffekseerEmitter>();
            }

            currentEmitter.EffectName = effectName;
            currentEmitter.loop = repeat;
            currentEmitter.Speed = speedSlider.Num;
            currentEmitter.EndFrame = endFrameSlider.Num;
            currentEmitter.delayFrame = delaySlider.Num;
            currentEmitter.postDelayFrame = postDelaySlider.Num;
            currentEmitter.SetAllColor(_color);
            currentEmitter.fixLocation = true;
            currentEmitter.fixRotation = true;
            currentEmitter.transform.localScale = scaleSlider.Num * ONE;
            //var pos = new Vector3(posXSlider.Num, posYSlider.Num, posZSlider.Num);
            var vals = posSlider.Value;
            _location.x = vals[0].Value;
            _location.y = vals[1].Value;
            _location.z = vals[2].Value;
            if (!AttachSlot(currentEmitter, _location)) {
                currentEmitter.transform.SetParent(null, false);
                currentEmitter.OffsetLocation = null;
                currentEmitter.transform.localPosition = _location;
                currentEmitter.transform.localRotation = GetQuat();
            }

            return currentEmitter;
        }
        
        internal bool AttachSlot(EffekseerEmitter emitter, Vector3 pos) {
            var hasAttached = false;
            var boneName = boneCombo.SelectedItem;
            var boneTrans = GetBoneTransform();
            if (boneTrans != null) {
                emitter.fixLocation = fixPosToggle.Value;
                emitter.fixRotation = fixRotToggle.Value;
                emitter.useLocalRotation = rotScopeToggle.Value;
                emitter.transform.SetParent(boneTrans, false);
                if (!emitter.fixLocation && fixOffsetToggle.Value) {
                    emitter.OffsetLocation = pos;
                } else {
                    emitter.OffsetLocation = null;
                    emitter.transform.localPosition = pos;
                }
                emitter.transform.localRotation = GetQuat();
                emitter.UpdateRotation();

                //emitter.offset = pos;
                Log.Debug("attach bone:", boneName, ", trans:", boneTrans.name);
                hasAttached = true;
            } else {
                Log.Debug("bone not found:", boneName);
            }
            return hasAttached;
        }

        private void ChangeCurrentSlot(TBodySkin slot) {
            if (slot != null && slot.obj != null) {
                _currentSlot = slot;
                _currentRID = slot.RID;
                if (ReloadBoneCombo(slot)) {
                    boneCombo.Enabled = true;
                    boneToggle.Enabled = true;
                    return;
                }
            }

            _boneRenderer.Clear();
            boneCombo.Enabled = false;
            boneToggle.Enabled = false;
            _currentSlot = slot;
            _currentRID = 0;
        }

        private void SlotChanged(object obj=null, EventArgs args=null) {
            var slot = _currentMaid != null ? SelectedSlot(_currentMaid) : null;
            ChangeCurrentSlot(slot);
        }

        private void OffsetPosChanged(object obj, EventArgs args) {
            if (currentEmitter == null) return;

            if (fixOffsetToggle.Value) {
                currentEmitter.OffsetLocation = _location;
            } else {
                currentEmitter.OffsetLocation = null;
                currentEmitter.transform.localPosition = _location;
            }
        }

        private void FixPosToggleChanged(object obj, EventArgs args) {
            if (currentEmitter == null) return;
            currentEmitter.fixLocation = fixPosToggle.Value;

            fixOffsetToggle.Enabled = !fixPosToggle.Value;
        }

        private void SkipRotChanged(object obj, EventArgs args) {
            if (currentEmitter == null) return;
            currentEmitter.fixRotation = fixRotToggle.Value;
        }

        private void RotScopeChanged(object obj, EventArgs args) {
            if (currentEmitter == null) return;

            currentEmitter.useLocalRotation = rotScopeToggle.Value;
            currentEmitter.transform.localRotation = GetQuat();
            currentEmitter.UpdateRotation();
        }

        // ReSharper disable UnusedParameter.Local
        private void BoneChanged(object obj, EventArgs args) {
            UpdateAttach(attachToggle.Value);
        }

        private void ScaleChanged(object obj, EventArgs args) {
            if (currentEmitter == null) return;

            var val = (EditTextValue)obj;
            currentEmitter.transform.localScale = val.Value * ONE;
        }

        private void SpeedChanged(object obj, EventArgs args) {
            if (currentEmitter == null) return;

            var val = (EditTextValue)obj;
            currentEmitter.Speed = val.Value;
        }

        private void EndFrameChanged(object obj, EventArgs args) {
            if (currentEmitter == null) return;

            var val = (EditTextValue)obj;
            currentEmitter.EndFrame = val.Value;
            if ((int) currentEmitter.EndFrame <= 0) {
                frameSlider.Value.SoftMax = frameSlider.Value.Max;
            } else {
                frameSlider.Value.SoftMax = currentEmitter.EndFrame;
            }
        }

        private void DelayChanged(object obj, EventArgs args) {
            if (currentEmitter == null) return;

            var val = (EditTextValue)obj;
            currentEmitter.delayFrame = val.Value;
        }

        private void PostDelayChanged(object obj, EventArgs args) {
            if (currentEmitter == null) return;

            var val = (EditTextValue)obj;
            currentEmitter.postDelayFrame = val.Value;
        }

        private void ColorsChanged(object obj, EventArgs args) {
            var vals = (EditTextValues)obj;

            _color.r = vals[0].Value;
            _color.g = vals[1].Value;
            _color.b = vals[2].Value;
            _color.a = vals[3].Value;

            if (currentEmitter == null) return;
            currentEmitter.SetAllColor(_color);
        }

        private void ColorChanged(float val, ref float col) {

            if (Math.Abs(col - val) < ConstantValues.EPSILON_RGB) return;

            col = val;
            if (currentEmitter == null) return;
            currentEmitter.SetAllColor(_color);
        }

        private void ColorRChanged(object obj, EventArgs args) {
            var val = (EditTextValue)obj;
            ColorChanged(val.Value, ref _color.r);
        }
        private void ColorGChanged(object obj, EventArgs args) {
            var val = (EditTextValue)obj;
            ColorChanged(val.Value, ref _color.g);
        }
        private void ColorBChanged(object obj, EventArgs args) {
            var val = (EditTextValue)obj;
            ColorChanged(val.Value, ref _color.b);
        }
        private void ColorAChanged(object obj, EventArgs args) {
            var val = (EditTextValue)obj;
            ColorChanged(val.Value, ref _color.a);
        }

        private void ApplyLocation() {
            if (currentEmitter == null) return;

            if (fixOffsetToggle.Value) {
                currentEmitter.OffsetLocation = _location;
            } else {
                currentEmitter.OffsetLocation = null;
                currentEmitter.transform.localPosition = _location;
            }
            currentEmitter.UpdateLocation();
        }

        private void PosChanged(float val,  Action<float> setPos) {
            setPos(val);
            ApplyLocation();

            UpdateGizmoPosition();
        }

        private void PosXChanged(object obj, EventArgs args) {
            var val = (EditTextValue)obj;
            PosChanged(val.Value, f => _location.x = f);
        }

        private void PosYChanged(object obj, EventArgs args) {
            var val = (EditTextValue)obj;
            PosChanged(val.Value, f => _location.y = f);
        }

        private void PosZChanged(object obj, EventArgs args) {
            var val = (EditTextValue)obj;
            PosChanged(val.Value, f => _location.z = f);
        }

        private void PosChanged(object obj, EventArgs args) {
            var val = (EditTextValues)obj;
            _location.x = val[0].Value;
            _location.y = val[1].Value;
            _location.z = val[2].Value;
            ApplyLocation();

            UpdateGizmoPosition();
        }

        //
        // 基本的に回転はEmitterのlocalRotationに設定する.
        // フラグにより、localRotationをそのまま使うかrotationを使うかをハンドリング.
        //
        delegate void SetTransformAction(ref Transform trans, float val);
        private void RotChanged(float val, SetTransformAction setTransform) {
            if (currentEmitter != null) {
                var trans = currentEmitter.transform;
                setTransform(ref trans, val);
                currentEmitter.UpdateRotation();
                // ApplyLocation();
            }

            UpdateGizmoRotation(GetQuat());
        }
        delegate void SetRotAction(ref Quaternion rot, float val);
        private void RotChanged(float val, SetRotAction setRotate) {
            if (currentEmitter != null) {
                var rot = currentEmitter.transform.localRotation;
                setRotate(ref rot, val);
                currentEmitter.transform.localRotation = rot;
                currentEmitter.UpdateRotation();
                // ApplyLocation();
            }

            UpdateGizmoRotation(GetQuat());
        }
        private void EulerXChanged(object obj, EventArgs args) {
            var etv = (EditTextValue)obj;
            RotChanged(etv.Value, (ref Transform trans, float val) => {
                _euler.x = val;
                trans.localRotation = Quaternion.Euler(_euler);
            });
        }

        private void EulerYChanged(object obj, EventArgs args) {
            var etv = (EditTextValue)obj;
            RotChanged(etv.Value, (ref Transform trans, float val) => {
                _euler.y = val;
                trans.localRotation = Quaternion.Euler(_euler);
            });
        }

        private void EulerZChanged(object obj, EventArgs args) {
            var etv = (EditTextValue)obj;
            RotChanged(etv.Value, (ref Transform trans, float val) => {
                _euler.z = val;
                trans.localRotation = Quaternion.Euler(_euler);
            });
        }

        private void EulerChanged(object obj, EventArgs args) {
            var etv = (EditTextValues)obj;
            RotChanged(0, (ref Transform trans, float val) => {
                _euler.x = etv[0].Value;
                _euler.y = etv[1].Value;
                _euler.z = etv[2].Value;
                trans.localRotation = Quaternion.Euler(_euler);
            });
        }

        private void RotXChanged(object obj, EventArgs args) {
            var etv = (EditTextValue)obj;
            RotChanged(etv.Value, (ref Quaternion rot, float val) => {
                rot.x = val;
            });
        }

        private void RotYChanged(object obj, EventArgs args) {
            var etv = (EditTextValue)obj;
            RotChanged(etv.Value, (ref Quaternion rot, float val) => {
                rot.y = val;
            });
        }

        private void RotZChanged(object obj, EventArgs args) {
            var etv = (EditTextValue)obj;
            RotChanged(etv.Value, (ref Quaternion rot, float val) => {
                rot.z = val;
            });
        }

        private void RotWChanged(object obj, EventArgs args) {
            var etv = (EditTextValue)obj;
            RotChanged(etv.Value, (ref Quaternion rot, float val) => {
                rot.w = val;
            });
        }
        private void RotChanged(object obj, EventArgs args) {
            var etv = (EditTextValues)obj;
            RotChanged(0f, (ref Quaternion rot, float val) => {
                rot.x = etv[0].Value;
                rot.y = etv[1].Value;
                rot.z = etv[2].Value;
                rot.w = etv[3].Value;
            });
        }

        private void UpdateGizmoPosition() {
            if (posGizmo != null && posGizmo.Visible) {
                posGizmo.transform.localPosition = _location;
            }
            if (rotGizmo != null && rotGizmo.Visible) {
                rotGizmo.transform.localPosition = _location;
            }
        }

        private void UpdateGizmoRotation(Quaternion rot) {
            if (posGizmo != null && posGizmo.Visible) {
                posGizmo.transform.localRotation = rot;
            }
            if (rotGizmo != null && rotGizmo.Visible) {
                rotGizmo.transform.localRotation = rot;
            }
        }

        private void ToLocationSlider(ref Vector3 location) {
            posSlider.Value.Set(new[]{location.x, location.y, location.z}, false, false );
        }

        private void ToRotationSlider(Quaternion rot) {
            if (eulerSlider.Enabled) {
                SetEulerSlider(ref rot, false);
            } else {
                quatSlider.Value.Set(new[] {rot.x, rot.y, rot.z, rot.w}, false, false);
            }
        }

        private void SetEulerSlider(ref Quaternion rot, bool notify) {
            _euler = rot.eulerAngles;
            // 範囲を [0, 360]から[-180, 180]に補正
            if (_euler.x > EULER_RANGE.Max) _euler.x -= EULER_RANGE.Delta;
            if (_euler.y > EULER_RANGE.Max) _euler.y -= EULER_RANGE.Delta;
            if (_euler.z > EULER_RANGE.Max) _euler.z -= EULER_RANGE.Delta;

            eulerSlider.Value.Set(new[] {_euler.x, _euler.y, _euler.z}, notify);
        }

        internal Quaternion GetQuat() {
            if (eulerSlider.Enabled) {
                _euler.x = eulerSlider.Value[0].Value;
                _euler.y = eulerSlider.Value[1].Value;
                _euler.z = eulerSlider.Value[2].Value;
                return Quaternion.Euler(_euler);
            } 
            return new Quaternion(quatSlider.Value[0].Value,
                quatSlider.Value[1].Value,
                quatSlider.Value[2].Value,
                quatSlider.Value[3].Value);
        }

        /// <summary>
        /// メイド情報のコンボボックスの内容を更新する.
        /// 選択情報が変更されたらイベント通知により、スロットコンボも更新される.
        /// </summary>
        private void ReloadMaidCombo() {
            var maidContents = _maidHolder.CreateActiveMaidContents();
            Log.Debug("maid combo updated.", maidContents.Count);
            maidCombo.Items = maidContents.ToArray();

            // 要素が一つの場合自動選択
            if (maidCombo.Count == 1) {
                maidCombo.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 選択されたメイド情報から、有効なスロットのみを抽出してコンボボックスを再生成する
        /// </summary>
        private void ReloadSlotCombo() {
            if (_allSlotContents == null) {
                _allSlotContents = CreateSlotContents();
                _slotContents = new List<GUIContent>(_allSlotContents.Length);
            }

            _slotContents.Clear();
            if (_currentMaid == null) return;
            for (var i=0; i<_allSlotContents.Length; i++) {
                var slotItem = _currentMaid.body0.goSlot[i];
                if (slotItem.obj != null && slotItem.morph != null) {
                    _slotContents.Add(_allSlotContents[i]);
                }
            }
            _slotItems = _slotContents.ToArray();
            FilterSlotCombo();
        }

        private static GUIContent[] CreateSlotContents() {
            var allSlotNames = Enum.GetNames(typeof(TBody.SlotID));
            const int max = (int)TBody.SlotID.moza;
            var items = new GUIContent[max];
            var idx = 0;
            foreach (var slotName in allSlotNames) {
                if (idx >= max) break;
                items[idx++] = new GUIContent(slotName);
            }
            return items;
        }

        private void FilterSlotCombo() {
            if (_slotItems == null) return;

            var filter = slotFilterText.Text.Trim();
            if (filter.Length == 0) {
                slotCombo.Items = _slotItems;
            } else {
                _includedSlots.Clear();
                foreach (var item in _slotItems) {
                    if (item.text.Contains(filter)) {
                        _includedSlots.Add(item);
                    }
                }
                slotCombo.Items = _includedSlots.ToArray();
            }
        }

        // ボーンコンボをリロードし、ついでにボーン表示を再セットアップ
        private bool ReloadBoneCombo(TBodySkin validSlot) {
            var boneNames = CreateBoneContents(validSlot);
            if (boneNames.Length == 0) return false;

            if (boneToggle.Value) _boneRenderer.Setup(validSlot.obj.transform);

            _boneItems = boneNames;
            FilterBoneCombo();
            return true;
        }

        private GUIContent[] CreateBoneContents(TBodySkin slot) {
            var boneNames = GetBoneNames(slot);
            var items = new GUIContent[boneNames.Count];
            var idx = 0;
            foreach (var name in boneNames) {
                items[idx++] = new GUIContent(name);
            }
            return items;
        }

        private List<string> GetBoneNames(TBodySkin slot) {
            var boneNames = new List<string>();

            var meshRenderer = slot.obj.gameObject.GetComponentInChildren<SkinnedMeshRenderer>(false);
            if (meshRenderer != null) {
                var bones = meshRenderer.bones;
                foreach (var bone in bones) {
                    boneNames.Add(bone.name);
                }
            } else {
                Log.Debug("SkinnedMeshRenderer not found");
                // SkinnedMeshRendererが無い場合の保険 (ただし、余計なボーンが含まれる場合あり _SCL_無しボーンなど)
                foreach (Transform child in slot.obj.transform) {
                    // SM_ボーン直下の子なしボーンはスキップ
                    if (child.childCount == 0) continue;

                    // アタッチされたEffekseerオブジェクトをスキップ
                    if (child.name.StartsWith(ConstantValues.EFK_PREFIX)) continue;
                    ParseBones(boneNames, child);
                }
            }
            return boneNames;
        }

        private void FilterBoneCombo() {
            if (_boneItems == null) return;

            var filter = boneFilterText.Text.Trim();
            if (filter.Length == 0) {
                boneCombo.Items = _boneItems;
            } else {
                // boneCombo.Items = _boneItems.Where(item => item.text.Contains(filter)).ToArray();
                var includedBones = new List<GUIContent>();
                foreach (var item in _boneItems) {
                    if (item.text.Contains(filter)) {
                        includedBones.Add(item);
                    }
                }
                boneCombo.Items = includedBones.ToArray();
            }
        }

        private Maid GetMaid() {
            var idx = maidCombo.SelectedIndex;
            return idx == -1 ? null : _maidHolder.Get(idx);
        }

        // _currentMaid != null
        private TBodySkin SelectedSlot() {
            var slot = SelectedSlot(_currentMaid);
            if (slot == null || slot.obj == null) return null;
            return slot;
        }

        private int SelectedSlotIndex() {
            var selectedSlot = slotCombo.SelectedItem;
            if (selectedSlot.Length == 0) return -1;

            var slotNo = TBody.hashSlotName[selectedSlot];
            if (slotNo == null) return -1;
            return (int)slotNo;
        }

        private TBodySkin SelectedSlot(Maid maid0) {
            var slotNo = SelectedSlotIndex();
            if (slotNo == -1) return null;

            Log.Debug("slotNo:", slotNo);
            return maid0.body0.goSlot[slotNo];
        }

        private void SelectSlot(TBody.SlotID slotID) {
            slotCombo.SelectedItem = Enum.GetName(typeof(TBody.SlotID), slotID);
        }

        private static void ParseBones(ICollection<string> boneNames, Transform bone) {
            boneNames.Add(bone.name);

            foreach (Transform child in bone) {
                // アタッチされたEffekseerオブジェクトをスキップ
                if (child.name.StartsWith(ConstantValues.EFK_PREFIX)) continue;

                ParseBones(boneNames, child);
            }
        }

        public bool CanRegister() {
            return !attachToggle.Value || slotCombo.SelectedIndex != -1;
        }

        public bool CanPlay() {
            return !attachToggle.Value || (slotCombo.SelectedIndex != -1 && boneCombo.SelectedIndex != -1);
        }

        private GameObject GetGameObject() {
            var go = GameObject.Find(EMITTER_NAME);
            if (go == null) {
                Log.Debug("GameObject recreated.");
                gobj = new GameObject(EMITTER_NAME);
            }
            return gobj;
        }

        private CustomGizmoRender CreateGizmo() {
            var go = GetGameObject();
            var gizmo = go.AddComponent<CustomGizmoRender>();
            gizmo.name = "___gizmo";
            gizmo.offsetScale = 0.5f;
            gizmo.lineSelectedThick = 0.1f;  // [0.04f]
            gizmo.lineRSelectedThick = 0.2f; // [0.1f]
            return gizmo;
        }

        public void ToggleGizmo(ref CustomGizmoRender gizmo, bool visible, bool eAxis, bool eRotate) {
            if (visible) {
                var boneTrans = GetBoneTransform();
                gizmo.transform.SetParent(boneTrans, false);
                gizmo.transform.localPosition = _location;
                gizmo.transform.localRotation = GetQuat();

                gizmo.eAxis = eAxis;
                gizmo.eRotate = eRotate;
                gizmo.InitStatus();
            }
            gizmo.Visible = visible;
        }

        /// <summary>
        /// アタッチ状態を更新する.
        /// アタッチフラグのほかに、メイド、スロット、ボーンの選択が行えていないとアタッチから外される.
        /// </summary>
        /// <param name="attach">アタッチの有無</param>
        private void UpdateAttach(bool attach) {
            Transform boneTrans = null;
            if (attach) {
                boneTrans = GetBoneTransform();
            }

            var rot = GetQuat();
            if (posGizmo != null && posGizmo.Visible) {
                posGizmo.transform.SetParent(boneTrans, false);
                posGizmo.transform.localPosition = _location;
                posGizmo.transform.localRotation = rot;
            }
            if (rotGizmo != null && rotGizmo.Visible) {
                rotGizmo.transform.SetParent(boneTrans, false);
                rotGizmo.transform.localPosition = _location;
                rotGizmo.transform.localRotation = rot;
            }
            if (currentEmitter != null) {
                currentEmitter.transform.SetParent(boneTrans, false);
                currentEmitter.transform.localPosition = _location;
                currentEmitter.transform.localRotation = rot;
            }
        }

        private Transform GetBoneTransform() {
            if (!attachToggle.Value || slotCombo.SelectedIndex == -1 || boneCombo.SelectedIndex == -1) return null;
            var maid0 = GetMaid();
            if (maid0 == null) return null;
            var slot = SelectedSlot(maid0);
            if (slot == null || slot.obj == null) return null;

            var boneName = boneCombo.SelectedItem;
            return MaidHelper.SearchBone(slot.obj.transform, boneName);
        }

        #region Fields
        private GameObject gobj; 
        internal EffekseerEmitter currentEmitter;
        private CustomBoneRenderer _boneRenderer;

        private Color _color = Color.white;
        private Vector3 _location;
        private Vector3 _euler;

        private GUIContent[] _allSlotContents;
        private List<GUIContent> _slotContents;
        // TODO CustomCombo内のSelectedItemに置き換え
        private Maid _currentMaid;
        private TBodySkin _currentSlot;
        private int _currentRID;

        private GUIContent[] _slotItems;
        private readonly List<GUIContent> _includedSlots = new List<GUIContent>();
        private GUIContent[] _boneItems;

        #endregion

        #region Static Fields
        private static readonly GUIContent[] EMPTY_CONTS = new GUIContent[0];
//        private static readonly GUIContent NONE = new GUIContent("none");
//        private static readonly List<string> EMPTY = new List<string>(0);

        private static readonly Vector3 ONE = new Vector3(1f, 1f, 1f);

        /// 位置座標の範囲
        private static readonly EditRange ROT_RANGE = new EditRange(5, -1f, 1f);
        private static readonly EditRange ROT_RANGEW = new EditRange(5, 0, 1f);
        private static readonly EditRange EULER_RANGE = new EditRange(5, -180, 180, true);
        private static readonly EditRange RGB_RANGE = new EditRange(2, 0f, 1f);

        //private static readonly EditRange POS_RANGE = new EditRange(5, -2f, 2f);
        //private static readonly EditRange SCALE_RANGE = new EditRange(5, 0.00001f, 2f);

        #endregion

    }
}