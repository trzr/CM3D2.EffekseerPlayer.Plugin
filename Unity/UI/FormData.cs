
using System;

namespace EffekseerPlayer.Unity.UI {
    public class FormData {
        public AttachData Left { get; set; }

        public AttachData Top { get; set; }
        public AttachData Right { get; set; }
        public AttachData Bottom { get; set; }

        public float Width {
            set {
                if (Left != null) {
                    Left.length = value;
                    // Right = null;
                } else if (Right != null) {
                    Right.length = value;
                }
            }
            get { return (Left != null) ? Left.length : 0; }
        }
        public float Height {
            set {
                if (Top != null) {
                    Top.length = value;
                    // Bottom = null;
                } else if (Bottom != null) {
                    Bottom.length = value;
                }
            }
            get { return (Top != null) ? Top.length : 0; }
        }

        public FormData(float left, float top, float right, float bottom) {
            Left = new AttachData(left);
            Top = new AttachData(top);
            Right = new AttachData(right);
            Bottom = new AttachData(bottom);
        }
        public FormData(float left, float top) {
            Left = new AttachData(left);
            Top = new AttachData(top);
        }
        public FormData(AttachData left, AttachData top) {
            Left = left;
            Top = top;
        }

    }

    // TODO ratioの追加

    public class AttachData {
        public GUIControl obj;
        public float offset;
        public float length;

        public AttachData(GUIControl obj1, float offset1=0, float length1=0) {
            obj = obj1;
            offset = offset1;
            length = length1;
        }

        public AttachData(float offset1) {
            offset = offset1;
        }

        public void Set(GUIControl obj1, float offset1=0, float length1=0) {
            obj = obj1;
            offset = offset1;
            length = length1;
        }
    }
}
