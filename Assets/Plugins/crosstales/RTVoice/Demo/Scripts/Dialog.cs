using UnityEngine;
using System.Collections;
using Crosstales.RTVoice.Model;

namespace Crosstales.RTVoice.Demo
{
    /// <summary>Simple dialog system with TTS voices.</summary>
    [HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_dialog.html")]
    public class Dialog : MonoBehaviour
    {

        #region Variables

        [Header("Configuration")]
        public string CultureA = "en";
        public string CultureB = "en";
        [Range(0f, 3f)]
        public float RateA = 1f;
        [Range(0f, 3f)]
        public float RateB = 1f;

        [Range(0f, 2f)]
        public float PitchA = 1f;
        [Range(0f, 2f)]
        public float PitchB = 1f;

        [Range(0f, 1f)]
        public float VolumeA = 1f;
        [Range(0f, 1f)]
        public float VolumeB = 1f;

        public AudioSource AudioPersonA;
        public AudioSource AudioPersonB;
        public GameObject VisualsA;
        public GameObject VisualsB;

        [Header("Dialogues")]
        public string[] DialogPersonA;
        public string[] DialogPersonB;
        public string CurrentDialogA = string.Empty;
        public string CurrentDialogB = string.Empty;

        public bool Running = false;

        private string uidSpeakerA;
        private string uidSpeakerB;

        private bool playingA = false;
        private bool playingB = false;

        #endregion


        #region MonoBehaviour methods

        public void Start()
        {
            if (VisualsA != null)
                VisualsA.SetActive(false);

            if (VisualsB != null)
                VisualsB.SetActive(false);

            // Subscribe event listeners
            Speaker.OnSpeakStart += speakStartMethod;
            Speaker.OnSpeakComplete += speakCompleteMethod;
        }

        void OnDestroy()
        {
            // Unsubscribe event listeners
            Speaker.OnSpeakStart -= speakStartMethod;
            Speaker.OnSpeakComplete -= speakCompleteMethod;
        }

        #endregion


        #region Public methods

        public IEnumerator DialogSequence()
        {
            if (!Running)
            {
                Running = true;

                playingA = false;
                playingB = false;

                Voice personA = Speaker.VoiceForCulture(CultureA);
                Voice personB = Speaker.VoiceForCulture(CultureB, 1);

                int index = 0;

                while (Running && (DialogPersonA != null && index < DialogPersonA.Length) || (DialogPersonB != null && index < DialogPersonB.Length))
                {

                    //Person A
                    if (VisualsA != null)
                        VisualsA.SetActive(true);

                    if (VisualsB != null)
                        VisualsB.SetActive(false);

                    if (DialogPersonA != null && index < DialogPersonA.Length)
                    {
                        CurrentDialogA = DialogPersonA[index];
                    }

                    uidSpeakerA = Speaker.Speak(CurrentDialogA, AudioPersonA, personA, true, RateA, PitchA, VolumeA);

                    //wait until ready
                    do
                    {
                        yield return null;
                    } while (!playingA && Running);

                    //wait until played
                    do
                    {
                        yield return null;
                    } while (playingA && Running);

                    CurrentDialogA = string.Empty;

                    if (Running)
                    { //ensure it's still running

                        // Person B
                        if (VisualsA != null)
                            VisualsA.SetActive(false);

                        if (VisualsB != null)
                            VisualsB.SetActive(true);

                        if (DialogPersonB != null && index < DialogPersonB.Length)
                        {
                            CurrentDialogB = DialogPersonB[index];
                        }

                        uidSpeakerB = Speaker.Speak(CurrentDialogB, AudioPersonB, personB, true, RateB, PitchB, VolumeB);

                        //wait until ready
                        do
                        {
                            yield return null;
                        } while (!playingB && Running);

                        //wait until played
                        do
                        {
                            yield return null;
                        } while (playingB && Running);

                        CurrentDialogB = string.Empty;
                    }
                    index++;
                }

                if (VisualsA != null)
                    VisualsA.SetActive(false);

                if (VisualsB != null)
                    VisualsB.SetActive(false);

                Running = false;
            }
        }

        #endregion


        #region Callback methods

        private void speakStartMethod(Wrapper wrapper)
        {
            if (wrapper.Uid.Equals(uidSpeakerA))
            {
                Debug.Log("speakStartMethod - Speaker A: " + wrapper);
                playingA = true;
            }
            else if (wrapper.Uid.Equals(uidSpeakerB))
            {
                Debug.Log("speakStartMethod - Speaker B: " + wrapper);
                playingB = true;
            }
            else
            {
                Debug.LogWarning("speakStartMethod - Unknown speaker: " + wrapper);
                //Debug.LogWarning("speakStartMethod - Unknown speaker: " + wrapper.Uid + " - A: " + uidSpeakerA + " - B: " + uidSpeakerB);

                Running = false;
            }
        }

        private void speakCompleteMethod(Wrapper wrapper)
        {
            if (wrapper.Uid.Equals(uidSpeakerA))
            {
                Debug.Log("speakCompleteMethod - Speaker A: " + wrapper);
                playingA = false;
            }
            else if (wrapper.Uid.Equals(uidSpeakerB))
            {
                Debug.Log("speakCompleteMethod - Speaker B: " + wrapper);
                playingB = false;
            }
            else
            {
                Debug.LogWarning("speakCompleteMethod - Unknown speaker: " + wrapper);
                //Debug.LogWarning("speakCompleteMethod - Unknown speaker: " + wrapper.Uid + " - A: " + uidSpeakerA + " - B: " + uidSpeakerB);

                Running = false;
            }
        }

        #endregion
    }
}
// © 2015-2017 crosstales LLC (https://www.crosstales.com)