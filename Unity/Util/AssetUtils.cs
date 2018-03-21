using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EffekseerPlayerPlugin.Unity.Util
{
    /// <summary>
    /// Description of AssetUtils.
    /// </summary>
    public class AssetUtils
    {
        private const string PREFIX = "assets/ui/";
        public static IEnumerator LoadAsync(byte[] bytes, Action<AssetBundle> setBundle) {

            var bundleLoadRequest = AssetBundle.LoadFromMemoryAsync(bytes);
            yield return bundleLoadRequest;

            var loadedBundle = bundleLoadRequest.assetBundle;
            if (loadedBundle == null) {
                Log.Info("Failed to load AssetBundle!");
                yield break;
            }
            setBundle(loadedBundle);
        }

        public static IEnumerator LoadAsync(string filepath, Action<AssetBundle> setBundle) {

            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(filepath);
            yield return bundleLoadRequest;
        
            var loadedBundle = bundleLoadRequest.assetBundle;
            if (loadedBundle == null) {
                Log.Info("Failed to load AssetBundle!");
                yield break;
            }
            setBundle(loadedBundle);
        }

        public string GetHierarchyPath(Transform self) {
            var path = self.gameObject.name;
            var parent = self.parent;
            while (parent != null) {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        public void SetChild(GameObject parent, GameObject child) {
            child.layer = parent.layer;
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale = Vector3.one;
            child.transform.rotation = Quaternion.identity;
        }
    }
}
