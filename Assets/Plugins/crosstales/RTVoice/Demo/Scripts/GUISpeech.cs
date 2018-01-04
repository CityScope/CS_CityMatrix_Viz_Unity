using UnityEngine;
using UnityEngine.UI;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Util;

namespace Crosstales.RTVoice.Demo
{
    /// <summary>Simple GUI for runtime TTS with all available OS voices.</summary>
    [HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_g_u_i_speech.html")]
    public class GUISpeech : MonoBehaviour
    {
        #region Variables

        [Header("Settings")]
        public bool StartAsNative = false;
        public GUIMultiAudioFilter AudioFilter;

        [Header("Table")]
        public GameObject ItemPrefab;
        public GameObject Target;
        public Scrollbar Scroll;

        public int ColumnCount = 1;
        public Vector2 SpaceWidth = new Vector2(8, 8);
        public Vector2 SpaceHeight = new Vector2(8, 8);

        [Header("UI Objects")]
        public InputField Input;
        public InputField Culture;
        public Text Cultures;
        public Toggle MaryToogle;
        public Text Voices;

        public static float Rate = 1f;
        public static float Pitch = 1f;
        public static float Volume = 1f;
        public static bool isNative = false;

        private string lastCulture = "unknown";
        private System.Collections.Generic.List<SpeakWrapper> wrappers = new System.Collections.Generic.List<SpeakWrapper>();

        #endregion


        #region MonoBehaviour methods

        public void Start()
        {
            // Subscribe event listeners
            Speaker.OnVoicesReady += onVoicesReady;
            Speaker.OnProviderChange += onProviderChange;

            if (Cultures != null)
                Cultures.text = string.Join(", ", Speaker.Cultures.ToArray());

            if (Speaker.isMaryMode || Helper.isWindowsPlatform)
            {
                if (Input != null)
                    Input.text = "Hi there, my name is RT-Voice, your runtime speaker!" + System.Environment.NewLine + "I can now speak with the complete SSML specification <prosody rate=\"-50%\">at half speed</prosody> or <prosody pitch=\"-50%\">50% lower pitched.</prosody>. " + System.Environment.NewLine + "<prosody contour=\"(0%,+20%) (40%,+40%) (60%,+60%) (80%,+80%) (100%,+100%)\">I can talk with rising intonation</prosody> <prosody contour=\"(0%,-20%) (40%,-40%) (60%,-60%) (80%,-80%) (100%,-100%)\">or with falling intonation.</prosody>" + System.Environment.NewLine + "This is <emphasis level=\"strong\">awesome</emphasis>!";
            }
            else
            {
                if (Input != null)
                    Input.text = "Hi there, my name is RT-Voice, your runtime speaker!";
            }

            if (Culture != null)
                Culture.text = string.Empty;

            isNative = StartAsNative;

            if (MaryToogle != null)
            {
                MaryToogle.isOn = Speaker.isMaryMode;
                MaryToogle.interactable = Helper.hasBuiltInTTS;
            }

            if (Voices != null)
                Voices.text = "Voices (" + Speaker.Voices.Count + ")";
        }

        public void Update()
        {
            if (Culture != null && !lastCulture.Equals(Culture.text))
            {
                buildVoicesList();

                lastCulture = Culture.text;
            }

            //            if (Helper.isIOSPlatform && Time.frameCount % 60 == 0 && !buildVoicesListIOS)
            //            {
            //                buildVoicesList();
            //                buildVoicesListIOS = true;
            //            }
        }

        public void OnDestroy()
        {
            // Unsubscribe event listeners
            Speaker.OnVoicesReady -= onVoicesReady;
            Speaker.OnProviderChange -= onProviderChange;

            if (Helper.hasBuiltInTTS && Speaker.isMaryMode)
            {
                Speaker.isMaryMode = false;
            }
        }

        #endregion


        #region Public methods

        public void Silence()
        {
            //foreach (SpeakWrapper wrapper in wrappers)
            //{
            //    if (wrapper.Audio != null)
            //    {
            //        wrapper.Audio.Stop();
            //        wrapper.Audio.clip = null;
            //    }
            //}

            Speaker.Silence();
        }

        public void ChangeRate(float rate)
        {
            Rate = rate;
        }

        public void ChangeVolume(float volume)
        {
            Volume = volume;
        }

        public void ChangePitch(float pitch)
        {
            Pitch = pitch;
        }

        public void ChangeNative(bool native)
        {
            isNative = native;
        }

        public void ChangeMaryTTS(bool maryTTS)
        {
            Speaker.isMaryMode = maryTTS;
        }

        #endregion


        #region Private methods

        private void onVoicesReady()
        {
            if (Cultures != null)
                Cultures.text = string.Join(", ", Speaker.Cultures.ToArray());

            buildVoicesList();

            if (Voices != null)
                Voices.text = "Voices (" + Speaker.Voices.Count + ")";
        }

        private void onProviderChange(string provider)
        {
            clearVoicesList();

            if (Voices != null)
                Voices.text = "Voices";
        }

        private void clearVoicesList()
        {
            wrappers.Clear();

            if (AudioFilter != null)
            {
                AudioFilter.ClearFilters();
            }

            if (Target != null)
            {
                for (int ii = Target.transform.childCount - 1; ii >= 0; ii--)
                {
                    Transform child = Target.transform.GetChild(ii);
                    child.SetParent(null);
                    Destroy(child.gameObject);
                }
            }
        }

        private void buildVoicesList()
        {
            clearVoicesList(); //make sure everything is deleted

            if (Target != null)
            {
                RectTransform containerRectTransform = Target.GetComponent<RectTransform>();
                System.Collections.Generic.List<Voice> items = Speaker.VoicesForCulture(Culture.text);

                if (items.Count == 0)
                {
                    if (!string.IsNullOrEmpty(Culture.text))
                    {
                        Debug.LogWarning("No voices for culture '" + Culture.text + "' found - using the default system voices.");
                    }
                    items = Speaker.Voices;
                }

                if (items.Count > 0)
                {
                    //calculate the width and height of each child item.
                    float width = containerRectTransform.rect.width / ColumnCount - SpaceWidth.x;
                    float height = SpaceHeight.x + SpaceHeight.y;

                    int rowCount = items.Count / ColumnCount;

                    if (rowCount > 0 && items.Count % rowCount > 0)
                    {
                        rowCount++;
                    }

                    //adjust the height of the container so that it will just barely fit all its children
                    float scrollHeight = height * rowCount;
                    containerRectTransform.offsetMin = new Vector2(containerRectTransform.offsetMin.x, -scrollHeight / 2);
                    containerRectTransform.offsetMax = new Vector2(containerRectTransform.offsetMax.x, scrollHeight / 2);

                    int j = 0;
                    for (int ii = 0; ii < items.Count; ii++)
                    {
                        //this is used instead of a double for loop because itemCount may not fit perfectly into the rows/columns
                        if (ii % ColumnCount == 0)
                        {
                            j++;
                        }

                        //create a new item, name it, and set the parent
                        GameObject newItem = Instantiate(ItemPrefab) as GameObject;
                        newItem.name = Target.name + " item at (" + ii + "," + j + ")";
                        newItem.transform.SetParent(Target.transform);
                        newItem.transform.localScale = Vector3.one;

                        if (AudioFilter != null)
                        {
                            AudioFilter.Sources.Add(newItem.GetComponent<AudioSource>());
                            AudioFilter.ReverbFilters.Add(newItem.GetComponent<AudioReverbFilter>());
                            AudioFilter.ChorusFilters.Add(newItem.GetComponent<AudioChorusFilter>());
                            AudioFilter.EchoFilters.Add(newItem.GetComponent<AudioEchoFilter>());
                            AudioFilter.DistortionFilters.Add(newItem.GetComponent<AudioDistortionFilter>());
                            AudioFilter.LowPassFilters.Add(newItem.GetComponent<AudioLowPassFilter>());
                            AudioFilter.HighPassFilters.Add(newItem.GetComponent<AudioHighPassFilter>());
                        }

                        SpeakWrapper wrapper = newItem.GetComponent<SpeakWrapper>();
                        wrapper.SpeakerVoice = items[ii];
                        wrapper.Input = Input;
                        wrapper.Label.text = items[ii].Name;
                        wrappers.Add(wrapper);

                        //move and size the new item
                        RectTransform rectTransform = newItem.GetComponent<RectTransform>();

                        float x = (width + SpaceWidth.x) * (ii % ColumnCount) + SpaceWidth.x;
                        float y = -height * j;

                        rectTransform.offsetMin = new Vector2(x, y);

                        x = rectTransform.offsetMin.x + width;
                        y = rectTransform.offsetMin.y + SpaceHeight.x;

                        rectTransform.offsetMax = new Vector2(x, y);
                    }

                    if (AudioFilter != null)
                    {
                        AudioFilter.ResetFilters();
                    }
                }

                if (Scroll != null)
                    Scroll.value = 1f;
            }
        }

        #endregion
    }
}
// © 2015-2017 crosstales LLC (https://www.crosstales.com)