using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EffekseerPlayer.Effekseer;
using UnityEngine;

namespace EffekseerPlayer {
    public sealed class Settings {

        private static readonly Settings INSTANCE = new Settings();
        public static Settings Instance {
            get { return INSTANCE; }
        }

        public KeyCode  toggleKey = KeyCode.F12;
        public EventModifiers toggleModifiers = EventModifiers.None;
        public string playStopKeyCode;
        public string playStopKeyCodeVR;
        public string playPauseKeyCode;
        public string playPauseKeyCodeVR;

        public string colorPresetDir;
        public int  colorPresetMax = 50;

        public string efkDir;
        //internal string currentDir;

        public float maxLocation = 2f;
        public float maxScale = 10f;
        public float maxSpeed = 10f;
        public float maxFrame = 6000f;

        public float sizeRate;
        public bool ssWithoutUI; 
//        public int detectRate = 60;
 
        private const int MAX_SCENES = 256;
        public List<int> enableScenes;
        public List<int> disableScenes;
        public List<int> enableOHScenes;
        public List<int> disableOHScenes;

        //// エディタのシーンビューに描画するかどうか
        //public bool drawInSceneView = EffekseerSystem.drawInSceneView;
        // エフェクトインスタンスの最大数
        public int effectInstances  = EffekseerSystem.effectInstances;
        // 描画できる四角形の最大数
        public int maxSquares       = EffekseerSystem.maxSquares;
        // サウンドインスタンスの最大数
        public int soundInstances   = EffekseerSystem.soundInstances;
        // エフェクトの座標系
        public bool isRightHandledCoordinateSystem = EffekseerSystem.isRightHandledCoordinateSystem;
        // 歪みエフェクトを有効にする
        public bool enableDistortion = EffekseerSystem.enableDistortion;
        // 音声を多重再生
        public bool suppressMultiplePlaySound;

        public delegate void UpdateHandler(Settings settings);
        public UpdateHandler Updated = delegate { };

        // 設定の読み込み
        public void Load(Func<string, string> getValue) {
            //Get(getValue("drawInSceneView"), ref drawInSceneView);
            Get(getValue("effectInstances"), ref effectInstances);
            Get(getValue("maxSquares"),      ref maxSquares);
            Get(getValue("soundInstances"), ref soundInstances);
            Get(getValue("isRightHandledCoordinateSystem"), ref isRightHandledCoordinateSystem);
            Get(getValue("enableDistortion"), ref enableDistortion);
            Get(getValue("suppressMultiplePlaySound"), ref suppressMultiplePlaySound);

            //Get(getValue("DetectNamePostfix"),  ref namePostfix);
            //namePostfixWithExt = namePostfix + ".tex";

            Get(getValue("ToggleWindow"), ref toggleKey);
            Get(getValue("PlayStopKeyCode"), ref playStopKeyCode);
            Get(getValue("PlayStopKeyCodeVR"), ref playStopKeyCodeVR);
            Get(getValue("PlayPauseKeyCode"), ref playPauseKeyCode);
            Get(getValue("PlayPauseKeyCodeVR"), ref playPauseKeyCodeVR);
            string keylist = null;
            if (Get(getValue("ToggleWindowModifier"), ref keylist) && keylist != null) {
                toggleModifiers = EventModifiers.None;
                
                // カンマで分割後trm
                keylist = keylist.ToLower();
                if (keylist.Contains("alt")) {
                    toggleModifiers |= EventModifiers.Alt;
                }
                if (keylist.Contains("control")) {
                    toggleModifiers |= EventModifiers.Control;
                }
                if (keylist.Contains("shift")) {
                    toggleModifiers |= EventModifiers.Shift;
                }
            }

            var listStr = string.Empty;
            Get(getValue("EnableScenes"),    ref listStr);
            if (listStr.Length > 0) ParseList(listStr, ref enableScenes);
            listStr = string.Empty;
            Get(getValue("EnableOHScenes"),  ref listStr);
            if (listStr.Length > 0) ParseList(listStr, ref enableOHScenes);
            listStr = string.Empty;
            Get(getValue("DisableScenes"),  ref listStr);
            if (listStr.Length > 0) ParseList(listStr, ref disableScenes);
            listStr = string.Empty;
            Get(getValue("DisableOHScenes"),  ref listStr);
            if (listStr.Length > 0) ParseList(listStr, ref disableOHScenes);

            Get(getValue("efkDir"), ref efkDir);
            Get(getValue("ColorPresetDir"), ref colorPresetDir);
            Get(getValue("ColorPresetMax"), ref colorPresetMax);

            Get(getValue("SSWithoutUI"), ref ssWithoutUI);
            Get(getValue("WindowSizeRate"), ref sizeRate);
            Get(getValue("SliderMaxLocation"), ref maxLocation);
            Get(getValue("SliderMaxScale"), ref maxScale);
            Get(getValue("SliderMaxSpeed"), ref maxSpeed);
            Get(getValue("SliderMaxEndFrame"), ref maxFrame);

            Updated(this);
        }

        public void Save(Action<string, string> setValue) {
            setValue("WindowSizeRate", sizeRate.ToString(CultureInfo.InvariantCulture));
//            setValue("DetectRate", detectRate.ToString());
            setValue("efkDir",  efkDir);
        }

        private void ParseList(string valString, ref List<int> ret) {
            var list0 = valString.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => {
                            int val;
                            return int.TryParse(p, out val) ? val : -1;
                        })
                .Where (val => (val > 0 && val < MAX_SCENES))
                .OrderByDescending(val => val);
            if (list0.Any()) {
                ret = list0 as List<int>;
            }
        }

        private bool Get(string valString, ref bool output) {
            bool v;
            if (!bool.TryParse(valString, out v)) return false;

            output = v;
            return true;
        }

        private void Get(string numString, ref int output) {
            int v;
            if (int.TryParse(numString, out v)) {
                output = v;
            }
        }

        private void Get(string numString, ref float output) {
            float v;
            if (float.TryParse(numString, out v)) {
                output = v;
            }
        }

        private bool Get(string stringVal, ref string output) {
            if (stringVal == null) return false;

            output = stringVal;
            return true;
        }

        private void Get(string keyString, ref KeyCode output) {
            if (string.IsNullOrEmpty(keyString)) return;

            try {
                var key = (KeyCode)Enum.Parse(typeof(KeyCode), keyString);
                output = key;
            } catch(ArgumentException) { }
        }
    }
}
