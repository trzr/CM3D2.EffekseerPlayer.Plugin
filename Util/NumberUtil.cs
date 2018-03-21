using System;

namespace EffekseerPlayerPlugin.Util {
    /// <summary>
    /// 数値関連のユーティリティクラス.
    /// 現在はフロート値の比較用のみ.
    /// </summary>
    public sealed class NumberUtil {
        private static readonly NumberUtil INSTANCE = new NumberUtil();
        
        public static NumberUtil Instance {
            get { return INSTANCE; }
        }
        
        private NumberUtil() { }

        /// <summary>
        /// 2つのフロート値の比較を行う.
        /// 2値の絶対値差分がある値より小さい場合に等しいとみなす
        /// </summary>
        /// <param name="f1">フロート値1</param>
        /// <param name="f2">フロート値2</param>
        /// <returns>等しい場合にtrueを返す</returns>
        public static bool Equals(float f1, float f2) {
            return Math.Abs(f1-f2) < ConstantValues.EPSILON;
        }

        /// <summary>
        /// 2つのフロート値の比較を行う.
        /// 2値の絶対値差分が、epsilonより小さい場合に等しいとみなす
        /// </summary>
        /// <param name="f1">フロート値1</param>
        /// <param name="f2">フロート値2</param>
        /// <param name="epsilon">基準値</param>
        /// <returns>等しい場合にtrueを返す</returns>
        public static bool Equals(float f1, float f2, float epsilon) {
            return Math.Abs(f1-f2) < epsilon;
        }
    }
}
