using UnityEngine;
using UnityEditor;

namespace Crosstales.Common.EditorTask
{
    /// <summary>Checks if 'Online Check' is installed.</summary>
    [InitializeOnLoad]
    public static class OCCheck
    {
        private const string KEY_OCCHECK_DATE = "CT_CFG_OCCHECK_DATE";

        #region Constructor

        static OCCheck()
        {
            string lastDate = EditorPrefs.GetString(KEY_OCCHECK_DATE);
            string date = System.DateTime.Now.ToString("yyyyMMdd"); // every day
            //string date = System.DateTime.Now.ToString("yyyyMMddHH"); // every hour
            //string date = System.DateTime.Now.ToString("yyyyMMddHHmm"); // every minute (for tests)

            if (!date.Equals(lastDate))
            {
#if !CT_OC
                Debug.LogWarning("+++ Could not reliable test the Internet availability. Please consider using 'Online Check': https://goo.gl/prBB6H +++");
#endif

                EditorPrefs.SetString(KEY_OCCHECK_DATE, date);
            }
        }

        #endregion

    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)