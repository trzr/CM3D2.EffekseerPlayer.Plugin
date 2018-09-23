using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EffekseerPlayer.Unity.UI {
    ///
    /// プリセット操作用リスナー
    ///
    public abstract class BasePresetListener {
        protected BasePresetListener(string text, int width1)
            :this(new GUIContent(text), width1) { }
        protected BasePresetListener(GUIContent label1, int width1) {
            label = label1;
            width = width1;
        }
        protected BasePresetListener(string label1, GUIStyle style)
            : this(new GUIContent(label1), style) { }
        protected BasePresetListener(GUIContent label1, GUIStyle style) {
            label = label1;
            width = (int)style.CalcSize(label).x;
        }

        public readonly int width;
        public readonly GUIContent label;

    }

    public class PresetListener<T> : BasePresetListener {
        public PresetListener(string text, int width1, Action<T> action1)
            :this(new GUIContent(text), width1, action1) { }
        public PresetListener(GUIContent label1, int width1, Action<T> action1) : base(label1, width1) {
            action = action1;
        }
        public PresetListener(string label1, GUIStyle style, Action<T> action1)
            : this(new GUIContent(label1), style, action1) { }
        public PresetListener(GUIContent label1, GUIStyle style, Action<T> action1) : base(label1, style) {
            action = action1;
        }

        public readonly Action<T> action;
    }

    public class SubPresetListener<T1, T2> : BasePresetListener {
        public SubPresetListener(string text, int width1, Action<T1, T2> action1)
            :this(new GUIContent(text), width1, action1) { }
        public SubPresetListener(GUIContent label1, int width1, Action<T1, T2> action1) : base(label1, width1) {
            action = action1;
        }
        public SubPresetListener(string label1, GUIStyle style, Action<T1, T2> action1)
            : this(new GUIContent(label1), style, action1) { }
        public SubPresetListener(GUIContent label1, GUIStyle style, Action<T1, T2> action1) : base(label1, style) {
            action = action1;
        }

        public readonly Action<T1, T2> action;
    }
}
