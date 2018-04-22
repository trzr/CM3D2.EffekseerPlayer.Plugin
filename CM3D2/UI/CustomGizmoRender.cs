using System;
using System.Reflection;
using UnityEngine;

namespace EffekseerPlayer.CM3D2.UI {
    public class CustomGizmoRender : GizmoRender {
        // ドラッグ前後の情報を保持するフィールド
        private Vector3? prevAxis;
        private Quaternion? prevRotate;

        /// <summary>ドラッグ中か判定する関数</summary>
        private readonly Func<bool> IsDrag;
        /// <summary>Gizmo情報から位置や回転の基準値を更新するアクション</summary>
        private Action UpdateGizmo = () => { };

        public CustomGizmoRender() {
            var type = typeof(GizmoRender);
            var field =  type.GetField("is_drag_", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (field != null) {
                IsDrag = () => (bool) field.GetValue(type);
            } else {
                IsDrag = () => false;
            }
        }

        /// <summary>
        /// 有効化された軸設定に応じて、Updateで呼び出すクロージャを初期化する.
        /// </summary>
        public void InitStatus() {
            if (eAxis && eRotate) {
                UpdateGizmo = () => {
                    if (prevAxis.HasValue || !IsDrag()) return;
                    prevAxis   = transform.localPosition;
                    prevRotate = transform.localRotation;
                };
            } else if (eAxis) {
                UpdateGizmo = () => {
                    if (!prevAxis.HasValue && IsDrag()) {
                        prevAxis = transform.localPosition;
                    }
                };
            } else if (eRotate) {
                UpdateGizmo = () => {
                    if (!prevRotate.HasValue && IsDrag()) {
                        prevRotate = transform.localRotation;
                    }
                };
            }
        }

        public override void Update() {
            base.Update();

            UpdateGizmo();
        }

        /// <summary>
        /// ドラッグが終わった時点で位置や回転に対して、変更の有無を検出し、変更があったら通知する
        /// </summary>
        public override void OnDragEnd() {
            if (prevAxis.HasValue) {
                if (prevAxis.Value != transform.localPosition) {
                    PosChanged(transform, EventArgs.Empty);
                }
                prevAxis = null;
            }

            if (prevRotate.HasValue) {
                if (prevRotate.Value != transform.localRotation) {
                    RotChanged(transform, EventArgs.Empty);
                }

                prevRotate = null;
            }
        }
        public EventHandler PosChanged = delegate {  };
        public EventHandler RotChanged = delegate {  };
    }
}