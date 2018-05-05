using UnityEditor;
using Crosstales.RTVoice.EditorUtil;

namespace Crosstales.RTVoice.EditorExtension
{
    /// <summary>Custom editor for the 'VoiceInitalizer'-class.</summary>
    [CustomEditor(typeof(Tool.VoiceInitalizer))]
    public class VoiceInitalizerEditor : Editor
    {
        #region Variables

        private Tool.VoiceInitalizer script;

        #endregion


        #region Editor methods

        public void OnEnable()
        {
            script = (Tool.VoiceInitalizer)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (script.isActiveAndEnabled)
            {
                if (script.AllVoices || (script.VoiceNames != null && script.VoiceNames.Length > 0))
                {
                    if (!Speaker.isTTSAvailable)
                    {
                        EditorHelper.SeparatorUI();
                        EditorHelper.NoVoicesUI();
                    }
                }
                else
                {
                    EditorHelper.SeparatorUI();
                    EditorGUILayout.HelpBox("Please add an entry to 'Voice Names'!", MessageType.Warning);
                }
            }
            else
            {
                EditorHelper.SeparatorUI();
                EditorGUILayout.HelpBox("Script is disabled!", MessageType.Info);
            }
        }

        #endregion
    }
}
// © 2017 crosstales LLC (https://www.crosstales.com)