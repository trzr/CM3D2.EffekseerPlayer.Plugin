using System;
using System.Text;
using EffekseerPlayer.CM3D2.Util;
using EffekseerPlayer.Effekseer;
using UnityEngine;

namespace EffekseerPlayer.CM3D2.Data {

    [Serializable]
    public class PlayRecipe : ISerializationCallbackReceiver {
        private static readonly Vector3 POS_ZERO = Vector3.zero;
        private static readonly Quaternion QUAT = Quaternion.identity;
        [NonSerialized]
        public bool selected;
        [NonSerialized]
        public bool loaded;
        [NonSerialized]
        public EffekseerEmitter emitter;

        private RecipeSet _parent;
        public RecipeSet Parent {
            get { return _parent; }
            set {
                _parent = value;
                _recipeId = null;
            }
        }
        public EffekseerEmitter.EmitterStatus Status {
            get {
                return emitter == null ? EffekseerEmitter.EmitterStatus.Empty : emitter.Status;
            }
        }

        public string name;
        public string effectName;
        public bool autoStart;
        private bool repeat;
        public bool Repeat {
            get { return repeat; }
            set {
                repeat = value;
                if (emitter != null) {
                    emitter.loop = repeat;
                }
            }
        }

        //public bool ignoreAttachRot;

        // アタッチ指定
        public bool attach;
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        private string attachSlotID;
        public TBody.SlotID? attachSlot;
        public string attachBone;
        public bool fixOffset;
        public bool fixLocation;
        public bool fixRotation;
        public bool useLocalRotation;
        public string maid;             // メイドの識別子
        public float scale = 1f;
        public float speed = 1f;
        public float endFrame;
        public float delayFrame;
        public float postDelayFrame;
        public Color color = Color.white;
        public Vector3 location;
        public Quaternion rotation;

        ~PlayRecipe() {
            Destroy();
        }

        /// <summary>
        /// 割り当てられたエミッターがあれば破棄する.
        /// 再生していたら停止し、オブジェクトを破棄する
        /// </summary>
        internal void Destroy() {
            if (emitter == null) return;

            emitter.Stop();
            UnityEngine.Object.Destroy(emitter);
            emitter = null;
        }

        private string _recipeId;
        public string RecipeId {
            get {
                // ReSharper disable once InvertIf
                if (_recipeId == null) {
                    var buf = new StringBuilder();
                    if (_parent != null) {
                        buf.Append(_parent.name);
                    }

                    buf.Append(':').Append(name);
                    _recipeId = buf.ToString();
                }
                return _recipeId;
            }
        }

        /// <summary>
        /// レシピとエミッターの関連付けを行い、
        /// エミッターにレシピの設定を反映する.
        /// </summary>
        /// <param name="emt">エミッター</param>
        public void Load(EffekseerEmitter emt) {
            emitter = emt;

            emitter.EffectName = effectName;
            emitter.loop = repeat;
            emitter.Speed = speed;
            emitter.EndFrame = endFrame;
            emitter.delayFrame = delayFrame;
            emitter.postDelayFrame = postDelayFrame;
            emitter.SetAllColor(color);
            emitter.transform.localScale = One * scale;

            emitter.enabled = Attach();
        }
        
        /// <summary>
        /// エミッターに対して、アタッチ情報の再設定を行う.
        /// エミッターが未設定の場合は何もしない.
        /// </summary>
        public void Reload() {
            if (emitter == null) return;
            emitter.enabled = Attach();
        }

        private bool Attach() {
            if (attach) {
                emitter.fixLocation = fixLocation;
                emitter.fixRotation = fixRotation;

                var boneTrans = GetBoneTrans();
                if (boneTrans != null) {
                    emitter.transform.SetParent(boneTrans, false);
                    if (!fixLocation && fixOffset) {
                        emitter.OffsetLocation = location;
                    } else {
                        emitter.OffsetLocation = null;
                        emitter.transform.localPosition = location;    
                    }

                    emitter.useLocalRotation = useLocalRotation;
                    emitter.transform.localRotation = rotation;
                    return true;
                }

                Log.Debug("load emitter. but bone not found:", attachBone, ", effectName:", effectName);
                return false;
            }

            emitter.fixLocation = true;
            emitter.fixRotation = true;
            emitter.OffsetLocation = null;
            emitter.transform.localPosition = POS_ZERO;
            emitter.transform.localRotation = QUAT;
            emitter.transform.position = location;
            emitter.transform.rotation = rotation;
            return true;
        }

        private Transform GetBoneTrans() {
            if (!attachSlot.HasValue) return null; // 直接jsonを変更したケースでしか発生しない

            var m = MaidHelper.GetMaid(guid: maid);
            if (m == null) {
                Log.Info("Attach target(maid) not found. recipe:", name, ", guid:", maid);
                return null;
            }

            var slot = m.body0.goSlot[(int)attachSlot.Value];
            if (slot != null && slot.obj != null) return MaidHelper.SearchBone(slot.obj.transform, attachBone);

            Log.Info("Attach target slot is empty. maid:", MaidHelper.GetName(m), ", recipe:", name, ", slot:", attachSlot.Value);
            return null;
        }

        private static readonly Vector3 One = Vector3.one;
        private void WriteField(StringBuilder builder, string key, object value, bool pretty, string indent, bool useComma=true) {
            if (value != null) {
                builder.Append(indent);
                builder.Append('"').Append(key).Append("\": ");
                builder.Append('"').Append(value).Append('"');
                if (useComma) builder.Append(',');
                if (pretty) builder.Append("\n");
            }
        }
        private void WriteField(StringBuilder builder, string key, float value, bool pretty, string indent, bool useComma=true) {
            builder.Append(indent);
            builder.Append('"').Append(key).Append("\": ");
            builder.Append(value);
            if (useComma) builder.Append(',');
            if (pretty) builder.Append("\n");
        }
        private void WriteField(StringBuilder builder, string key, bool value, bool pretty, string indent, bool useComma=true) {
            builder.Append(indent);
            builder.Append('"').Append(key).Append("\": ");
            builder.Append(value ? "true" : "false");
            if (useComma) builder.Append(',');
            if (pretty) builder.Append("\n");
        }
        private void WriteField(StringBuilder builder, string key, Color col, bool pretty, string indent, bool useComma=true) {
            builder.Append(indent);
            builder.Append('"').Append(key).Append("\": ");
            builder.Append("{ \"r\": ").Append(col.r)
                .Append(", \"g\": ").Append(col.g)
                .Append(", \"b\": ").Append(col.b)
                .Append(", \"a\": ").Append(col.a).Append(" }");
            if (useComma) builder.Append(',');
            if (pretty) builder.Append("\n");
        }
        private void WriteField(StringBuilder builder, string key, Vector3 vec, bool pretty, string indent, bool useComma=true) {
            builder.Append(indent);
            builder.Append('"').Append(key).Append("\": ");
            builder.Append("{ \"x\": ").Append(vec.x).Append(", \"y\": ").Append(vec.y).Append(", \"z\": ").Append(vec.z).Append(" }");
            if (useComma) builder.Append(',');
            if (pretty) builder.Append("\n");
        }

        private void WriteField(StringBuilder builder, string key, Quaternion rot, bool pretty, string indent, bool useComma=true) {
            builder.Append(indent);
            builder.Append('"').Append(key).Append("\": ");
            builder.Append("{ \"x\": ").Append(rot.x).Append(", \"y\": ").Append(rot.y).Append(", \"z\": ").Append(rot.z).Append(", \"w\": ").Append(rot.w).Append(" }");
            if (useComma) builder.Append(',');
            if (pretty) builder.Append("\n");
        }

        public void ToJSON(StringBuilder builder, bool pretty=false, string indent="") {
            builder.Append(indent).Append("{");
            if (pretty) builder.Append("\n");
            var subIndent = indent;
            if (pretty) subIndent += "  ";
            WriteField(builder, "name", name, pretty, subIndent);
            WriteField(builder, "effectName", effectName, pretty, subIndent);
            // WriteField(builder, "autoStart", autoStart, pretty, subIndent);
            if (repeat) WriteField(builder, "repeat", repeat, pretty, subIndent);
            WriteField(builder, "attach", attach, pretty, subIndent);

            if (attachSlot.HasValue) {
                WriteField(builder, "attachSlotID", attachSlot.Value, pretty, subIndent);
            }
            WriteField(builder, "attachBone", attachBone, pretty, subIndent);
            WriteField(builder, "fixLocation", fixLocation, pretty, subIndent);
            WriteField(builder, "fixRotation", fixRotation, pretty, subIndent);
            WriteField(builder, "useLocalRotation", useLocalRotation, pretty, subIndent);
            WriteField(builder, "maid", maid, pretty, subIndent);
            if (Math.Abs(scale - 1f) > ConstantValues.EPSILON) {
                WriteField(builder, "scale", scale, pretty, subIndent);
            }
            if (Math.Abs(speed - 1f) > ConstantValues.EPSILON) {
                WriteField(builder, "speed", speed, pretty, subIndent);
            }
            if (endFrame > 0) {
                WriteField(builder, "endFrame", endFrame, pretty, subIndent);
            }
            if (delayFrame > 0) {
                WriteField(builder, "delayFrame", delayFrame, pretty, subIndent);
            }
            if (postDelayFrame > 0) {
                WriteField(builder, "postDelayFrame", postDelayFrame, pretty, subIndent);
            }
            if (color != Color.white) {
                WriteField(builder, "color", color, pretty, subIndent);
            }
            WriteField(builder, "location", location, pretty, subIndent);
            WriteField(builder, "rotation", rotation, pretty, subIndent, false);
            builder.Append(indent).Append("}");
        }

        public void OnBeforeSerialize() {
            attachSlotID = attachSlot.HasValue ? attachSlot.ToString() : null;
        }

        public void OnAfterDeserialize() {
            if (string.IsNullOrEmpty(attachSlotID)) return;
            attachSlot = (TBody.SlotID)Enum.Parse(typeof(TBody.SlotID), attachSlotID);
        }
    }
}