using System.Collections.Generic;
using UnityEngine;

namespace EffekseerPlayer.CM3D2.Render {
    ///
    /// ボーン描画クラス.
    ///  オリジナルのオブジェクトへのアタッチを避け、
    ///  Updateにより位置を適宜更新する.
    ///
    public class CustomBoneRenderer {
        #region Fields
        private const string NAME_LINE_PREFIX = "___LINE_";
        private const string NAME_SCL = "_SCL_";

        private readonly Dictionary<string, LineRenderer> _lineDict = new Dictionary<string, LineRenderer>();
        private readonly List<GameObject> _cache = new List<GameObject>();

        private Material _lineMaterial;
        private readonly float _lineWidth = 0.006f;
        private readonly Color _color = Color.white;

        private Transform _rootBone;
        private bool _isVisible;
        private bool _skipVisble;


        // bool _isFirst = true;
        #endregion

        ~CustomBoneRenderer() {
            ClearCache();
        }

        /// 指定されたメイドのインスタンスID
        private int _targetId;
        public int TargetId {
            get { return _targetId; }
            set { _targetId = value;  }
        }

        public bool IsEnabled() {
            return _rootBone != null;
        }

        public void SetVisible(bool visible) {
            if (_isVisible != visible) {
                SetVisibleAll(visible);
            }
            _isVisible = visible;
        }

        private void SetVisibleAll(bool visible) {
            foreach (var obj in _cache) {
                obj.SetActive(visible);
            }
        }

        public void Setup(Transform bone) {
            // Log.Debug("setup bone render:", bone.name);
            if (_rootBone == bone) return;

            Clear();
            _rootBone = bone;

            foreach (Transform child in bone) {
                if (child.childCount == 0) continue;
                SetupBone(child);
            }

            foreach (var obj in _cache) {
                obj.SetActive(_isVisible);
            }
        }

        private void SetupBone(Transform bone) {
            if (_lineDict.ContainsKey(bone.name)) return;

            var lineRenderer = CreateComponent();
            lineRenderer.gameObject.name = NAME_LINE_PREFIX + bone.name;
            _lineDict.Add(bone.name, lineRenderer);

            foreach (Transform child in bone) {
                if (child.childCount == 0) continue;
                if (child.name.StartsWith(ConstantValues.NAME_PREFIX)) continue;

                SetupBone(child);
            }
        }

        private void UpdateVisible(bool visible) {
            if (_skipVisble != visible) return;

            _skipVisble = !visible;
            SetVisibleAll(visible);
        }

        public void Update() {
            if (_rootBone == null) {
                if (_isVisible) SetVisible(false);
                return;
            }
            if (!_rootBone.gameObject.activeSelf) {
                // 一時非表示
                UpdateVisible(false);
                return;
            } 
            // 一時非表示からの復帰
            UpdateVisible(true);

            foreach (Transform child in _rootBone) {
                if (child.childCount == 0) continue;

                if (child.gameObject.activeSelf) {
                    UpdatePosition(child, true);
                }
            }
            // _isFirst = false;
        }

        private void EmptyBone(LineRenderer renderer) {
#if UNITY_5_6_OR_NEWER
            renderer.positionCount = 0;
#else
            renderer.SetVertexCount(0);
#endif
            renderer.enabled = false;
            // if (_isFirst) Log.Debug(renderer.name, " is leaf");
        }

        public void UpdatePosition(Transform bone, bool isRoot=false) {
            LineRenderer boneLine;
            if (!_lineDict.TryGetValue(bone.name, out boneLine)) return;

            if (bone.childCount == 0) {
                EmptyBone(boneLine);
                return;
            }
#if UNITY_5_6_OR_NEWER
            boneLine.positionCount = 2;
#else
            boneLine.SetVertexCount(2);
#endif
            boneLine.SetPosition(0, bone.position);

            Vector3? pos = null;
            if (bone.childCount == 1) {
                var child0 = bone.GetChild(0);
                if (child0.name.StartsWith(ConstantValues.NAME_PREFIX)) {
                    EmptyBone(boneLine);
                    return;
                }
                pos = child0.position;
                if (pos == bone.position) {
                    //if (_isFirst) Log.Debug(bone.name, " is zero length. child count=", bone.childCount);
                    var vec = new Vector3(-0.1f, 0f, 0f);
                    var loc = bone.rotation * vec;
                    pos = bone.position + loc;
                }
            } else {
                if (bone.childCount == 2) {
                    var child0 = bone.GetChild(0);
                    var child1 = bone.GetChild(1);
                    if (child0.name.EndsWith(NAME_SCL) || child0.name.StartsWith(ConstantValues.NAME_PREFIX)) {
                        pos = child1.position;
                    } else if (child1.name.EndsWith(NAME_SCL) || child1.name.StartsWith(ConstantValues.NAME_PREFIX)) {
                        pos = child0.position;
                    }
                }
                if (!pos.HasValue) {
                    var maxLength = 0.1f;
                    if (!isRoot) {
                        foreach (Transform child in bone) {
                            if (child.name.StartsWith(ConstantValues.NAME_PREFIX)) continue;
                            var delta = child.position - bone.position;
                            var length = delta.magnitude;

                            if (length > maxLength) maxLength = length;
                        }
                    }
                    //if (_isFirst) {
                    //    Log.Debug(bone.name, " has multi-child. ", bone.childCount, ", length=", maxLength);
                    //    //boneLine.material.color = Color.magenta;
                    //}
                    var vec = new Vector3(-maxLength, 0f, 0f);
                    var loc = bone.rotation * vec;
                    pos = bone.position + loc;
                }
            }

            boneLine.SetPosition(1, pos.Value);

            // if (_isFirst) OutputLog(bone.position, pos.Value, bone.name);

            foreach (Transform childBone in bone) {
                UpdatePosition(childBone);
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OutputLog(Vector3 pos, Vector3 pos2, string name) {
            Log.Debug(name, "(", pos.x, ",", pos.y, ",", pos.z, ")=>(", pos2.x, ",", pos2.y, ",", pos2.z, ")");
        }

        public void Clear() {
            _lineDict.Clear();
            ClearCache();
            _rootBone = null;
			_targetId = -1;
        }

        private void ClearCache() {
            foreach (var obj in _cache) {
                Object.Destroy(obj);
            }
            _cache.Clear();
        }

        private LineRenderer CreateComponent() {

            if (_lineMaterial == null) {
                var shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(shader) {
                    hideFlags = HideFlags.HideAndDontSave
                };
                _lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Disabled);
                _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _lineMaterial.SetInt("_ZWrite", 0);
                _lineMaterial.renderQueue = 5000;
            }
            _lineMaterial.color = _color;
            var obj = new GameObject();
            _cache.Add(obj);

            var line = obj.AddComponent<LineRenderer>();
            line.materials = new[] { _lineMaterial, };
#if UNITY_5_6_OR_NEWER
            line.startWidth = _lineWidth;
            line.endWidth   = _lineWidth * 0.2f;
#else            
            line.SetWidth(_lineWidth, _lineWidth * 0.20f);
#endif
            return line;
        }
    }
}