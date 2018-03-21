using System.Collections.Generic;
using EffekseerPlayerPlugin.CM3D2.Data;
using EffekseerPlayerPlugin.Effekseer;
using UnityEngine;

namespace EffekseerPlayerPlugin.CM3D2
{
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
                emtDic[recipeId] = emitter;

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
            EffekseerEmitter emitter;
            if (!emtDic.TryGetValue(recipeId, out emitter)) return false;

            if (!emitter.enabled) return false;
            Log.Debug("to play...", emitter);
            emitter.Play();
            return true;
        }

        /// <summary>
        /// 登録されている有効なエミッタ―をすべて再生する.
        /// </summary>
        public void PlayAll() {
            foreach (var emitter in emtDic.Values) {
                if (emitter.enabled) emitter.Play();
            }
        }

        public void Stop(PlayRecipe recipe) {
            if (recipe.emitter == null) return;
            Log.Debug(recipe.RecipeId, " stop");

            recipe.emitter.Stop();
        }

        public void Stop(string recipeId) {
            EffekseerEmitter emitter;
            if (!emtDic.TryGetValue(recipeId, out emitter)) return;

            Log.Debug(recipeId, " stop");
            emitter.Stop();
        }

        public void StopRoot(string recipeId) {
            EffekseerEmitter emitter;
            if (!emtDic.TryGetValue(recipeId, out emitter)) return;

            Log.Debug(recipeId, " stopRoot");
            emitter.StopRoot();
        }

        public void Shown(string recipeId, bool show) {
            EffekseerEmitter emitter;
            if (!emtDic.TryGetValue(recipeId, out emitter)) return;

            emitter.Shown = show;
        }

        public void ShownAll(bool show) {
            foreach (var emitter in emtDic.Values) {
                emitter.Shown = show;
            }
        }

        public void Clear() {
            foreach (var emitter in emtDic.Values) {
                emitter.Stop();
                Object.Destroy(emitter);
            }
            emtDic.Clear();
        }


        public void StopAll() {
            //EffekseerSystem.StopAllEffects();
            foreach (var emitter in emtDic.Values) {
                emitter.Stop();
            }
        }

        public void StopRootAll() {
            foreach (var emitter in emtDic.Values) {
                if (emitter.Exists) {
                    emitter.StopRoot();
                }
            }
        }

        /// <summary>
        /// 指定したIDのオブジェクトをポーズする
        /// </summary>
        /// <param name="recipeId">レシピID</param>
        public void Pause(string recipeId) {
            EffekseerEmitter emitter;
            if (!emtDic.TryGetValue(recipeId, out emitter)) return;

            // Log.Debug(recipeId, " pausing...", emitter.Paused);
            emitter.Paused = !emitter.Paused;
        }

        public void PauseAll(bool pause) {
            EffekseerSystem.SetPausedToAllEffects(pause);
        }

        #region Fields
        public readonly Dictionary<string, EffekseerEmitter> emtDic = new Dictionary<string, EffekseerEmitter>();
        #endregion
    }
}
