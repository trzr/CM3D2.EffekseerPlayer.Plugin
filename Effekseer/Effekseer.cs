using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace EffekseerPlayerPlugin.Effekseer
{
	[Serializable]
	internal abstract class Resource
	{
		public string Path {get; protected set;}
		public AssetBundle Bundle {get; protected set;}

		public abstract bool Load(string path, AssetBundle assetBundle);
		public abstract void Unload();
		protected readonly string workDir;
        internal Resource(string workDir) {
            this.workDir = workDir;
        }

		protected T LoadAsset<T>(string path, bool removeExtension, AssetBundle assetBundle) where T : UnityEngine.Object {
            //Log.Debug("load asset:", path);
            Path = path;
			Bundle = assetBundle;
			if (assetBundle != null) {
				return assetBundle.LoadAsset<T>(
					Utility.ResourcePath(path, removeExtension));
			}

			return Resources.Load<T>(
				Utility.ResourcePath(path, removeExtension));
		}

		protected void UnloadAsset(UnityEngine.Object asset) {
			if (asset == null) return;

			if (Bundle == null) {
				Resources.UnloadAsset(asset);
			} else {
				Bundle.Unload(asset);
			}
		}
	}

	[Serializable]
	internal class TextureResource : Resource
	{
		public Texture2D texture;

        internal TextureResource(string dir=null) : base(dir) {}

        public override bool Load(string path, AssetBundle assetBundle) {
            // Log.Debug("load tex:", path);
            if (assetBundle != null) {
                texture = LoadAsset<Texture2D>(path, false, assetBundle);
                if (texture != null) return true;
            }
            var filepath = System.IO.Path.Combine(workDir, path);
            if (File.Exists(filepath)) {
                texture = UTY.LoadTexture(filepath);
            } else {
                //texture = LoadAsset<Texture2D>(path, true, assetBundle);
                Debug.LogError("[Effekseer] Failed to load Texture: " + path);
                return false;
            }
			return true;
		}

		public override void Unload() {
			UnloadAsset(texture);
			texture = null;
		}

		public IntPtr GetNativePtr() {
			return texture.GetNativeTexturePtr();
		}
	}

	[Serializable]
	internal class ModelResource : Resource
	{
		public TextAsset modelData;
        private byte[] _bytes;

        internal ModelResource(string dir=null) : base(dir) {}

		public int Length {
			get {
				if (modelData != null) return modelData.bytes.Length;
				return _bytes != null ? _bytes.Length : 0;
			}
		}

        public override bool Load(string path, AssetBundle assetBundle) {
	        Log.Debug("model load path=", path);
            if (assetBundle != null) {
                modelData = LoadAsset<TextAsset>(path, false, assetBundle);
                if (modelData == null) {
                    Debug.LogError("[Effekseer] Failed to load Model: " + path);
                    return false;
                }
            }
            var filepath = System.IO.Path.Combine(workDir, path);
	        if (!File.Exists(filepath)) {
		        Log.Debug("model file not found.", filepath);
		        return false;
	        }

	        _bytes = File.ReadAllBytes(filepath);
	        modelData = null;
	        if (_bytes != null) return true;

	        Debug.LogError("[Effekseer] Failed to load Model: " + path);
	        return false;
        }

		public override void Unload() {
            if (modelData != null) UnloadAsset(modelData);
			modelData = null;
            _bytes = null;
		}

		public bool Copy(IntPtr buffer, int bufferSize) {
			var bytes = (modelData != null) ? modelData.bytes : _bytes;
			if (bytes == null || bytes.Length >= bufferSize) return false;

			Marshal.Copy(bytes, 0, buffer, bytes.Length);
			return true;
		}
	}

	[Serializable]
	internal class SoundResource : Resource
	{
		public AudioClip audio;

        internal SoundResource(string dir=null) : base(dir) { }

        public override bool Load(string path, AssetBundle assetBundle) {
		    if (assetBundle != null) {
                audio = LoadAsset<AudioClip>(path, true, assetBundle);
                //Debug.Log("load sound path: " + path);
                if (audio != null) return true;
		    }

	        var audioType = AudioType.WAV;
	        var compressed = false;
	        var filepath = System.IO.Path.Combine(workDir, path);
	        // 見つからない場合、oggがないかも探してみる
	        if (!File.Exists(filepath)) {
		        filepath = filepath.Substring(0, filepath.Length - 3) + "ogg";
		        if (File.Exists(filepath)) {
			        audioType = AudioType.OGGVORBIS;
			        compressed = true;
		        }
	        }
            if (File.Exists(filepath)) {
                Debug.Log("load sound file: " + filepath);
                var uri = new Uri(filepath);
                var www = new WWW(uri.AbsoluteUri);
	            audio = compressed ? www.GetAudioClipCompressed(true, audioType) : www.GetAudioClip(true, true, audioType);
	            Path = path;
	            if (audio != null) return true;
            }
            
            Debug.LogError("[Effekseer] Failed to load Sound: " + path);
            return false;
        }

		public override void Unload() {
			UnloadAsset(audio);
			audio = null;
		}
	}

	internal static class Utility
	{
		public static float[] Matrix2Array(Matrix4x4 mat) {
			var res = new float[16];
			res[ 0] = mat.m00; res[ 1] = mat.m01; res[ 2] = mat.m02; res[ 3] = mat.m03;
			res[ 4] = mat.m10; res[ 5] = mat.m11; res[ 6] = mat.m12; res[ 7] = mat.m13;
			res[ 8] = mat.m20; res[ 9] = mat.m21; res[10] = mat.m22; res[11] = mat.m23;
			res[12] = mat.m30; res[13] = mat.m31; res[14] = mat.m32; res[15] = mat.m33;
			return res;
		}

		public static string StrPtr16ToString(IntPtr strptr16, int len) {
			var strarray = new byte[len * 2];
			Marshal.Copy(strptr16, strarray, 0, len * 2);
			return Encoding.Unicode.GetString(strarray);
		}

		public static string ResourcePath(string path, bool removeExtension) {
			var dir = Path.GetDirectoryName(path);
			var file = (removeExtension) ? 
				Path.GetFileNameWithoutExtension(path) : 
				Path.GetFileName(path);
			return "Effekseer/" + ((dir.Length == 0) ? file : dir + "/" + file);
		}
	}
	
	internal static class Plugin
	{
#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_WEBGL)
		public const string pluginName = "__Internal";
#else
		public const string PLUGIN_NAME = "EffekseerUnity";
#endif

		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerInit(int maxInstances, int maxSquares, bool isRightHandedCoordinate, bool reversedDepth);
		
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerTerm();
		
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerUpdate(float deltaTime);
		
		[DllImport(PLUGIN_NAME)]
        public static extern IntPtr EffekseerGetRenderFunc(int renderId = 0);

        [DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetProjectionMatrix(int renderId, float[] matrix);
	
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetCameraMatrix(int renderId, float[] matrix);
		
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetBackGroundTexture(int renderId, IntPtr background);
		
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetRenderSettings(int renderId, bool renderIntoTexture);

		[DllImport(PLUGIN_NAME)]
		public static extern IntPtr EffekseerLoadEffect(IntPtr path);
		
		[DllImport(PLUGIN_NAME)]
		public static extern IntPtr EffekseerLoadEffectOnMemory(IntPtr data, int size);
	
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerReleaseEffect(IntPtr effect);
	
		[DllImport(PLUGIN_NAME)]
		public static extern int EffekseerPlayEffect(IntPtr effect, float x, float y, float z);
	
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerUpdateHandle(int handle, float deltaDrame);
	
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerStopEffect(int handle);
	
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerStopRoot(int handle);
	
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerStopAllEffects();
	
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetPausedToAllEffects(bool paused);

		[DllImport(PLUGIN_NAME)]
		public static extern bool EffekseerGetShown(int handle);

		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetShown(int handle, bool shown);
	
		[DllImport(PLUGIN_NAME)]
		public static extern bool EffekseerGetPaused(int handle);

		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetPaused(int handle, bool paused);
	
		[DllImport(PLUGIN_NAME)]
		public static extern float EffekseerGetSpeed(int handle);

		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetSpeed(int handle, float speed);

//		[DllImport(PLUGIN_NAME)]
//		public static extern float EffekseerGetFrame(int handle);

		[DllImport(PLUGIN_NAME)]
		public static extern bool EffekseerExists(int handle);

		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetLocation(int handle, float x, float y, float z);

		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetRotation(int handle, float x, float y, float z, float angle);

		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetScale(int handle, float x, float y, float z);

		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetAllColor(int handle, int r, int g, int b, int a);

		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetTargetLocation(int handle, float x, float y, float z);

		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetTextureLoaderEvent(
			EffekseerTextureLoaderLoad load,
			EffekseerTextureLoaderUnload unload);
        public delegate IntPtr EffekseerTextureLoaderLoad(IntPtr path, out int width, out int height, out int format);
        public delegate void EffekseerTextureLoaderUnload(IntPtr path);
		
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetModelLoaderEvent(
			EffekseerModelLoaderLoad load,
			EffekseerModelLoaderUnload unload);
		public delegate int EffekseerModelLoaderLoad(IntPtr path, IntPtr buffer, int bufferSize);
		public delegate void EffekseerModelLoaderUnload(IntPtr path);
		
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetSoundLoaderEvent(
			EffekseerSoundLoaderLoad load,
			EffekseerSoundLoaderUnload unload);
		public delegate int EffekseerSoundLoaderLoad(IntPtr path);
		public delegate void EffekseerSoundLoaderUnload(IntPtr path);
		
		[DllImport(PLUGIN_NAME)]
		public static extern void EffekseerSetSoundPlayerEvent(
			EffekseerSoundPlayerPlay play, 
			EffekseerSoundPlayerStopTag stopTag, 
			EffekseerSoundPlayerPauseTag pauseTag, 
			EffekseerSoundPlayerCheckPlayingTag checkPlayingTag, 
			EffekseerSoundPlayerStopAll atopAll);
		public delegate void EffekseerSoundPlayerPlay(IntPtr tag, 
			int data, float volume, float pan, float pitch, 
			bool mode3D, float x, float y, float z, float distance);
		public delegate void EffekseerSoundPlayerStopTag(IntPtr tag);
		public delegate void EffekseerSoundPlayerPauseTag(IntPtr tag, bool pause);
		public delegate bool EffekseerSoundPlayerCheckPlayingTag(IntPtr tag);
		public delegate void EffekseerSoundPlayerStopAll();
	}

	public class SoundInstance : MonoBehaviour {
		public int AudioTag;

        public AudioSource Audio { get; set; }

		void Awake() {
			Audio = gameObject.AddComponent<AudioSource>();
			Audio.playOnAwake = false;
		}

		void Update() {
			if (Audio.clip && !Audio.isPlaying) {
				Audio.clip = null;
			}
		}

		public void Play(int audioTag, AudioClip clip, 
			float volume, float pan, float pitch, 
			bool mode3D, float x, float y, float z, float distance) {
			AudioTag = audioTag;
			transform.position = new Vector3(x, y, z);
			Audio.spatialBlend = mode3D ? 1.0f : 0.0f;
			Audio.volume = volume; // [0, 1]
			Audio.pitch = Mathf.Pow(2.0f, pitch);
			Audio.panStereo = pan;
			Audio.minDistance = distance;
			Audio.maxDistance = distance * 2;
			Audio.clip = clip;
			Audio.Play();
			// clip.loadStateへのアクセスがないとなぜか音が出ない…
			var state = clip.loadState;
			// Log.Debug("Play sound:", audioTag, ", state=", clip.loadState);
		}

		public void SetPosition(float x, float y, float z) {
			transform.position = new Vector3(x, y, z);
		}

		public void Stop() {
			Audio.Stop();
		}

		public void Pause(bool paused) {
			if (paused) Audio.Pause();
			else Audio.UnPause();
		}

		public bool CheckPlaying() {
			return Audio.isPlaying;
		}
	}
}