using UnityEngine;
using UnityEditor;
using Crosstales.RTVoice.EditorUtil;

namespace Crosstales.RTVoice.EditorExtension
{
    /// <summary>Custom editor for the 'TextFileSpeaker'-class.</summary>
    [CustomEditor(typeof(Tool.TextFileSpeaker))]
    [CanEditMultipleObjects]
    public class TextFileSpeakerEditor : Editor
    {

        #region Variables

        private Tool.TextFileSpeaker script;

        #endregion


        #region Editor methods

        public void OnEnable()
        {
            script = (Tool.TextFileSpeaker)target;
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
                if (script.TextFiles != null && script.TextFiles.Length > 0)
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
                                    if (GUILayout.Button(new GUIContent(" Speak", EditorHelper.Icon_Speak, "Speaks a random text file with the selected voice and settings.")))
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
    }
}
// © 2016-2018 crosstales LLC (https://www.crosstales.com)