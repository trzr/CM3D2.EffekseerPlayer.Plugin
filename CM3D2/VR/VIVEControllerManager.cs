
namespace EffekseerPlayer.CM3D2.VR {
    public class VIVEControllerManager {
        VIVEControllerHandler leftHandler;
        VIVEControllerHandler rightHandler;
        public void Setup() {
            if (!GameMain.Instance.VRMode) return;

            var ovrObj = GameMain.Instance.OvrMgr.ovr_obj;
            if (ovrObj != null) {
                if (leftHandler == null && ovrObj.left_controller.hand_trans != null) {
                    var left = ovrObj.left_controller.hand_trans.gameObject;
                    leftHandler = left.GetComponent<VIVEControllerHandler>();
                    if (leftHandler == null) {
                        leftHandler = left.AddComponent<VIVEControllerHandler>();
                        leftHandler.name = "VIVEControllerHandler(L)";
                    }
                }

                if (rightHandler == null && ovrObj.right_controller.hand_trans != null) {
                    var right = ovrObj.right_controller.hand_trans.gameObject;
                    rightHandler = right.GetComponent<VIVEControllerHandler>();
                    if (rightHandler == null) {
                        rightHandler = right.AddComponent<VIVEControllerHandler>();
                        rightHandler.name = "VIVEControllerHandler(R)";
                    }
                }
            }
        }

        /// <summary>
        /// 左右のコントローラに対して、指定したキーがすべて押下されているかを判定する
        /// </summary>
        /// <param name="left">左コントローラのキーマスク</param>
        /// <param name="right">右コントローラのキーマスク</param>
        /// <returns>指定したキーがすべて押下されていた場合(ただし、前回のフレームで既に押下されていた場合を除く)</returns>
        public bool GetKeyPressed(ulong left, ulong right) {
            if (left != 0) {
                if (leftHandler == null) return false;
                if (right == 0) {
                    return leftHandler.GetKeyDowned(left);
                }
                if (rightHandler == null) return false;

                // left && rightの場合は、両方のstateからの判断が必要
                // (一方はprevとnowで変化しないケースもある)
                var lDev = leftHandler.Device;
                var rDev = rightHandler.Device;
                return ((lDev.GetPrevState().ulButtonPressed & left) != left ||
                        (rDev.GetPrevState().ulButtonPressed & right) != right) &&
                       ((lDev.GetState().ulButtonPressed & left) == left &&
                        (rDev.GetState().ulButtonPressed & right) == right);
            }
            return rightHandler != null && rightHandler.GetKeyDowned(right);
        }
    }

    public enum VIVEButton {
        LTrigger, // 人指し指トリガー Press, TouchDown
        LTouchpad, // Press, Touch
        LMenu,   // メニュー Press, TouchDown
        LGrip,   // 中指トリガー Press

        RTrigger, // 人指し指トリガー Press, TouchDown
        RTouchpad, // Press, Touch
        RMenu,   // メニュー Press, TouchDown
        RGrip,   // 中指トリガー Press
    }
}