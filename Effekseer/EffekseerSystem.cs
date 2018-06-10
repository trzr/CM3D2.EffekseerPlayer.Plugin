using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EffekseerPlayer.Effekseer {

    public class EffekseerSystem : MonoBehaviour {

        /// <summary xml:lang="en">
        /// Whether it does draw in scene view for editor.
        /// </summary>
        /// <summary xml:lang="ja">
        /// エディタのシーンビューに描画するかどうか
        /// </summary>
        public static bool drawInSceneView = true;

        /// <summary xml:lang="en">
        /// Maximum number of effect instances.
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクトインスタンスの最大数
        /// </summary>
        public static int effectInstances = 1600;

        /// <summary xml:lang="en">
        /// Maximum number of quads that can be drawn.
        /// </summary>
        /// <summary xml:lang="ja">
        /// 描画できる四角形の最大数
        /// </summary>
        public static int maxSquares = 8192;

        /// <summary xml:lang="en">
        /// The coordinate system of effects.
        /// if it is true, effects is loaded as same as before version 1.3.
        /// if it is false, effects is shown as same as the editor.
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクトの座標系
        /// trueならば、version1.3以前と同じように読み込まれる。
        /// falseならば、エディタと同じように表示される。
        /// </summary>
        public static bool isRightHandledCoordinateSystem = false;

        /// <summary xml:lang="en">
        /// Maximum number of sound instances.
        /// </summary>
        /// <summary xml:lang="ja">
        /// サウンドインスタンスの最大数
        /// </summary>
        public static int soundInstances = 16;

        /// <summary xml:lang="ja">
        /// サウンドの多重再生の抑制
        /// trueならば、多重再生を抑える。
        /// </summary>
        public static bool suppressMultiplePlaySound = false;

        /// <summary xml:lang="en">
        /// Enables distortion effect.
        /// When It has set false, rendering will be faster.
        /// </summary>
        /// <summary xml:lang="ja">
        /// 歪みエフェクトを有効にする。
        /// falseにすると描画処理が軽くなります。
        /// </summary>
        public static bool enableDistortion = true;

        /// <summary xml:lang="en">
        /// A CameraEvent to draw all effects.
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクトの描画するタイミング
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private const CameraEvent cameraEvent = CameraEvent.BeforeImageEffects;

        public static string baseDirectory;
        public static AssetBundle defaultBundle;
        /// <summary xml:lang="ja">
        /// エフェクトの再生
        /// </summary>
        /// <param name="name" xml:lang="ja">エフェクト名</param>
        /// <param name="location" xml:lang="ja">再生開始する位置</param>
        /// <returns>再生したエフェクトインスタンス</returns>
        public static EffekseerHandle PlayEffect(string name, Vector3 location) {
            var effect = Instance._GetEffect(name);
            if (effect == IntPtr.Zero) return new EffekseerHandle(-1);

            var handle = Plugin.EffekseerPlayEffect(effect, location.x, location.y, location.z);
            return new EffekseerHandle(handle);
        }

        /// <summary xml:lang="ja">
        /// 全エフェクトの再生停止
        ///
        /// FIXME Emitter側の状態管理と整合が取れなくなるため注意
        /// </summary>
        public static void StopAllEffects() {
            Plugin.EffekseerStopAllEffects();
        }

        /// <summary xml:lang="ja">
        /// 全エフェクトの一時停止、もしくは再開
        ///
        /// FIXME Emitter側の状態管理と整合が取れなくなるため注意
        /// </summary>
        public static void SetPausedToAllEffects(bool paused) {
            Plugin.EffekseerSetPausedToAllEffects(paused);
        }

        /// <summary xml:lang="ja">
        /// エフェクトのロード (指定パスから)
        /// </summary>
        /// <param name="name" xml:lang="ja">エフェクト名 (efkファイルの名前から".efk"を取り除いたもの)</param>
        public static void LoadEffect(string name) {
            Instance._LoadEffect(name, defaultBundle);
        }

        /// <summary xml:lang="ja">
        /// エフェクトのロード (AssetBundleから)
        /// </summary>
        /// <param name="name" xml:lang="ja">エフェクト名 (efkファイルの名前から".efk"を取り除いたもの)</param>
        /// <param name="assetBundle" xml:lang="ja">ロード元のAssetBundle</param>
        public static void LoadEffect(string name, AssetBundle assetBundle) {
            Instance._LoadEffect(name, assetBundle);
        }

        /// <summary xml:lang="ja">
        /// エフェクトの解放
        /// </summary>
        /// <param name="effectName" xml:lang="ja">エフェクト名 (efkファイルの名前から".efk"を取り除いたもの)</param>
        public static void ReleaseEffect(string effectName) {
            Instance._ReleaseEffect(effectName);
        }

        #region Internal Implimentation

        private const string NAME = "Effekseer";
        // Singleton instance
        private static EffekseerSystem _instance;
        public static EffekseerSystem Instance {
            get {
                if (_instance != null) return _instance;

                //if (destroyed) return null;
                // Find instance when is not set static variable
                var system = FindObjectOfType<EffekseerSystem>();
                if (system != null) {
                    // Sets static variable when instance is found
                    _instance = system;
                } else {
                    // Create instance when instance is not found
                    var go = GameObject.Find(NAME) ?? new GameObject(NAME);
                    _instance = go.AddComponent<EffekseerSystem>();
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }

        public static void SetActive(bool active) {
            if (_instance == null) return;

            _instance.gameObject.SetActive(active);
        }

        public static void Destroy() {
            if (_instance == null) return;

            Destroy(_instance);
            _instance = null;
            //destroyed = true;
        }
        internal static string workDir;

        private int _initedCount;

        // Loaded effects
        private Dictionary<string, IntPtr> _effectList = new Dictionary<string, IntPtr>();
        // Loaded effect resources
        private List<TextureResource> _textureList;
        private List<ModelResource> _modelList;
        private List<SoundResource> _soundList;
        private List<SoundInstance> _soundInstanceList;

        // A AssetBundle that current loading
        private AssetBundle _assetBundle;

#if UNITY_EDITOR
// ホットリロードの退避用
        private List<string> savedEffectList = new List<string>();
#endif

        // カメラごとのレンダーパス
        class RenderPath : IDisposable {
            private readonly Camera _camera;
            private CommandBuffer _commandBuffer;
            private readonly CameraEvent _cameraEvent;
            public readonly int renderId;
            public RenderTexture renderTexture;

            public RenderPath(Camera camera, CameraEvent cameraEvent, int renderId) {
                _camera = camera;
                this.renderId = renderId;
                _cameraEvent = cameraEvent;
            }

            public void Init(bool enableDistortion) {
                // プラグイン描画するコマンドバッファを作成
                _commandBuffer = new CommandBuffer {
                    name = "Effekseer Rendering"
                };

#if UNITY_5_6_OR_NEWER
                if (enableDistortion) {
                    var format = _camera.allowHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
#else
                if (enableDistortion && _camera.cameraType == CameraType.Game) {
                    var format = (_camera.hdr) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
#endif
                    // 歪みテクスチャを作成
                    renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 0, format);
                    renderTexture.Create();
                    // 歪みテクスチャへのコピーコマンドを追加
                    _commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, renderTexture);
                    _commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                }

                // プラグイン描画コマンドを追加
                _commandBuffer.IssuePluginEvent(Plugin.EffekseerGetRenderFunc(), renderId);
                // コマンドバッファをカメラに登録
                _camera.AddCommandBuffer(_cameraEvent, _commandBuffer);
            }

            public void Dispose() {
                if (_commandBuffer == null) return;

                if (_camera != null) {
                    _camera.RemoveCommandBuffer(_cameraEvent, _commandBuffer);
                }
                _commandBuffer.Dispose();
                _commandBuffer = null;
            }

            public bool IsValid() {
                if (renderTexture != null) {
                    return _camera.pixelWidth == renderTexture.width &&
                           _camera.pixelHeight == renderTexture.height;
                }
                return true;
            }
        };
        private readonly Dictionary<Camera, RenderPath> _renderPaths = new Dictionary<Camera, RenderPath>();

        private IntPtr _GetEffect(string effectName) {
            return _LoadEffect(effectName, defaultBundle);
        }

        /// 指定ディレクトリ以下1階層分のみ走査
        /// 指定ファイルが存在しない場合は、nullを返す
        private string ScanFile(string filename, string dir) {
            var filepath = Path.Combine(dir, filename);
            if (File.Exists(filepath)) return filepath;

            filepath = null;
            var di = new DirectoryInfo(dir);
            //IEnumerable<DirectoryInfo> subFolders =
            //    di.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
            IEnumerable<DirectoryInfo> subFolders =
                di.GetDirectories("*", SearchOption.TopDirectoryOnly);
            foreach (var folder in subFolders) {
                filepath = Path.Combine(folder.FullName, filename);
                if (File.Exists(filepath)) break;
                filepath = null;
            }
            return filepath;
        }

//        private readonly object _lockObj = new object();
        private IntPtr _LoadEffect(string effectName, AssetBundle bundle) {
            IntPtr effect;
            if (_effectList.TryGetValue(effectName, out effect)) {
                return effect;
            }
            //Log.Debug("LoadEffect:", effectName, ", bundle:", bundle);

            byte[] bytes = null;
            if (bundle != null) {
                var asset = bundle.LoadAsset<TextAsset>(effectName);
                bytes = asset.bytes;

                effect = _LoadEffect(bytes, bundle);

            } else {
                // Resourcesから読み込む
                var filename = effectName + ".efk";
                var filepath = ScanFile(filename, baseDirectory);
                if (filepath == null) {
                    filename = effectName + ".bytes";
                    filepath = ScanFile(filename, baseDirectory);
                    if (filepath == null) {
                        var asset = Resources.Load<TextAsset>(Utility.ResourcePath(effectName, true));
                        if (asset != null) bytes = asset.bytes;
                        if (bytes == null) {
                            Debug.LogError("[Effekseer] Failed to load effect: " + effectName);
                            return IntPtr.Zero;
                        }
                    }
                }
                var fileLoaded = false;
                if (bytes == null) {
                    Log.Debug("load file: ", filepath);
                    bytes = File.ReadAllBytes(filepath);
                    fileLoaded = true;
                }

                // efkファイルのロードディレクトリを基準として、他のファイルを参照できるようにworkDirを設定
                {
                    if (fileLoaded) workDir = Path.GetDirectoryName(filepath);
                    else if (workDir == null) workDir = baseDirectory;

                    effect = _LoadEffect(bytes, null);
                    workDir = null;
                }
            }
            _effectList.Add(effectName, effect);

            return effect;
        }

        private IntPtr _LoadEffect(ICollection<byte> bytes, AssetBundle bundle) {
            _assetBundle = bundle;
            try {
                var ghc = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                var effect = Plugin.EffekseerLoadEffectOnMemory(ghc.AddrOfPinnedObject(), bytes.Count);
                ghc.Free();
                return effect;
            } finally {
                _assetBundle = null;
            }
        }

        private void _ReleaseEffect(string effectName) {
            if (_effectList.ContainsKey(effectName)) return;

            var effect = _effectList[effectName];
            Plugin.EffekseerReleaseEffect(effect);
            _effectList.Remove(effectName);
        }

        internal void Init() {
            if (_initedCount++ > 0) return;

            // Log.Debug("graphicDevice:", SystemInfo.graphicsDeviceType);
            // サポート外グラフィックスAPIのチェック
            switch (SystemInfo.graphicsDeviceType) {
#if UNITY_5_5_OR_NEWER
            case GraphicsDeviceType.Vulkan:
#elif UNITY_5_4_OR_NEWER
            case GraphicsDeviceType.Direct3D12:
#else
            case GraphicsDeviceType.Metal:
#endif
                Debug.LogError("[Effekseer] Graphics API \"" + SystemInfo.graphicsDeviceType + "\" is not supported.");
                return;
            }

            // ReSharper disable once ConvertToConstant.Local
            // Zのnearとfarの反転対応
            var reversedDepth = false;
#if UNITY_5_5_OR_NEWER
            switch (SystemInfo.graphicsDeviceType) {
            case GraphicsDeviceType.Direct3D11:
            case GraphicsDeviceType.Direct3D12:
            case GraphicsDeviceType.Metal:
                reversedDepth = true;
                break;
            }
#endif
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // Effekseerライブラリの初期化
            Plugin.EffekseerInit(effectInstances, maxSquares, reversedDepth, isRightHandledCoordinateSystem);
            _effectList = new Dictionary<string, IntPtr>();
            _textureList = new List<TextureResource>();
            _modelList = new List<ModelResource>();
            _soundList = new List<SoundResource>();
            _soundInstanceList = new List<SoundInstance>();

            Plugin.EffekseerSetTextureLoaderEvent(
                TextureLoaderLoad,
                TextureLoaderUnload);
            Plugin.EffekseerSetModelLoaderEvent(
                ModelLoaderLoad,
                ModelLoaderUnload);
            Plugin.EffekseerSetSoundLoaderEvent(
                SoundLoaderLoad,
                SoundLoaderUnload);
            Plugin.EffekseerSetSoundPlayerEvent(
                SoundPlayerPlay,
                SoundPlayerStopTag,
                SoundPlayerPauseTag,
                SoundPlayerCheckPlayingTag,
                SoundPlayerStopAll);

            if (Application.isPlaying) {
                // サウンドインスタンスを作る
                for (var i = 0; i < soundInstances; i++) {
                    var go = new GameObject("Sound Instance");
                    go.transform.parent = transform;
                    _soundInstanceList.Add(go.AddComponent<SoundInstance>());
                }
            }

            Camera.onPreCull += OnPreCullEvent;

            Log.Debug("Lib initialized");
        }

        internal void Term() {
            if (--_initedCount > 0) return;
            Log.Debug("Lib Terminating");

            // ReSharper disable once DelegateSubtraction
            Camera.onPreCull -= OnPreCullEvent;
            if (_effectList != null) {
                foreach (var pair in _effectList) {
                    Plugin.EffekseerReleaseEffect(pair.Value);
                }
                _effectList = null;
            }
            // Effekseerライブラリの終了処理
            Plugin.EffekseerTerm();
            // レンダリングスレッドで解放する環境向けにレンダリング命令を投げる
            GL.IssuePluginEvent(Plugin.EffekseerGetRenderFunc(), 0);

            Log.Debug("Lib Terminated");
        }

        void Awake() {
            Init();
        }

        void OnDestroy() {
            Term();
        }

        void OnEnable() {
#if UNITY_EDITOR
            Resume();
#endif
            CleanUp();
        }

        void OnDisable() {
#if UNITY_EDITOR
            Suspend();
#endif
            CleanUp();
        }

#if UNITY_EDITOR
        void Suspend() {
            // Dictionaryは消えるので文字列にして退避
            foreach (var pair in effectList) {
                savedEffectList.Add(pair.Key + "," + pair.Value.ToString());
            }
            effectList.Clear();
        }
    
        void Resume() {
            // ホットリロード時はリジューム処理
            foreach (var effect in savedEffectList) {
                string[] tokens = effect.Split(',');
                if (tokens.Length == 2) {
                    effectList.Add(tokens[0], (IntPtr)ulong.Parse(tokens[1]));
                }
            }
            savedEffectList.Clear();
        }
#endif

        void CleanUp() {
            // レンダーパスの破棄
            foreach (var pair in _renderPaths) {
//                var camera = pair.Key;
                var path = pair.Value;
                path.Dispose();
            }
            _renderPaths.Clear();
        }

        void LateUpdate() {
            var deltaFrames = Time.deltaTime * 60.0f;
            var updateCount = Mathf.Max(1, Mathf.RoundToInt(deltaFrames));
            for (var i = 0; i < updateCount; i++) {
                Plugin.EffekseerUpdate(deltaFrames / updateCount);
            }
        }

        void OnPreCullEvent(Camera camera) {
#if UNITY_EDITOR
            if (camera.cameraType ==  CameraType.SceneView) {
                // シーンビューのカメラはチェック
                if (drawInSceneView == false) return;
            }
#endif
            RenderPath path;

            // カリングマスクをチェック
            if ((Camera.current.cullingMask & (1 << gameObject.layer)) == 0) {
                if (!_renderPaths.ContainsKey(camera)) return;

                // レンダーパスが存在すればコマンドバッファを解除
                path = _renderPaths[camera];
                path.Dispose();
                _renderPaths.Remove(camera);
                return;
            }

            if (_renderPaths.ContainsKey(camera)) {
                // レンダーパスが有れば使う
                path = _renderPaths[camera];
            } else {
                // 無ければレンダーパスを作成
                path = new RenderPath(camera, cameraEvent, _renderPaths.Count);
                path.Init(enableDistortion);
                _renderPaths.Add(camera, path);
            }

            if (!path.IsValid()) {
                path.Dispose();
                path.Init(enableDistortion);
            }

            // 歪みテクスチャをセット
            if (path.renderTexture) {
                Plugin.EffekseerSetBackGroundTexture(path.renderId, path.renderTexture.GetNativeTexturePtr());
            }

#if UNITY_5_4_OR_NEWER
            // ステレオレンダリング(VR)用に左右目の行列を設定
            if (camera.stereoEnabled) {
                var projMatL = Utility.Matrix2Array(GL.GetGPUProjectionMatrix(camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left), false));
                var projMatR = Utility.Matrix2Array(GL.GetGPUProjectionMatrix(camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right), false));
                var camMatL = Utility.Matrix2Array(camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left));
                var camMatR = Utility.Matrix2Array(camera.GetStereoViewMatrix(Camera.StereoscopicEye.Right));
                Plugin.EffekseerSetStereoRenderingMatrix(path.renderId, projMatL, projMatR, camMatL, camMatR);
            } else
#endif
            {
                // ビュー関連の行列を更新
                Plugin.EffekseerSetProjectionMatrix(path.renderId, Utility.Matrix2Array(
                    GL.GetGPUProjectionMatrix(camera.projectionMatrix, false)));
                Plugin.EffekseerSetCameraMatrix(path.renderId, Utility.Matrix2Array(camera.worldToCameraMatrix));
            }
        }

        void OnRenderObject() {
            if (!_renderPaths.ContainsKey(Camera.current)) return;

            var path = _renderPaths[Camera.current];
            Plugin.EffekseerSetRenderSettings(path.renderId,
                (RenderTexture.active != null));
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(Plugin.EffekseerTextureLoaderLoad))]
        private static IntPtr TextureLoaderLoad(IntPtr pathPtr, out int width, out int height, out int format) {
            var path = Marshal.PtrToStringUni(pathPtr);
            Log.Debug("texLoader load:", path);
            var res = new TextureResource(workDir);
            if (res.Load(path, Instance._assetBundle)) {
                Instance._textureList.Add(res);
                width = res.texture.width;
                height = res.texture.height;
                switch (res.texture.format) {
                case TextureFormat.DXT1:
                    format = 1;
                    break;
                case TextureFormat.DXT5:
                    format = 2;
                    break;
                default:
                    format = 0;
                    break;
                }

                return res.GetNativePtr();
            }
            width = 0;
            height = 0;
            format = 0;
            return IntPtr.Zero;
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(Plugin.EffekseerTextureLoaderUnload))]
        private static void TextureLoaderUnload(IntPtr pathPtr) {
            var path = Marshal.PtrToStringUni(pathPtr);
            Log.Debug("texLoader unload:", path);
            foreach (var res in Instance._textureList) {
                if (res.Path != path) continue;
                Instance._textureList.Remove(res);
                return;
            }
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(Plugin.EffekseerModelLoaderLoad))]
        private static int ModelLoaderLoad(IntPtr pathPtr, IntPtr buffer, int bufferSize) {
            var path = Marshal.PtrToStringUni(pathPtr);
            var res = new ModelResource(workDir);
            Log.Debug("modelLoader load:", path);
            if (!res.Load(path, Instance._assetBundle) || !res.Copy(buffer, bufferSize)) return 0;

            Instance._modelList.Add(res);
            return res.Length;
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(Plugin.EffekseerModelLoaderUnload))]
        private static void ModelLoaderUnload(IntPtr pathPtr) {
            var path = Marshal.PtrToStringUni(pathPtr);
            Log.Debug("modelLoader unload:", path);
            foreach (var res in Instance._modelList) {
                if (res.Path != path) continue;
                Instance._modelList.Remove(res);
                return;
            }
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(Plugin.EffekseerSoundLoaderLoad))]
        private static int SoundLoaderLoad(IntPtr pathPtr) {
            var path = Marshal.PtrToStringUni(pathPtr);
            if (path == null) return 0;
            var res = new SoundResource(workDir);
            if (!res.Load(path, Instance._assetBundle)) return 0;

            Instance._soundList.Add(res);

            var count = Instance._soundList.Count;
            // Log.Debug("soundLoader load:", path, ", count=", count);
            return count;
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(Plugin.EffekseerSoundLoaderUnload))]
        private static void SoundLoaderUnload(IntPtr pathPtr) {
            var path = Marshal.PtrToStringUni(pathPtr);

            Log.Debug("soundLoader unload:", path);
            for (var i=0; i<Instance._soundList.Count; i++) {
                var res = Instance._soundList[i];
                if (res.Path != path) continue;

                Instance._soundList.RemoveAt(i);
                return;
            }
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(Plugin.EffekseerSoundPlayerPlay))]
        private static void SoundPlayerPlay(IntPtr tag,
            int data, float volume, float pan, float pitch,
            bool mode3D, float x, float y, float z, float distance) {
            Instance.PlaySound(tag, data, volume, pan, pitch, mode3D, x, y, z, distance);
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(Plugin.EffekseerSoundPlayerStopTag))]
        private static void SoundPlayerStopTag(IntPtr tag) {
            // Log.Debug("stopSound:", tag);
            Instance.StopSound(tag);
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(Plugin.EffekseerSoundPlayerPauseTag))]
        private static void SoundPlayerPauseTag(IntPtr tag, bool pause) {
            Log.Debug("SoundPause:", tag);
            Instance.PauseSound(tag, pause);
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(Plugin.EffekseerSoundPlayerCheckPlayingTag))]
        private static bool SoundPlayerCheckPlayingTag(IntPtr tag) {
            return Instance.CheckSound(tag);
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(Plugin.EffekseerSoundPlayerStopAll))]
        private static void SoundPlayerStopAll() {
            Instance.StopAllSounds();
        }

        private void PlaySound(IntPtr audioTag,
            int data, float volume, float pan, float pitch,
            bool mode3D, float x, float y, float z, float distance) {
            if (data <= 0) return;

            var resource = _soundList[data - 1];
            if (resource == null) return;

            var tagInt = audioTag.ToInt32();
            foreach (var soundInstance in _soundInstanceList) {
                // Effekseerは同一音声を多重で再生する？
                //  -> 一部ノイズがのるため、以下フラグにて多重再生を抑制する
                if (suppressMultiplePlaySound && soundInstance.AudioTag == tagInt) {
                    if (!soundInstance.CheckPlaying()) {
                        soundInstance.Play(tagInt, resource.audio, volume, pan, pitch, mode3D, x, y, z, distance);
                    } else {
                        // 既に再生中の場合は座標のみ変更
                        soundInstance.SetPosition(x, y, z);
                    }
                    break;
                }
                if (soundInstance.CheckPlaying()) continue;

                soundInstance.Play(tagInt, resource.audio, volume, pan, pitch, mode3D, x, y, z, distance);
                break;
            }
        }

        private void StopSound(IntPtr audioTag) {
            var tagInt = audioTag.ToInt32();

            foreach (var sound in _soundInstanceList) {
                if (sound.AudioTag == tagInt) {
                    sound.Stop();
                }
            }
        }

        private void PauseSound(IntPtr audioTag, bool paused) {
            var tagInt = audioTag.ToInt32();

            foreach (var sound in _soundInstanceList) {
                if (sound.AudioTag == tagInt) {
                    sound.Pause(paused);
                }
            }
        }

        private bool CheckSound(IntPtr audioTag) {
            var tagInt = audioTag.ToInt32();

            var playing = false;
            foreach (var sound in _soundInstanceList) {
                if (sound.AudioTag != tagInt) continue;
                playing |= sound.CheckPlaying();
                break;
            }
            return playing;
        }

        private void StopAllSounds() {
            foreach (var sound in _soundInstanceList) {
                sound.Stop();
            }
        }

    #endregion
    }

    /// <summary xml:lang="ja">
    /// A instance handle of played effect
    /// </summary>
    /// <summary xml:lang="ja">
    /// 再生したエフェクトのインスタンスハンドル
    /// </summary>
    public struct EffekseerHandle {
        private readonly int _mHandle;

        public EffekseerHandle(int handle = -1) {
            _mHandle = handle;
        }

        internal void UpdateHandle(float deltaFrame) {
            Plugin.EffekseerUpdateHandle(_mHandle, deltaFrame);
        }

        /// <summary xml:lang="en">
        /// Stops the played effect.
        /// All nodes will be destroyed.
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクトを停止する
        /// 全てのエフェクトが瞬時に消える
        /// </summary>
        public void Stop() {
            Plugin.EffekseerStopEffect(_mHandle);
        }

        /// <summary xml:lang="en">
        /// Stops the root node of the played effect.
        /// The root node will be destroyed. Then children also will be destroyed by their lifetime.
        /// </summary>
        /// <summary xml:lang="ja">
        /// 再生中のエフェクトのルートノードだけを停止
        /// ルートノードを削除したことで子ノード生成が停止され寿命で徐々に消える
        /// </summary>
        public void StopRoot() {
            Plugin.EffekseerStopRoot(_mHandle);
        }

        /// <summary xml:lang="en">
        /// Sets the effect location
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクトの位置を設定
        /// </summary>
        /// <param name="location">位置</param>
        public void SetLocation(Vector3 location) {
            Plugin.EffekseerSetLocation(_mHandle, location.x, location.y, location.z);
        }

        /// <summary xml:lang="en">
        /// Sets the effect rotation
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクトの回転を設定
        /// </summary>
        /// <param name="rotation">回転</param>
        public void SetRotation(Quaternion rotation) {
            Vector3 axis;
            float angle;
            rotation.ToAngleAxis(out angle, out axis);
            Plugin.EffekseerSetRotation(_mHandle, axis.x, axis.y, axis.z, angle * Mathf.Deg2Rad);
        }

        /// <summary xml:lang="en">
        /// Sets the effect scale
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクトの拡縮を設定
        /// </summary>
        /// <param name="scale">拡縮</param>
        public void SetScale(Vector3 scale) {
            Plugin.EffekseerSetScale(_mHandle, scale.x, scale.y, scale.z);
        }

        /// <summary xml:lang="en">
        /// Specify the color of overall effect.
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクト全体の色を指定する。
        /// </summary>
        /// <param name="color">Color</param>
        public void SetAllColor(Color color) {
            Plugin.EffekseerSetAllColor(_mHandle, (byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255), (byte)(color.a * 255));
        }

        /// <summary xml:lang="en">
        /// Sets the effect target location
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクトのターゲット位置を設定
        /// </summary>
        /// <param name="targetLocation">ターゲット位置</param>
        public void SetTargetLocation(Vector3 targetLocation) {
            Plugin.EffekseerSetTargetLocation(_mHandle, targetLocation.x, targetLocation.y, targetLocation.z);
        }

        /// <summary xml:lang="en">
        /// Pausing the effect
        /// <para>true:  It will update on Update()</para>
        /// <para>false: It will not update on Update()</para>
        /// </summary>
        /// <summary xml:lang="ja">
        /// ポーズ設定
        /// <para>true:  停止中。Updateで更新しない</para>
        /// <para>false: 再生中。Updateで更新する</para>
        /// </summary>
        public bool Paused {
            set {
                Plugin.EffekseerSetPaused(_mHandle, value);
            }
            get {
                return Plugin.EffekseerGetPaused(_mHandle);
            }
        }

        /// <summary xml:lang="en">
        /// Showing the effect
        /// <para>true:  It will be rendering.</para>
        /// <para>false: It will not be rendering.</para>
        /// </summary>
        /// <summary xml:lang="ja">
        /// 表示設定
        /// <para>true:  表示ON。Drawで描画する</para>
        /// <para>false: 表示OFF。Drawで描画しない</para>
        /// </summary>
        public bool Shown {
            set {
                Plugin.EffekseerSetShown(_mHandle, value);
            }
            get {
                return Plugin.EffekseerGetShown(_mHandle);
            }
        }

        /// <summary xml:lang="en">
        /// Playback speed
        /// </summary>
        /// <summary xml:lang="ja">
        /// 再生速度
        /// </summary>
        public float Speed {
            set {
                Plugin.EffekseerSetSpeed(_mHandle, value);
            }
            get {
                return Plugin.EffekseerGetSpeed(_mHandle);
            }
        }

//        /// <summary xml:lang="en">
//        /// play frame
//        /// </summary>
//        /// <summary xml:lang="ja">
//        /// 再生フレーム
//        /// </summary>
//        public float Frame {
//            get {
//                return Plugin.EffekseerGetFrame(_mHandle);
//            }
//        }

        /// <summary xml:lang="ja">
        /// Whether the effect instance is enabled<br/>
        /// <para>true:  enabled</para>
        /// <para>false: disabled</para>
        /// </summary>
        /// <summary xml:lang="ja">
        /// インスタンスハンドルが有効かどうか<br/>
        /// <para>true:  有効</para>
        /// <para>false: 無効</para>
        /// </summary>
        public bool Enabled {
            get {
                return _mHandle >= 0;
            }
        }

        /// <summary xml:lang="en">
        /// Existing state
        /// <para>true:  It's existed.</para>
        /// <para>false: It isn't existed or stopped.</para>
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクトのインスタンスが存在しているかどうか
        /// <para>true:  存在している</para>
        /// <para>false: 再生終了で破棄。もしくはStopで停止された</para>
        /// </summary>
        public bool Exists {
            get {
                return Plugin.EffekseerExists(_mHandle);
            }
        }
    }
}