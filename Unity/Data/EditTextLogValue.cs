using System;
using UnityEngine;

namespace EffekseerPlayer.Unity.Data {
    /// <summary>
    /// 編集用の文字列と浮動小数点の変数を扱うクラス.
    /// 
    /// SrcVal:取得値
    /// val:スライダー値
    /// editVal:テキストフィールドの値
    /// isSynchedは _valueと _textの同期状態
    /// IsDirtyは_valueの変更状態（SrcValueとの不一致）
    /// </summary>
    public class EditTextLogValue : EditTextValue {
        #region Methods
        public EditTextLogValue(string name, float val1, int _decimal, float min, float max)
            : this(name, val1, new EditRange(_decimal, min, max)) { }

        public EditTextLogValue(string name, float val1, EditRange range, bool decimalCheck = false)
            : this(name, val1, range, null, decimalCheck) {
        }

        public EditTextLogValue(string name, float val1, EditRange range, EventHandler handler, bool decimalCheck = false)
            : base(name, val1, range, handler, decimalCheck) {
            if (range.Min <= 0) throw new ArgumentOutOfRangeException("range", "min value must be positive value");

            LogMin = Mathf.Log10(range.Min);
            LogMax = Mathf.Log10(range.Max);
        }

        /// <summary>
        /// 値をセットする.
        /// notifyに応じて変更通知、
        /// withCheckに応じて範囲チェックを行う.
        /// </summary>
        /// <param name="val1">変更値</param>
        /// <param name="notify">変更通知</param>
        /// <param name="withCheck">変更チェックの有無</param>
        /// <returns>変更した場合にtrueを返す</returns>
        public override bool Set(float val1, bool notify=false, bool withCheck=false) {
            var changed = base.Set(val1, notify, withCheck);
            LogValue = (float)Math.Log10(Value);
            return changed;
        }

        /// <summary>
        /// テキスト値をセットする.
        /// notifyに応じて変更通知、
        /// withCheckに応じて範囲チェックを行う.
        /// テキスト値の場合はテキスト値の変更で戻り値の変更を判定する.
        /// </summary>
        /// <param name="val1">変更値</param>
        /// <param name="notify">変更通知</param>
        /// <param name="withCheck">範囲チェックの有無</param>
        /// <returns>変更した場合にtrueを返す</returns>
        public override bool Set(string val1, bool notify=false, bool withCheck=false) {
            var changed = base.Set(val1, notify, withCheck);
            if (changed) LogValue = (float)Math.Log10(Value);
            return changed;
        }

        public override void ReflectToSrc(float value1) {
            base.ReflectToSrc(value1);
            LogValue = (float)Math.Log10(Value);
        }
        #endregion

        #region Fields/Properties
        public float LogValue { get; set; }
        public float LogMin { get; private set; }
        public float LogMax { get; private set; }

        public override float Value {
            get { return val; }
            set {
                Set(value, true);
            }
        }
        /// テキストを設定する. 値のチェック
        public override string Text {
            get { return text; }
            set {
                Set(value, true, true);
            }
        }
        #endregion
    }
}
