using UnityEngine;
using UnityEditor;

namespace Crosstales.RTVoice.EditorUtil
{
    /// <summary>Editor configuration for the asset.</summary>
    [InitializeOnLoad]
    public static class EditorConfig
    {

        #region Variables

        /// <summary>Enable or disable update-checks for the asset.</summary>
        public static bool UPDATE_CHECK = EditorConstants.DEFAULT_UPDATE_CHECK;

        /// <summary>Enable or disable reminder-checks for the asset.</summary>
        public static bool REMINDER_CHECK = EditorConstants.DEFAULT_REMINDER_CHECK;

        /// <summary>Enable or disable anonymous telemetry data.</summary>
        public static bool TELEMETRY = EditorConstants.DEFAULT_TELEMETRY;

        /// <summary>Automatically load and add the prefabs to the scene.</summary>
        public static bool PREFAB_AUTOLOAD = EditorConstants.DEFAULT_PREFAB_AUTOLOAD;

        /// <summary>Enable or disable the icon in the hierarchy.</summary>
        public static bool HIERARCHY_ICON = EditorConstants.DEFAULT_HIERARCHY_ICON;

        /// <summary>Is the configuration loaded?</summary>
        public static bool isLoaded = false;

        private static string assetPath = null;
        private const string idPath = "Documentation/id/";
        private readonly static string idName = EditorConstants.ASSET_UID + ".txt";

        #endregion


        #region Constructor

        static EditorConfig()
        {
            if (!isLoaded)
            {
                Load();
            }
        }

        #endregion


        #region Properties

        /// <summary>Returns the path to the asset inside the Unity project.</summary>
        /// <returns>The path to the asset inside the Unity project.</returns>
        public static string ASSET_PATH
        {
            get
            {
                if (assetPath == null)
                {
                    initAssetPath();
                }

                return assetPath;
            }
        }

        /// <summary>Returns the path of the prefabs.</summary>
        /// <returns>The path of the prefabs.</returns>
        public static string PREFAB_PATH
        {
            get
            {
                return ASSET_PATH + EditorConstants.PREFAB_SUBPATH;
            }
        }

        #endregion


        #region Public static methods

        /// <summary>Resets all changable variables to their default value.</summary>
        public static void Reset()
        {
            UPDATE_CHECK = EditorConstants.DEFAULT_UPDATE_CHECK;
            REMINDER_CHECK = EditorConstants.DEFAULT_REMINDER_CHECK;
            TELEMETRY = EditorConstants.DEFAULT_TELEMETRY;
            PREFAB_AUTOLOAD = EditorConstants.DEFAULT_PREFAB_AUTOLOAD;
            HIERARCHY_ICON = EditorConstants.DEFAULT_HIERARCHY_ICON;
        }

        /// <summary>Loads all changable variables.</summary>
        public static void Load()
        {
            initAssetPath();

            if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_UPDATE_CHECK))
            {
                UPDATE_CHECK = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_UPDATE_CHECK);
            }

            if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_REMINDER_CHECK))
            {
                REMINDER_CHECK = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_REMINDER_CHECK);
            }

            if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_TELEMETRY))
            {
                TELEMETRY = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_TELEMETRY);
            }

            if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_PREFAB_AUTOLOAD))
            {
                PREFAB_AUTOLOAD = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_PREFAB_AUTOLOAD);
            }

            if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_HIERARCHY_ICON))
            {
                HIERARCHY_ICON = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_HIERARCHY_ICON);
            }

            isLoaded = true;
        }

        /// <summary>Saves all changable variables.</summary>
        public static void Save()
        {
            Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_UPDATE_CHECK, UPDATE_CHECK);
            Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_REMINDER_CHECK, REMINDER_CHECK);
            Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_TELEMETRY, TELEMETRY);
            Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_PREFAB_AUTOLOAD, PREFAB_AUTOLOAD);
            Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_HIERARCHY_ICON, HIERARCHY_ICON);

            Common.Util.CTPlayerPrefs.Save();
        }

        #endregion

        private static void initAssetPath()
        {
            if (assetPath == null)
            {
                try
                {
                    if (System.IO.File.Exists(Application.dataPath + EditorConstants.DEFAULT_ASSET_PATH + idPath + idName))
                    {
                        assetPath = EditorConstants.DEFAULT_ASSET_PATH;
                    }
                    else
                    {
                        string[] files = System.IO.Directory.GetFiles(Application.dataPath, idName, System.IO.SearchOption.AllDirectories);

                        if (files.Length > 0)
                        {
                            string name = files[0].Substring(Application.dataPath.Length);
                            assetPath = name.Substring(0, name.Length - idPath.Length - idName.Length).Replace("\\", "/");
                        }
                        else
                        {
                            Debug.LogWarning("Could not locate the asset! File not found: " + idName);
                            assetPath = EditorConstants.DEFAULT_ASSET_PATH;
                        }
                    }

                    //Debug.LogWarning("PATH : " + assetPath);
                    Common.Util.CTPlayerPrefs.SetString(Util.Constants.KEY_ASSET_PATH, assetPath);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Could not locate asset: " + ex);
                }
            }
        }
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)