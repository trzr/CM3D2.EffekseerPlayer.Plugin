using System;
using EffekseerPlayerPlugin.Unity.Data;
using UnityEngine;

namespace EffekseerPlayerPlugin.Unity.UI
{
    /// <summary>
    /// CustomTextSliderを拡張し、スライダーを対数の縮尺で動作させるクラス.
    /// </summary>
    public class CustomTextLogSlider : BaseSingleTextSlider<EditTextLogValue>
    {
        #region Methods
        /// <summary>コンストラクタ.</summary>
        /// <param name="parent">親要素</param>
        protected CustomTextLogSlider(GUIControl parent) : base(parent) { }

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="value">初期値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="dec">小数点桁数</param>
        public CustomTextLogSlider(GUIControl parent, float value, 
            float min, float max, int dec) : base(parent, value, min, max, dec)  { }

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="value">初期値</param>
        /// <param name="range">値の範囲</param>
        public CustomTextLogSlider(GUIControl parent, float value, EditRange range)
            : base(parent, value, range) { }

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="val">編集値オブジェクト</param>
        public CustomTextLogSlider(GUIControl parent, EditTextLogValue val) : base(parent, val) { }

//        protected override EditTextLogValue CreateInstance(string name, float value, EditRange range) {
//            return new EditTextLogValue(name, value, range);
//        }

//        private void InitLogValue() {
//            if (Value.Min <= 0) throw new ArgumentOutOfRangeException("", "min value must be positive value");
//        }

        protected override void OnSlider() {
            var logVal = GUI.HorizontalSlider(sliderRect, Value.LogValue, Value.LogMin, Value.LogMax);
            if (Math.Abs(logVal - Value.LogValue) > Value.epsilon) {
                // 値が変更された場合のみ、対数から元の値に戻してセット
                Value.Value = Mathf.Pow(10f, logVal);
            }
        }

        #endregion

        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Events
        #endregion
    }
}
