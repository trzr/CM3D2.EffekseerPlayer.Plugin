using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EffekseerPlayerPlugin.Unity.Data {

    /// <summary>
    /// 
    /// TODO 未完成
    /// </summary>
    public class EditTextVector3 {
        public EditTextVector3(ref Vector3 val1, EditRange range) {
            _value = val1;
            this.range = range;
        }

        /// <summary>
        /// 変更チェックなしに強制的に上書きする.
        /// 変更通知は行わない.
        /// </summary>
        /// <param name="val1">変更値</param>
        internal void Set(ref Vector3 val1) {
            _value = val1;

            for (var i = 0; i < texts.Length; i++) {
                texts[i] = val1[i].ToString(range.Format);
            }
            isDirty = false;
        }

        internal void SetValue(int idx, float editVal) {
            if (idx >= texts.Length) return;

            if (decimalCheck) {
                editVal = (float)Math.Round(editVal, range.Decimal);
            }
            if (Math.Abs(editVal - _value[idx]) <= epsilon) return;

            _value[idx] = editVal;
            texts[idx] = editVal.ToString(range.Format);

            ValueChanged(this, EventArgs.Empty);
        }

        internal void SetText(int idx, string editText) {
            if (idx >= texts.Length) return;

            if (texts[idx] == editText) return;
            texts[idx] = editText;

            float v;
            if (!float.TryParse(editText, out v)) return;

            //bool synched = false;
            range.TryEval(v, out v);
            //else synched = true;

            if (Math.Abs(_value[idx] - v) <= epsilon) return; // 変更チェック

            _value[idx] = v;
            isDirty = true;

            ValueChanged(this, EventArgs.Empty);
        }

        public Vector3 Value {
            get { return _value; }
            set {
                if (_value == value) return;

                var hasChanged = false;
                for (var i = 0; i < 3; i++) {
                    var v = value[i];
                    if (decimalCheck) {
                        v = (float)Math.Round(v, range.Decimal);
                    }
                    if (Math.Abs(v - _value[i]) <= epsilon) continue;

                    hasChanged = true;// 変更チェック
                    _value[i] = v;
                    texts[i] = v.ToString(range.Format);
                }
                if (!hasChanged) return;

                isDirty = true;

                ValueChanged(this, EventArgs.Empty);
            }
        }

        public string GetText(int idx) {
            return texts[idx];
        }

        #region Fields
        public float epsilon = ConstantValues.EPSILON;
        public bool isDirty;
        public bool filtered;
        public bool decimalCheck;

        private readonly string[] texts = new string[3];
        private Vector3 _value;

        public EditRange range;
        //public float Max { get { return range.Max; } }
        //public float Min { get { return range.Min; } }
        #endregion

        #region Events
        public readonly EventHandler ValueChanged = delegate { };
        #endregion
    }
}
