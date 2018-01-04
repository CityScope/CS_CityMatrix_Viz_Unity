using UnityEngine;

namespace Crosstales.UI
{
    /// <summary>Static Button Manager.</summary>
    public class StaticManager : MonoBehaviour
    {
        public string AssetstoreURL;

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
        }

        public void OpenCrosstales()
        {
            Application.OpenURL("https://crosstales.com/");
        }

        public void OpenAssetstore()
        {
            Application.OpenURL(AssetstoreURL);
        }
    }
}
// © 2017 crosstales LLC (https://www.crosstales.com)