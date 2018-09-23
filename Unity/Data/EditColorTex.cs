using System;
using EffekseerPlayer.Unity.Util;
using UnityEngine;

namespace EffekseerPlayer.Unity.Data {
    /// <summary>
    /// カラーを扱うエディット用データクラス.
    ///
    /// カラーと対応したtextureを保持し、カラーの変更に対してtextureを連動する.
    /// また、エディットを行うためのRGBマップtexture、輝度textureも併せて管理する.
    /// </summary>
    public abstract class EditColorTex : EditTextColor {
        protected EditColorTex(ref Color val1, Texture2D mapBaseTex, Texture2D lightTex) : base(ref val1) {
            if (mapBaseTex == null || lightTex == null) {
                throw new NullReferenceException("mapBaseTex and lightTex must be not null.");
            }
            this.mapBaseTex = mapBaseTex;
            this.lightTex = lightTex;
        }

        #region Methods
        protected void SearchPos(Texture2D tex, ref Color col, out Vector2 destPos) {
            var min = 3f;
            var minx = 0;
            var miny = 0;
            for (var x = 0; x < tex.width; x++) {
                for (var y = 0; y < tex.height; y++) {
                    var dist = ColorUtils.Diff(tex.GetPixel(x, y), col);
                    if (dist < 0.001f) {
                        destPos.x = x;
                        destPos.y = tex.height - 1 - y;
                        return;
                    }
                    if (dist < min) {
                        min = dist;
                        minx = x;
                        miny = y;
                    }
                }
            }
            destPos.x = minx;
            destPos.y = tex.height - 1 - miny;
        }

        /// <summary>
        /// 輝度を設定し、カラーマップを輝度に応じたテクスチャに変更する.
        /// 設定に応じて変更通知を行う.
        /// </summary>
        /// <param name="value">輝度値</param>
        /// <param name="notify">通知の有無</param>
        /// <returns>変更を行った場合にtrueを返す</returns>
        protected bool SetLight(float value, bool notify=true) {
            if (Equals(light, value)) return false;

            light = value;
            ColorUtils.Transfer(mapBaseTex, MapTex, value);

            if (notify) ValueChanged(ref color);
            return true;
        }

        /// <summary>
        /// カラーを設定し、指定したカラーから輝度の算出、カラーマップの位置特定、カラーアイコンの更新を行う.
        /// 設定に応じて変更通知を行う.
        /// </summary>
        /// <param name="value">カラー値</param>
        /// <param name="notify">通知の有無</param>
        /// <returns>変更された場合にtrueを返す</returns>
        public override bool SetColor(ref Color value, bool notify=true) {
            return SetColor(ref value, null, notify);
        }

        /// <summary>
        /// カラーを設定し、指定したカラーから輝度の算出、カラーマップの位置特定、カラーアイコンの更新を行う.
        /// 設定に応じて変更通知を行う.
        /// </summary>
        /// <param name="value">カラー値</param>
        /// <param name="code">カラーコード</param>
        /// <param name="notify">通知の有無</param>
        /// <returns>変更された場合にtrueを返す</returns>
        protected bool SetColor(ref Color value, string code=null, bool notify=true) {
            var changed = base.SetColor(ref value, false);
            if (!changed) return false;

            UpdateColor(ref value, code);

            if (notify) ValueChanged(ref value);
            return true;
        }

        protected void WriteColor(ref Color value, string code) {
            base.WriteColor(ref value);

            UpdateColor(ref value, code);
        }

        protected override bool SetColorValueImpl(int idx, float editVal1) {
            var changed = base.SetColorValueImpl(idx, editVal1);
            if (!changed) return false;

            UpdateColor(ref color, null);
            return true;
        }
        protected override bool SetTextImpl(int idx, string editText) {
            var changed = base.SetTextImpl(idx, editText);
            if (!changed) return false;

            UpdateColor(ref color, null);
            return true;
        }

        // Lightの変更があれば先に行う必要がある
        private void UpdateColor(ref Color value, string code) {
            // 内部処理として、輝度変更を先とする(light変更でMapTexが変わる)
            SetLight(Math.Max(value.r, Math.Max(value.g, value.b)), false);

            var posColor = GetMapColor(ref pos);
            if (value != posColor) {
                SearchPos(MapTex, ref value, out pos);
            }
            UpdateTexColor(ref value);

            if (code == null) code = ColorUtils.ToCode(ref value);
            colorCode = code;
        }

        /// <summary>
        /// カラーコードを設定する.
        /// 既存のカラーコードと同じ場合は何もしない.
        /// 有効なカラーコードであれば、textやcolorに反映し、設定に応じて変更通知を行う.
        /// </summary>
        /// <param name="code">カラーコード</param>
        /// <param name="notify">変更通知の有無</param>
        /// <returns>変更された場合にtrueを返す</returns>
        public bool SetColorCode(string code, bool notify=true) {
            if (code == colorCode) return false;
            if (!ColorUtils.IsColorCode(code)) return false;

            var r = Uri.FromHex(code[1]) * 16 + Uri.FromHex(code[2]);
            var g = Uri.FromHex(code[3]) * 16 + Uri.FromHex(code[4]);
            var b = Uri.FromHex(code[5]) * 16 + Uri.FromHex(code[6]);
            var color1 = new Color(r/255f, g/255f, b/255f, color.a);

            WriteColor(ref color1, code);
            if (notify) ValueChanged(ref color1);

            return true;
        }

        public void UpdateTexColor(ref Color col) {
            UpdateTexColor(ref col, colorIconBorder);
        }

        public void UpdateTexColor(ref Color col, int frame) {
            var tex = (colorTex == null) ? ColorTex : colorTex;
            var blockWidth  = tex.width - frame * 2;
            var blockHeight = tex.height - frame * 2;
            var pixels = tex.GetPixels(frame, frame, blockWidth, blockHeight, 0);
            for (var i = 0; i< pixels.Length; i++) {
                pixels[i] = col;
            }
            tex.SetPixels(frame, frame, blockWidth, blockHeight, pixels);
            tex.Apply();
        }

//        private bool IsValidPos(Texture2D tex1, ref Color col1, ref Vector2 pos1) {
//            return col1 == tex1.GetPixel((int) pos1.x, tex1.height - (int) pos1.y);
//        }

        internal Color GetMapColor(ref Vector2 pos1) {
            return MapTex.GetPixel((int)pos1.x, MapTex.height - (int)pos1.y);
        }

        internal Color GetMapColor(int x, int y) {
            return MapTex.GetPixel(x, MapTex.height - y);
        }
        #endregion

        #region Fields
        // ベースとなるテクスチャ
        public readonly Texture2D lightTex;
        public readonly Texture2D mapBaseTex;
        /// <summary>色テクスチャの縁サイズ</summary>
        public int colorIconBorder = 0;
        // 外部から指定する必要のあるUIサイズ
        /// <summary>カラーアイコンの幅</summary>
        public int colorIconWidth = 32;
        /// <summary>カラーアイコンの高さ</summary>
        public int colorIconHeight = 16;

        public Vector2 pos;
        #endregion

        #region Properties
        private Texture2D colorTex;
        /// <summary>カラー テクスチャ(透過を含まないRGBのみの色を表示)</summary>
        public Texture2D ColorTex {
            set { colorTex = value; }
            get {
                if (colorTex == null) {
                    colorTex = new Texture2D(colorIconWidth, colorIconHeight, TextureFormat.RGB24, false);
                }
                return colorTex;
            }
        }

        protected string colorCode;
        /// <summary>カラーコード</summary>
        public string ColorCode {
            set { SetColorCode(value, true); }
            get { return colorCode; }
        }

        protected float light = 1;
        /// <summary>輝度(0-1)</summary>
        public float Light {
            set {
                if (!SetLight(value, false)) return;

                // 輝度変更に合わせて、カラー情報を更新する
                var color1 = GetMapColor(ref pos);
                UpdateTexColor(ref color1);
                colorCode = ColorUtils.ToCode(ref color1);

                ValueChanged(ref color1);
            }
            get { return light; }
        }

        private Texture2D mapTex;
        /// <summary>カラーマップ テクスチャ</summary>
        public Texture2D MapTex {
            get {
                if (mapTex == null) {
                    mapTex = new Texture2D(mapBaseTex.width, mapBaseTex.height, mapBaseTex.format, false);
                    ColorUtils.Transfer(mapBaseTex, mapTex, light);
                }
                return mapTex;
            }
        }
        #endregion
    }
    /// <summary>
    /// RGBの編集値を扱うクラス.
    /// </summary>
    public sealed class EditRGBTex : EditColorTex {

        public EditRGBTex(Color val1, Texture2D mapBaseTex, Texture2D lightTex)
            : this(ref val1, mapBaseTex, lightTex) {}

        public EditRGBTex(ref Color val1, Texture2D mapBaseTex, Texture2D lightTex)
            : base(ref val1, mapBaseTex, lightTex) {
            Size = 3;
            WriteColor(ref val1, null);
        }
    }

    /// <summary>
    /// RGBAの編集値を扱うクラス.
    /// </summary>
    public sealed class EditRGBATex : EditColorTex {

        public EditRGBATex(Color val1, Texture2D mapBaseTex, Texture2D lightTex)
            : this(ref val1, mapBaseTex, lightTex) {}

        public EditRGBATex(ref Color val1, Texture2D mapBaseTex, Texture2D lightTex)
            : base(ref val1, mapBaseTex, lightTex) {
            Size = 4;
            WriteColor(ref val1, null);
        }
    }
}
