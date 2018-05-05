using UnityEngine;

namespace Crosstales.RTVoice.EditorUtil
{
    /// <summary>GA-wrapper API.</summary>
    public class GAApi
    {
        #region Variables

        private static string clientId = SystemInfo.deviceUniqueIdentifier;
        private static string screenResolution = Screen.currentResolution.ToString();
        private static string userLanguage = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
        private static string unityVersion = Application.unityVersion;
        private static string os = SystemInfo.operatingSystem;
        private static string cpu = SystemInfo.processorType;
        private static int cpuCores = SystemInfo.processorCount;
        private static int cpuFrequency = SystemInfo.processorFrequency;
        private static int memory = SystemInfo.systemMemorySize;
        private static string gpu = SystemInfo.graphicsDeviceName;
        private static int gpuMemory = SystemInfo.graphicsMemorySize;
        private static int gpuShaderLevel = SystemInfo.graphicsShaderLevel;
        private static string productName = Application.productName;
        private static string companyName = Application.companyName;

        #endregion


        #region Public methods

        /// <summary>Tracks an event from the asset.</summary>
        /// <param name="category">Specifies the event category.</param>
        /// <param name="action">Specifies the event action.</param>
        /// <param name="label">Specifies the event label.</param>
        /// <param name="value">Specifies the event value.</param>
        public static void Event(string category, string action, string label = "", int value = 0)
        {
            new System.Threading.Thread(() => trackEvent(Util.Constants.ASSET_NAME, Util.Constants.ASSET_VERSION, category, action, label, value)).Start();
            //trackEvent(appName, appVersion, category, action, label, value);
        }

        #endregion


        #region Private methods

        private static void trackEvent(string appName, string appVersion, string category, string action, string label, int value)
        {
            post(generalInfo(appName, appVersion) +
            "&t=event" +
            "&ec=" + category +
            "&ea=" + action +
            (string.IsNullOrEmpty(label) ? string.Empty : "&el=" + label) +
            (value > 0 ? "&ev=" + value : string.Empty) +
            customDimensions()
            );
        }

        private static void post(string postData)
        {
            if (EditorConfig.TELEMETRY)
            {
                byte[] data = new System.Text.ASCIIEncoding().GetBytes(postData);

                try
                {
                    System.Net.ServicePointManager.ServerCertificateValidationCallback = Util.Helper.RemoteCertificateValidationCallback;

                    using (System.Net.WebClient client = new Util.CTWebClient())
                    {
                        client.Headers[System.Net.HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        client.Headers[System.Net.HttpRequestHeader.UserAgent] = "Mozilla/5.0 (" + userAgent() + ")";

                        client.UploadData("https://www.google-analytics.com/collect", data);

                        if (Util.Constants.DEV_DEBUG)
                            Debug.Log("Data uploaded: " + postData);
                    }
                }
                catch (System.Exception ex)
                {
                    if (Util.Constants.DEV_DEBUG)
                        Debug.LogError("Could not upload GA-data: " + System.Environment.NewLine + ex);
                }
            }
        }

        private static string userAgent()
        {
            if (Util.Helper.isWindowsPlatform)
            {
                return "compatible; Windows NT 10.0; WOW64";
            }
            else if (Util.Helper.isMacOSPlatform)
            {
                return "compatible; Macintosh; Intel Mac OS X";
            }
            else
            {
                return "compatible; X11; Linux i686";
            }
        }

        private static string generalInfo(string appName, string appVersion)
        {
            return "v=1&tid=UA-45810925-1" +
                "&ds=app" +
                "&cid=" + clientId +
                "&ul=" + userLanguage +
                "&an=" + appName +
                "&av=" + appVersion +
                "&sr=" + screenResolution;
        }

        private static string customDimensions()
        {
            return "&cd1=" + os +
                "&cd2=" + memory +
                "&cd3=" + cpu +
                "&cd4=" + gpu +
                "&cd5=" + productName +
                "&cd6=" + companyName +
                //"&cd7=" + usage +
                "&cd8=" + unityVersion +
                "&cd9=" + cpuCores +
                "&cd10=" + cpuFrequency +
                "&cd11=" + gpuMemory +
                "&cd12=" + gpuShaderLevel;
        }

        #endregion

    }
}