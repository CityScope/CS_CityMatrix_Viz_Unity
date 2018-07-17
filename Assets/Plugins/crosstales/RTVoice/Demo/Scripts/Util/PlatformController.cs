using UnityEngine;

namespace Crosstales.RTVoice.Demo.Util
{
    /// <summary>Enables or disable game objects for a given platform.</summary>
    [HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_util_1_1_platform_controller.html")]
    public class PlatformController : Common.Util.PlatformController
    {

        #region MonoBehaviour methods

        public override void Start()
        {
            Speaker.OnProviderChange += onProviderChange;

            onProviderChange(string.Empty);
        }

        public void OnDestroy()
        {
            Speaker.OnProviderChange -= onProviderChange;
        }

        #endregion


        #region Private methods

        private void onProviderChange(string provider)
        {
            if (Speaker.isMaryMode)
            {
                if (Platforms.Contains(Common.Model.Enum.Platform.Web) && RTVoice.Util.Helper.isWebPlatform)
                { // special case since Web always needs MaryTTS
                    currentPlatform = Common.Model.Enum.Platform.Web;
                }
                else
                {
                    currentPlatform = Common.Model.Enum.Platform.MaryTTS;
                }

                activateGO();
            }
            else
            {
                selectPlatform();
            }

            //Debug.Log (currentPlatform);

        }

        #endregion

        /*
        public enum Platform
        {
            OSX,
            Windows,
            IOS,
            Android,
            WSA,
            MaryTTS,
            Web,
            Unsupported
        }
        */
    }
}
// © 2016-2018 crosstales LLC (https://www.crosstales.com)