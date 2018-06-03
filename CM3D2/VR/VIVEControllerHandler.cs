using UnityEngine;
using Valve.VR;

namespace EffekseerPlayer.CM3D2.VR {
    public class VIVEControllerHandler : MonoBehaviour {

        private SteamVR_TrackedObject trackedObj;
        public SteamVR_TrackedObject TrackedObj {
            get {
                if (trackedObj == null) {
                    trackedObj = GetComponent<SteamVR_TrackedObject>();
                }
                return trackedObj;
            }
        }

        public bool IsValid() {
            return TrackedObj.isValid;
        }

        public VRControllerState_t State {
            get { return Device.GetState(); }
        }

        public VRControllerState_t PrevState {
            get { return Device.GetPrevState(); }
        }

        private SteamVR_Controller.Device device;
        public SteamVR_Controller.Device Device {
            get { return device ?? (device = SteamVR_Controller.Input((int) TrackedObj.index)); }
        }

        public bool GetKeyDowns() {
            // 単発のキーしか反応しない
            return Device.GetPressDown(SteamVR_Controller.ButtonMask.Grip);
        }

        public bool GetKeyDowned(ulong keyMask) {
            if (!IsValid()) return false;

            var dev = Device;
            var state = dev.GetState();
            var prevState = dev.GetPrevState();
            return (prevState.ulButtonPressed & keyMask) != keyMask
                   && (state.ulButtonPressed & keyMask) == keyMask;
        }
    }
}