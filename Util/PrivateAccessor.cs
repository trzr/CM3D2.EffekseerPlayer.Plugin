using System;
using System.Reflection;

namespace EffekseerPlayerPlugin.Util {
    /// <summary>
    /// privateフィールド用のアクセサ
    /// とりあえず使う処理だけ実装版
    /// </summary>
    public sealed class PrivateAccessor {
        private static readonly PrivateAccessor INSTANCE = new PrivateAccessor();
        
        public static PrivateAccessor Instance {
            get { return INSTANCE; }
        }

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
