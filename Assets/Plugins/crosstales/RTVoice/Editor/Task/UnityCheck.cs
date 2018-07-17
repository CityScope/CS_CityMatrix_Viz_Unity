using UnityEngine;
using UnityEditor;
using Crosstales.RTVoice.EditorUtil;

namespace Crosstales.RTVoice.EditorTask
{
    /// <summary>Checks if the current Unity version is still supported by this edition of RT-Voice.</summary>
    [InitializeOnLoad]
    public static class UnityCheck
    {

        #region Constructor

        static UnityCheck()
        {
            if (!Util.Constants.isPro)
            {

                string lastDate = EditorPrefs.GetString(EditorConstants.KEY_UNITY_DATE);
                string date = System.DateTime.Now.ToString("yyyyMMddHHm"); // every 10 minutes
                //string date = System.DateTime.Now.ToString("yyyyMMddHHmm"); // every minute (for tests)

                if (!date.Equals(lastDate))
                {

#if UNITY_5_6 || UNITY_2017_0 || UNITY_2017_1 || UNITY_2017_2
                    showRedownload();
#else
                    //showRedownload();
#endif
                }
                else
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("No Unity check needed.");
                }

                EditorPrefs.SetString(EditorConstants.KEY_UNITY_DATE, date);
            }

            //showRedownload(); // for tests
        }

        #endregion

        private static void showRedownload()
        {
            if (EditorUtility.DisplayDialog(Util.Constants.ASSET_NAME + " - Incompatible Unity version!",
                "This version of RT-Voice is not compatible with the current Unity version." +
                System.Environment.NewLine +
                "Creating builds may fail and audio output from RT-Voice will not work!" +
                System.Environment.NewLine +
                System.Environment.NewLine +
                "Please download RT-Voice again from the Unity AssetStore within your current Unity version.",
                "Yes, let's do it!", "Not right now"))
            {
                Application.OpenURL(EditorConstants.ASSET_URL);
                //UnityEditorInternal.AssetStore.Open("content/" + EditorConstants.ASSET_ID);
            }
        }
    }
}
// © 2018 crosstales LLC (https://www.crosstales.com)