using System.Collections.Generic;
using System.IO;

namespace EffekseerPlayerPlugin.CM3D2 {

    /// <summary>
    /// efkファイルマネージャ
    /// 指定ディレクトリ下のefk/bytesファイルをeffectファイルとして抽出し、管理する.
    /// ただし、サブフォルダは1階層のみとし、それ以降のファイルは抽出しない
    /// </summary>
    public class EfkManager {
        private readonly string _efkDir;
        private readonly Dictionary<string, string> _fileDic = new Dictionary<string, string>();
        private readonly List<string> _effectNames = new List<string>();
        public List<string> EffectNames {
            get {
                return _effectNames;
            }
        }

        internal EfkManager(string efkDir) {
            _efkDir = efkDir;
        }

        public void ScanDir() {
            _effectNames.Clear();

            var di = new DirectoryInfo(_efkDir);
            ScanFiles(di);
            var dirs = di.GetDirectories("*", SearchOption.TopDirectoryOnly);

//            var dirs = Directory.GetDirectories(_efkDir, "*", SearchOption.TopDirectoryOnly);//Directory.EnumerateDirectories(efkDir);
            foreach (var dir in dirs) {
                if (dir.Name.StartsWith("_")) continue;
                ScanFiles(dir);
            }
        }

        protected void ScanFiles(DirectoryInfo dir) {
            //var files = Directory.EnumerateFiles(efkDir, "*.*", SearchOption.TopDirectoryOnly);
            var files = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            var count = 0;
            foreach (var file in files) {

                // ReSharper disable once PossibleNullReferenceException
                var ext = file.Extension.ToLower();
                switch (ext) {
                case ".efk":
                case ".bytes":
                    var effectName = Path.GetFileNameWithoutExtension(file.Name);
                    // TODO ToLower
                    // ReSharper disable once AssignNullToNotNullAttribute
                    if (!_fileDic.ContainsKey(effectName)) {
                        _fileDic.Add(effectName, file.Name);
                        _effectNames.Add(effectName);
                        count++;
                    } else {
                        Log.InfoF("efkファイルが重複しています. ({0}) スキップします: {1}", effectName, file);
                    }

                    break;
                }
            }

            Log.Debug("loaded efk:", count, " scan dir:", dir);
        }
    }
}
