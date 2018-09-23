using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EffekseerPlayer.Unity.Util;
using EffekseerPlayer.Util;
using UnityEngine;

namespace EffekseerPlayer.Unity.Data {
    public class ColorPresetManager {
        #region Static Fields/Properties
        public static readonly ColorPresetManager Instance = new ColorPresetManager();
        #endregion

        public readonly List<Texture2D> presetIcons = new List<Texture2D>();
        public readonly List<string> presetCodes = new List<string>();

        private Texture2D emptyIcon;
        public Texture2D EmptyIcon {
            get {
                if (emptyIcon == null) {
                    emptyIcon = ResourceHolder.Instance.PresetEmptyIcon;
                }

                return emptyIcon;
            }
            set { emptyIcon = value; }
        }
        private Texture2D baseIcon;
        public Texture2D BaseIcon {
            get {
                if (baseIcon == null) {
                    baseIcon = ResourceHolder.Instance.PresetBaseIcon;
                }

                return baseIcon;
            }
            set { baseIcon = value; }
        }
        private Texture2D focusIcon;
        public Texture2D FocusIcon {
            get {
                if (focusIcon == null) {
                    focusIcon = ResourceHolder.Instance.PresetFocusIcon;
                }

                return focusIcon;
            }
            set { focusIcon = value; }
        }

        public int maxCount = 50;

        public int Count {
            get { return presetIcons.Count; }
        }
        public string PresetPath { get; private set; }

        /// <summary>
        /// カラープリセットのCSVファイルパスを設定する.
        /// 設定しない場合はロードも保存もできない.
        /// </summary>
        /// <param name="path">CSVファイルパス</param>
        public void SetPath(string path) {
            PresetPath = path;
            var loaded = Load();
            Log.Info("Color preset filepath: ", path, ", loaded=", loaded);
        }

        /// <summary>
        /// 指定した位置の色情報が有効であるか判断する.
        /// カラーコードの書式の正しさはチェックしない
        /// </summary>
        /// <param name="idx">位置</param>
        /// <returns></returns>
        public bool IsValid(int idx) {
            if (0 <= idx && idx < presetCodes.Count) {
                // return ColorUtils.IsColorCode(presetCodes[idx]);
                return presetCodes[idx].Length > 0;
            }

            return false;
        }

        public void ClearColor(int idx) {
            if (idx < 0 && presetCodes.Count <= idx) return;
            presetCodes[idx] = string.Empty;
            presetIcons[idx].SetPixels32(EmptyIcon.GetPixels32(0), 0);
            presetIcons[idx].Apply();

            Save();
        }

        public void SetColor(int idx, string code, ref Color col) {
            presetCodes[idx] = code;
            SetTexColor(ref col, BaseIcon, presetIcons[idx]);
            Save();
        }

        public void SetTexColor(ref Color col, Texture2D srcTex, Texture2D dstTex) {
            var pixels = srcTex.GetPixels32(0);
            for (var i = 0; i< pixels.Length; i++) {
                if (pixels[i].a > 0f) {
                    pixels[i] = col;
                }
            }
            dstTex.SetPixels32(pixels, 0);
            dstTex.Apply();
        }

        /// <summary>
        /// 指定されているPresetPathからプリセット情報をロードする.
        /// パスが設定されていなかったり、存在しない場合は何もしない.
        /// それ以外ではいったん保持しているカラーコードとアイコンを破棄して、ロードしなおす
        /// </summary>
        /// <returns>ロードに成功した場合にtrueを返す</returns>
        public bool Load() {
            if (PresetPath == null || !File.Exists(PresetPath)) return false;

            presetCodes.Clear();
            presetIcons.Clear();

            var loaded = false;
            var empty = EmptyIcon;
            try {
                var presets = File.ReadAllText(PresetPath, Encoding.UTF8);
                var codes = presets.Split(',');
                foreach (var code in codes) {
                    var trimmedCode = code.Trim();
                    var col = ColorUtils.ToColor(trimmedCode);
                    Texture2D tex;
                    if (col.a > 0f) {
                        presetCodes.Add(trimmedCode);
                        var baseTex = BaseIcon;
                        tex = new Texture2D(baseTex.width, baseTex.height, baseTex.format, false);
                        SetTexColor(ref col, baseTex, tex);
                    } else {
                        presetCodes.Add(string.Empty);
                        tex = CreateEmpty();
                    }

                    presetIcons.Add(tex);
                    if (presetIcons.Count >= maxCount) break;
                }

                loaded = true;
            } catch (Exception e) {
                Log.Error("カラープリセットのロードに失敗しました。", PresetPath, e);
            }
            for (var i = presetIcons.Count; i < maxCount; i++) {
                var tex = new Texture2D(empty.width, empty.height, empty.format, false);
                tex.SetPixels32(empty.GetPixels32(0), 0);
                tex.Apply();
                presetIcons.Add(tex);
                presetCodes.Add(string.Empty);
            }

            return loaded;
        }

        private Texture2D CreateEmpty() {
            var empty = EmptyIcon;
            var tex = new Texture2D(empty.width, empty.height, empty.format, false);
            tex.SetPixels32(empty.GetPixels32(0), 0);
            tex.Apply();
            return tex;
        }

        public bool Save() {
            if (PresetPath == null) return false;

            try {
                using (var writer = new StreamWriter(PresetPath, false, Encoding.UTF8, 8192)) {
                    foreach (var code in presetCodes) {
                        writer.Write(code);
                        writer.Write(',');
                    }
                }
            } catch (IOException e) {
                Log.Error("カラープリセットの保存に失敗しました。", PresetPath, e);
                return false;
            }

            Log.Debug("save to color preset:", PresetPath);
            return true;
        }
    }
}
