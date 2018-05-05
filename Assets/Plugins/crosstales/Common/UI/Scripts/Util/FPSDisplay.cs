using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.UI.Util
{
    /// <summary>Simple FPS-Counter.</summary>
    public class FPSDisplay : MonoBehaviour
    {
        public Text FPS;

        public int FrameRefresh = 5;

        private float deltaTime = 0f;
        private float elapsedTime = 0f;

        public void Update()
        {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            elapsedTime += Time.deltaTime;

            if (elapsedTime > 1f)
            {
                if (FPS != null && Time.frameCount % FrameRefresh == 0)
                {
                    float msec = deltaTime * 1000f;
                    float fps = 1f / deltaTime;

                    if (fps < 15)
                    {
                        FPS.text = string.Format("<color=red><b>FPS: {0:0.}</b> ({1:0.0} ms)</color>", fps, msec);
                    }
                    else if (fps < 29)
                    {
                        FPS.text = string.Format("<color=orange><b>FPS: {0:0.}</b> ({1:0.0} ms)</color>", fps, msec);
                    }
                    else
                    {
                        FPS.text = string.Format("<color=green><b>FPS: {0:0.}</b> ({1:0.0} ms)</color>", fps, msec);
                    }
                }
            }
            else
            {
                FPS.text = "<i>...calculating <b>FPS</b>...</i>";
            }
        }
    }
}
// © 2017 crosstales LLC (https://www.crosstales.com)