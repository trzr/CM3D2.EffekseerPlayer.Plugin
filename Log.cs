
using System;
using System.Collections.Generic;
using System.Text;

namespace EffekseerPlayer {
    /// <summary>
    /// ログ出力ユーティリティ
    /// </summary>
    public static class Log {

        public static void DebugF(string format, params object[] message) {
#if DEBUG
            var sb = string.Format(format, message);
            Debug(sb);
#endif
        }

        public static void Debug(params object[] message) {
#if DEBUG
            var sb = CreateMessage(message, "[DEBUG]");
            UnityEngine.Debug.Log(sb);
#endif
        }

        public static StringBuilder InfoF(string format, params object[] message) {
            var sb = CreateMessage(format, message, "[INFO ]");
//            var sb = string.Format(format, message);
            UnityEngine.Debug.Log(sb);
            return sb;
        }

        public static StringBuilder Info(params object[] message) {
            var sb = CreateMessage(message, "[INFO ]");
            UnityEngine.Debug.Log(sb);
            return sb;
        }

        public static StringBuilder ErrorF(string format, params object[] message) {
            var sb = CreateMessage(format, message, "[ERROR]");
//            var sb = string.Format(format, message);
            UnityEngine.Debug.LogError(sb);
            return sb;
        }

        public static StringBuilder Error(params object[] message) {
            var sb = CreateMessage(message, "[ERROR]");
            UnityEngine.Debug.LogError(sb);
            return sb;
        }

        private static StringBuilder CreateMessage(string format, IEnumerable<object> message, string prefix = null) {
            var sb = new StringBuilder();
            if (prefix != null) sb.Append(prefix);
            sb.Append(EffekseerPlayerPlugin.PluginName).Append(':');
            sb.AppendFormat(format, message);
            return sb;
        }

        private static StringBuilder CreateMessage(IEnumerable<object> message, string prefix = null) {
            var sb = new StringBuilder();
            if (prefix != null) sb.Append(prefix);
            sb.Append(EffekseerPlayerPlugin.PluginName).Append(':');
            foreach (var t in message) {
                //if (i > 0) sb.Append(',');
                if (t is Exception) sb.Append(' ');
                sb.Append(t);
            }
            return sb;
        }
    }
}
