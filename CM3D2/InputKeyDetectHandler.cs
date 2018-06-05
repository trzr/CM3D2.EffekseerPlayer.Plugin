using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using EffekseerPlayer.CM3D2.Data;
using EffekseerPlayer.CM3D2.VR;
using EffekseerPlayer.Util;
using UnityEngine;

namespace EffekseerPlayer.CM3D2 {
    /// <summary>
    /// 指定したキーを検知し、アクションを実行するための制御ハンドラクラス
    /// </summary>
    public class InputKeyDetectHandler<T> {
        private static readonly InputKeyDetectHandler<T> INSTANCE = new InputKeyDetectHandler<T>();
        public static InputKeyDetectHandler<T> Get() {
            return INSTANCE;
        }

        public void Detect() {
            if (!initialized) return;

            foreach (var handler in keyHandlers) {
                if (handler.detector()) handler.handle();
            }
        }

        public struct KeyHolder {
            public KeyCode[] codes;
            public EventModifiers modifierKeys;
            public OVRInput.RawButton ovrButton;
            public ulong lMask;
            public ulong rMask;
            // public OVRInput.RawTouch ovrTouch;
            // public OVRInput.RawNearTouch ovrNearTouch;
            // public OVRInput.RawAxis1D ovrAxis1D;
            // public OVRInput.RawAxis2D ovrAxis2D;
            // public OVRInput.Controller ovrController;
            public bool IsEmpty() {
                return (codes == null || codes.Length == 0) &&
                       modifierKeys == EventModifiers.None &&
                       ovrButton == OVRInput.RawButton.None &&
                       lMask == 0 && rMask == 0;
            }

            public override string ToString() {
                var sb = new StringBuilder("KeyHolder[");
                if (codes != null && codes.Length > 0) {
                    sb.Append("codes=");
                    foreach (var code in codes) sb.Append(code).Append(',');
                }
                if (modifierKeys != EventModifiers.None) {
                    sb.Append("modifiers=").Append(modifierKeys).Append(',');
                }
                if (ovrButton != OVRInput.RawButton.None) {
                    sb.Append("ovrButton=").Append(ovrButton).Append(',');;
                }
                if (lMask != 0) {
                    sb.Append("lMask=").Append(lMask).Append(',');;
                }
                if (rMask != 0) {
                    sb.Append("rMask=").Append(rMask).Append(',');;
                }
                sb.Append(']');
                return sb.ToString();
            }
        }

        public delegate void CallbackHandler();
        public class KeyHandler {
            public KeyHolder keyHolder;
            public Func<bool> detector;
            public IList<T> dataList;
            public CallbackHandler handle = delegate { };
        }

        public KeyHolder Parse(string keys) {
            var keysArray = keys.Split(',');
            var keyHolder = new KeyHolder();
            var keyCodes = new List<KeyCode>();
            foreach (var key in keysArray) {
                var trimed = key.Trim();
                if (trimed.Length == 0) continue;

                KeyCode parsedCode;
                if (EnumUtil.TryParse(trimed, true, out parsedCode)) {
                    keyCodes.Add(parsedCode);
                } else {
                    EventModifiers parsedModifiers;
                    if (EnumUtil.TryParse(trimed, true, out parsedModifiers)) {
                        keyHolder.modifierKeys |= parsedModifiers;
                    } else {
                        if (trimed.StartsWith(PREFIX_OVR_BUTTON)) {
                            var ovrKey = trimed.Substring(PREFIX_OVR_BUTTON.Length);
                            OVRInput.RawButton rawButton;
                            if (EnumUtil.TryParse(ovrKey, true, out rawButton)) {
                                keyHolder.ovrButton |= rawButton;
                            }
                        } else if (trimed.StartsWith(PREFIX_VIVE_BUTTON)) {
                            var viveKey = trimed.Substring(PREFIX_VIVE_BUTTON.Length);
                            VIVEButton viveButton;
                            if (EnumUtil.TryParse(viveKey, true, out viveButton)) {
                                switch (viveButton) {
                                case VIVEButton.LTrigger:
                                    keyHolder.lMask |= SteamVR_Controller.ButtonMask.Trigger;
                                    break;
                                case VIVEButton.LTouchpad:
                                    keyHolder.lMask |= SteamVR_Controller.ButtonMask.Touchpad;
                                    break;
                                case VIVEButton.LMenu:
                                    keyHolder.lMask |= SteamVR_Controller.ButtonMask.ApplicationMenu;
                                    break;
                                case VIVEButton.LGrip:
                                    keyHolder.lMask |= SteamVR_Controller.ButtonMask.Grip;
                                    break;
                                case VIVEButton.RTrigger:
                                    keyHolder.rMask |= SteamVR_Controller.ButtonMask.Trigger;
                                    break;
                                case VIVEButton.RTouchpad:
                                    keyHolder.rMask |= SteamVR_Controller.ButtonMask.Touchpad;
                                    break;
                                case VIVEButton.RMenu:
                                    keyHolder.rMask |= SteamVR_Controller.ButtonMask.ApplicationMenu;
                                    break;
                                case VIVEButton.RGrip:
                                    keyHolder.rMask |= SteamVR_Controller.ButtonMask.Grip;
                                    break;
                                }
                                //keyHolder.ovrButton |= viveButton;
                            }
//                        } else if (trimed.StartsWith(PREFIX_TOUCH)) {
//                            var ovrKey = trimed.Substring(PREFIX_TOUCH.Length);
//                            OVRInput.RawTouch rawTouch;
//                            if (Enum.TryParse(ovrKey, true, out rawTouch)) {
//                                setKey.ovrTouch |= rawTouch;
//                            }
                        } else {
                            Log.Info("parse failed:", trimed);
                        }
                    }
                }
            }

            keyHolder.codes = keyCodes.ToArray();
            return keyHolder;
        }

        public Func<bool> CreateKeyDetector(KeyHolder keyHolder) {
            // 条件に応じて、判定関数を作成
            if (keyHolder.codes.Length > 0) {
                if (keyHolder.modifierKeys == EventModifiers.None) {
                    // EventModifiers is None && keyCodes
                    return () => {
                        if (Event.current.modifiers != EventModifiers.None) return false;
                        foreach (var code in keyHolder.codes) {
                            if (!Input.GetKeyDown(code)) return false;
                        }
                        return true;
                    };
                }
                // EventModifiers && keyCodes
                return () => {
                    if ((Event.current.modifiers & keyHolder.modifierKeys) != keyHolder.modifierKeys) return false;
                    foreach (var code in keyHolder.codes) {
                        if (!Input.GetKeyDown(code)) return false;
                    }
                    return true;
                };
            }

            var game = GameMain.Instance;
            if (!game.VRMode) return null;

            if (game.VRFamily == GameMain.VRFamilyType.HTC) {
                // VIVE Button
                if (viveCtrlMgr != null && (keyHolder.lMask != 0 || keyHolder.rMask != 0)) {
                    return () => viveCtrlMgr.GetKeyPressed(keyHolder.lMask, keyHolder.rMask);
                }
            } else {
                // ovrButton
                if (keyHolder.ovrButton != OVRInput.RawButton.None) {
                    return () => GetDown(keyHolder.ovrButton);
                }
            }
            return null;
        }

        public void Init() {
            initialized = true;
            var game = GameMain.Instance;
            if (game.VRFamily == GameMain.VRFamilyType.HTC) {
                viveCtrlMgr = new VIVEControllerManager();
                viveCtrlMgr.Setup();

            } else if (game.VRFamily == GameMain.VRFamilyType.Oculus) {
                // rawButtonのフラグを入力とし、指定されたキーすべてが押されたか判断するクロージャを生成する
                // OVRInput.GetDownは単一キーの判断しかできないため、複数キー同時押下を判定するためのクロージャを用意する必要がある.
                // ただし、non-publicのフィールドやクラスがあるため、reflectionを駆使して無理やり呼び出しを実現している. (可視性を変えてしまった方が早い)
                var type = typeof(OVRInput);
                var ctrollerField =  type.GetField("controllers", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                if (ctrollerField == null) return;

                // 参考 https://stackoverflow.com/questions/17114110/cast-listobject-to-unknown-t-using-reflection
                var list = ctrollerField.GetValue(type);
                var castElemType = typeof(object[]).GetElementType();
                // ReSharper disable once PossibleNullReferenceException
                var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(castElemType);
                // ReSharper disable once PossibleNullReferenceException
                var toArray    = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(castElemType);
                var castedObjectEnum = castMethod.Invoke(null, new[] { list });
                var array = (object[])toArray.Invoke(null, new[] {castedObjectEnum});
                if (array.Length == 0) return;
                var ctrl = array[0];
                var ctrlType = ctrl.GetType().BaseType;
                if (ctrlType == null) return;
                var prevStateField = ctrlType.GetField("previousState", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                var curStateField = ctrlType.GetField("currentState",  BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (curStateField == null || prevStateField == null) return;
                var state = curStateField.GetValue(ctrl);
                var buttonsField = state.GetType().GetField("Buttons",  BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (buttonsField == null) return;
                var ctrlTypeField = ctrlType.GetField("controllerType",  BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (ctrlTypeField == null) return;

                GetDown = rawButton => {
                    var mask = OVRInput.Controller.Active | OVRInput.GetActiveController();
                    foreach (var controller in array) {
                        if (ResolveController((OVRInput.Controller) ctrlTypeField.GetValue(controller), mask)) {
                            var prevButtons = (uint) buttonsField.GetValue(prevStateField.GetValue(controller));
                            if (((OVRInput.RawButton) prevButtons & rawButton) == rawButton) {
                                return false;
                            }

                            var curButtons = (uint) buttonsField.GetValue(curStateField.GetValue(controller));
                            return ((OVRInput.RawButton) curButtons & rawButton) == rawButton;
                        }
                    }
                    return false;
                };
            }
        }

        private static bool ResolveController(OVRInput.Controller ctrlType, OVRInput.Controller ctrlMask) {
            var result = (ctrlType & ctrlMask) == ctrlType;
            if ((ctrlMask & OVRInput.Controller.Touch) == OVRInput.Controller.Touch &&
                (ctrlType & OVRInput.Controller.Touch) != OVRInput.Controller.None &&
                (ctrlType & OVRInput.Controller.Touch) != OVRInput.Controller.Touch) {
                result = false;
            }

            return result;
        }

        public PlayManager playManager;
        private Func<OVRInput.RawButton, bool> GetDown = button => OVRInput.GetDown(button);
        private VIVEControllerManager viveCtrlMgr;
        private bool initialized;

        public readonly IList<KeyHandler> keyHandlers = new List<KeyHandler>();
        private const string PREFIX_OVR_BUTTON = "ovr_";
        private const string PREFIX_TOUCH  = "ovrt_";
        private const string PREFIX_VIVE_BUTTON = "vive_";
    }
}