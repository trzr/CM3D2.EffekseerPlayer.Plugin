﻿using System;
using System.Linq;
using EffekseerPlayer.Unity.Data;
using UnityEngine;

namespace EffekseerPlayer.Unity.UI {
    /// <summary>
    /// テキストフィールドとスライダー（水平）を横に並べ、連動するコントロールの抽象クラス
    /// </summary>
    public abstract class BaseSingleTextSlider<T> : BaseTextSlider<T> where T : EditTextValue {
        #region Methods
        /// <summary>コンストラクタ.</summary>
        /// <param name="parent">親要素</param>
        protected BaseSingleTextSlider(GUIControl parent) : base(parent) { }

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="value">初期値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="dec">小数点桁数</param>
        protected BaseSingleTextSlider(GUIControl parent, float value,
            float min, float max, int dec) : this(parent, value, new EditRange(dec, min, max))  {
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="value">初期値</param>
        /// <param name="range">値の範囲</param>
        protected BaseSingleTextSlider(GUIControl parent, float value, EditRange range)
            : this(parent) {
            //Value = new EditTextValue(string.Empty, value, range);
            var type = typeof(T);
            Value = (T)Activator.CreateInstance(type, string.Empty, value, range, false);
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">親要素</param>
        /// <param name="val">編集値オブジェクト</param>
        protected BaseSingleTextSlider(GUIControl parent, T val) : this(parent) {
            Value = val;
        }

//        protected abstract T CreateInstance(string name, float value, EditRange range);

        protected override void DrawGUI() {
            if (Text != null) GUI.Label(labelRect, Text, LabelStyle);
            if (listeners != null) {
                var xPos = xMax;
                for (var i = listeners.Length - 1; i >= 0; i--) {
                    var listener = listeners[i];
                    presetRect.x = xPos - listener.width - margin;
                    presetRect.width = listener.width;
                    xPos -= (listener.width + margin);
                    if (GUI.Button(presetRect, listener.label, ButtonStyle)) {
                        listener.action(Value);
                    }
                }
            }

            // テキストフィールド
            if (!Value.isSynched && warnColor.HasValue) txtColorStore.SetColor(TextFStyle, warnColor.Value);
            try {
                Value.Text = GUI.TextField(textRect, Value.Text, TextFStyle);
            } finally {
                txtColorStore.Restore();
            }

            if (prevListeners != null) {
                var xPos = textRect.xMax;
                foreach (var listener in prevListeners) {
                    subPresetRect.x = xPos + margin;
                    subPresetRect.width = listener.width;
                    if (GUI.Button(subPresetRect, listener.label, ButtonStyle)) {
                        listener.action(Value);
                    }
                    xPos += listener.width + margin;
                }
            }
            if (nextListeners != null) {
                var xPos = sliderRect.xMax;
                foreach (var listener in nextListeners) {
                    subPresetRect.x = xPos + margin;
                    subPresetRect.width = listener.width;
                    xPos += (listener.width + margin);
                    if (GUI.Button(subPresetRect, listener.label, ButtonStyle)) {
                        listener.action(Value);
                    }
                }
            }
            // スライダー
            colorStore.SetColor(ref textColor, ref backgroundColor);
            try {
                OnSlider();
            } finally {
                colorStore.Restore();
            }
        }

        protected virtual void OnSlider() {
            Value.Value = GUI.HorizontalSlider(sliderRect, Value.Value, Value.SoftMin, Value.SoftMax);
        }

        protected override void Layout(UIParamSet uiParams) {

            // 各コントロールの位置調整
            var yPos = Top;
            if (Text != null) {
                var labelWidth = LabelStyle.CalcSize(new GUIContent(Text)).x;
                labelRect.Set(Left, Top, labelWidth, TextHeight);
                yPos = labelRect.yMax;
            }
            if (listeners != null) { 
                presetRect.y = labelRect.y;
                presetRect.height = TextHeight;

                yPos = presetRect.yMax;
            }

            // indent: 外から指定
            textRect.x = Left + indent;
            textRect.y = yPos;

            var prevWidth = 0f;
            var nextWidth = 0f;
            if (prevListeners != null) {
                prevWidth += prevListeners.Sum(prev => prev.width + margin);
            }
            if (nextListeners != null) {
                nextWidth += nextListeners.Sum(next => next.width + margin);
            }
            subPresetRect.y = yPos;
            subPresetRect.height = TextHeight;

            sliderRect.x = textRect.xMax + margin + prevWidth;
            sliderRect.y = yPos + margin * 2;
            sliderRect.width = Width - textRect.width - indent - margin * 2 - prevWidth - nextWidth;
            sliderRect.height = TextHeight - margin * 2;
        }
        #endregion

        #region Fields
        public Rect subPresetRect;
        public PresetListener<T>[] prevListeners;
        public PresetListener<T>[] nextListeners;
        #endregion

        #region Properties
        public T Value { get; private set; }
        public string SliderText {
            get { return Value.Text;  }
            set { Value.Text = value; }
        }
        public float Num {
            get { return Value.Value; }
            set { Value.Value = value; }
        }
        public override float TextHeight {
            get { return textRect.height; }
            set {
                textRect.height = value;
                if (Text != null || listeners != null) {
                    Height = value * 2;
                } else {
                    Height = value;
                }
            }
        }
        #endregion

        #region Events
        #endregion
    }
}
