using System;
using UnityEngine;

namespace EffekseerPlayerPlugin.Unity.Data {

    /// <summary>
    /// RGBの編集値を扱うクラス.
    /// TODO 未完成
    /// </summary>
    public class EditTextRGB {
        internal static readonly EditRange RANGE = new EditRange(3, 0f, 2f);

        public EditTextRGB(ref Color val1) {
            texts = ToText(ref val1);
            isDirties = new bool[texts.Length];
            _value = val1;
            size = texts.Length;
        }

        public virtual EditRange GetRange(int idx) {
            return RANGE;
        }

        protected virtual string[] ToText(ref Color c) {
            return new[] {
                c.r.ToString(RANGE.Format),
                c.g.ToString(RANGE.Format),
                c.b.ToString(RANGE.Format)};
        }

        public float GetValue(int idx) {
            switch (idx) {
            case 0:
                return _value.r;
            case 1:
                return _value.g;
            case 2:
                return _value.b;
            case 3:
                return _value.a;
            }
            return 0;
        }

        /// <summary>
        /// 変更チェックなしに強制的に上書きする.
        /// 変更通知は行わない.
        /// </summary>
        /// <param name="val1">変更値</param>
        internal void Set(ref Color val1) {
            _value = val1;

            for (int i = 0; i < texts.Length; i++) {
                texts[i] = val1[i].ToString(RANGE.Format);
                isDirties[i] = false;
            }
        }

        internal void SetValue(int idx, float editVal1) {
            if (idx >= texts.Length) return;

            var range = GetRange(idx);
            var old = _value[idx];

            if (Math.Abs(old - editVal1) <= epsilon) return;

            _value[idx] = editVal1;
            texts[idx] = editVal1.ToString(range.Format);

            isDirties[idx] = true;
            ValueChanged(this, EventArgs.Empty);
        }

        internal void SetText(int idx, string editText) {
            if (idx >= texts.Length) return;

            if (texts[idx] == editText) return;
            texts[idx] = editText;

            float v;
            //bool synched = false;
            if (!float.TryParse(editText, out v)) return;

            var range = GetRange(idx);
            range.TryEval(v, out v);
            //else synched = true;

            if (Math.Abs(_value[idx] - v) <= epsilon) return; // 変更チェック

            _value[idx] = v;
            isDirties[idx] = true;

            ValueChanged(this, EventArgs.Empty);
        }

        public virtual Color Value {
            get { return _value; }
            set {
                if (_value == value) return;

                bool hasChanged = false;
                for (var i = 0; i < size; i++) {
                    var range1 = GetRange(i);
                    var v = value[i];
                    if (decimalCheck) {
                        v = (float) Math.Round(v, range1.Decimal);
                    }
                    if (Math.Abs(v - _value[i]) <= epsilon) continue;
                    hasChanged = true;// 変更チェック

                    _value[i] = v;
                    isDirties[i] = true;
                    texts[i] = v.ToString(range1.Format); 
                }
                if (!hasChanged) return;

                ValueChanged(this, EventArgs.Empty);
            }
        }

        public string GetText(int idx) {
            return texts[idx];
        }

        #region Fields
        protected int size;
        public float epsilon = 0.00001f;
        //public bool Filtered;
        public bool decimalCheck;

        //public float srcValue;

        protected Color _value;

        public bool[] isDirties;
        private string[] texts;
        #endregion

        #region Events
        public readonly EventHandler ValueChanged = delegate { };
        #endregion
    }

    /// <summary>
    /// RGBAの編集値を扱うクラス.
    /// 
    /// </summary>
    public class EditTextRGBA : EditTextRGB
    {
        internal static readonly EditRange RANGE_A = new EditRange(3, 0f, 1f);

        public EditTextRGBA(ref Color val1) : base(ref val1) { }

        protected override string[] ToText(ref Color c) {
            return new[] {
                c.r.ToString(RANGE.Format),
                c.g.ToString(RANGE.Format),
                c.b.ToString(RANGE.Format),
                c.a.ToString(RANGE_A.Format)};
        }

        public override EditRange GetRange(int idx) {
            return (idx == 3) ? RANGE_A : RANGE;
        }

        #region Fields

        #endregion
    }
}