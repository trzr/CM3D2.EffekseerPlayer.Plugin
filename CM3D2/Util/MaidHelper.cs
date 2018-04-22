using System.Collections.Generic;
using UnityEngine;

namespace EffekseerPlayer.CM3D2.Util {
    public static class MaidHelper {
        
        private static readonly Dictionary<string, Maid> GuidCache = new Dictionary<string, Maid>();
//        public static readonly Func<Maid, string> GetGuid = maid => string.Empty;
//        public static readonly Func<Maid, string> GetName = maid => string.Empty;
//
//        static MaidHelper() {
//            // CM3D2とCOM3D2でguidのアクセス方法が異なるため、
//            // リフレクションにてアクセス方法を切り替え
//            var typeObj = typeof(Maid);
//            var paramField = typeObj.GetField("m_Param", BindingFlags.Instance | BindingFlags.NonPublic);
//            if (paramField != null) {
//                // for CM3D2
//                var statusProp = paramField.FieldType.GetProperty("status");
//                if (statusProp == null) return;
//                var guidProp = statusProp.PropertyType.GetProperty("guid");
//                if (guidProp != null) {
//                    GetGuid = maid => {
//                        var param = paramField.GetValue(maid);
//                        var status = statusProp.GetValue(param, null);
//                        if (status == null) return null;
//                        return (string) guidProp.GetValue(status, null);
//                    };
//                }
//                var lastNameProp = statusProp.PropertyType.GetProperty("last_name");
//                var firstNameProp = statusProp.PropertyType.GetProperty("first_name");
//                if (lastNameProp != null && firstNameProp != null) {
//                    GetName = maid => {
//                        var param = paramField.GetValue(maid);
//                        var status = statusProp.GetValue(param, null);
//                        if (status == null) return string.Empty;
//                        return (string) lastNameProp.GetValue(status, null) + " " + (string) firstNameProp.GetValue(status, null);
//                    };
//                }
//                
//            } else {
//                // for COM3D2
//                var statusField = typeObj.GetField("m_Status", BindingFlags.Instance | BindingFlags.NonPublic);
//                if (statusField == null) return;
//                var guidProp = statusField.FieldType.GetProperty("guid");
//                if (guidProp != null) {
//                    GetGuid = (maid) => {
//                        var status = statusField.GetValue(maid);
//                        if (status == null) return null;
//                        return (string) guidProp.GetValue(status, null);
//                    };
//                }
//                var nameProp = statusField.FieldType.GetProperty("fullNameJpStyle");
//                if (nameProp != null) {
//                    GetName = maid => {
//                        var status = statusField.GetValue(maid);
//                        if (status == null) return string.Empty;
//                        return (string) nameProp.GetValue(status, null);
//                    };
//                }
//            }
//        }

        public static string GetGuid(Maid maid) {
#if COM3D2
            return maid.status.guid;
#else
            return maid.Param.status.guid;
#endif
        }

        public static string GetName(Maid maid) {
#if COM3D2
            return maid.status.fullNameJpStyle;
#else
            var status = maid.Param.status; 
            return status.last_name + " " + status.first_name;
#endif
        }

        public static Maid GetMaid(string guid) {
            if (guid == null) return null;
            Maid maid;
            if (GuidCache.TryGetValue(guid, out maid)) return maid;

            maid = GameMain.Instance.CharacterMgr.GetMaid(guid);
            if (maid == null) {
                maid = GameMain.Instance.CharacterMgr.GetStockMaid(guid);
                if (maid == null) return null;
            }

            GuidCache[guid] = maid;

            return maid;
        }

        public static void Clear() {
            GuidCache.Clear();
        }

        public static Transform SearchBone(Transform bone, string boneName) {
            if (bone.name == boneName) return bone;

            foreach (Transform child in bone) {
                var result = SearchBone(child, boneName);
                if (result != null) return result;
            }
            return null;
        }

        public static List<string> GetBoneNames(Maid maid, int slotNo) {
            var slot = maid.body0.goSlot[slotNo];
            if (slot == null || slot.obj == null) return EMPTY;

            var boneNames = new List<string>();

            foreach (Transform child in slot.obj.transform) {
                // 子無しはスキップ
                if (child.childCount == 0) continue;
                ParseBones(boneNames, child);
            }
            return boneNames;
        }

        private static void ParseBones(ICollection<string> boneNames, Transform bone) {
            boneNames.Add(bone.name);

            foreach (Transform child in bone) {
                ParseBones(boneNames, child);
            }
        }
        private static readonly List<string> EMPTY = new List<string>(0);
    }
}