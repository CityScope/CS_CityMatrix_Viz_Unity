using UnityEngine;
using UnityEngine.UI;
using Crosstales.RTVoice.Model;

namespace Crosstales.RTVoice.Demo
{
    /// <summary>Simple TTS example.</summary>
    [HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_simple.html")]
    public class Simple : MonoBehaviour
    {

        #region Variables

        [Header("Configuration")]
        public AudioSource SourceA;
        public AudioSource SourceB;

        [Range(0f, 3f)]
        public float RateSpeakerA = 1.25f;

        [Range(0f, 3f)]
        public float RateSpeakerB = 1.75f;

        public bool PlayOnStart = false;

        [Header("UI Objects")]
        public Text TextSpeakerA;
        public Text TextSpeakerB;

        public Text PhonemeSpeakerA;
        public Text PhonemeSpeakerB;

        public Text VisemeSpeakerA;
        public Text VisemeSpeakerB;

        private string uidSpeakerA;
        private string uidSpeakerB;

        private string textA = "Text A";
        private string textB = "Text B";

        #endregion


        #region MonoBehaviour methods

        public void Start()
        {
            // Subscribe event listeners
            Speaker.OnSpeakAudioGenerationStart += speakAudioGenerationStartMethod;
            Speaker.OnSpeakAudioGenerationComplete += speakAudioGenerationCompleteMethod;
            Speaker.OnSpeakCurrentWord += speakCurrentWordMethod;
            Speaker.OnSpeakCurrentPhoneme += speakCurrentPhonemeMethod;
            Speaker.OnSpeakCurrentViseme += speakCurrentVisemeMethod;
            Speaker.OnSpeakStart += speakStartMethod;
            Speaker.OnSpeakComplete += speakCompleteMethod;

            if (TextSpeakerA != null)
                textA = TextSpeakerA.text;

            if (TextSpeakerB != null)
                textB = TextSpeakerB.text;

            if (PlayOnStart)
            {
                Play();
            }
        }

        public void OnDestroy()
        {
            // Unsubscribe event listeners
            Speaker.OnSpeakAudioGenerationStart -= speakAudioGenerationStartMethod;
            Speaker.OnSpeakAudioGenerationComplete -= speakAudioGenerationCompleteMethod;
            Speaker.OnSpeakCurrentWord -= speakCurrentWordMethod;
            Speaker.OnSpeakCurrentPhoneme -= speakCurrentPhonemeMethod;
            Speaker.OnSpeakCurrentViseme -= speakCurrentVisemeMethod;
            Speaker.OnSpeakStart -= speakStartMethod;
            Speaker.OnSpeakComplete -= speakCompleteMethod;
        }

        #endregion


        #region Public methods

        public void Play()
        {
            if (TextSpeakerA != null)
                TextSpeakerA.text = textA;

            if (TextSpeakerB != null)
                TextSpeakerB.text = textB;

            //usedGuids.Clear();

            SpeakerA(); //start with speaker A
            //SpeakerB(); //start with speaker B
        }

        public void SpeakerA()
        { //Don't speak the text immediately
            uidSpeakerA = Speaker.Speak(textA, SourceA, Speaker.VoiceForCulture("en"), false, RateSpeakerA);
        }

        public void SpeakerB()
        { //Don't speak the text immediately
            uidSpeakerB = Speaker.Speak(textB, SourceB, Speaker.VoiceForCulture("en", 1), false, RateSpeakerB);
        }

        public void Silence()
        {
            Speaker.Silence();

            if (SourceA != null)
                SourceA.Stop();

            if (SourceB != null)
                SourceB.Stop();

            if (TextSpeakerA != null)
                TextSpeakerA.text = textA;

            if (TextSpeakerB != null)
                TextSpeakerB.text = textB;

            VisemeSpeakerB.text = PhonemeSpeakerB.text = VisemeSpeakerA.text = PhonemeSpeakerA.text = "-";
        }

        #endregion


        #region Callback methods

        private void speakAudioGenerationStartMethod(Wrapper wrapper)
        {
            Debug.Log("speakAudioGenerationStartMethod: " + wrapper);
        }

        private void speakAudioGenerationCompleteMethod(Wrapper wrapper)
        {
            Debug.Log("speakAudioGenerationCompleteMethod: " + wrapper);

            Speaker.SpeakMarkedWordsWithUID(wrapper);
        }

        private void speakStartMethod(Wrapper wrapper)
        {
            if (wrapper.Uid.Equals(uidSpeakerA))
            {
                Debug.Log("Speaker A - Speech start: " + wrapper);
            }
            else if (wrapper.Uid.Equals(uidSpeakerB))
            {
                Debug.Log("Speaker B - Speech start: " + wrapper);
            }
            else
            {
                Debug.LogWarning("Unknown speaker: " + wrapper);
            }
        }

        private void speakCompleteMethod(Wrapper wrapper)
        {
            if (wrapper.Uid.Equals(uidSpeakerA))
            {
                Debug.Log("Speaker A - Speech complete: " + wrapper);

                if (TextSpeakerA != null)
                    TextSpeakerA.text = wrapper.Text;

                if (VisemeSpeakerA != null)
                    VisemeSpeakerA.text = PhonemeSpeakerA.text = "-";

                SpeakerB();
            }
            else if (wrapper.Uid.Equals(uidSpeakerB))
            {
                Debug.Log("Speaker B - Speech complete: " + wrapper);

                if (TextSpeakerB != null)
                    TextSpeakerB.text = wrapper.Text;

                if (VisemeSpeakerB != null)
                    VisemeSpeakerB.text = PhonemeSpeakerB.text = "-";

                SpeakerA();
            }
            else
            {
                Debug.LogWarning("Unknown speaker: " + wrapper);
            }
        }

        private void speakCurrentWordMethod(Model.Wrapper wrapper, string[] speechTextArray, int wordIndex)
        {
            //Debug.Log(speechTextArray [wordIndex]);

            if (wrapper.Uid.Equals(uidSpeakerA))
            {
                if (TextSpeakerA != null)
                    TextSpeakerA.text = RTVoice.Util.Helper.MarkSpokenText(speechTextArray, wordIndex);
            }
            else if (wrapper.Uid.Equals(uidSpeakerB))
            {
                if (TextSpeakerB != null)
                    TextSpeakerB.text = RTVoice.Util.Helper.MarkSpokenText(speechTextArray, wordIndex);
            }
            else
            {
                Debug.LogWarning("Unknown speaker: " + wrapper);
            }
        }

        private void speakCurrentPhonemeMethod(Model.Wrapper wrapper, string phoneme)
        {
            if (wrapper.Uid.Equals(uidSpeakerA))
            {
                if (PhonemeSpeakerA != null)
                    PhonemeSpeakerA.text = phoneme;
            }
            else if (wrapper.Uid.Equals(uidSpeakerB))
            {
                if (PhonemeSpeakerB != null)
                    PhonemeSpeakerB.text = phoneme;
            }
            else
            {
                Debug.LogWarning("Unknown speaker: " + wrapper);
            }
        }

        private void speakCurrentVisemeMethod(Model.Wrapper wrapper, string viseme)
        {
            if (wrapper.Uid.Equals(uidSpeakerA))
            {
                if (VisemeSpeakerA != null)
                    VisemeSpeakerA.text = viseme;
            }
            else if (wrapper.Uid.Equals(uidSpeakerB))
            {
                if (VisemeSpeakerB != null)
                    VisemeSpeakerB.text = viseme;
            }
            else
            {
                Debug.LogWarning("Unknown speaker: " + wrapper);
            }
        }

        #endregion
    }
}
// © 2015-2018 crosstales LLC (https://www.crosstales.com)