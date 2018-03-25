﻿using System;
using System.Collections;
using System.IO;
using System.Reflection;
using EffekseerPlayerPlugin.CM3D2;
using EffekseerPlayerPlugin.CM3D2.Data;
using EffekseerPlayerPlugin.CM3D2.UI;
using EffekseerPlayerPlugin.Effekseer;
using EffekseerPlayerPlugin.Unity.UI;
using EffekseerPlayerPlugin.Unity.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;
using UnityInjector.Attributes;

[assembly: AssemblyVersion("0.1.0")]
namespace EffekseerPlayerPlugin {
#if COM3D2    
    [PluginFilter("COM3D2x64"),
#else
    [PluginFilter("CM3D2x64"),
     PluginFilter("CM3D2OHx64"),
#endif
     PluginName("EffekseerPlayerPlugin"),
     PluginVersion("0.1.0")]
    public class EffekseerPlayerPlugin : PluginBase {
        #region Variables
        private const EventModifiers MODIFIER_KEYS = EventModifiers.Shift | EventModifiers.Control | EventModifiers.Alt;
        private static readonly Settings Settings = Settings.Instance;

        private readonly UIParamSet _uiParamSet = UIParamSet.Instance;
        private readonly UIHelper _uiHelper = new UIHelper();

        private bool _started;
        private PlayManager _playMgr;
        private EfkManager _efkMgr;
        private RecipeManager _recipeMgr;

        private EditRecipeView _editView;
        private MainPlayView _playView;
        //private AssetBundle effectBundle;

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
        internal PluginBase plugin;
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

            ReloadConfig();
            Settings.Load(key => Preferences["Config"][key].Value);
            _uiParamSet.SizeRate = Settings.sizeRate;

            // 設定の反映
            if (!Directory.Exists(Settings.efkDir)) {
                if (!Directory.Exists(efkDir)) {
                    Directory.CreateDirectory(efkDir);
                    Log.Info("directory created: ", efkDir);
                }
                Settings.efkDir = efkDir;
            }

            Log.Debug("Effekseer Dir: ", Settings.efkDir);
            _efkMgr = new EfkManager(Settings.efkDir);

            _playMgr = new PlayManager();

            var recipeDir = Path.GetFullPath(Path.Combine(Settings.efkDir, @"_recipes"));
            if (!Directory.Exists(recipeDir)) {
                Directory.CreateDirectory(recipeDir);
                Log.Info("Recipe directory created: ", recipeDir);
            }
            _recipeMgr = new RecipeManager(recipeDir);

            EffekseerSystem.baseDirectory   = Settings.efkDir;
            EffekseerSystem.effectInstances = Settings.effectInstances;
            EffekseerSystem.maxSquares      = Settings.maxSquares;
            EffekseerSystem.soundInstances  = Settings.soundInstances;
            EffekseerSystem.isRightHandledCoordinateSystem = Settings.isRightHandledCoordinateSystem;
            EffekseerSystem.enableDistortion = Settings.enableDistortion;
            EffekseerSystem.suppressMultiplePlaySound = Settings.suppressMultiplePlaySound;

            DontDestroyOnLoad(this);
            _started = true;
        }

#if UNITY_5_5_OR_NEWER
        public void SceneLoaded(Scene scene, LoadSceneMode sceneMode) {
            // Log.Debug(scene.name);
            
            SceneLoaded();
        }
#else
        public void OnLevelWasLoaded(int level) {
            // Log.Debug("OnLevelWasLoaded ", level);
            SceneLoaded();
        }
#endif
        private void SceneLoaded() {
            _uiHelper.InitStatus();
            // Effekseerの再生状態を一旦クリア
            _playMgr.Clear();
            // 必要に応じて各コンボの情報(メイド一覧など)をクリア
        }

        public void Update() {
            try {
                if (InputModifierKey() && Input.GetKeyDown(Settings.toggleKey)) {
                    if (_state == InitState.NotInitialized) Init();
                    else {
                        _playView.Visibled = !_playView.Visibled;
                        Log.Debug("Visibled:", _playView.Visibled);
                        if (_playView.Visibled && _state == InitState.Initialized) _uiParamSet.Update();
                    }
                }
            } catch (Exception e) {
                Log.Error(e);
                return;
            }
            if (_state != InitState.Initialized) return;

            if (_playView.Visibled) _uiHelper.UpdateCursor();
            _uiHelper.UpdateCameraControl();

            _playView.Update();
            _editView.Update();
        }

        public void OnGUI() {
            if (_state != InitState.Initialized || !_playView.Visibled) return;
            if (Settings.ssWithoutUI && !_uiHelper.IsEnabledUICamera()) return; // UI無し撮影

            _playView.OnGUI();
            _editView.OnGUI();
            _uiHelper.UpdateDrag();
        }

        public void OnEnable() {
            Log.Debug("plugin on enable");
            EffekseerSystem.SetActive(true);
        }

        public void OnDisable() {
            Log.Debug("plugin on disable");
            _uiHelper.InitStatus();
            EffekseerSystem.SetActive(false);
        }

        public void OnDestroy() {
            //SetActive(false);
            Log.Debug("plugin on destroy");
            Dispose();
            EffekseerSystem.Destroy();
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
            _uiParamSet.Update();
            _editView = new EditRecipeView(_uiParamSet, _recipeMgr, _efkMgr) {
                Text = "Playエディット"
            };

            _playView = new MainPlayView(_uiParamSet, _editView, _recipeMgr, _playMgr) {
                Text = "EffekseerPlayer",
                Version = Version,
                FollowedWin = _editView
            };
            _uiHelper.targets.Add(_playView);
            _uiHelper.targets.Add(_editView);
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
            _playView.Visibled = true;
            Log.Debug("Initialized");
        }

        public void SetActive(bool active) {
            EffekseerSystem.SetActive(active);
        }

        private void Dispose() {
            _uiHelper.SetCameraControl(true);
            _uiHelper.InitStatus();
        }

        private bool InputModifierKey() {
            var em = Event.current.modifiers;
            if (Settings.toggleModifiers == EventModifiers.None) {
                // 修飾キーが押されていない事を確認(Shift/Alt/Ctrl)
                return (em & MODIFIER_KEYS) == EventModifiers.None;
            }

            return (em & Settings.toggleModifiers) != EventModifiers.None;
        }

        private string ResolveDir() {
            // 以下はCM3D2向け  (Sybaris 2以降では不要)
            // リダイレクトで存在しないパスが渡されてしまうケースがあるため、
            // 旧Sybarisチェックを先に行う (リダイレクトによるパスではディレクトリ作成・削除が動作しない）
            var dllpath = Path.Combine(DataPath, @"..\..\opengl32.dll");
            var dirPath = Path.Combine(DataPath, @"..\..\Sybaris");
            if (File.Exists(dllpath) && Directory.Exists(dirPath)) {
                dirPath = Path.GetFullPath(dirPath);
                var confPath = Path.Combine(dirPath, @"UnityInjector\Config");
                if (Directory.Exists(confPath)) return confPath;
                confPath = Path.Combine(dirPath, @"Plugins\UnityInjector\Config");
                return confPath;
            }

            return DataPath;
        }

        // プラグイン名
        public static volatile string PluginName;
        // プラグインバージョン
        public static volatile string Version;
        static EffekseerPlayerPlugin() {
            // 属性クラスからプラグイン名取得
            try {
                var attr = Attribute.GetCustomAttribute(typeof(EffekseerPlayerPlugin), typeof(PluginNameAttribute)) as PluginNameAttribute;
                if (attr != null) PluginName = attr.Name;
            } catch (Exception e) {
                Log.Error(e);
            }
            // プラグインバージョン取得
            try {
                var attr = Attribute.GetCustomAttribute(typeof(EffekseerPlayerPlugin), typeof(PluginVersionAttribute)) as PluginVersionAttribute;
                if (attr != null) Version = attr.Version;
            } catch (Exception e) {
                Log.Error(e);
            }
        }
    }
}