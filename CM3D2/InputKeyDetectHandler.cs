using System;
using System.Collections.Generic;
using System.Linq;
using EffekseerPlayer.CM3D2.Data;
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
            foreach (var handler in handlers) {
                if (handler.detector()) handler.handle();
            }
        }

        public struct KeyHolder {
            public KeyCode[] codes;
            public EventModifiers modifierKeys;
            public OVRInput.RawButton ovrButton;
            // public OVRInput.RawTouch ovrTouch;
            // public OVRInput.RawNearTouch ovrNearTouch;
            // public OVRInput.RawAxis1D ovrAxis1D;
            // public OVRInput.RawAxis2D ovrAxis2D;
            // public OVRInput.Controller ovrController;
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
                if (EffekseerPlayer.Util.Enum.TryParse(trimed, true, out parsedCode)) {
                    keyCodes.Add(parsedCode);
                } else {
                    EventModifiers parsedModifiers;
                    if (EffekseerPlayer.Util.Enum.TryParse(trimed, true, out parsedModifiers)) {
                        keyHolder.modifierKeys |= parsedModifiers;
                    } else {
                        if (trimed.StartsWith(PREFIX_BUTTON)) {
                            var ovrKey = trimed.Substring(PREFIX_BUTTON.Length);
                            OVRInput.RawButton rawButton;
                            if (EffekseerPlayer.Util.Enum.TryParse(ovrKey, true, out rawButton)) {
                                keyHolder.ovrButton |= rawButton;
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
            if (keyHolder.codes.Length > 0) {
                if (keyHolder.modifierKeys == EventModifiers.None) {
                    return () => {
                        if (Event.current.modifiers != EventModifiers.None) return false;
                        if (keyHolder.ovrButton != OVRInput.RawButton.None) {
                            if (!OVRInput.GetDown(keyHolder.ovrButton)) return false;
                        }
                        return keyHolder.codes.All(Input.GetKeyDown);
                    };
                    
                }
                return () => {
                    if ((Event.current.modifiers & keyHolder.modifierKeys) != keyHolder.modifierKeys) return false;
                    if (keyHolder.ovrButton != OVRInput.RawButton.None) {
                        if (!OVRInput.GetDown(keyHolder.ovrButton)) return false;
                    }
                    return keyHolder.codes.All(Input.GetKeyDown);
                };
            }

            if (keyHolder.modifierKeys == EventModifiers.None) {
                if (keyHolder.ovrButton == OVRInput.RawButton.None) return null;
                return () => OVRInput.GetDown(keyHolder.ovrButton);
            }

            return () => {
                if ((Event.current.modifiers & keyHolder.modifierKeys) != keyHolder.modifierKeys) return false;
                if (keyHolder.ovrButton == OVRInput.RawButton.None) return true;
                return OVRInput.GetDown(keyHolder.ovrButton);
            };
        }

        public PlayManager playManager;

        public readonly IList<KeyHandler> handlers = new List<KeyHandler>();
        private const string PREFIX_BUTTON = "ovr_";
        private const string PREFIX_TOUCH  = "ovrt_";
    }
}