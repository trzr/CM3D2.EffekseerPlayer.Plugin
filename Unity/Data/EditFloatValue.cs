using System;
using UnityEngine;

namespace EffekseerPlayer.Unity.Data {
    /// <summary>
    /// 編集用の浮動小数点の変数を扱うクラス.
    /// 
    /// SrcValue:取得値
    /// Value   :変数値
    /// IsDirtyはValueの変更状態（SrcValueとの不一致）
    /// </summary>
    public class EditFloatValue {
        #region Methods
        public EditFloatValue(float val1, int _decimal, float min, float max) 
            : this(val1, new EditRange(_decimal, min, max)) { }

        public EditFloatValue(float val1, EditRange range, bool decimalCheck=false) {
            this.decimalCheck = decimalCheck;
            this.range = range;
            epsilon = Mathf.Pow(10, -range.Decimal-1);
            srcValue = val1;
            SetInternal( val1 );
        }

        public EditFloatValue(float val1, EditRange range, EventHandler handler, bool decimalCheck=false)  
            : this(val1, range, decimalCheck) {
            ValueChanged += handler;
        }

        private bool SetInternal(float value1, bool notify=false, bool withCheck=false) {
            var v = value1;
            if (withCheck) {
                range.TryEval(value1, out v);
                if (decimalCheck) v = (float) Math.Round(v, range.Decimal);
            }

            if (notify) {
                if (Equals(v - val)) return false; // 変更チェック

                val = v;
                ValueChanged(this, EventArgs.Empty);
            } else {
                val = v;
            }
            return true;
        }

        /// <summary>
        /// 値をセットする.
        /// notifyに応じて変更通知、
        /// withCheckに応じて範囲チェックを行う.
        /// </summary>
        /// <param name="value1">変更値</param>
        /// <param name="notify">変更通知</param>
        /// <param name="withCheck">範囲チェックの有無</param>
        /// <returns>変更した場合にtrueを返す</returns>
        public virtual bool Set(float value1, bool notify=false, bool withCheck=false) {
            if (Equals(val, value1)) return false;

            return SetInternal(value1, notify, withCheck);
        }

        public virtual void ReflectToSrc(float value1) {
            srcValue = value1;
            val = value1;
        }

        public void Multiply(float rate, bool notify=false) {
            Set(val * rate, notify, true);
        }

        public void Add(float rate, bool notify=false) {
            Set(val + rate, notify, true);
        }

        public bool Equals(float val1, float val2) {
            return Math.Abs(val1 - val2) < epsilon;
        }

        #endregion

        #region Fields/Properties
        public readonly float epsilon;
        public readonly bool decimalCheck;
        public bool filtered;

        public float srcValue;
        public bool IsDirty {
            get { return !Equals(srcValue, val); }
        }

        protected float val;
        public virtual float Value {
            get { return val; }
            set {
                Set(value, true);
            }
        }

        protected readonly EditRange range;
        public float Max { get { return range.Max;} }
        public float Min { get { return range.Min;} }

        public float SoftMax {
            get { return range.SoftMax; }
            set { range.SoftMax = value; }
        }

        public float SoftMin {
            get { return range.SoftMin;}
            set { range.SoftMin = value; }
        }
        #endregion

        #region Events
        public EventHandler ValueChanged = delegate { };
        #endregion
    }
}
