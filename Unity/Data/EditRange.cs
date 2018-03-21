
using System;

namespace EffekseerPlayerPlugin.Unity.Data
{
    /// <summary>
    /// 値範囲を扱うクラス.
    /// </summary>
    public class EditRange {
        internal EditRange(int _decimal, float min, float max, bool cyclic=false) {
            Decimal = _decimal;
            Min = min;
            Max = max;
            _cyclic = cyclic;
        }

        /// <summary>桁数</summary>
        public int Decimal {
            get { return _decimal; }
            set { 
                _decimal = value;
                Format = "F" + _decimal;
            }
        }
        public float Min     { get; private set;}
        public float Max     { get; private set;}
        public string Format { get; private set;}
        private int _decimal;
        private readonly bool _cyclic;

        /// <summary>
        /// 値範囲と比較して、最小値・最大値を逸脱したか判定する.
        /// 逸脱した場合にその範囲内に収まる値に修正する.
        /// 逸脱した場合は、vは入力値valに設定される.
        /// </summary>
        /// <param name="val">比較する値</param>
        /// <param name="outVal">範囲内の値</param>
        /// <returns>値が範囲を逸脱した場合にfalseを返す</returns>
        public bool TryEval(float val, out float outVal) {
            if (val < Min) {
                outVal = _cyclic ? ToCorrectMin(val) : Min;
            } else if (Max < val) {
                outVal = _cyclic ? ToCorrectMax(val) : Max;
            } else {
                outVal = val;
                return true;
            }
            return false;
        }

        // 最小値以下の値を補正する
        private float ToCorrectMin(float val) {
            var cycle = Max - Min;
            do {
                val += cycle;
            } while (val < Min);
            return val;
        }

        // 最大値以上の値を補正する
        private float ToCorrectMax(float val) {
            var cycle = Max - Min;
            do {
                val -= cycle;
            } while (val > Max);
            return val;
        }
//        
//        public static readonly EditRange MORPH;
//        public static readonly EditRange RQ;
//        public static readonly EditRange SHINE;
//        public static readonly EditRange OUTLINE;
//        public static readonly EditRange RIM_POW;
//        public static readonly EditRange RIM_SHIFT;
//        public static readonly EditRange HI_RATE;
//        public static readonly EditRange HI_POW;
//        public static readonly EditRange FVAL1;
//        public static readonly EditRange FVAL2;
//        public static readonly EditRange FVAL3;
//
//        static EditRange() {
//            var settings = Settings.Instance;
//            MORPH     = new EditRange(4, 0, 1f);
//            RQ        = new EditRange(0, 0, 5000f);
//            //SHINE     = new EditRange(2, settings.shininessEditMin, settings.shininessEditMax);
//            //OUTLINE   = new EditRange(5, settings.outlineWidthEditMin, settings.outlineWidthEditMax);
//            //RIM_POW   = new EditRange(3, settings.rimPowerEditMin,  settings.rimPowerEditMax);
//            //RIM_SHIFT = new EditRange(3, settings.rimShiftEditMin,  settings.rimShiftEditMax);
//            //HI_RATE   = new EditRange(2, settings.hiRateEditMin,    settings.hiRateEditMax);
//            //HI_POW    = new EditRange(4, settings.hiPowEditMin,     settings.hiPowEditMax);
//            //FVAL1     = new EditRange(2, settings.floatVal1EditMin, settings.floatVal1EditMax);
//            //FVAL2     = new EditRange(3, settings.floatVal2EditMin, settings.floatVal2EditMax);
//            //FVAL3     = new EditRange(3, settings.floatVal3EditMin, settings.floatVal3EditMax);
//    
//            //settings.SettingUpdated += Update;            
//        }
//
//        private static void Update(object obj, EventArgs args) {
//            var setting = (Settings)obj;
//            //SHINE.Min   = setting.shininessEditMin;
//            //SHINE.Max   = setting.shininessEditMax;
//            //OUTLINE.Min = setting.outlineWidthEditMin;
//            //OUTLINE.Max = setting.outlineWidthEditMax;
//            //RIM_POW.Min = setting.rimPowerEditMin;
//            //RIM_POW.Max = setting.rimPowerEditMax;
//            //RIM_SHIFT.Min = setting.rimShiftEditMin;
//            //RIM_SHIFT.Max = setting.rimShiftEditMax;
//            //HI_RATE.Min   = setting.hiRateEditMin;
//            //HI_RATE.Max   = setting.hiRateEditMax;
//            //HI_POW.Min    = setting.hiPowEditMin;
//            //HI_POW.Max    = setting.hiPowEditMax;
//            //FVAL1.Min     = setting.floatVal1EditMin;
//            //FVAL1.Max     = setting.floatVal1EditMax;
//            //FVAL2.Min     = setting.floatVal2EditMin;
//            //FVAL2.Max     = setting.floatVal2EditMax;
//            //FVAL3.Min     = setting.floatVal3EditMin;
//            //FVAL3.Max     = setting.floatVal3EditMax;
//        }
    }
}
