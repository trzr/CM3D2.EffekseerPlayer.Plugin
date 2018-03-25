using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace EffekseerPlayerPlugin.Util {
    /// <summary>
    /// リソースのロードユーティリティ
    /// </summary>
    public sealed class ResourceHolder {
        private static readonly ResourceHolder INSTANCE = new ResourceHolder();
        public static ResourceHolder Instance {
            get {
                return INSTANCE;
            }
        }
        private ResourceHolder() {}

        ~ResourceHolder() {
            Clear();
        }
        private readonly Assembly _asmbl = Assembly.GetExecutingAssembly();
        private Texture2D _dirImage;
        private Texture2D _fileImage;
        private Texture2D _pictImage;
        private Texture2D _copyImage;
        private Texture2D _pasteImage;

        private Texture2D _plusImage;
        private Texture2D _minusImage;
        private Texture2D _checkonImage;
        private Texture2D _checkoffImage;
        private Texture2D _checkpartImage;
        private Texture2D _reloadImage;
        private Texture2D _repeatImage;
        private Texture2D _repeatoffImage;
        private Texture2D _playImage;
        private Texture2D _stopImage;
        private Texture2D _stopRImage;
        private Texture2D _pauseImage;
        private Texture2D _deleteImage;

        private Texture2D _frameImage;
        public Texture2D PictImage {
            get { return _pictImage ?? (_pictImage = LoadTex("picture")); }
        }
        public Texture2D FileImage {
            get { return _fileImage ?? (_fileImage = LoadTex("file")); }
        }
        public Texture2D DirImage {
            get { return _dirImage ?? (_dirImage = LoadTex("folder")); }
        }
        public Texture2D CopyImage {
            get { return _copyImage ?? (_copyImage = LoadTex("copy")); }
        }
        public Texture2D PasteImage {
            get { return _pasteImage ?? (_pasteImage = LoadTex("paste")); }
        }
        public Texture2D PlusImage {
            get { return _plusImage ?? (_plusImage = LoadTex("node_plus")); }
        }
        public Texture2D MinusImage {
            get { return _minusImage ?? (_minusImage = LoadTex("node_minus")); }
        }
        public Texture2D CheckoffImage {
            get { return _checkoffImage ?? (_checkoffImage = LoadTex("check_off")); }
        }
        public Texture2D CheckonImage {
            get { return _checkonImage ?? (_checkonImage = LoadTex("check_on")); }
        }
        public Texture2D CheckpartImage {
            get { return _checkpartImage ?? (_checkpartImage = LoadTex("check_part")); }
        }
        public Texture2D FrameImage {
            get { return _frameImage ?? (_frameImage = LoadTex("frame")); }
        }
        public Texture2D ReloadImage {
            get { return _reloadImage ?? (_reloadImage = LoadTex("reload")); }
        }
        public Texture2D RepeatImage {
            get { return _repeatImage ?? (_repeatImage = LoadTex("repeat")); }
        }
        public Texture2D RepeatOffImage {
            get { return _repeatoffImage ?? (_repeatoffImage = LoadTex("repeat_off")); }
        }
        public Texture2D PlayImage {
            get { return _playImage ?? (_playImage = LoadTex("play")); }
        }
        public Texture2D StopImage {
            get { return _stopImage ?? (_stopImage = LoadTex("stop")); }
        }
        public Texture2D StopRImage {
            get { return _stopRImage ?? (_stopRImage = LoadTex("stopr")); }
        }
        public Texture2D PauseImage {
            get { return _pauseImage ?? (_pauseImage = LoadTex("pause")); }
        }
        public Texture2D DeleteImage {
            get { return _deleteImage ?? (_deleteImage = LoadTex("delete")); }
        }

        private Texture2D LoadTex(string name) {
            try {
                using (var fs = _asmbl.GetManifestResourceStream(name + ".png")) {
                    var tex2D = LoadTexture(fs);
                    tex2D.name = name;
//                    Log.Debug("resource file image loaded :", name);
                    return tex2D;
                }
            } catch(Exception e) {
                Log.Info("アイコンリソースのロードに失敗しました。空の画像として扱います", name, e);
                return new Texture2D(2, 2);
            }
        }
        public static Texture2D LoadTexture(Stream stream) {
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            var tex2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex2D.LoadImage(bytes);
            tex2D.wrapMode = TextureWrapMode.Clamp;

            return tex2D;
        }

        internal byte[] LoadBytes(string path) {
            try {
                using (var fs = _asmbl.GetManifestResourceStream(path)) {
                    if (fs == null) return null;
                    var buffer = new byte[8192];
                    using (var ms = new MemoryStream((int) fs.Length)) {
                        int read;
                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0) {
                            ms.Write(buffer, 0, read);
                        }

                        return ms.ToArray();
                    }
                }
            } catch(Exception e) {
                Log.Error("リソースのロードに失敗しました。path=", path, e);
                throw;
            }
        }
        public void Clear() {
            Delete(ref _pictImage);
            Delete(ref _dirImage);
            Delete(ref _fileImage);
            Delete(ref _copyImage);
            Delete(ref _pasteImage);
            Delete(ref _plusImage);
            Delete(ref _minusImage);
            Delete(ref _checkonImage);
            Delete(ref _checkoffImage);
            Delete(ref _checkpartImage);
            Delete(ref _frameImage);
            Delete(ref _reloadImage);
            Delete(ref _repeatImage);
            Delete(ref _repeatoffImage);
            Delete(ref _playImage);
            Delete(ref _stopImage);
            Delete(ref _stopRImage);
            Delete(ref _pauseImage);
            Delete(ref _deleteImage);
        }

        private void Delete(ref Texture2D tex) {
            if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            tex = null;
        } 
        
    }
}
