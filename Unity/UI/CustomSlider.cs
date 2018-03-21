using System;
using EffekseerPlayerPlugin.Unity.Data;
using UnityEngine;

namespace EffekseerPlayerPlugin.Unity.UI
{
    /// <summary>
    /// スライダーコントロール
    /// </summary>
    public class CustomSlider : GUIControl
    {
        #region Methods
        /// <summary>デフォルトコンストラクタ.</summary>
        /// <param name="parent">親要素</param>
        public CustomSlider(GUIControl parent) : base(parent) {
            // disable once DoNotCallOverridableMethodsInConstructor
            backgroundColor = Color.white;
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="val">変数値</param>
        public CustomSlider(GUIControl parent, EditFloatValue val) : this(parent) {
            Value = val;
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="value">初期値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="dec">小数点桁数</param>
        public CustomSlider(GUIControl parent, float value, float min, float max, int dec) 
            : this(parent, new EditFloatValue(value, dec, min, max)) { }

        //public override void UpdateUI() {}
        public override void OnGUI() {
            try {
                // スライダー表示
                _colorStore.SetColor(ref textColor, ref backgroundColor);
                try {
                    Value.Value = GUI.HorizontalSlider(Rect, Value.Value, Value.Min, Value.Max);
                } finally {
                    _colorStore.Restore();
                }
            } catch (Exception e) {
                Log.Error(e);
            }
        }
        #endregion

        #region Properties
        public EditFloatValue Value { get; private set; }

        #endregion
        private readonly GUIColorStore _colorStore = new GUIColorStore();

        #region Events
//        public virtual EventHandler ValueChanged
        #endregion
    }
}
