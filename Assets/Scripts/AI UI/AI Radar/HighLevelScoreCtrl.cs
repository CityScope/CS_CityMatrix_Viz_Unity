using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class HighLevelScoreCtrl : MonoBehaviour
{
    //public GameObject orange;
    public CityMatrixRadarChart CMRadarChart;

    public Text innovationPotentialScore;
    public Text resourceEfficiencyScore;
    public Text totalScore;
    public Text innovationPotentialScoreSuggested;
    public Text resourceEfficiencyScoreSuggested;
    public GameObject currentScores;
    public GameObject suggestedScores;
    public GameObject arrows;
    public bool showAISuggestion;
    public Text weight1;
    public Text weight2;

    void Update()
    {
        // visibility control
        if (this.showAISuggestion)
        {
            //currentScores.transform.localPosition = new Vector3(-40.0f, 0.0f, 0.0f);
            suggestedScores.SetActive(true);
            //arrows.SetActive(true);
        }
        else
        {
            //currentScores.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            suggestedScores.SetActive(false);
            //arrows.SetActive(false);
        }

        // current
        float IPScore = (CMRadarChart.metrics[0] + CMRadarChart.metrics[1]) / 2.0f * 100.0f;
        innovationPotentialScore.text = string.Format("{0:0}", IPScore);

        float REScore = (CMRadarChart.metrics[2] + CMRadarChart.metrics[3]
                         + CMRadarChart.metrics[4]) / 3.0f * 100.0f;
        resourceEfficiencyScore.text = string.Format("{0:0}", REScore);

        float w1 = weight1.text == "" ? 0 : float.Parse(weight1.text);
        float w2 = weight2.text == "" ? 0 : float.Parse(weight2.text);
        float tScore = IPScore * w1 / 100.0f + REScore * w2 / 100.0f;
        totalScore.text = string.Format("{0:0}", tScore);

        // suggested
        float IPScoreSuggested = (CMRadarChart.metricsSuggested[0] + CMRadarChart.metricsSuggested[1]) / 2.0f * 100.0f;
        float IPDelta = IPScoreSuggested - IPScore;
        string IPDeltaStr;
        if (IPDelta >= 0.0f)
        {
            IPDeltaStr = "+" + string.Format("{0:0}", IPDelta);
        }
        else
        {
            IPDeltaStr = "-" + string.Format("{0:0}", -IPDelta);
        }
        innovationPotentialScoreSuggested.text = IPDeltaStr;

        float REScoreSuggested = (CMRadarChart.metricsSuggested[2] + CMRadarChart.metricsSuggested[3]
                                  + CMRadarChart.metricsSuggested[4]) / 3.0f * 100.0f;
        float REDelta = REScoreSuggested - REScore;
        string REDeltaStr;
        if (REDelta >= 0.0f)
        {
            REDeltaStr = "+" + string.Format("{0:0}", REDelta);
        }
        else
        {
            REDeltaStr = "-" + string.Format("{0:0}", -REDelta);
        }
        resourceEfficiencyScoreSuggested.text = REDeltaStr;
    }
}