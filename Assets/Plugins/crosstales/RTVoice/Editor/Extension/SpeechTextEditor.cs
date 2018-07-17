using UnityEngine;
using UnityEditor;
using Crosstales.RTVoice.EditorUtil;

namespace Crosstales.RTVoice.EditorExtension
{
    /// <summary>Custom editor for the 'SpeechText'-class.</summary>
    [CustomEditor(typeof(Tool.SpeechText))]
    [CanEditMultipleObjects]
    public class SpeechTextEditor : Editor
    {

        #region Variables

        private Tool.SpeechText script;

        #endregion


        #region Editor methods

        public void OnEnable()
        {
            script = (Tool.SpeechText)target;
        }

        public void OnDisable()
        {
            if (Util.Helper.isEditorMode)
            {
                Speaker.Silence();
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorHelper.SeparatorUI();

            if (script.isActiveAndEnabled)
            {
                if (!string.IsNullOrEmpty(script.Text))
                {
                    if (Speaker.isTTSAvailable && EditorHelper.isRTVoiceInScene)
                    {
                        GUILayout.Label("Test-Drive", EditorStyles.boldLabel);

                        if (Util.Helper.isEditorMode)
                        {
                            if (Speaker.isMaryMode)
                            {
                                EditorGUILayout.HelpBox("Test-Drive is not supported for MaryTTS.", MessageType.Info);
                            }
                            else
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    if (GUILayout.Button(new GUIContent(" Speak", EditorHelper.Icon_Speak, "Speaks the text with the selected voice and settings.")))
                                    {
                                        script.Speak();
                                    }

                                    GUI.enabled = Speaker.isSpeaking;
                                    if (GUILayout.Button(new GUIContent(" Silence", EditorHelper.Icon_Silence, "Silence the active speaker.")))
                                    {
                                        script.Silence();
                                    }
                                    GUI.enabled = true;
                                }
                                GUILayout.EndHorizontal();
                            }

                            EditorHelper.SeparatorUI();

                            GUILayout.Label("Editor", EditorStyles.boldLabel);

                            if (GUILayout.Button(new GUIContent(" Refresh AssetDatabase", EditorHelper.Icon_Refresh, "Refresh the AssetDatabase from the Editor.")))
                            {
                                refreshAssetDatabase();
                            }
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
                    EditorGUILayout.HelpBox("Please enter a 'Text'!", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Script is disabled!", MessageType.Info);
            }
        }

        #endregion


        #region Private methods

        private void refreshAssetDatabase()
        {
            if (Util.Helper.isEditorMode)
            {
                AssetDatabase.Refresh();
            }
        }

        #endregion
    }
}
// © 2016-2018 crosstales LLC (https://www.crosstales.com)