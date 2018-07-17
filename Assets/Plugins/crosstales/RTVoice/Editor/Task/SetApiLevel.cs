using UnityEditor;

namespace Crosstales.RTVoice.EditorTask
{
    /// <summary>Sets the required API levels.</summary>
    [InitializeOnLoad]
    public static class SetApiLevel
    {

        #region Constructor

        static SetApiLevel()
        {
            // set the miniumum API-level for Android to 15/16
            int androidVersion;

            if (int.TryParse(PlayerSettings.Android.minSdkVersion.ToString().Substring("AndroidApiLevel".Length), out androidVersion))
            {
                if (androidVersion < 16)
                {
                    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel16;
                }
            }

            // set the miniumum version for iOS to 8.0
            float iOSVersion;

#if UNITY_5_5_OR_NEWER
            if (float.TryParse(PlayerSettings.iOS.targetOSVersionString.ToString(), out iOSVersion)) {
                if (iOSVersion < 8f) {
                    PlayerSettings.iOS.targetOSVersionString = "8.0";
                }
            }
#else
            if (float.TryParse(PlayerSettings.iOS.targetOSVersion.ToString().Substring("iOS_".Length).Replace("_", "."), out iOSVersion))
            {
                if (iOSVersion < 8f)
                {
                    PlayerSettings.iOS.targetOSVersion = iOSTargetOSVersion.iOS_8_0;
                }
            }
#endif
        }

        #endregion
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)