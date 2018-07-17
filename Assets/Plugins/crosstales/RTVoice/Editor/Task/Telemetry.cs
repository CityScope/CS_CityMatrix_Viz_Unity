using UnityEditor;
using Crosstales.RTVoice.EditorUtil;

namespace Crosstales.RTVoice.EditorTask
{
    /// <summary>Gather some telemetry data for the asset.</summary>
    [InitializeOnLoad]
    public static class Telemetry
    {
        #region Constructor

        static Telemetry()
        {
            string lastDate = string.Empty;
            if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_TELEMETRY_DATE))
            {
                lastDate = Common.Util.CTPlayerPrefs.GetString(EditorConstants.KEY_TELEMETRY_DATE);
            }
            //string lastDate = EditorPrefs.GetString(Util.Constants.KEY_TELEMETRY_DATE);

            string date = System.DateTime.Now.ToString("yyyyMMdd"); // every day
            //string date = System.DateTime.Now.ToString("yyyyMMddHHmm"); // every minute (for tests)

            if (!date.Equals(lastDate))
            {
                GAApi.Event(typeof(Telemetry).Name, "Startup");

                Common.Util.CTPlayerPrefs.SetString(EditorConstants.KEY_TELEMETRY_DATE, date);
                //EditorPrefs.SetString(Util.Constants.KEY_TELEMETRY_DATE, date);
            }
        }

        #endregion

    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)