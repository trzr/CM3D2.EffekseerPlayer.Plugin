using System;

namespace EffekseerPlayer.Unity.Data {
    /// <inheritdoc />
    /// <summary>
    /// 編集用の文字列と浮動小数点の変数を扱うクラス.
    /// SrcVal:取得値
    /// val:スライダー値
    /// editVal:テキストフィールドの値
    /// isSynchedは _valueと _textの同期状態
    /// IsDirtyは_valueの変更状態（SrcValueとの不一致）
    /// </summary>
    public class EditTextValue : EditFloatValue {
        #region Methods
        public EditTextValue(string name, float val1, int _decimal, float min, float max)
            : this(name, val1, new EditRange(_decimal, min, max), null) { }

        public EditTextValue(string name, float val1, EditRange range, bool decimalCheck = false)
            : this(name, val1, range, null, decimalCheck) { }

        public EditTextValue(string name, float val1, EditRange range, EventHandler handler, bool decimalCheck = false)
            : base(val1, range, decimalCheck) {
            this.name = name;

            UpdateText();
            if (handler != null) {
                ValueChanged += handler;
            }
        }

        private void UpdateText() {
            text = val.ToString(range.Format);
            isSynched = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// 値をセットする.
        /// notifyに応じて変更通知、
        /// withCheckに応じて範囲チェックを行う.
        /// </summary>
        /// <param name="val1">変更値</param>
        /// <param name="notify">変更通知</param>
        /// <param name="withCheck">範囲チェックの有無</param>
        /// <returns>変更した場合にtrueを返す</returns>
        public override bool Set(float val1, bool notify=false, bool withCheck=false) {
            var changed = base.Set(val1, notify, withCheck);
            if (changed) UpdateText();

            return changed;
        }

        /// <summary>
        /// テキスト値をセットする.
        /// notifyに応じて変更通知、
        /// withCheckに応じて範囲チェックを行う.
        /// テキスト値の場合はテキスト値の変更で戻り値の変更を判定する.
        /// ただし、テキスト値が変更されたとしても、数値が変わっているとは限らないものとする.
        /// </summary>
        /// <param name="val1">変更値</param>
        /// <param name="notify">変更通知</param>
        /// <param name="withCheck">範囲チェックの有無</param>
        /// <returns>変更した場合にtrueを返す.</returns>
        public virtual bool Set(string val1, bool notify=false, bool withCheck=false) {
            if (text == val1) return false;
            text = val1;

            float v;
            isSynched = false;
            if (!float.TryParse(val1, out v)) return true;
            if (withCheck) {
                if (range.TryEval(v, out v)) isSynched = true;
                if (decimalCheck) v = (float) Math.Round(v, range.Decimal);
            }

            // withCheck=false 上記で実施済(isSynched更新用に改造が必要なため)
            base.Set(v, notify);
            return true;
        }

        public override void ReflectToSrc(float value1) {
            base.ReflectToSrc(value1);
            text = val.ToString(range.Format);
            isSynched = true;
        }
        #endregion

        #region Fields/Properties
        public readonly string name;
        public bool isSynched = true;

        public override float Value {
            get { return val; }
            set {
                Set(value, true);
            }
        }
        protected string text;
        /// <summary>
        /// テキストを設定する. 値のチェック
        /// </summary>
        public virtual string Text {
            get { return text; }
            set {
                Set(value, true, true);
            }
        }

        #endregion
    }
}
