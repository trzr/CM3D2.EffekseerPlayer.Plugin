using EffekseerPlayer.Unity.Data;

namespace EffekseerPlayer.Unity.UI {
    /// <inheritdoc />
    /// <summary>
    /// テキストフィールドとスライダー（水平）を横に並べ、連動するコントロール.
    /// warnColorは未指定の場合、red
    /// </summary>
    public class CustomTextSlider : BaseSingleTextSlider<EditTextValue> {
        #region Methods
        /// <inheritdoc />
        /// <summary>コンストラクタ.</summary>
        /// <param name="parent">親要素</param>
        protected CustomTextSlider(GUIControl parent) : base(parent) { }

        /// <inheritdoc />
        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="value">初期値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="dec">小数点桁数</param>
        public CustomTextSlider(GUIControl parent, float value, 
            float min, float max, int dec) : base(parent, value, min, max, dec)  { }

        /// <inheritdoc />
        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="value">初期値</param>
        /// <param name="range">値の範囲</param>
        public CustomTextSlider(GUIControl parent, float value, EditRange range)
            : base(parent, value, range) { }

        /// <inheritdoc />
        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="val">編集値オブジェクト</param>
        public CustomTextSlider(GUIControl parent, EditTextValue val) : base(parent, val) { }

//        protected override EditTextValue CreateInstance(string name, float value, EditRange range) {
//            return new EditTextValue(name, value, range);
//        }

        #endregion

        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Events
        #endregion
    }
}
