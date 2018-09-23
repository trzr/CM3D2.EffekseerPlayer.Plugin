
namespace EffekseerPlayer.Unity.Data {
    /// <summary>
    /// 値範囲を扱うクラス.
    ///
    /// Max, Min, SoftMax, SoftMinにより範囲を制御する.
    /// SoftMax, SoftMinはスライダーなどの最大値、最小値とし
    /// Max, Minは本来の範囲として使用される想定とする.
    /// </summary>
    public class EditRange {
        public EditRange(int _decimal, float min, float max, bool cyclic=false) {
            Decimal = _decimal;
            SoftMin = Min = min;
            SoftMax = Max = max;
            _cyclic = cyclic;
        }

        public EditRange(int _decimal, float min, float max, float softMin, float softMax, bool cyclic=false) {
            Decimal = _decimal;
            Min = min;
            Max = max;
            SoftMin = softMin;
            SoftMax = softMax;
            _cyclic = cyclic;
        }

        private int _decimal;
        /// <summary>桁数</summary>
        public int Decimal {
            get { return _decimal; }
            set {
                _decimal = value;
                Format = "F" + _decimal;
            }
        }
        public float SoftMin { get; set;}
        public float SoftMax { get; set;}

        public float Clamp(float val) {
            if (val < Min) return Min;
            return Max < val ? Max : val;
        }

        private float min;
        public float Min {
            get { return min;}
            private set {
                min = value;
                Delta = Max - min;
            }
        }
        private float max;
        public float Max {
            get { return max;}
            private set {
                max = value;
                Delta = max - min;
            }
        }
        public float Delta   { get; private set;}
        public string Format { get; private set;}
        private readonly bool _cyclic;

        /// <summary>
        /// 値範囲と比較して、最小値・最大値を逸脱したか判定する.
        /// 逸脱した場合にその範囲内に収まる値に修正する.
        /// 逸脱していない場合は、入力値valに設定される.
        /// </summary>
        /// <param name="val">比較する値</param>
        /// <param name="outVal">補正後の値</param>
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
        //  ただし、最小値より小さい値が入力される前提とする(このため、privateに制限)
        private float ToCorrectMin(float val) {
            do {
                val += Delta;
            } while (val < Min);
            return val;
        }

        // 最大値以上の値を補正する
        //  ただし、最大値より大きい値が入力される前提とする(このため、privateに制限)
        private float ToCorrectMax(float val) {
            do {
                val -= Delta;
            } while (val > Max);
            return val;
        }
    }
}
