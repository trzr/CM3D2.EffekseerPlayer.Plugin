﻿using System;

namespace EffekseerPlayer.Util {
    public static partial class Enum {
        /// <summary>
        /// Enum型をパースする.
        /// </summary>
        /// <param name="src">パース対象文字列</param>
        /// <param name="ignoreCase">大文字小文字の区別を指定するフラグ</param>
        /// <param name="result">パース結果</param>
        /// <typeparam name="T">enum 型</typeparam>
        /// <returns>パースに成功した場合にtrueを返す</returns>
        public static bool TryParse<T>(string src, bool ignoreCase, out T result) {
            var names = System.Enum.GetNames(typeof(T));
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            foreach (var name in names) {
                if (!string.Equals(name, src, comparison)) continue;
                result = (T)System.Enum.Parse(typeof(T), name);
                return true;
            }
            result = default(T);
            return false;
        }
    }
}