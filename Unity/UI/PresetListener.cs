using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EffekseerPlayerPlugin.Unity.UI {
    ///
    /// プリセット操作用リスナー
    /// 
    public class PresetListener<T> {
        public PresetListener(string text, int width1, Action<T> action1) {
            label = new GUIContent(text);
            width = width1;
            action = action1;
        }
        public PresetListener(GUIContent label1, int width1, Action<T> action1) {
            label = label1;
            width = width1;
            action = action1;
        }

        public readonly int width;
        public readonly GUIContent label;
        public readonly Action<T> action;
    }
}
