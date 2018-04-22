using System.Collections.Generic;
using EffekseerPlayer.Unity.UI;
using UnityEngine;

namespace EffekseerPlayer.Unity.Util {
    ///
    /// カーソル・ドラッグ操作などを対象としたユーティリティクラス.
    ///
    internal class UIHelper {

        internal bool cmrCtrlChanged;

        internal readonly List<BaseWindow> targets = new List<BaseWindow>();

        internal bool IsEnabledUICamera() {
            return UICamera.currentCamera != null && UICamera.currentCamera.enabled;
        }

        internal void SetCameraControl(bool enable) {
            if (cmrCtrlChanged != enable) return;

            GameMain.Instance.MainCamera.SetControl(enable);
            UICamera.InputEnable = enable;
            cmrCtrlChanged = !enable;
        }

        /// <summary> カーソル位置に応じて、カメラコントロールの有効化/無効化を行う </summary>
        internal void UpdateCameraControl() {
            // カメラコントロールの有効化/無効化 (Windowの範囲外では、自身がコントロールを変更したケース以外は更新しない)
            var contains = false;
            foreach (var target in targets) {
                contains |= target.CursorContains;
                if (contains) break;
            }

            if (contains) {
                if (GameMain.Instance.MainCamera.GetControl()) {
                    SetCameraControl(false);
                }
            } else {
                SetCameraControl(true);
            }
        }

        internal void InitStatus() {
            foreach (var target in targets) {
                target.InitStatus();
            }
        }

        internal void UpdateCursor() {
            var cursor = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            foreach (var target in targets) {
                target.UpdateCursor(ref cursor);
            }
        }

        /// ドラッグ操作を更新する
        internal void UpdateDrag() {
            foreach (var target in targets) {
                target.UpdateDrag();
            }
        }
    }
}
