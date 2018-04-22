using System;
using EffekseerPlayer.CM3D2.Data;
using UnityEngine;

namespace EffekseerPlayer.CM3D2.UI {

    public partial class EditRecipeView {

        private void Register(object obj, EventArgs args) {
            if (!subEditView.CanRegister()) return;
            if (efkCombo.SelectedIndex == -1) return;

            var recipe = CreateRecipe();
            var filename = groupnameText.Text;
            recipeMgr.Register(filename, recipe);
        }

        public PlayRecipe CreateRecipe() {
            var recipe = new PlayRecipe {
                name = nameText.Text,
                effectName = efkCombo.SelectedItem,
                repeat = repeatToggle.Value,
            };
            subEditView.SetupRecipe(recipe);

            return recipe;
        }

        public void SetGroupName(string name) {
            groupnameText.Text = name;
        }

        /// <summary>
        /// レシピ情報をエディットの各パラメータに反映する.
        /// </summary>
        /// <param name="recipe">レシピ情報</param>
        public void ToEditView(PlayRecipe recipe) {
            nameText.Text = recipe.name;
            efkCombo.Index = -1; // 見つからない場合を未選択とするため-1をセット
            efkCombo.SelectedItem = recipe.effectName;
            repeatToggle.Value = recipe.repeat;

            subEditView.ToEditView(recipe);
        }

        private void Play(object obj, EventArgs args) {
            var effectName = efkCombo.SelectedItem;

            var emitter = subEditView.LoadEmitter(effectName, repeatToggle.Value);
            Log.Debug("to play...", emitter);
            emitter.Play();
        }

        
        private GUIContent[] CreateEfkItems() {
            var effectNames = efkMgr.EffectNames;
            var items = new GUIContent[effectNames.Count];
            var idx = 0;

            foreach (var name in effectNames) {
                items[idx++] = new GUIContent(name);
            }

            return items;
        }

        private void Stop(object obj, EventArgs args) {
            if (subEditView.currentEmitter != null) {
                subEditView.currentEmitter.Stop();
            }
        }

        private void StopRoot(object obj, EventArgs args) {
            if (subEditView.currentEmitter != null) {
                subEditView.currentEmitter.StopRoot();
            }
        }

        private void Pause(object obj, EventArgs args) {
            if (subEditView.currentEmitter == null) return;
            subEditView.currentEmitter.Paused = !subEditView.currentEmitter.Paused;
        }

        private void CheckValidate(object obj, EventArgs args) {
            if (CanPlay()) {
                registButton.Enabled = CanRegister();
                EnablePlayButtons(true);
            } else {
                registButton.Enabled = false;
                EnablePlayButtons(false);
            }
        }

        private void EnablePlayButtons(bool enable) {
            playButton.Enabled = enable;
            stopButton.Enabled = enable;
            stopRButton.Enabled = enable;
            pauseButton.Enabled = enable;
        }

        private bool CanRegister() {
            return groupnameText.Text.Length > 0
                   && !groupnameText.hasError
                   && nameText.Text.Length > 0;
        }

        private bool CanPlay() {
            return efkCombo.SelectedIndex != -1 && subEditView.CanPlay();
        }

//        #region Properties
//        private readonly bool _hasDirty;
//        public bool HasDirty { get { return _hasDirty; } }
//        #endregion
    }
}
