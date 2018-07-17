namespace Crosstales.RTVoice.EditorUtil
{
    /// <summary>Collected constants of very general utility for the asset.</summary>
    public static class EditorConstants
    {

        #region Constant variables

        // Keys for the configuration of the asset
        public const string KEY_UPDATE_CHECK = Util.Constants.KEY_PREFIX + "UPDATE_CHECK";
        public const string KEY_REMINDER_CHECK = Util.Constants.KEY_PREFIX + "REMINDER_CHECK";
        public const string KEY_TELEMETRY = Util.Constants.KEY_PREFIX + "TELEMETRY";
        public const string KEY_PREFAB_AUTOLOAD = Util.Constants.KEY_PREFIX + "PREFAB_AUTOLOAD";

        public const string KEY_HIERARCHY_ICON = Util.Constants.KEY_PREFIX + "HIERARCHY_ICON";

        public const string KEY_UPDATE_DATE = Util.Constants.KEY_PREFIX + "UPDATE_DATE";

        public const string KEY_REMINDER_DATE = Util.Constants.KEY_PREFIX + "REMINDER_DATE";
        public const string KEY_REMINDER_COUNT = Util.Constants.KEY_PREFIX + "REMINDER_COUNT";

        public const string KEY_UNITY_DATE = Util.Constants.KEY_PREFIX + "UNITY_DATE";

        public const string KEY_LAUNCH = Util.Constants.KEY_PREFIX + "LAUNCH";

        public const string KEY_TELEMETRY_DATE = Util.Constants.KEY_PREFIX + "TELEMETRY_DATE";

        // Default values
        public const string DEFAULT_ASSET_PATH = "/Plugins/crosstales/RTVoice/";
        public const bool DEFAULT_UPDATE_CHECK = true;
        public const bool DEFAULT_UPDATE_OPEN_UAS = false;
        public const bool DEFAULT_REMINDER_CHECK = true;
        public const bool DEFAULT_TELEMETRY = true;
        public const bool DEFAULT_PREFAB_AUTOLOAD = false;

        public const bool DEFAULT_HIERARCHY_ICON = true;

        #endregion


        #region Changable variables

        // Technical settings

        /// <summary>Sub-path to the prefabs.</summary>
        public static string PREFAB_SUBPATH = "Prefabs/";

        #endregion


        #region Properties

        /// <summary>Returns the URL of the asset in UAS.</summary>
        /// <returns>The URL of the asset in UAS.</returns>
        public static string ASSET_URL
        {
            get
            {

                if (Util.Constants.isPro)
                {
                    return Util.Constants.ASSET_PRO_URL;
                }
                else
                {
                    return "https://www.assetstore.unity3d.com/#!/content/48394?aid=1011lNGT&pubref=" + Util.Constants.ASSET_NAME;
                }
            }
        }

        /// <summary>Returns the ID of the asset in UAS.</summary>
        /// <returns>The ID of the asset in UAS.</returns>
        public static string ASSET_ID
        {
            get
            {
                if (Util.Constants.isPro)
                {
                    return "41068";
                }
                else
                {
                    return "48394";
                }
            }
        }

        /// <summary>Returns the UID of the asset.</summary>
        /// <returns>The UID of the asset.</returns>
        public static System.Guid ASSET_UID
        {
            get
            {
                if (Util.Constants.isPro)
                {
                    return new System.Guid("181f4dab-261f-4746-85f8-849c2866d353");
                }
                else
                {
                    return new System.Guid("1ffe8fd4-4e17-497b-9df7-7cd9200d0941");
                }
            }
        }

        #endregion
    }
}
// © 2015-2018 crosstales LLC (https://www.crosstales.com)