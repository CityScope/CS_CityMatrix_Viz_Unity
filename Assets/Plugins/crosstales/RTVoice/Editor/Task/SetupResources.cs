using UnityEngine;
using UnityEditor;

namespace Crosstales.RTVoice.EditorTask
{
    /// <summary>Moves all needed resources to 'Editor Default Resources'.</summary>
    [InitializeOnLoad]
    public class SetupResources : Common.EditorTask.BaseSetupResources
    {

        #region Constructor

        static SetupResources()
        {

#if !rtv_ignore_setup

            string path = Application.dataPath;
            string assetpath = "Assets" + EditorUtil.EditorConfig.ASSET_PATH;

            string sourceFolder = path + EditorUtil.EditorConfig.ASSET_PATH + "Icons/";
            string source = assetpath + "Icons/";

            string targetFolder = path + "/Editor Default Resources/crosstales/RTVoice/";
            string target = "Assets/Editor Default Resources/crosstales/RTVoice/";
            string metafile = assetpath + "Icons.meta";
            
            setupResources(source, sourceFolder, target, targetFolder, metafile);
#endif
        }

        #endregion
    }
}
// © 2016-2018 crosstales LLC (https://www.crosstales.com)