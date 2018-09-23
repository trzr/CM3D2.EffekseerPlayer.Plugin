using System;
using System.Collections.Generic;

namespace EffekseerPlayer.Unity.Data {
    /// <summary>
    /// 複数のEditTextValueにアクセスするクラス.
    /// 
    /// SrcVal:取得値
    /// val:スライダー値
    /// editVal:テキストフィールドの値
    /// isSynchedは _valueと _textの同期状態
    /// IsDirtyは_valueの変更状態（SrcValueとの不一致）
    /// </summary>
    public class EditTextValues {
        #region Methods
        public EditTextValues(IList<string> names, IList<float> vals, int _decimal, float min, float max)
            : this(names, vals, new EditRange(_decimal, min, max)) { }

        public EditTextValues(IList<string> names, IList<float> vals, EditRange range, 
            EventHandler handler, bool decimalCheck=false)
            : this(names, vals, range, decimalCheck) {

            if (handler != null) ValueChanged += handler;
        }
        public EditTextValues(IList<string> names, IList<float> vals, EditRange range1, bool decimalCheck=false) {
            values = new EditTextValue[vals.Count];

            for (var i=0; i< vals.Count; i++) {
                values[i] = new EditTextValue(names[i], vals[i], range1, decimalCheck);
            }
        }
        public EditTextValues(IList<string> names, IList<float> vals, IList<EditRange> ranges, 
            EventHandler handler, bool decimalCheck=false)
            : this(names, vals, ranges, decimalCheck) {

            if (handler != null) ValueChanged += handler;
        }
        public EditTextValues(IList<string> names, IList<float> vals, IList<EditRange> ranges, bool decimalCheck=false) {
            values = new EditTextValue[vals.Count];

            for (var i=0; i< vals.Count; i++) {
                values[i] = new EditTextValue(names[i], vals[i], ranges[i], decimalCheck);
            }
        }
        public EditTextValues(IList<EditTextValue> vals, EventHandler handler=null) {
            values = vals;

            if (handler != null) ValueChanged += handler;
        }

        /// <summary>
        /// 値に強制的に上書きする.
        /// notifyに応じて変更通知を行う
        /// withCheckに応じて範囲チェックを行う.
        /// </summary>
        /// <param name="values1">変更値</param>
        /// <param name="notify">通知の有無</param>
        /// <param name="withCheck">変更チェックの有無</param>
        public virtual void Set(float[] values1, bool notify=false, bool withCheck=false) {
            for (var i = 0; i < Size; i++) {
                values[i].Set(values1[i], notify, withCheck);
            }
        }

        /// <summary>
        /// 変更通知なしのセット後に強制的に通知
        /// </summary>
        /// <param name="values1">変更値</param>
        public virtual void SetWithNotify(params float[] values1) {
            for (var i = 0; i < values1.Length; i++) {
                if (i >= Size) break;
                values[i].Set(values1[i]);
            }

            ValueChanged(this, EventArgs.Empty);
        }

        public virtual void ReflectToSrc(float[] values1) {
            for (var i = 0; i < Size; i++) {
                values[i].ReflectToSrc(values1[i]);
            }
        }

        public virtual void ReflectToSrc(int idx, float value) {
            values[idx].ReflectToSrc(value);
        }

        internal virtual void Set(int idx, float val1, bool notify=false, bool withCheck=false) {
            values[idx].Set(val1, notify, withCheck);
        }

        public void Multiply(float rate, bool notify=false) {
            foreach (var val in values) {
                val.Multiply(rate);
            }
            if (notify) ValueChanged(this, EventArgs.Empty);
        }

        public void Multiply(float rate, int idx, bool notify = false) {
            values[idx].Multiply(rate, notify);
        }

        public void Add(float[] diff, bool notify = false) {
            for (var i=0; i<diff.Length && i<values.Count; i++) {
                values[i].Add(diff[i]);
            }
            if (notify) ValueChanged(this, EventArgs.Empty);
        }
        public void Add(float diff, bool notify = false) {
            foreach (var val in values) {
                val.Add(diff);
            }
            if (notify) ValueChanged(this, EventArgs.Empty);
        }

        public void Add(float diff, int idx, bool notify = false) {
            values[idx].Add(diff, notify);
        }

        internal void SetValue(int idx, float editVal) {
            values[idx].Value = editVal;
        }

        internal void SetText(int idx, string editText) {
            values[idx].Text = editText;
        }
        #endregion

        #region Fields
        public readonly IList<EditTextValue> values;
        #endregion

        #region Properties
        public EditTextValue this[int i] {
            get {
                return values[i];
            }
        }
        public int Size {
            get {
                return values.Count;
            }
        }

        public bool IsDirty { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// 変更時のイベントハンドラ
        /// ただし、個々の値の変更通知ではなく、一括変更に対する通知ハンドラ
        /// </summary>
        public EventHandler ValueChanged = delegate { };
        #endregion
    }
}
