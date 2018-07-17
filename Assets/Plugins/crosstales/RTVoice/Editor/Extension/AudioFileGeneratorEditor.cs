using UnityEngine;
using UnityEditor;
using Crosstales.RTVoice.EditorUtil;

namespace Crosstales.RTVoice.EditorExtension
{
    /// <summary>Custom editor for the 'SpeechText'-class.</summary>
    [CustomEditor(typeof(Tool.AudioFileGenerator))]
    [CanEditMultipleObjects]
    public class AudioFileGeneratorEditor : Editor
    {

        #region Variables

        private Tool.AudioFileGenerator script;

        #endregion


        #region Editor methods

        public void OnEnable()
        {
            script = (Tool.AudioFileGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorHelper.SeparatorUI();

            if (script.isActiveAndEnabled)
            {
                if (script.TextFiles != null && script.TextFiles.Length > 0)
                {
                    if (Speaker.isTTSAvailable && EditorHelper.isRTVoiceInScene)
                    {
                        GUILayout.Label("Generate Audio Files", EditorStyles.boldLabel);

                        if (Util.Helper.isEditorMode)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                if (Speaker.isMaryMode)
                                {
                                    EditorGUILayout.HelpBox("Generate is not supported for MaryTTS inside the Editor.", MessageType.Info);
                                }
                                else
                                {
                                    if (GUILayout.Button(new GUIContent(" Generate", EditorHelper.Icon_Speak, "Generates the speeches from the text files.")))
                                    {
                                        script.Generate();
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();

                            /*
                            EditorHelper.SeparatorUI();

                            GUILayout.Label("Editor", EditorStyles.boldLabel);

                            if (GUILayout.Button(new GUIContent(" Refresh AssetDatabase", EditorHelper.Icon_Refresh, "Refresh the AssetDatabase from the Editor.")))
                            {
                                refreshAssetDatabase();
                            }
                            */
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Disabled in Play-mode!", MessageType.Info);
                        }
                    }
                    else
                    {
                        EditorHelper.NoVoicesUI();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Please add an entry to 'Text Files'!", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Script is disabled!", MessageType.Info);
            }
        }

        #endregion

        /*
        #region Private methods

        private void refreshAssetDatabase()
        {
            if (Util.Helper.isEditorMode)
            {
                AssetDatabase.Refresh();
            }
        }

        #endregion
    */
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)