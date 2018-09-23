using System;
using UnityEngine;

namespace EffekseerPlayer.Unity.Data {
    /// <summary>
    /// カラー編集値を扱うクラス.
    /// </summary>
    public abstract class EditTextColor {
        internal static readonly EditRange RANGE_HDR = new EditRange(3, 0f, 2f);
        internal static readonly EditRange RANGE = new EditRange(3, 0f, 1f);

        protected EditTextColor(ref Color val1) { }

        public EditRange GetRange(int idx) {
            if (useHDR && idx <= 2) {
                return RANGE_HDR;
            }
            return RANGE;
        }

        protected void SynchToTexts() {
            for (var i = 0; i < size; i++) {
                var range1 = GetRange(i);
                Texts[i] = color[i].ToString(range1.Format);
                Synched[i] = true;
            }
        }

        // 現在値と異なる場合に値を設定する
        public virtual bool SetColor(ref Color color1, bool notify = true) {
            if (color == color1) return false;

            for (var i = 0; i < size; i++) {
                SetColorValueImpl(i, color1[i]);
            }

            if (notify) ValueChanged(ref color);
            return true;
        }

        // カラー値を書き込み、テキストに反映する
        protected void WriteColor(ref Color color1) {
            color = color1;
            SynchToTexts();
        }

        public void SetColorValue(int idx, float editVal1, bool notify=true) {
            var changed = SetColorValueImpl(idx, editVal1);
            if (changed && notify) ValueChanged(ref color);
        }

        protected virtual bool SetColorValueImpl(int idx, float editVal1) {
            var oldVal = color[idx];
            if (Equals(oldVal, editVal1)) return false;

            Synched[idx] = true;
            var range = GetRange(idx);

            if (decimalCheck) editVal1 = (float) Math.Round(editVal1, range.Decimal);
            color[idx] = editVal1;
            Texts[idx] = editVal1.ToString(range.Format);

            return true;
        }

        public void SetText(int idx, string editText, bool notify=false) {
            var changed = SetTextImpl(idx, editText);
            if (!changed) return;

            if (notify) ValueChanged(ref color);
        }

        protected virtual bool SetTextImpl(int idx, string editText) {
            if (Texts[idx] == editText) return false;
            Texts[idx] = editText;

            float editVal;
            if (!float.TryParse(editText, out editVal)) {
                Synched[idx] = false;
                return false;
            }

            var range = GetRange(idx);
            editVal = range.Clamp(editVal);

            Synched[idx] = true;
            if (Equals(color[idx], editVal)) return false; // 変更チェック
            color[idx] = editVal;

            return true;
        }

        public float GetValue(int idx) {
            switch (idx) {
            case 0:
                return color.r;
            case 1:
                return color.g;
            case 2:
                return color.b;
            case 3:
                return color.a;
            }
            return 0;
        }

        public void Multiply(float rate, bool notify=false) {
            var col = color;

            var max = MaxRGB();
            col.r = Mathf.Clamp(col.r * rate, 0f, max);
            col.g = Mathf.Clamp(col.g * rate, 0f, max);
            col.b = Mathf.Clamp(col.b * rate, 0f, max);

            SetColor(ref col, notify);
        }

        public void Multiply(float rate, int idx, bool notify = false) {
            var before = color[idx];
            var max = (idx < 3) ? MaxRGB() : 1f;
            var after = Mathf.Clamp(before*rate, 0f, max);
            if (Equals(before, after)) return;

            SetColorValue(idx, after, true);
        }

        public void Add(float[] diff, bool notify = false) {
            var col = color;
            var max = MaxRGB();

            var length = Math.Min(diff.Length, 2);
            for (var i = 0; i < length; i++) {
                col[i] = Mathf.Clamp(col[i] + diff[i], 0f, max);
            }

            if (3 <= diff.Length) {
                col[3] = Mathf.Clamp01(col[3] + diff[3]);
            }
            SetColor(ref col, notify);
        }

        public void Add(int idx, float diff, bool notify = false) {
            var col = color;

            var max = (idx < 3) ? MaxRGB() : 1f;
            col[idx] = Mathf.Clamp(col[idx] + diff, 0f, max);

            SetColor(ref col, notify);
        }

        public void Add(float diff, bool notify = false) {
            var col = color;
            var max = MaxRGB();
            col.r = Mathf.Clamp(col.r + diff, 0f, max);
            col.g = Mathf.Clamp(col.g + diff, 0f, max);
            col.b = Mathf.Clamp(col.b + diff, 0f, max);
            if (col == color) return;

            SetColor(ref col, notify);
        }

        protected bool Equals(float val1, float val2) {
            return Math.Abs(val1 - val2) < EPSILON;
        }

        public string GetText(int idx) {
            return Texts[idx];
        }

        public float MaxRGB() {
            return useHDR ? 2f : 1f;
        }

        #region Properties
        public float this[int i] {
            get { return color[i]; }
        }
        
        protected Color color;
        public Color Value {
            get { return color; }
            set {
                SetColor(ref value, true);
            }
        }

        protected int size;
        public int Size {
            protected set {
                if (size != value) {
                    size = value;
                    Texts = new string[size];
                    Synched = new bool[size];
                }
            }
            get { return size; }
        }

        /// <summary>RGB[A]のテキスト表現となる配列</summary>
        public string[] Texts { protected set; get; }
        /// <summary>テキストと浮動小数点の値が一致しているかを表すフラグ</summary>
        public bool[] Synched { protected set; get; }
        #endregion

        #region Fields
        public static readonly string[] LABELS = {"R", "G", "B", "A"};
        public const float EPSILON = 0.0001f;
        public bool decimalCheck;
        public bool useHDR = false;

        #endregion

        #region Events
        public delegate void ColorChangeHandler(ref Color col);
        /// <summary>変更されたColorを通知するイベントハンドラ</summary>
        public ColorChangeHandler ValueChanged = delegate { };
        #endregion
    }

    /// <summary>
    /// RGBの編集値を扱うクラス.
    /// </summary>
    public sealed class EditTextRGB : EditTextColor {

        public EditTextRGB(Color val1) : this(ref val1) { }

        public EditTextRGB(ref Color val1) : base(ref val1) {
            Size = 3;
            WriteColor(ref val1);
        }
    }

    /// <summary>
    /// RGBAの編集値を扱うクラス.
    /// </summary>
    public sealed class EditTextRGBA : EditTextColor {

        public EditTextRGBA(Color val1) : this(ref val1) {}

        public EditTextRGBA(ref Color val1) : base(ref val1) {
            Size = 4;
            WriteColor(ref val1);
        }
    }
}
