using System;
using UnityEngine;

namespace EffekseerPlayer.Unity.Util {
    public static class ColorUtils {
        private static readonly Color Empty = Color.clear;
        private const float EPSILON = 0.001f;
        private const float RANGE_UNIT = 3f / Mathf.PI; // 角度の単位

        private static bool Equals(float f1, float f2) {
            return Mathf.Abs(f1 - f2) < EPSILON;
        }
        /// <summary>
        /// 指定された文字列がカラーコードを表す文字列かを判定する.
        /// #から始まる7文字であり、２文字目以降から16進数の文字列である必要がある.
        /// ただし、大文字小文字は区別しない
        /// </summary>
        /// <param name="code">文字列</param>
        /// <returns>カラーコードである場合にtrueを返す</returns>
        public static bool IsColorCode(string code) {
            if (code != null && code.Length == 7 && code[0] == '#') {
                for (var i = 1; i < 7; i++) {
                    if (!Uri.IsHexDigit(code[i])) return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// カラーコードを表す文字列から、Unityカラーを取得する.
        /// ただし、カラーコードが正しくない場合はColor.clear(0,0,0,0)を返す.
        /// </summary>
        /// <param name="code">カラーコード</param>
        /// <returns>カラー</returns>
        public static Color ToColor(string code) {
            if (!IsColorCode(code)) return Empty;

            var r = Uri.FromHex(code[1]) * 16 + Uri.FromHex(code[2]);
            var g = Uri.FromHex(code[3]) * 16 + Uri.FromHex(code[4]);
            var b = Uri.FromHex(code[5]) * 16 + Uri.FromHex(code[6]);
            return new Color(r/255f, g/255f, b/255f);
        }

        /// <summary>
        /// カラーからカラーコード(RGB)へ変換する.
        /// A値は考慮されない.
        /// </summary>
        /// <param name="color">カラー</param>
        /// <returns>カラーコード</returns>
        public static string ToCode(ref Color color) {
            var r = (int)(color.r * 255);
            var g = (int)(color.g * 255);
            var b = (int)(color.b * 255);

            // 4つまでなら、+が早い? http://dobon.net/vb/dotnet/string/concat.html
            return '#' + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
        }

        /// <summary>
        /// 指定したテクスチャに、指定色を設定する.
        /// ただし、変更対照は、フレームで指定された幅の分だけ内側の領域のみ.
        /// </summary>
        /// <param name="targetTex">対照のテクスチャ</param>
        /// <param name="col">色</param>
        /// <param name="frame">フレーム幅</param>
        public static void SetTexColor(Texture2D targetTex, ref Color col, int frame) {
            if (frame == 0) {
                SetTexColor(targetTex, ref col);
            } else {
                var blockWidth = targetTex.width - frame * 2;
                var blockHeight = targetTex.height - frame * 2;
                var pixels = targetTex.GetPixels(frame, frame, blockWidth, blockHeight, 0);
                for (var i = 0; i < pixels.Length; i++) {
                    pixels[i] = col;
                }

                targetTex.SetPixels(frame, frame, blockWidth, blockHeight, pixels);
                targetTex.Apply();
            }
        }

        /// <summary>
        /// 指定したテクスチャに、指定色を設定する.
        /// </summary>
        /// <param name="targetTex">対照のテクスチャ</param>
        /// <param name="col">色</param>
        public static void SetTexColor(Texture2D targetTex, ref Color col) {
            var pixels = targetTex.GetPixels32(0);
            for (var i = 0; i < pixels.Length; i++) {
                pixels[i] = col;
            }

            targetTex.SetPixels32(pixels, 0);
            targetTex.Apply();
        }

        /// <summary>
        /// 指定したテクスチャに、指定色を設定する.
        /// </summary>
        /// <param name="targetTex">対照のテクスチャ</param>
        /// <param name="col">色</param>
        public static void SetTexColor(Texture2D targetTex, ref Color32 col) {
            var pixels = targetTex.GetPixels32(0);
            for (var i = 0; i < pixels.Length; i++) {
                pixels[i] = col;
            }

            targetTex.SetPixels32(pixels, 0);
            targetTex.Apply();
        }

        /// <summary>
        /// RGB -> HSL変換
        /// Vector4:(H, S, L, Alpha)
        /// h: [0, 1]
        /// s: [0, 1]
        /// l: [0, 1]
        /// </summary>
        /// <param name="c">カラー</param>
        /// <returns>HSL+alphaの4値ベクター</returns>
        // https://www.peko-step.com/tool/hslrgb.html
        public static Vector4 RGB2HSL(ref Color c) {
            var r = Mathf.Clamp01(c.r);
            var g = Mathf.Clamp01(c.g);
            var b = Mathf.Clamp01(c.b);

            var max = Mathf.Max(r, Mathf.Max(g, b));
            var min = Mathf.Min(r, Mathf.Min(g, b));

            var h = 0f;
            var s = 0f;
            var l = (max + min) * 0.5f;
            var cnt = max - min; // 収束値CNT

            if (Equals(cnt, 0f)) return new Vector4(h, s, l, c.a);

            s = l > 0.5f ? (cnt / (2f - max - min)) : (cnt / (max + min));
            if (Equals(max, r)) {
                h = (g - b) / cnt + (g < b ? 6f : 0f);
            } else if (Equals(max, g)) {
                h = (b - r) / cnt + 2f;
            } else {
                h = (r - g) / cnt + 4f;
            }

            h /= 6f;
            return new Vector4(h, s, l, c.a);
        }

        /// <summary>
        /// HSL空間からRGB空間へ変換する.
        /// h: [0, 1]
        /// s: [0, 1]
        /// l: [0, 1]
        /// </summary>
        /// <param name="h">H:色相</param>
        /// <param name="s">S:彩度</param>
        /// <param name="l">L:輝度</param>
        /// <returns>RGBカラー</returns>
        public static Color HSL2RGB(float h, float s, float l) {
            return HSL2RGB(h, s, l, 1f);
        }

        // HSL -> RGB 変換
        public static Color HSL2RGB(float h, float s, float l, float a) {
            Color c;
            c.a = a;

            if (Equals(s, 0f)) {
                c.r = l;
                c.g = l;
                c.b = l;
            } else {
                var y = (l < 0.5f) ? (l * (1f + s)) : ((l + s) - l * s);
                var x = 2f * l - y;
                c.r = Hue(x, y, h + 1f / 3f);
                c.g = Hue(x, y, h);
                c.b = Hue(x, y, h - 1f / 3f);
            }
            return c;
        }

        public static Color HSL2RGB(ref Vector4 hsl) {
            return HSL2RGB(hsl.x, hsl.y, hsl.z, hsl.w);
        }

        public static Vector3 RGB2HSV(ref Color c) {
            var r = Mathf.Clamp01(c.r);
            var g = Mathf.Clamp01(c.g);
            var b = Mathf.Clamp01(c.b);

            var max = Mathf.Max(r, Mathf.Max(g, b));
            var min = Mathf.Min(r, Mathf.Min(g, b));

            var d = max - min;
            float s;
            if (Equals(max, 0f)) {
                s = 0f;
            } else {
                s = d / max;
            }

            float h;
            if (Equals(max, r)) {
                h = (b - g) / d;
            } else if (Equals(max, g)) {
                h = (b - r) / d + 2f;
            } else {
                h = (r - g) / d + 4f;
            }

            h /= 6f;
            if (h < 0f) {
                h += 1f;
            }

            var v = max;
            return new Vector3(h, s, v);
        }

        /// <summary>
        /// HSVからRGBに変換する.
        /// </summary>
        /// <param name="h">色相 H:[0,1]</param>
        /// <param name="s">彩度 S:[0,1]</param>
        /// <param name="v">明度 V:[0,1]</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Color HSV2RGB(float h, float s, float v) {
            if (Equals(s, 0f)) {
                return new Color(v, v, v);
            }

            if (Equals(v, 0f)) {
                return Color.black;
            }

            var h0 = h * 6f;
            var hi = (int) Mathf.Floor(h0);
            var r = h0 - hi;
            var m = v * (1f - s);
            var n = v * (1f - s * r);
            var k = v * (1f - s * (1f - r));
            switch (hi) {
            case 0:
            case 6:
                return new Color(v, k, m);
            case 1:
            case 7:
                return new Color(n, v, m);
            case 2:
                return new Color(m, v, k);
            case 3:
                return new Color(m, n, v);
            case 4:
                return new Color(k, m, v);
            case 5:
                return new Color(v, m, n);
            }

            throw new ArgumentException("failed to convert Color(HSV to RGB)");
        }

        private static float Hue(float x, float y, float t) {
            if (t < 0f) {
                t += 1f;
            } else if (t > 1f) {
                t -= 1f;
            }

            if (t < 1f / 6f) return x + (y - x) * 6f * t;
            if (t < 3f / 6f) return y;
            if (t < 4f / 6f) return x + (y - x) * 6f * (4f / 6f - t);
            return x;
        }

        /// <summary>
        /// 輝度のグラデーションテクスチャを生成する.
        /// Y軸の小さい方から大きい方に向かって、輝度が小さくなる.
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <param name="frameWidth">外周縁の幅</param>
        /// <returns></returns>
        public static Texture2D CreateLightTex(int width, int height, int frameWidth) {
            var tex = new Texture2D(width+frameWidth*2, height+frameWidth*2, TextureFormat.RGB24, false);
            var denom = height - 1;
            var frameCol = Color.gray;
            for (var y = frameWidth; y < height+frameWidth; y++) {
                var r = 1 - (float)y / denom;
                var col = new Color(r, r, r);
                for (var x = frameWidth; x < width+frameWidth; x++) {
                    tex.SetPixel(x, height - 1 - y, col);
                }
                // フレーム(左右)
                for (var x = 0; x < frameWidth; x++) {
                    tex.SetPixel(x, height - 1 - y, frameCol);
                }
                for (var x = width+frameWidth; x < frameWidth+frameWidth*2; x++) {
                    tex.SetPixel(x, height - 1 - y, frameCol);
                }
            }
            // フレーム(上下)
            for (var x = 0; x < width + frameWidth * 2; x++) {
                for (var y = 0; y < frameWidth; y++) {
                    tex.SetPixel(x, y, frameCol);
                }
                for (var y = height + frameWidth; y < height + frameWidth * 2; y++) {
                    tex.SetPixel(x, y, frameCol);
                }
            }

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 円型のカラーマップ（RGB）を生成する.
        /// 指定された幅と高さのテクスチャを作成するが、幅と高さの小さい方に合わせた正円とする.
        /// また、円の外側は無色透明Color:(0,0,0,0)
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <returns>テクスチャ</returns>
        public static Texture2D CreateRGBMapTex(int width, int height) {
            var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            var centerX = width/2;
            var centerY = height/2;　//　Mathf.FloorToInt(height/2f);
            var radius = Math.Min(centerX, centerY);
            var centerCol = Color.white;
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var dist = Distance(x, y, centerX, centerY);
                    var distRatio = dist/radius;
                    if (1f < distRatio) {
                        tex.SetPixel(x, y, Empty);
                    } else if (Equals(distRatio, 0f)) {
                        tex.SetPixel(x, y, centerCol);
                    } else {
                        var vecX = x - centerX;
                        var vecY = y - centerY;
                        var edgeCol = GetEdgeColor(vecX, vecY, dist);

                        var color = GetColor(ref centerCol, ref edgeCol, distRatio);
                        tex.SetPixel(x, y, color);
                    }
                }
            }

            return tex;
        }

        /// <summary>
        /// 円型のカラーマップの外周色を取得する.
        /// 指定位置が外周以上の外側である想定し、それ以外の値の場合は考慮していない.
        /// </summary>
        /// <param name="vecX">ベクトルのX方向</param>
        /// <param name="vecY">ベクトルのY方向</param>
        /// <param name="dist">位置の距離</param>
        /// <returns>外周の色</returns>
        public static Color GetEdgeColor(float vecX, float vecY, float dist) {
            var theta = (float)Math.Acos(vecX / dist);
            if (vecY > 0) theta = -theta;

            theta *= RANGE_UNIT;
            if (-0.5f <= theta && theta < 0.5f) {
                var rotRatio = 0.5f - theta;
                return new Color(rotRatio, 0f, 1f);
            } else if (0.5f <= theta && theta < 1.5f) {
                var rotRatio = theta - 0.5f;
                return new Color(0f, rotRatio, 1f);
            } else if (1.5f <= theta && theta < 2.5f) {
                var rotRatio = 2.5f - theta;
                return new Color(0f, 1f, rotRatio);
            } else if (2.5f <= theta && theta <= 3f) {
                var rotRatio = theta - 2.5f;
                return new Color(rotRatio, 1f, 0f);
            } else if (-3f <= theta && theta < - 2.5f) {
                var rotRatio = theta + 3.5f;
                return new Color(rotRatio, 1f, 0f);
            } else if (-2.5f <= theta && theta < -1.5f) {
                var rotRatio = -1.5f - theta;
                return new Color(1f, rotRatio, 0f);
            } else {//if (-1.5f <= theta && theta < -0.5f) {
                var rotRatio = theta + 1.5f;
                return new Color(1f, 0f, rotRatio);
            }
        }

        /// <summary>
        /// 2点(A,B)の距離を算出する
        /// </summary>
        /// <param name="x1">AのX座標</param>
        /// <param name="y1">AのY座標</param>
        /// <param name="x2">BのX座標</param>
        /// <param name="y2">BのY座標</param>
        /// <returns>距離</returns>
        public static float Distance(int x1, int y1, int x2, int y2) {
            var dX = x1 - x2;
            var dY = y1 - y2;
            return Mathf.Sqrt(dX * dX + dY * dY);
        }

        /// <summary>2色と比率から、２色間の比率に合わせた割合の色を抽出する</summary>
        /// <param name="c1">色1</param>
        /// <param name="c2">色2</param>
        /// <param name="ratio">割合(0-1)</param>
        /// <returns>色</returns>
        private static Color GetColor(ref Color c1, ref Color c2, float ratio) {
            var r = c1.r + ratio * (c2.r - c1.r);
            var g = c1.g + ratio * (c2.g - c1.g);
            var b = c1.b + ratio * (c2.b - c1.b);
            return new Color(r, g, b);
        }

        public static void Sub(ref Color col, float delta) {
            if (col.r < delta) col.r = 0;
            else col.r -= delta;
            if (col.g < delta) col.g = 0;
            else col.g -= delta;
            if (col.b < delta) col.b = 0;
            else col.b -= delta;
        }

        public static void Add(ref Color col, float delta, float max) {
            if (col.r + delta > max) col.r = max;
            else col.r += delta;
            if (col.g + delta > max) col.g = max;
            else col.g += delta;
            if (col.b + delta > max) col.b = max;
            else col.b += delta;
        }

        /// <summary>
        /// ソースのテクスチャから、比率を指定して宛先のテクスチャへ色を転送する.
        /// 転送元・先のテクスチャは同じサイズを前提とし、A値はソースのまま転送する.
        /// </summary>
        /// <param name="srcTex">転送元テクスチャ</param>
        /// <param name="dstTex">転送先テクスチャ</param>
        /// <param name="ratio">比率</param>
        public static void Transfer(Texture2D srcTex, Texture2D dstTex, float ratio) {
            var src = srcTex.GetPixels32(0);
            var dst = dstTex.GetPixels32(0);
            var maxIndex = dstTex.width * dstTex.height;
            for (var i = 0; i < maxIndex; i++) {
                dst[i].r = (byte)(src[i].r * ratio);
                dst[i].g = (byte)(src[i].g * ratio);
                dst[i].b = (byte)(src[i].b * ratio);
                dst[i].a = src[i].a;
            }
            dstTex.SetPixels32(dst, 0);
            dstTex.Apply();
        }

        /// <summary>
        /// 2色(RGB)の距離を算出する.
        /// 距離：各色要素の値の差分の絶対値和
        /// </summary>
        /// <param name="c1">色1</param>
        /// <param name="c2">色2</param>
        /// <returns>距離</returns>
        public static float Diff(Color c1, Color c2) {
            return Math.Abs(c1.r - c2.r) + Math.Abs(c1.g - c2.g) + Math.Abs(c1.b - c2.b);
        }

        /// <summary>
        /// 2色(RGB)の距離を算出する.
        /// 距離：各色要素の値の差分の絶対値和
        /// </summary>
        /// <param name="c1">色1</param>
        /// <param name="c2">色2</param>
        /// <returns>距離</returns>
        public static float Diff(ref Color c1, ref Color c2) {
            return Math.Abs(c1.r - c2.r) + Math.Abs(c1.g - c2.g) + Math.Abs(c1.b - c2.b);
        }
    }
}
