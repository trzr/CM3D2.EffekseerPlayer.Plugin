using System;
using System.Text;
using UnityEngine;
// ReSharper disable PossibleInvalidOperationException

namespace EffekseerPlayerPlugin.Effekseer {
    /// <summary xml:lang="en">
    /// A emitter of the Effekseer effect
    /// </summary>
    /// <summary xml:lang="ja">
    /// エフェクトの発生源
    /// </summary>
    public class EffekseerEmitter : MonoBehaviour {
        [Flags]
        public enum EmitterStatus {
            Empty = 0,
            Playing = 1,
            Paused = 2,
            Stopping = 4,
            Stopped = 8,
        }
        /// <summary xml:lang="en">
        /// Effect name
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクト名
        /// </summary>
        public string EffectName {
            get { return _effectName;}
            set {
                // Effect名が異なる場合に停止
                if (_effectName != value) Stop();

                _effectName = value;
            }
        }

        private string _effectName;

        /// <summary xml:lang="en">
        /// Whether it does play the effect on Start()
        /// </summary>
        /// <summary xml:lang="ja">
        /// Start()時に再生開始するかどうか
        /// </summary>
        public bool playOnStart;

        /// <summary xml:lang="en">
        /// Whether it does loop playback.
        /// </summary>
        /// <summary xml:lang="ja">
        /// ループ再生するかどうか
        /// </summary>
        public bool loop;

        /// <summary xml:lang="en">
        /// end frame
        /// </summary>
        /// <summary xml:lang="ja">
        /// エンドフレーム
        /// </summary>
        public float endFrame;

        /// <summary xml:lang="en">
        /// delay frame. Number of frames before starting the effect.
        /// </summary>
        /// <summary xml:lang="ja">
        /// ディレイフレーム. エフェクトを開始するまでのフレーム数
        /// </summary>
        public float delayFrame;

        /// <summary xml:lang="en">
        /// post-delay frame for repeat.
        /// </summary>
        /// <summary xml:lang="ja">
        /// エンドフレーム以降リピートするまでのフレーム数.
        /// </summary>
        public float postDelayFrame;

        /// <summary>
        /// フレーム
        /// </summary>
        public float Frame { get; private set; }

        /// <summary xml:lang="en">
        /// The last played handle.
        /// </summary>
        /// <summary xml:lang="ja">
        /// 最後にPlayされたハンドル
        /// </summary>
        private EffekseerHandle? _handle;

        private EmitterStatus _status = EmitterStatus.Empty;
        /// <summary>再生状態を表すステータス.</summary>
        public EmitterStatus Status {
            get { return _status; }
            private set {
                if (_status != value) {
                    _prevStatus = _status;
                    Log.Debug("status:", value);
                }
                _status = value;
            }
        }
        /// <summary>一つ前の再生状態.ポーズ解除時などで状態を戻すための内部変数</summary>
        private EmitterStatus _prevStatus = EmitterStatus.Empty;

        /// <summary>
        /// 位置を固定するかどうか.
        /// falseにすると<c>Update()</c>で位置を更新する.
        /// 親にアタッチしない場合はtrue固定でよい
        /// </summary>
        public bool fixLocation = true;
        /// <summary>
        /// 回転を固定するかどうか.
        /// falseにするとUpdate()で回転を更新する.
        /// 親にアタッチしない場合はtrue固定でよい
        /// </summary>
        public bool fixRotation = true;
        /// <summary>
        /// 再生時の回転角に、localRotationを使用するか.
        /// falseの場合は<c>Transform.rotation</c>が使用される.
        /// </summary>
        public bool useLocalRotation;

        /// <summary>
        /// 回転角を取得する
        /// useLocalRotationに従って<c>Transform.localRotation</c>か<c>Transform.rotation</c>を返す
        /// </summary>
        public Quaternion Rotation {
            get { return useLocalRotation ? transform.localRotation : transform.rotation; }
        }

        /// <summary>
        /// 位置を取得する
        /// useLocalRotationに従って<c>Transform.localRotation</c>か<c>Transform.rotation</c>を返す
        /// </summary>
        private Vector3 Location {
            get { return _offsetLocation.HasValue ? transform.position + _offsetLocation.Value : transform.position; }
        }

        /// <summary>
        /// 回転を無視した相対位置を指定する.
        /// ただし、fixLocation=falseの場合のみ効果がある.
        /// </summary>
        private Vector3? _offsetLocation;
        public Vector3? OffsetLocation {
            get { return _offsetLocation; }
            set {
                if (value != null) {
                    transform.localPosition = Vector3.zero;
                    _offsetLocation = value;
                } else {
                    if (_offsetLocation.HasValue) {
                        transform.localPosition = _offsetLocation.Value;
                    }
                    _offsetLocation = null;
                }
            }
        }

        /// <summary xml:lang="en">
        /// Plays the effect.
        /// <param name="effectName">Effect name</param>
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクトを再生
        /// <param name="effectName">エフェクト名</param>
        /// </summary>
        public void Play(string effectName) {
            _effectName = effectName;
            Play();
        }

        /// <summary xml:lang="en">
        /// Plays the effect that has been set.
        /// </summary>
        /// <summary xml:lang="ja">
        /// 設定されているエフェクトを再生
        /// </summary>
        public void Play() {
            if (_handle.HasValue) {// 古いものがあれば停止
                _handle.Value.Stop();
                _handle = null;
            }

            var h = EffekseerSystem.PlayEffect(_effectName, Location);
            h.SetRotation(Rotation);

            h.SetScale(transform.localScale);
            h.SetAllColor(_color);
            h.Speed = _speed;
            _handle = h;
            Frame = 0;
            Status = EmitterStatus.Playing;
        }

        /// <summary xml:lang="en">
        /// Stops the played effect.
        /// All nodes will be destroyed.
        /// </summary>
        /// <summary xml:lang="ja">
        /// 再生中のエフェクトを停止
        /// 全てのノードが即座に消える
        /// </summary>
        public void Stop() {
            if (!_handle.HasValue) return;

            _handle.Value.Stop();
            _handle = null;
            Status = EmitterStatus.Stopped;
        }

        /// <summary xml:lang="en">
        /// Stops the root node of the played effect.
        /// The root node will be destroyed. Then children also will be destroyed by their lifetime.
        /// </summary>
        /// <summary xml:lang="ja">
        /// 再生中のエフェクトのルートノードだけを停止
        /// ルートノードを削除したことで子ノード生成が停止され寿命で徐々に消える
        /// </summary>
        public void StopRoot() {
            if ((_status & (EmitterStatus.Playing | EmitterStatus.Paused)) == EmitterStatus.Empty) return;
            if (!Exists) return;

            _handle.Value.StopRoot();
            Status = EmitterStatus.Stopping;
        }

        private Color _color = Color.white;
        /// <summary xml:lang="en">
        /// Specify the color of overall effect.
        /// </summary>
        /// <summary xml:lang="ja">
        /// エフェクト全体の色を指定する。
        /// </summary>
        /// <param name="color">Color</param>
        public void SetAllColor(Color color) {
            if (!_handle.HasValue) return;
            if (_color == color) return;
            _color = color;

            // 色自体の変更はしても再生中/一時停止中でなければ、handlerには反映しない
            //  StopRoot後のPauseだとアクセス違反が発生するため、Playingか、Pausedで且つ直前がPlayingの場合に限る
            if (_status == EmitterStatus.Playing ||
                (_status == EmitterStatus.Paused && _prevStatus == EmitterStatus.Playing)) {
                _handle.Value.SetAllColor(_color);
            }
        }

        /// <summary xml:lang="en">
        /// Sets the target location of the played effects.
        /// <param name="targetLocation" xml:lang="en">Target location</param>
        /// </summary>
        /// <summary xml:lang="ja">
        /// 再生中のエフェクトのターゲット位置を設定
        /// <param name="targetLocation" xml:lang="ja">ターゲット位置</param>
        /// </summary>
        public void SetTargetLocation(Vector3 targetLocation) {
            if (_handle.HasValue) {
                _handle.Value.SetTargetLocation(targetLocation);
            }
        }

        /// <summary xml:lang="en">
        /// Pausing the effect
        /// <para>true:  It will update on Update()</para>
        /// <para>false: It will not update on Update()</para>
        /// </summary>
        /// <summary xml:lang="ja">
        /// ポーズ設定
        /// <para>true:  Updateで更新しない</para>
        /// <para>false: Updateで更新する</para>
        /// </summary>
        public bool Paused {
            set {
                if (_status == EmitterStatus.Stopped || !Exists) return;

                var h = _handle.Value;
//                if (!h.Paused) {
//                    prevStatus = status;
//                }
                h.Paused = value;
                Status = h.Paused ? EmitterStatus.Paused : _prevStatus;
            }
            get {
                return _handle.HasValue && _handle.Value.Paused;
            }
        }

        /// <summary xml:lang="en">
        /// Showing the effect
        /// <para>true:  It will be rendering.</para>
        /// <para>false: It will not be rendering.</para>
        /// </summary>
        /// <summary xml:lang="ja">
        /// 表示設定
        /// <para>true:  描画する</para>
        /// <para>false: 描画しない</para>
        /// </summary>
        public bool Shown {
            set {
                if (!_handle.HasValue) return;

                var h = _handle.Value;
                h.Shown = value;
            }
            get {
                return _handle.HasValue && _handle.Value.Shown;
            }
        }

        private float _speed = 1f;
        /// <summary xml:lang="en">
        /// Playback speed
        /// </summary>
        /// <summary xml:lang="ja">
        /// 再生速度
        /// </summary>
        public float Speed { 
            set {
                if (Math.Abs(_speed - value) < ConstantValues.EPSILON_SPEED) return;

                _speed = value;
                
                if (!_handle.HasValue) return;
                var h = _handle.Value;
                h.Speed = _speed;
            }
            get {
                return _handle.HasValue ? _handle.Value.Speed : 0.0f;
            }
        }

//        /// <summary xml:lang="en">
//        /// Play frame
//        /// </summary>
//        /// <summary xml:lang="ja">
//        /// 再生フレーム
//        /// </summary>
//        public float Frame { 
//            get {
//                return _handle.HasValue ? _handle.Value.Frame : -1f;
//            }
//        }

        /// <summary xml:lang="en">
        /// Existing state
        /// <para>true:  It's existed.</para>
        /// <para>false: It isn't existed or stopped.</para>
        /// </summary>
        /// <summary xml:lang="ja">
        /// 再生中のエフェクトが存在しているか
        /// <para>true:  存在している</para>
        /// <para>false: 再生終了で破棄。もしくはStopで停止された</para>
        /// </summary>
        public bool Exists {
            get {
                return _handle.HasValue && _handle.Value.Exists;
            }
        }

        public void UpdateLocation() {
            if (!Exists) return;

            //  StopRoot後のPauseだとアクセス違反が発生するためPlayingで条件を絞る
            if (_status != EmitterStatus.Playing &&
                (_status != EmitterStatus.Paused || _prevStatus != EmitterStatus.Playing)) return;

            var h = _handle.Value;
            h.SetLocation(Location);
        }

        public void UpdateRotation() {
            if (!Exists) return;

            //  StopRoot後のPauseだとアクセス違反が発生するためPlayingで条件を絞る
            if (_status != EmitterStatus.Playing &&
                (_status != EmitterStatus.Paused || _prevStatus != EmitterStatus.Playing)) return;

            var h = _handle.Value;
            h.SetRotation(Rotation);
        }

        public override string ToString() {
            var builder = new StringBuilder();
            builder.Append("EffekseerEmitter[");
            builder.Append("EffectName=").Append(EffectName).Append(", status=").Append(_status);
            if (fixLocation) builder.Append(", fixLocation");
            if (fixRotation) builder.Append(", fixRotation");
            if (useLocalRotation) builder.Append(", useLocalRotation");
            builder.Append(", location=").Append(transform.position)
                .Append(", localLoc=").Append(transform.localPosition)
                .Append(", rotation=").Append(transform.rotation)
                .Append(", localRot=").Append(transform.localRotation)
                .Append(", scale=").Append(transform.localScale)
                .Append(", speed=").Append(_speed)
                .Append(", color=").Append(_color);
            builder.Append(']');
            return builder.ToString();
        }

        #region Internal Implimentation

        void Start() {
            if (string.IsNullOrEmpty(_effectName)) return;

            EffekseerSystem.LoadEffect(_effectName);
            if (playOnStart) Play();
        }

        void OnDestroy() {
            Stop();
        }

        void Update() {
            if (!_handle.HasValue) return;

            try {
                var h = _handle.Value;
                if (h.Exists) {
                    if (_status != EmitterStatus.Playing) return;

                    Frame += _speed;
                    if (0 < endFrame && endFrame < Frame) {
                        if (loop) Play();
                        else Stop();
                        return;
                    }

                    if (!fixLocation) h.SetLocation(Location);
                    if (!fixRotation) h.SetRotation(Rotation);

                    h.SetScale(transform.localScale);
                } else if (loop) {
                    Play();
                } else {
                    Stop();
                }
            } catch(Exception e) {
                Debug.LogError("update error:"+ e.Message);
            }
        }

        #endregion

   }
}