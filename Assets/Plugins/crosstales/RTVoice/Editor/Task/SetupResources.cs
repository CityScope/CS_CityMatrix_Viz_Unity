using UnityEngine;
using UnityEditor;

namespace Crosstales.RTVoice.EditorTask
{
    /// <summary>Moves all needed resources to 'Editor Default Resources'.</summary>
    [InitializeOnLoad]
    public static class SetupResources
    {

        #region Constructor

        static SetupResources()
        {

#if !rtv_ignore_setup

            string path = Application.dataPath;
            string assetpath = "Assets" + EditorUtil.EditorConfig.ASSET_PATH;

            string sourceFolder = path + EditorUtil.EditorConfig.ASSET_PATH + "Icons/";
            string source = assetpath + "Icons/";

            string targetFolder = path + "/Editor Default Resources/RTVoice/";
            string target = "Assets/Editor Default Resources/RTVoice/";
            string metafile = assetpath + "Icons.meta";
            bool exists = false;

            try
            {
                if (System.IO.Directory.Exists(sourceFolder))
                {
                    exists = true;

                    if (!System.IO.Directory.Exists(targetFolder))
                    {
                        System.IO.Directory.CreateDirectory(targetFolder);
                    }

                    var dirSource = new System.IO.DirectoryInfo(sourceFolder);

                    foreach (var file in dirSource.GetFiles("*"))
                    {
                        AssetDatabase.MoveAsset(source + file.Name, target + file.Name);

                        if (Util.Config.DEBUG)
                            Debug.Log("File moved: " + file);
                    }

                    dirSource.Delete();

                    if (System.IO.File.Exists(metafile))
                    {
                        System.IO.File.Delete(metafile);
                    }
                }
            }
            catch (System.Exception)
            {
                //Debug.LogError("Could not move all files: " + ex);
            }
            finally
            {
                if(exists)
                    AssetDatabase.Refresh();
            }
#endif
        }

        #endregion
    }
}
// © 2016-2017 crosstales LLC (https://www.crosstales.com)