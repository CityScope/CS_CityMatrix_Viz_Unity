using UnityEngine;
using UnityEditor;
using Crosstales.RTVoice.EditorUtil;

namespace Crosstales.RTVoice.EditorIntegration
{
    /// <summary>Editor component for the "Hierarchy"-menu.</summary>
	public class RTVoiceGameObject : MonoBehaviour
    {

        [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/" + Util.Constants.RTVOICE_SCENE_OBJECT_NAME, false, EditorHelper.GO_ID)]
        private static void AddRTVoice()
        {
            EditorHelper.InstantiatePrefab(Util.Constants.RTVOICE_SCENE_OBJECT_NAME);
            GAApi.Event(typeof(RTVoiceGameObject).Name, "Add " + Util.Constants.RTVOICE_SCENE_OBJECT_NAME);
        }

        [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/" + Util.Constants.RTVOICE_SCENE_OBJECT_NAME, true)]
        private static bool AddRTVoiceValidator()
        {
            return !EditorHelper.isRTVoiceInScene;
        }

        [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/AudioFileGenerator", false, EditorHelper.GO_ID + 1)]
        private static void AddAudioFileGenerator()
        {
            EditorHelper.InstantiatePrefab("AudioFileGenerator");
            GAApi.Event(typeof(RTVoiceGameObject).Name, "Add AudioFileGenerator");
        }

        [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/SpeechText", false, EditorHelper.GO_ID + 2)]
        private static void AddSpeechText()
        {
            EditorHelper.InstantiatePrefab("SpeechText");
            GAApi.Event(typeof(RTVoiceGameObject).Name, "Add SpeechText");
        }

        [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/Sequencer", false, EditorHelper.GO_ID + 3)]
        private static void AddSequencer()
        {
            EditorHelper.InstantiatePrefab("Sequencer");
            GAApi.Event(typeof(RTVoiceGameObject).Name, "Add Sequencer");
        }

        [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/TextFileSpeaker", false, EditorHelper.GO_ID + 4)]
        private static void AddTextFileSpeaker()
        {
            EditorHelper.InstantiatePrefab("TextFileSpeaker");
            GAApi.Event(typeof(RTVoiceGameObject).Name, "Add TextFileSpeaker");
        }

        [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/Loudspeaker", false, EditorHelper.GO_ID + 5)]
        private static void AddLoudspeaker()
        {
            EditorHelper.InstantiatePrefab("Loudspeaker");
            GAApi.Event(typeof(RTVoiceGameObject).Name, "Add Loudspeaker");
        }

        [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/VoiceInitalizer", false, EditorHelper.GO_ID + 6)]
        private static void AddVoiceInitalizer()
        {
            EditorHelper.InstantiatePrefab("VoiceInitalizer");
            GAApi.Event(typeof(RTVoiceGameObject).Name, "Add VoiceInitalizer");
        }
    }
}
// © 2017 crosstales LLC (https://www.crosstales.com)
