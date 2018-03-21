
using System;
using EffekseerPlayerPlugin.Util;

namespace EffekseerPlayerPlugin.CM3D2.Util
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ConstantHolder
    {
        private static readonly ConstantHolder INSTANCE = new ConstantHolder();
        
        public static ConstantHolder Instance {
            get { return INSTANCE; }
        }
        
        public readonly int SLOT_COUNT;
        private ConstantHolder() {
            var cnt = PrivateAccessor.Get<int>(typeof(TBody),"strSlotNameItemCnt");
            SLOT_COUNT = TBody.m_strDefSlotName.Length/cnt;
        }
        public int ParseSlot(string str) {
            return (int)Enum.ToObject(typeof(TBody.SlotID), str);
        }
    }
}
