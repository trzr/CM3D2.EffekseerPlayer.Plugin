﻿using System;
using System.Collections;
using System.IO;
using System.Reflection;
using EffekseerPlayer.CM3D2;
using EffekseerPlayer.CM3D2.Data;
using EffekseerPlayer.CM3D2.UI;
using EffekseerPlayer.Effekseer;
using EffekseerPlayer.Unity.Data;
using EffekseerPlayer.Unity.UI;
using EffekseerPlayer.Unity.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;
using UnityInjector.Attributes;

[assembly: AssemblyVersion("0.3.1")]
namespace EffekseerPlayer {
#if COM3D2    
    [PluginFilter("COM3D2x64"),
#else
    [PluginFilter("CM3D2x64"),
     PluginFilter("CM3D2OHx64"),
#endif
     PluginName("EffekseerPlayerPlugin"),
     PluginVersion("0.3.1")]
    public class EffekseerPlayerPlugin : PluginBase {
        #region Variables
        private const EventModifiers MODIFIER_KEYS = EventModifiers.Shift | EventModifiers.Control | EventModifiers.Alt;
        private readonly Settings settings = Settings.Instance;

        private readonly UIParamSet uiParamSet = UIParamSet.Instance;
        private readonly UIHelper uiHelper = new UIHelper();
        private readonly InputKeyDetectHandler<RecipeSet> keyDetector = InputKeyDetectHandler<RecipeSet>.Get();
        private readonly CM3D2SceneChecker sceneChecker = new CM3D2SceneChecker();

        private bool _started;
        private bool _isTargetScene;
        private PlayManager _playMgr;
        private EfkManager _efkMgr;
        private RecipeManager _recipeMgr;

        private EditRecipeView _editView;
        private MainPlayView _playView;
        //private AssetBundle effectBundle;

        private Func<bool> InputVisibleKey = () => false;

        /// <summary>初期化状態を表す列挙型</summary>
        public enum InitState {
            /// <summary>未初期化</summary>
            NotInitialized = 0,
            /// <summary>初期化中</summary>
            Initializing = 1,
            /// <summary>初期化済み</summary>
            Initialized = 2
        }
        private InitState _state = InitState.NotInitialized;

        #endregion
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        internal MonoBehaviour plugin;
        public EffekseerPlayerPlugin() {
            plugin = this;
        }

        #region Monobehaviour Methods
        public void Awake() {
            Log.Debug("Awake");

#if UNITY_5_5_OR_NEWER
            SceneManager.sceneLoaded += SceneLoaded;
#endif
        }

        public void Start() {
            Log.Debug("Start");
            if (_started) return;

            var confDir = ResolveDir();

            // デフォルトのefkパスを取得（configにパスが設定されている場合は、configを優先）
            var efkDir = Path.GetFullPath(Path.Combine(confDir, @"efk"));

            settings.Updated += UpdateSetting;
            ReloadConfig();
            settings.Load(key => Preferences["Config"][key].Value);
            uiParamSet.SizeRate = settings.sizeRate;

            // 設定の反映
            if (!Directory.Exists(settings.efkDir)) {
                if (!Directory.Exists(efkDir)) {
                    Directory.CreateDirectory(efkDir);
                    Log.Info("directory created: ", efkDir);
                }
                settings.efkDir = efkDir;
            }

            // colorPresetDir
            // 未指定時：efkDirと同等
            // 指定時　：絶対パスか判定し、絶対パスであればそのまま使用し、
            //           そうでなければconfからの想定パスとして扱う
            if (settings.colorPresetDir == null) {
                settings.colorPresetDir = settings.efkDir;
            } else {
                if (!Path.IsPathRooted(settings.colorPresetDir)) {
                    settings.colorPresetDir = Path.Combine(confDir, settings.colorPresetDir);
                }
                if (!Directory.Exists(settings.colorPresetDir)) {
                    Directory.CreateDirectory(settings.colorPresetDir);
                    Log.Info("directory created: ", settings.colorPresetDir);
                }
            }

            Log.Debug("Effekseer Dir: ", settings.efkDir);
            _efkMgr = new EfkManager(settings.efkDir);
            _playMgr = new PlayManager();

            var recipeDir = Path.GetFullPath(Path.Combine(settings.efkDir, @"_recipes"));
            if (!Directory.Exists(recipeDir)) {
                Directory.CreateDirectory(recipeDir);
                Log.Info("Recipe directory created: ", recipeDir);
            }
            _recipeMgr = new RecipeManager(recipeDir, _playMgr);
            _recipeMgr.SetupStopKey(settings.playStopKeyCode);
            _recipeMgr.SetupPauseKey(settings.playPauseKeyCode);
            if (GameMain.Instance.VRMode) {
                _recipeMgr.SetupStopKey(settings.playStopKeyCodeVR);
                _recipeMgr.SetupPauseKey(settings.playPauseKeyCodeVR);
            }
            var colorPresetMgr = ColorPresetManager.Instance;
            var colorPresetFile = Path.Combine(settings.colorPresetDir, "_ColorPreset.csv");
            colorPresetMgr.maxCount = settings.colorPresetMax;
            colorPresetMgr.SetPath(colorPresetFile);

            EffekseerSystem.baseDirectory = settings.efkDir;
            keyDetector.Init();
            DontDestroyOnLoad(this);
            _started = true;
        }

        private void UpdateSetting(Settings settings1) {
            EffekseerSystem.effectInstances  = settings1.effectInstances;
            EffekseerSystem.maxSquares       = settings1.maxSquares;
            EffekseerSystem.soundInstances   = settings1.soundInstances;
            EffekseerSystem.isRightHandledCoordinateSystem = settings1.isRightHandledCoordinateSystem;
            EffekseerSystem.enableDistortion = settings1.enableDistortion;
            EffekseerSystem.suppressMultiplePlaySound = settings1.suppressMultiplePlaySound;
            
            if (settings1.toggleModifiers == EventModifiers.None) {
                // 修飾キーが押されていない事を確認(Shift/Alt/Ctrl)
                InputVisibleKey = () => (Event.current.modifiers & MODIFIER_KEYS) == EventModifiers.None && Input.GetKeyDown(settings1.toggleKey);
            } else {
                InputVisibleKey = () => (Event.current.modifiers & settings1.toggleModifiers) != EventModifiers.None && Input.GetKeyDown(settings1.toggleKey);
            }
        }

#if UNITY_5_5_OR_NEWER
        public void SceneLoaded(Scene scene, LoadSceneMode sceneMode) {
            // Log.Debug(scene.name);
            
            OnSceneLoaded(scene.buildIndex);
        }
#else
        public void OnLevelWasLoaded(int level) {
            // Log.Debug("OnLevelWasLoaded ", level);
            OnSceneLoaded(level);
        }
#endif
        private void OnSceneLoaded(int sceneIdx) {
            if (!_started) return;
            _recipeMgr.InitState();
            uiHelper.InitStatus();
            // Effekseerの再生状態を一旦クリア
            _playMgr.Clear();
            // 必要に応じて各コンボの情報(メイド一覧など)をクリア

            _isTargetScene = sceneChecker.IsTarget(sceneIdx);
            if (_isTargetScene) {
                if (_state == InitState.NotInitialized) {
                    _state = InitState.Initializing;
                    plugin.StartCoroutine(DelayFrame(60, Init));
                }
            }
        }

        public void Update() {
            if (!_isTargetScene) return;

            try {
                if (InputVisibleKey()) {
                    if (_state != InitState.NotInitialized) {
                        _playView.Visible = !_playView.Visible;
                        Log.Debug("Visible:", _playView.Visible);
                        if (_playView.Visible && _state == InitState.Initialized) uiParamSet.Update();
                    }
                }
            } catch (Exception e) {
                Log.Error(e);
                return;
            }
            if (_state != InitState.Initialized) return;

            keyDetector.Detect();

            if (_playView.Visible) uiHelper.UpdateCursor();
            uiHelper.UpdateCameraControl();

            _playView.Update();
            _editView.Update();
        }

        public void OnGUI() {
            if (!_isTargetScene) return;

            if (_state != InitState.Initialized || !_playView.Visible) return;
            if (settings.ssWithoutUI && !uiHelper.IsEnabledUICamera()) return; // UI無し撮影

            _playView.OnGUI();
            _editView.OnGUI();
            uiHelper.UpdateDrag();
        }

        public void OnEnable() {
            Log.Debug("plugin on enable");
            EffekseerSystem.SetActive(true);
        }

        public void OnDisable() {
            Log.Debug("plugin on disable");
            uiHelper.InitStatus();
            EffekseerSystem.SetActive(false);
        }

        public void OnDestroy() {
            //SetActive(false);
            Log.Debug("plugin on destroy");
            Dispose();
            EffekseerSystem.Destroy();

#if UNITY_5_5_OR_NEWER
            SceneManager.sceneLoaded -= SceneLoaded;
#endif
        }
        #endregion

        // http://qiita.com/toRisouP/items/e402b15b36a8f9097ee9
        private IEnumerator DelayFrame(int delayFrame, Action act)　{
            for (var i = 0; i < delayFrame; i++) {
                yield return null;
            }
            act();
        }

        private void Init() {
            _state = InitState.Initializing;

            plugin.StartCoroutine(InitAsync());
        }

        public IEnumerator InitAsync() {
            uiParamSet.Update();
            _editView = new EditRecipeView(uiParamSet, _recipeMgr, _efkMgr) {
                Text = "Playエディット"
            };

            _playView = new MainPlayView(uiParamSet, _editView, _recipeMgr, _playMgr) {
                Text = "EffekseerPlayer",
                Version = Version,
                FollowedWin = _editView
            };
            uiHelper.targets.Add(_playView);
            uiHelper.targets.Add(_editView);
            yield return null;

            _efkMgr.ScanDir();
            yield return null;

            // レシピをロードしてから、ビューを初期化
            // (リストのビューサイズなどが必要なため)
            _recipeMgr.Load();
            yield return null;

            // 先にMainの方の初期化をする
            _playView.Awake();
            _editView.Awake();
            yield return null;

            SetActive(true);
            _state = InitState.Initialized;
            _playView.Visible = true;
            Log.Debug("Initialized");
        }

        public void SetActive(bool active) {
            EffekseerSystem.SetActive(active);
        }

        private void Dispose() {
            uiHelper.SetCameraControl(true);
            uiHelper.InitStatus();
        }

        private string ResolveDir() {
            // 以下はCM3D2向け  (Sybaris 2以降では不要)
            // リダイレクトで存在しないパスが渡されてしまうケースがあるため、
            // 旧Sybarisチェックを先に行う (リダイレクトによるパスではディレクトリ作成・削除が動作しない）
            var dllPath = Path.Combine(DataPath, @"..\..\opengl32.dll");
            var dirPath = Path.Combine(DataPath, @"..\..\Sybaris");
            if (File.Exists(dllPath) && Directory.Exists(dirPath)) {
                dirPath = Path.GetFullPath(dirPath);
                var confPath = Path.Combine(dirPath, @"UnityInjector\Config");
                if (Directory.Exists(confPath)) return confPath;
                confPath = Path.Combine(dirPath, @"Plugins\UnityInjector\Config");
                return confPath;
            }

            return DataPath;
        }

        #region Static Fields
        public static volatile string PluginName;
        public static volatile string Version;
        static EffekseerPlayerPlugin() {
            // 属性クラスからプラグイン名とプラグインバージョンを取得
            try {
                var attr = Attribute.GetCustomAttribute(typeof(EffekseerPlayerPlugin), typeof(PluginNameAttribute)) as PluginNameAttribute;
                if (attr != null) PluginName = attr.Name;
            } catch (Exception e) {
                Log.Error(e);
            }
            try {
                var attr = Attribute.GetCustomAttribute(typeof(EffekseerPlayerPlugin), typeof(PluginVersionAttribute)) as PluginVersionAttribute;
                if (attr != null) Version = attr.Version;
            } catch (Exception e) {
                Log.Error(e);
            }
        }
        #endregion
    }
}
