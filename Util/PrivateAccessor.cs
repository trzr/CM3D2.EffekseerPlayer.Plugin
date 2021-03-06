﻿using System;
using System.Reflection;

namespace EffekseerPlayer.Util {
    /// <summary>
    /// privateフィールド用のアクセサ
    /// とりあえず使う処理だけ実装版
    /// </summary>
    public sealed class PrivateAccessor {
        public static readonly PrivateAccessor Instance = new PrivateAccessor();

        private PrivateAccessor() { }
        public static T Get<T>(object instance, string fieldName) {
            try {
                var field =  instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);//  | BindingFlags.GetField | BindingFlags.SetField 
                if (field != null) return (T) field.GetValue(instance);
            } catch(Exception e) {
                Log.Debug(e);
            }
            return default (T);
        }

        public static T Get<T>(Type type, string fieldName) {
            try {
                var field =  type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);//  | BindingFlags.GetField | BindingFlags.SetField 
                if (field != null) return (T) field.GetValue(null);
            } catch(Exception e) {
                Log.Debug(e);
            }
            return default (T);
        }
    }
}
