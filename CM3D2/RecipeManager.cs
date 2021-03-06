﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EffekseerPlayer.CM3D2.Data;

namespace EffekseerPlayer.CM3D2 {

    /// <summary>
    /// 再生レシピのセーブ・ロードを行う管理するマネージャクラス.
    /// </summary>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class RecipeManager {

        public RecipeManager(string directory, PlayManager playManager) {
            this.directory = directory;
            this.playManager = playManager;
        }

        public void InitState() {
            paused = false;
        }

        public bool Any() {
            return _recipeSets.Any();
        }

        public List<RecipeSet> GetRecipeSets() {
            return _recipeSets;
        }

        /// <summary>
        /// レシピセットを追加する.
        /// </summary>
        /// <param name="set">レシピセット</param>
        /// <returns>上書き前の同名レシピセット</returns>
        public RecipeSet Add(RecipeSet set) {
            RecipeSet recipeSet;
            if (_recipeSetDic.TryGetValue(set.name, out recipeSet)) {
                var idx = _recipeSets.IndexOf(recipeSet);
                _recipeSets[idx] = set;
                _recipeSetDic[set.name] = set;
                return recipeSet;
            }
            
            _recipeSets.Add(set);
            _recipeSetDic[set.name] = set;
            return null;
        }

        /// <summary>
        /// 指定したセットにレシピを登録する.
        /// </summary>
        /// <param name="setName">セット名</param>
        /// <param name="recipe">レシピ</param>
        public void Register(string setName, PlayRecipe recipe) {
            Log.Debug("register set. name:", setName);
            RecipeSet recipeSet;
            if (!_recipeSetDic.TryGetValue(setName, out recipeSet)) {
                recipeSet = new RecipeSet() {
                    name = setName,
                };
                _recipeSets.Add(recipeSet);
                _recipeSetDic[setName] = recipeSet;
            }
            recipeSet.Register(recipe);

            Save(recipeSet);
        }

        /// <summary>
        /// 指定セットを削除する.
        /// </summary>
        /// <param name="setName">セット名</param>
        /// <param name="deleteFile">ファイルを削除するか</param>
        /// <returns>削除に成功した場合trueを返す</returns>
        public bool RemoveSet(string setName, bool deleteFile=false) {
            RecipeSet recipeSet;
            if (!_recipeSetDic.TryGetValue(setName, out recipeSet)) return false;

            _recipeSetDic.Remove(setName);
            var removed = _recipeSets.Remove(recipeSet);
            recipeSet.Destroy();
            if (!deleteFile) return false;

            var filepath = Path.Combine(directory, setName + ConstantValues.EXT_JSON);
            try {
                File.Delete(filepath);
            } catch(Exception e) {
                Log.Info("Failed to delete file:", filepath, e);
            }
            return removed;
        }

        /// <summary>
        /// 指定セットを削除する.
        /// セット名が一致したセットを削除する.
        /// </summary>
        /// <param name="set">セット</param>
        /// <param name="deleteFile">ファイルを削除するか</param>
        /// <returns>削除に成功した場合trueを返す</returns>
        public bool RemoveSet(RecipeSet set, bool deleteFile=false) {
            return RemoveSet(set.name, deleteFile);
        }

        /// <summary>
        /// 指定セットからレシピ名でレシピを削除する.
        /// updateFile=true指定の場合、レシピ削除が成功したらファイルを更新する.
        /// </summary>
        /// <param name="setName">セット名</param>
        /// <param name="name">レシピ名</param>
        /// <param name="updateFile">ファイルを更新するか</param>
        /// <returns>削除に成功した場合trueを返す</returns>
        public bool Remove(string setName, string name, bool updateFile=false) {
            RecipeSet recipeSet;
            if (_recipeSetDic.TryGetValue(setName, out recipeSet)) {
                if (recipeSet.Remove(name)) {
                    if (updateFile) Save(recipeSet);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 指定したレシピを削除する.
        /// ただし、同オブジェクトとは限らなくとも同名のレシピであれば削除される.
        /// </summary>
        /// <param name="setName">セット名</param>
        /// <param name="recipe">レシピ</param>
        /// <param name="updateFile">ファイルを更新するか</param>
        /// <returns>削除に成功した場合trueを返す</returns>
        public bool Remove(string setName, PlayRecipe recipe, bool updateFile=false) {
            return Remove(setName, recipe.name, updateFile);
        }

        /// <summary>
        /// レシピ名でレシピを取得する.
        /// 該当するレシピが存在しない場合はnullを返す.
        /// </summary>
        /// <param name="setName">セット名</param>
        /// <param name="name">レシピ名</param>
        /// <returns>レシピ</returns>
        public PlayRecipe Get(string setName, string name) {
            RecipeSet recipeSet;
            return _recipeSetDic.TryGetValue(setName, out recipeSet) ? recipeSet.Get(name) : null;
        }

        /// <summary>
        /// ファイルの最終更新日時が変更のあったファイルのみを更新する.
        /// 更新のあったファイルに関連するオブジェクトは一旦すべて破棄する.
        /// </summary>
        public void Reload() {
            foreach (var set in _recipeSets) {
                set.loaded = false;
            }
            Load();

            // 存在しないファイルのデータ破棄
            foreach (var set in _recipeSets) {
                if (set.loaded) continue;

                _recipeSetDic.Remove(set.name);
                set.Destroy();
            }
            _recipeSets.RemoveAll((set) => !set.loaded);
        }

        public void Load() {
            Log.Debug("Loading recipeSet.");
            var filenames = Directory.GetFiles(directory, "*" + ConstantValues.EXT_JSON, SearchOption.TopDirectoryOnly);
            foreach (var filename in filenames) {
                var filepath = Path.Combine(directory, filename);
                var file = new FileInfo(filepath);

                // 既存ファイルの存在確認
                var recipeSetName = Path.GetFileNameWithoutExtension(filepath);
                RecipeSet prevSet;
                if (_recipeSetDic.TryGetValue(recipeSetName, out prevSet)) {
                    // 既にあるファイルは、更新日時が変更されていた場合のみロード
                    if (prevSet.lastWriteTime == file.LastWriteTime.Ticks) {
                        prevSet.loaded = true;
                        continue;
                    }
                }
                try {
                    var recipeSet = LoadJson(file);

                    Log.Debug("Loaded recipeSet :", recipeSetName);
                    recipeSet.name = recipeSetName;
                    recipeSet.loaded = true;
                    recipeSet.lastWriteTime = file.LastWriteTime.Ticks;
                    recipeSet.Synch();

                    prevSet = Add(recipeSet);
                    if (prevSet != null) prevSet.Destroy();
                    
                } catch(Exception e) {
                    Log.Error("failed to load recipeSet. file=", filename, e);
                }
            }

            InitKeyHandler();
        }

        protected virtual RecipeSet LoadJson(FileInfo fi) {
            using (var sr = fi.OpenText()) {
                return RecipeParser.Instance.ParseSet(sr);
            }
        }

        /// <summary>
        /// 指定したレシピセットをファイルに保存する.
        /// 
        /// 一時ファイルを作成して書き出した後に、ファイルをリネームして上書きする.
        /// </summary>
        /// <param name="recipeSet">レシピセット</param>
        public void Save(RecipeSet recipeSet) {
            var filepath = Path.Combine(directory, recipeSet.name + ConstantValues.EXT_JSON);
            var tmppath = Path.Combine(directory, "tmp"+Path.GetRandomFileName());
            try {
                SaveJson(tmppath, recipeSet);

                if (File.Exists(filepath)) File.Delete(filepath);
                File.Move(tmppath, filepath);
                Log.Debug("recipe file written.", recipeSet.name, ConstantValues.EXT_JSON);

                // オブジェクトで保持している更新日時を更新
                recipeSet.lastWriteTime = new FileInfo(filepath).LastWriteTime.Ticks;

            } catch (Exception e) {
                Log.Error("failed to save json. file=", filepath, e);
            } finally {
                if (File.Exists(tmppath)) File.Delete(tmppath);
            }
        }

        protected virtual void SaveJson(string filepath, RecipeSet recipeSet) {
            using (var sw = new StreamWriter(filepath, false, Encoding.UTF8)) {
                recipeSet.ToJSON(sw, true);
            }
        }

        public void InitKeyHandler() {
            var detectHandler = InputKeyDetectHandler<RecipeSet>.Get();
            detectHandler.keyHandlers.Clear();
            detectHandler.keyHandlers.AddRange(Handlers);

            // 指定されたキー情報が同じレシピセットをまとめる
            var workDic = new Dictionary<InputKeyDetectHandler<RecipeSet>.KeyHolder, IList<RecipeSet>>();
            foreach (var recipeSet in _recipeSets) {
                // スキップ対象: 再読み込み時の削除ターゲット, キーコードが空のターゲット
                if (!recipeSet.loaded || recipeSet.playKeyCode == null || recipeSet.playKeyCode.Trim().Length == 0) continue;

                Log.Debug("recipe playKeyCode:", recipeSet.playKeyCode);
                var keyHolder = detectHandler.Parse(recipeSet.playKeyCode);
                if (keyHolder.IsEmpty()) continue;

                IList<RecipeSet> old;
                if (!workDic.TryGetValue(keyHolder, out old)) {
                    old = new List<RecipeSet>();
                    workDic[keyHolder] = old;
                }
                old.Add(recipeSet);
            }

            // 
            foreach (var entry in workDic) {
                var detector = detectHandler.CreateKeyDetector(entry.Key);
                if (detector == null) {
                    Log.Debug("failed to create KeyDetector:", entry.Key);
                    continue;
                }

                var keyHandler = new InputKeyDetectHandler<RecipeSet>.KeyHandler {
                    detector = detector,
                    keyHolder = entry.Key,
                    dataList = entry.Value,
                };
                foreach (var rset in keyHandler.dataList) {
                    rset.keyHandler = keyHandler;
                }

                keyHandler.handle = () => {
                    foreach (var rset in keyHandler.dataList) {
                        foreach (var recipe in rset.recipeList) {
                            playManager.Play(recipe);
                        }
                    }
                };

                detectHandler.keyHandlers.Add(keyHandler);
            }

            Log.Debug("Input-keyCode-handler initialized. count=", detectHandler.keyHandlers.Count);
        }

        public void SetupStopKey(string stopKey) {
            Log.Debug("Setup StopKey:", stopKey);
            var handler = SetupKey(stopKey);
            if (handler == null) return;
            handler.handle = playManager.StopAll;
            Handlers.Add(handler);
        }

        public void SetupPauseKey(string key) {
            Log.Debug("Setup Pausekey:", key);
            var handler = SetupKey(key);
            if (handler == null) return;
            handler.handle =  () => {
                playManager.PauseAll(!paused);
                paused = !paused;
            };
            Handlers.Add(handler);
        }

        private InputKeyDetectHandler<RecipeSet>.KeyHandler SetupKey(string key) {
            if (key == null || key.Trim().Length == 0) return null;

            var detectHandler = InputKeyDetectHandler<RecipeSet>.Get();
            var keyHolder = detectHandler.Parse(key);
            if (keyHolder.IsEmpty()) return null;

            var detector = detectHandler.CreateKeyDetector(keyHolder);

            var handler = new InputKeyDetectHandler<RecipeSet>.KeyHandler {
                detector = detector,
                keyHolder = keyHolder,
            };
            detectHandler.keyHandlers.Add(handler);
            return handler;
        }

        #region Fields
        // 保存ディレクトリ
        protected readonly string directory;
        private readonly List<InputKeyDetectHandler<RecipeSet>.KeyHandler> Handlers = new List<InputKeyDetectHandler<RecipeSet>.KeyHandler>();
        public bool paused;

        private readonly PlayManager playManager;
        private readonly List<RecipeSet> _recipeSets = new List<RecipeSet>();
        private readonly Dictionary<string, RecipeSet> _recipeSetDic = new Dictionary<string, RecipeSet>();
        #endregion
    }
}
