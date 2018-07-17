using UnityEngine;
using UnityEditor;
using Crosstales.RTVoice.EditorUtil;

namespace Crosstales.RTVoice.EditorTask
{
    /// <summary>Reminds the customer to create an UAS review.</summary>
    [InitializeOnLoad]
    public static class ReminderCheck
    {
        #region Constructor

        static ReminderCheck()
        {
            string lastDate = EditorPrefs.GetString(EditorConstants.KEY_REMINDER_DATE);
            string date = System.DateTime.Now.ToString("yyyyMMdd"); // every day
            //string date = System.DateTime.Now.ToString("yyyyMMddHHmm"); // every minute (for tests)

            if (!date.Equals(lastDate))
            {
                int count = EditorPrefs.GetInt(EditorConstants.KEY_REMINDER_COUNT) + 1;

                if (Util.Constants.DEV_DEBUG)
                    Debug.Log("Current count: " + count);

                //if (count % 1 == 0) // for testing only
                if (count % 13 == 0 && EditorConfig.REMINDER_CHECK)
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Reminder active...");

                    int option = EditorUtility.DisplayDialogComplex(Util.Constants.ASSET_NAME + " - Reminder",
                                "Please don't forget to rate " + Util.Constants.ASSET_NAME + " or even better write a little review – it would be very much appreciated!",
                                "Yes, let's do it!",
                                "Not right now",
                                "Don't ask again!");

                    if (option == 0)
                    {
                        Application.OpenURL(EditorConstants.ASSET_URL);
                        EditorConfig.REMINDER_CHECK = false;

                        Debug.LogWarning("+++ Thank you for rating " + Util.Constants.ASSET_NAME + "! +++");
                    }
                    else if (option == 1)
                    {
                        // do nothing!
                    }
                    else
                    {
                        EditorConfig.REMINDER_CHECK = false;
                    }

                    EditorConfig.Save();
                }
                else
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("No reminder needed.");
                }

                EditorPrefs.SetString(EditorConstants.KEY_REMINDER_DATE, date);
                EditorPrefs.SetInt(EditorConstants.KEY_REMINDER_COUNT, count);
            }
        }

        #endregion

    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)