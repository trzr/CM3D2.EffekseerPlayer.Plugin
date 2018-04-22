using System.Collections.Generic;
using EffekseerPlayer.CM3D2.Data;
using EffekseerPlayer.Effekseer;
using UnityEngine;

namespace EffekseerPlayer.CM3D2 {
    public class PlayManager {
        
        ~PlayManager() {
            Clear();
        }

        /// <summary>
        /// 指定されたレシピのEffectをロードし、再生する.
        /// </summary>
        /// <param name="recipe">レシピオブジェクト</param>
        public void Play(PlayRecipe recipe) {
            Load(recipe);

            Log.Debug("to play...", recipe.emitter);
            if (recipe.emitter.enabled) recipe.emitter.Play();
        }

        /// <summary>
        /// レシピにエミッターをロードする.
        /// </summary>
        /// <param name="recipe">レシピ</param>
        public void Load(PlayRecipe recipe) {
            if (recipe.emitter == null) {
                var recipeId = recipe.RecipeId;
                Log.Debug("Load emitter.id=", recipeId, ", effekseer=", recipe.effectName);
                var gobj = new GameObject(ConstantValues.EFK_PREFIX + recipeId);

                var emitter = gobj.AddComponent<EffekseerEmitter>();
                cache[recipeId] = recipe;

                recipe.Load(emitter);
                
            } else {
                recipe.Reload();    
            }
        }

        /// <summary>
        /// IDを指定して、エフェクトを再生する.
        /// IDに対応するエミッターが見つからない場合は何もしない
        /// </summary>
        /// <param name="recipeId">レシピID</param>
        /// <returns>再生に成功した場合はtrueを返す</returns>
        public bool Play(string recipeId) {
            PlayRecipe recipe;
            if (!cache.TryGetValue(recipeId, out recipe)) return false;

            if (!recipe.emitter.enabled) return false;
            Log.Debug("to play...", recipe.emitter);
            recipe.emitter.Play();
            return true;
        }

        /// <summary>
        /// 登録されている有効なエミッタ―をすべて再生する.
        /// </summary>
        public void PlayAll() {
            foreach (var recipe in cache.Values) {
                if (recipe.emitter.enabled) recipe.emitter.Play();
            }
        }

        public void Stop(PlayRecipe recipe) {
            if (recipe.emitter == null) return;
            Log.Debug(recipe.RecipeId, " stop");

            recipe.emitter.Stop();
        }

        public void Stop(string recipeId) {
            PlayRecipe recipe;
            if (!cache.TryGetValue(recipeId, out recipe)) return;
            if (recipe.emitter == null) return;

            Log.Debug(recipeId, " stop");
            recipe.emitter.Stop();
        }

        public void StopRoot(string recipeId) {
            PlayRecipe recipe;
            if (!cache.TryGetValue(recipeId, out recipe)) return;
            if (recipe.emitter == null) return;

            Log.Debug(recipeId, " stopRoot");
            recipe.emitter.StopRoot();
        }

        public void Shown(string recipeId, bool show) {
            PlayRecipe recipe;
            if (!cache.TryGetValue(recipeId, out recipe)) return;
            if (recipe.emitter == null) return;

            recipe.emitter.Shown = show;
        }

        public void ShownAll(bool show) {
            foreach (var recipe in cache.Values) {
                if (recipe.emitter != null) {
                    recipe.emitter.Shown = show;
                }
            }
        }

        public void Clear() {
            foreach (var recipe in cache.Values) {
                if (recipe.emitter == null) continue;

                recipe.emitter.Stop();
                Object.Destroy(recipe.emitter);
                recipe.emitter = null;
            }
            cache.Clear();
        }

        public void StopAll() {
            //EffekseerSystem.StopAllEffects();
            foreach (var recipe in cache.Values) {
                if (recipe.emitter == null) continue;
                recipe.emitter.Stop();
            }
        }

        public void StopRootAll() {
            foreach (var recipe in cache.Values) {
                if (recipe.emitter == null) continue;
                if (recipe.emitter.Exists) {
                    recipe.emitter.StopRoot();
                }
            }
        }

        /// <summary>
        /// 指定したIDのオブジェクトをポーズする
        /// </summary>
        /// <param name="recipeId">レシピID</param>
        public void Pause(string recipeId) {
            PlayRecipe recipe;
            if (!cache.TryGetValue(recipeId, out recipe)) return;
            if (recipe.emitter == null) return;

            // Log.Debug(recipeId, " pausing...", emitter.Paused);
            recipe.emitter.Paused = !recipe.emitter.Paused;
        }

        public void PauseAll(bool pause) {
            EffekseerSystem.SetPausedToAllEffects(pause);
        }

        #region Fields
        public readonly Dictionary<string, PlayRecipe> cache = new Dictionary<string, PlayRecipe>();
        #endregion
    }
}
