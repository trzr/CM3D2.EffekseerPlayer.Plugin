using System.Collections.Generic;
using UnityEngine;

namespace EffekseerPlayer.CM3D2.Util {
    /// <summary>
    /// コンボボックスの選択、メイドアクセスを容易にするためのヘルパークラス
    /// </summary>
    public class MaidHolder {
        private readonly List<Maid> _maidList = new List<Maid>();
        private readonly Dictionary<string, GUIContent> _contentsCache = new Dictionary<string, GUIContent>();

        public Maid Get(int selectedIdx) {
            if (selectedIdx >= 0 && selectedIdx < _maidList.Count) {
                return _maidList[selectedIdx];
            } 
            return null;
        }

        public int GetMaidIndex(string guid) {
            return _maidList.FindIndex(maid => MaidHelper.GetGuid(maid) == guid);
        }

        public IList<GUIContent> CreateActiveMaidContents() {
            var charMgr = GameMain.Instance.CharacterMgr;
            var count = charMgr.GetMaidCount();
            _maidList.Clear();
            var contents = new List<GUIContent>();

            for (var i = 0; i < count; i++) {
                var maid = charMgr.GetMaid(i);
                if (maid == null || !maid.enabled) continue;
                _maidList.Add(maid);
                GUIContent content;
                var guid = MaidHelper.GetGuid(maid);
                if (!_contentsCache.TryGetValue(guid, out content)) {
                    // TODO サイズ変更: しなくても動く. 想定サイズに変更した方がコンボ表示への影響を抑えられる
                    var tex2D = maid.GetThumIcon();
                    var maidName = MaidHelper.GetName(maid);
                    content = new GUIContent(maidName, tex2D);
                    _contentsCache[guid] = content;
                }
                contents.Add(content);
            }

            return contents;
        }
    }
}