using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EffekseerPlayer.Effekseer;
using EffekseerPlayer.Unity.Data;

namespace EffekseerPlayer.CM3D2.Data {
    /// <summary>
    /// PlayRecipeの集合を表すデータクラス.
    /// 1つのjsonファイルに対応する.
    /// </summary>
    [Serializable]
    public class RecipeSet {

        ~RecipeSet() {
            Destroy();
        }

        internal void Destroy() {
            foreach (var recipe in recipeList) {
                recipe.Destroy();
            }
            recipeList.Clear();
            _recipeDic.Clear();

            ClearKeyHandler();
        }

        public CheckStatus Check {
            get { return check; }
            set {
                check = value;
                switch (check) {
                case CheckStatus.NoChecked:
                    foreach (var recipe in recipeList) {
                        recipe.selected = false;
                    }
                    break;
                case CheckStatus.Checked:
                    foreach (var recipe in recipeList) {
                        recipe.selected = true;
                    }
                    break;
                case CheckStatus.PartChecked:
                    break;
                }
            }
        }

        /// <summary>
        /// PlayRecipeと情報を同期する.
        /// <c>PlayRecipe.Parent</c>の設定や、Dictionaryの再生成、名前が重複しているデータの削除
        /// ファイルからロード後などに実行することを想定.
        /// </summary>
        internal void Synch() {
            _recipeDic.Clear();
            for (var i=recipeList.Count-1; i>=0; i--) {
                var recipe = recipeList[i];
                recipe.Parent = this;
                // 重複をチェックし、削除
                if (!_recipeDic.ContainsKey(recipe.name)) {
                    _recipeDic[recipe.name] = recipe;
                } else {
                    recipeList.RemoveAt(i);
                    Log.Debug("recipe was deleted because of duplication. name=", recipe.name);
                }
            }
        }

        /// <summary>
        /// レシピのアイテム数を取得する.
        /// </summary>
        /// <returns>レシピのアイテム数</returns>
        public int Size() {
            return recipeList.Count;
        }

 
        /// <summary>
        /// レシピを登録する.
        /// </summary>
        /// <param name="recipe">レシピ</param>
        public void Register(PlayRecipe recipe) {
            PlayRecipe old;
            if (_recipeDic.TryGetValue(recipe.name, out old)) {
                var idx = recipeList.IndexOf(old);
                recipe.selected = old.selected;
                if (idx != -1) {
                    recipeList[idx] = recipe;
                } else {
                    recipeList.Add(recipe);
                }
                old.Destroy();
            } else {
                recipeList.Add(recipe);
            }
            Log.Debug("register recipe:", recipe.name, ", count=", recipeList.Count);

            _recipeDic[recipe.name] = recipe;
            recipe.Parent = this;

        }

        /// <summary>
        /// レシピ名でレシピを削除する.
        /// 削除時に再生中のオブジェクトは停止し、破棄する.
        /// </summary>
        /// <param name="recipeName">レシピ名</param>
        /// <returns>削除に成功した場合trueを返す</returns>
        public bool Remove(string recipeName) {
            PlayRecipe old;
            if (!_recipeDic.TryGetValue(recipeName, out old)) return false;

            recipeList.Remove(old);
            _recipeDic.Remove(recipeName);
            old.Parent = null;
            old.Destroy();
            return true;
        }

        /// <summary>
        /// 指定したレシピを削除する.
        /// ただし、同オブジェクトとは限らなくとも同名のレシピであれば削除される.
        /// </summary>
        /// <param name="recipe">レシピ</param>
        /// <returns>削除に成功した場合trueを返す</returns>
        public bool Remove(PlayRecipe recipe) {
            return Remove(recipe.name);
        }

        /// <summary>
        /// レシピ名でレシピを取得する.
        /// 該当するレシピが存在しない場合はnullを返す.
        /// </summary>
        /// <param name="recipeName">レシピ名</param>
        /// <returns>レシピ</returns>
        public PlayRecipe Get(string recipeName) {
            PlayRecipe recipe;
            return _recipeDic.TryGetValue(recipeName, out recipe) ? recipe : null;
        }

        /// <summary>
        /// セット内のレシピが停止状態(未初期化)である場合にtrueを返す.
        /// </summary>
        /// <returns>停止状態の場合にtrue</returns>
        public bool IsStopped() {
            foreach (var recipe in recipeList) {
                if (recipe.Status != EffekseerEmitter.EmitterStatus.Stopped && recipe.Status != EffekseerEmitter.EmitterStatus.Empty) {
                    return false;
                }
            }
            return true;
        }

        public void ClearKeyHandler() {
            if (keyHandler == null) return;

            keyHandler.dataList.Remove(this);
            keyHandler = null;
        }

        /// <summary>
        /// レシピセットの情報をJSON形式で出力する.
        /// </summary>
        /// <param name="writer">出力先ライター</param>
        /// <param name="pretty">整形フラグ</param>
        public void ToJSON(StreamWriter writer, bool pretty=false) {
            var buff = new StringBuilder();
            buff.Append("{");
            if (pretty) buff.Append("\n");
            buff.Append("  \"name\": ").Append('"').Append(name).Append("\",");
            if (pretty) buff.Append("\n");

            if (playKeyCode != null) {
                buff.Append("  \"playKeyCode\": ").Append('"').Append(playKeyCode).Append("\",");
                if (pretty) buff.Append("\n");
            }

            buff.Append("  \"recipeList\": [");
            if (pretty) buff.Append("\n");
            writer.Write(buff);
            buff.Length = 0;

            var isFirst = true;
            foreach (var recipe in recipeList) {
                if (isFirst) isFirst = false;
                else {
                    buff.Append(',');
                    if (pretty) buff.Append("\n");
                }
                recipe.ToJSON(buff, pretty, ConstantValues.JSON_INDENT);
                writer.Write(buff);
                buff.Length = 0;
            }
            if (pretty) buff.Append("\n");
            buff.Append("  ]");
            if (pretty) buff.Append("\n");
            buff.Append("}");
            writer.Write(buff);
        }

        #region Fields
        public readonly List<PlayRecipe> recipeList = new List<PlayRecipe>();
        private readonly Dictionary<string, PlayRecipe> _recipeDic = new Dictionary<string, PlayRecipe>();

        public string name;
        public string playKeyCode;

        public bool expand;
        private CheckStatus check;
        public long lastWriteTime;
        public bool loaded;
        public InputKeyDetectHandler<RecipeSet>.KeyHandler keyHandler;
        #endregion
    }
}
