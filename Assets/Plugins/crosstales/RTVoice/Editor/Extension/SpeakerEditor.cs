using UnityEngine;
using UnityEditor;
using Crosstales.RTVoice.EditorUtil;

namespace Crosstales.RTVoice.EditorExtension
{
    /// <summary>Custom editor for the 'Speaker'-class.</summary>
    [InitializeOnLoad]
    [CustomEditor(typeof(Speaker))]
    public class SpeakerEditor : Editor
    {

        #region Variables

        private int voiceIndex;
        private float rate = 1f;
        private float pitch = 1f;
        private float volume = 1f;
        private Speaker script;

        private bool showVoices = false;

        private bool maryTTSMode;
        private string maryTTSUrl;
        private int maryTTSPort;
        private string maryTTSUser;
        private string maryTTSPassword;
        private Model.Enum.MaryTTSType maryTTSType;
        private bool autoClearTags;
        private bool silenceOnDisable;
        private bool silenceOnFocusLost;
        private bool dontDestroy;

        #endregion


        #region Static constructor

        static SpeakerEditor()
        {
            //EditorApplication.update += onEditorUpdate;
            EditorApplication.hierarchyWindowItemOnGUI += hierarchyItemCB;
        }

        #endregion


        #region Editor methods

        public void OnEnable()
        {
            script = (Speaker)target;

            maryTTSMode = script.MaryTTSMode;
            maryTTSUrl = script.MaryTTSUrl;
            maryTTSPort = script.MaryTTSPort;
            maryTTSUser = script.MaryTTSUser;
            maryTTSPassword = script.MaryTTSPassword;
        }

        public void OnDisable()
        {
            if (Util.Helper.isEditorMode)
            {
                Speaker.Silence();
            }
        }

        //      public override bool RequiresConstantRepaint()
        //      {
        //          return true;
        //      }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //DrawDefaultInspector();

            GUILayout.Label("MaryTTS", EditorStyles.boldLabel);

            maryTTSMode = EditorGUILayout.BeginToggleGroup(new GUIContent("MaryTTS Mode", "Enables or disables MaryTTS (default: false)."), script.MaryTTSMode);
            if (maryTTSMode != script.MaryTTSMode)
            {
                serializedObject.FindProperty("MaryTTSMode").boolValue = maryTTSMode;
                Speaker.isMaryMode = maryTTSMode;
                Speaker.ReloadProvider();
            }

            EditorGUI.indentLevel++;

            maryTTSUrl = EditorGUILayout.TextField(new GUIContent("MaryTTS URL", "Server URL for MaryTTS."), script.MaryTTSUrl);
            if (!maryTTSUrl.Equals(script.MaryTTSUrl))
            {
                serializedObject.FindProperty("MaryTTSUrl").stringValue = maryTTSUrl;
            }

            maryTTSPort = EditorGUILayout.IntSlider("MaryTTS Port", script.MaryTTSPort, 0, 65535);
            if (maryTTSPort != script.MaryTTSPort)
            {
                serializedObject.FindProperty("MaryTTSPort").intValue = maryTTSPort;
            }

            maryTTSUser = EditorGUILayout.TextField(new GUIContent("MaryTTS User", "User name for MaryTTS (default: empty)."), script.MaryTTSUser);
            if (!maryTTSUser.Equals(script.MaryTTSUser))
            {
                serializedObject.FindProperty("MaryTTSUser").stringValue = maryTTSUser;
            }

            maryTTSPassword = EditorGUILayout.PasswordField(new GUIContent("MaryTTS Password", "User password for MaryTTS (default: empty)."), script.MaryTTSPassword);
            if (!maryTTSPassword.Equals(script.MaryTTSPassword))
            {
                serializedObject.FindProperty("MaryTTSPassword").stringValue = maryTTSPassword;
            }

            maryTTSType = (Model.Enum.MaryTTSType)EditorGUILayout.EnumPopup(new GUIContent("MaryTTS Type", "Input type for MaryTTS (default: MaryTTSType.RAWMARYXML)."), script.MaryTTSType);
            if (maryTTSType != script.MaryTTSType)
            {
                serializedObject.FindProperty("MaryTTSType").enumValueIndex = (int)maryTTSType;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndToggleGroup();

            GUILayout.Space(8);
            GUILayout.Label("Advanced Settings", EditorStyles.boldLabel);

            autoClearTags = EditorGUILayout.Toggle(new GUIContent("Auto Clear Tags", "Automatically clear tags from speeches depending on the capabilities of the current TTS-system (default: false)."), script.AutoClearTags);
            if (autoClearTags != script.AutoClearTags)
            {
                serializedObject.FindProperty("AutoClearTags").boolValue = autoClearTags;
            }

            GUILayout.Space(8);
            GUILayout.Label("Behaviour Settings", EditorStyles.boldLabel);
            
            silenceOnDisable = EditorGUILayout.Toggle(new GUIContent("Silence On Disable", "Silence any speeches if this component gets disabled (default: false)."), script.SilenceOnDisable);
            if (silenceOnDisable != script.SilenceOnDisable)
            {
                serializedObject.FindProperty("SilenceOnDisable").boolValue = silenceOnDisable;
            }

            silenceOnFocusLost = EditorGUILayout.Toggle(new GUIContent("Silence On Focus Lost", "Silence any speeches if the application loses the focus (default: true)."), script.SilenceOnFocustLost);
            if (silenceOnFocusLost != script.SilenceOnFocustLost)
            {
                serializedObject.FindProperty("SilenceOnFocustLost").boolValue = silenceOnFocusLost;
            }

            dontDestroy = EditorGUILayout.Toggle(new GUIContent("Dont Destroy", "Don't destroy gameobject during scene switches (default: true)."), script.DontDestroy);
            if (dontDestroy != script.DontDestroy)
            {
                serializedObject.FindProperty("DontDestroy").boolValue = dontDestroy;
            }
            
            GUILayout.Space(8);

            if (GUILayout.Button(new GUIContent(" Reload", EditorHelper.Icon_Refresh, "Reload the provider.")))
            {
                Speaker.ReloadProvider();
                GAApi.Event(typeof(SpeakerEditor).Name, "Reload the provider");
            }

            EditorHelper.SeparatorUI();

            if (script.isActiveAndEnabled)
            {
                GUILayout.Label("Data", EditorStyles.boldLabel);

                showVoices = EditorGUILayout.Foldout(showVoices, "Voices (" + Speaker.Voices.Count + ")");
                if (showVoices)
                {
                    EditorGUI.indentLevel++;

                    foreach (string voice in Speaker.Voices.CTToString())
                    {
                        EditorGUILayout.SelectableLabel(voice, GUILayout.Height(16), GUILayout.ExpandHeight(false));
                    }

                    EditorGUI.indentLevel--;
                }

                EditorHelper.SeparatorUI();

                if (Speaker.Voices.Count > 0)
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
                            voiceIndex = EditorGUILayout.Popup("Voice", voiceIndex, Speaker.Voices.CTToString().ToArray());
                            rate = EditorGUILayout.Slider("Rate", rate, 0f, 3f);

                            if (Util.Helper.isWindowsPlatform)
                            {
                                pitch = EditorGUILayout.Slider("Pitch", pitch, 0f, 2f);

                                volume = EditorGUILayout.Slider("Volume", volume, 0f, 1f);
                            }

                            GUILayout.Space(8);

                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Button(new GUIContent(" Speak", EditorHelper.Icon_Speak, "Speaks the text with the selected voice and settings.")))
                                {
                                    Speaker.SpeakNativeInEditor("You have selected " + Speaker.Voices[voiceIndex].Name, Speaker.Voices[voiceIndex], rate, pitch, volume);
                                    GAApi.Event(typeof(SpeakerEditor).Name, "Speak");
                                }

                                GUI.enabled = Speaker.isSpeaking;

                                if (GUILayout.Button(new GUIContent(" Silence", EditorHelper.Icon_Silence, "Silence all active speakers.")))
                                {
                                    Speaker.Silence();
                                    GAApi.Event(typeof(SpeakerEditor).Name, "Silence");
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
                    EditorGUILayout.HelpBox("TTS with the current settings is not possible!", MessageType.Error);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Script is disabled!", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion


        #region Private methods

        private static void hierarchyItemCB(int instanceID, Rect selectionRect)
        {
            if (EditorConfig.HIERARCHY_ICON)
            {
                GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

                if (go != null && go.GetComponent<Speaker>())
                {
                    Rect r = new Rect(selectionRect);
                    r.x = r.width - 4;

                    //Debug.Log("HierarchyItemCB: " + r);

                    GUI.Label(r, EditorHelper.Logo_Asset_Small);
                }
            }
        }

        #endregion

    }
}
// © 2016-2017 crosstales LLC (https://www.crosstales.com)