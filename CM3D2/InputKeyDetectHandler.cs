﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using EffekseerPlayer.CM3D2.Data;
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

        public InputKeyDetectHandler() {
            Init();
            if (GetDown == null) {
                Log.Debug("GetDown method not created");
                GetDown = button => OVRInput.GetDown(button);
            }
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
            public bool IsEmpty() {
                return (codes == null || codes.Length == 0) &&
                       modifierKeys == EventModifiers.None &&
                       ovrButton == OVRInput.RawButton.None ;
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
                        if (trimed.StartsWith(PREFIX_BUTTON)) {
                            var ovrKey = trimed.Substring(PREFIX_BUTTON.Length);
                            OVRInput.RawButton rawButton;
                            if (EnumUtil.TryParse(ovrKey, true, out rawButton)) {
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
            // 条件に応じて、細かく判定関数を作成する

            if (keyHolder.codes.Length > 0) {
                if (keyHolder.modifierKeys == EventModifiers.None) {
                    if (keyHolder.ovrButton == OVRInput.RawButton.None) {
                        // EventModifiers is None && keyCodes
                        return () => {
                            if (Event.current.modifiers != EventModifiers.None) return false;
                            return keyHolder.codes.All(Input.GetKeyDown);
                        };
                    }
                    // EventModifiers is None && keyCodes && ovrButton
                    return () => {
                        if (Event.current.modifiers != EventModifiers.None) return false;
                        if (!GetDown(keyHolder.ovrButton)) return false;
                        return keyHolder.codes.All(Input.GetKeyDown);
                    };

                }

                if (keyHolder.ovrButton == OVRInput.RawButton.None) {
                    // EventModifiers && keyCodes
                    return () => {
                        if ((Event.current.modifiers & keyHolder.modifierKeys) != keyHolder.modifierKeys) return false;
                        return keyHolder.codes.All(Input.GetKeyDown);
                    };
                }

                // EventModifiers && keyCodes && ovrButton
                return () => {
                    if ((Event.current.modifiers & keyHolder.modifierKeys) != keyHolder.modifierKeys) return false;
                    if (!GetDown(keyHolder.ovrButton)) return false;
                    return keyHolder.codes.All(Input.GetKeyDown);
                };
            }

            if (keyHolder.modifierKeys == EventModifiers.None) {
                if (keyHolder.ovrButton == OVRInput.RawButton.None) return null;
                // ovrButton
                return () => GetDown(keyHolder.ovrButton);
            }

            if (keyHolder.ovrButton == OVRInput.RawButton.None) {
                // EventModifiers
                return () => (Event.current.modifiers & keyHolder.modifierKeys) == keyHolder.modifierKeys;
            }
            // EventModifiers && ovrButton
            return () => (Event.current.modifiers & keyHolder.modifierKeys) == keyHolder.modifierKeys && GetDown(keyHolder.ovrButton);
        }

        private void Init() {
            // rawButtonのフラグを入力とし、指定されたキーすべてが押されたか判断するクロージャを生成する
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
                    if (ResolverController((OVRInput.Controller) ctrlTypeField.GetValue(controller), mask)) {
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

        private static bool ResolverController(OVRInput.Controller ctrlType, OVRInput.Controller ctrlMask) {
            var result = (ctrlType & ctrlMask) == ctrlType;
            if ((ctrlMask & OVRInput.Controller.Touch) == OVRInput.Controller.Touch &&
                (ctrlType & OVRInput.Controller.Touch) != OVRInput.Controller.None &&
                (ctrlType & OVRInput.Controller.Touch) != OVRInput.Controller.Touch) {
                result = false;
            }

            return result;
        }

        public PlayManager playManager;
        private Func<OVRInput.RawButton, bool> GetDown;

        public readonly IList<KeyHandler> handlers = new List<KeyHandler>();
        private const string PREFIX_BUTTON = "ovr_";
        private const string PREFIX_TOUCH  = "ovrt_";
    }
}