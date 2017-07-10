using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityRadarChartCtrl : MonoBehaviour
{
    public CityObserver CityObserver;

    public CityMatrixRadarChart CityMatrixRadarChart;
    public HighLevelScoreCtrl HighLevelScoreCtrl;
    public AxisScoreCtrl AxisScoreCtrl;


    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (CityObserver.Fresh)
        { var predictCity = CityObserver.LastPacket.predict; 
            var mlMetrics = new float[]
            {
                predictCity.objects.metrics.Density.metric,
                predictCity.objects.metrics.Diversity.metric,
                predictCity.objects.metrics.Energy.metric,
                predictCity.objects.metrics.Traffic.metric,
                predictCity.objects.metrics.Solar.metric
            };
            this.CityMatrixRadarChart.metrics = mlMetrics;

            var showAi = CityObserver.LastPacket.ShowAi;
            this.CityMatrixRadarChart.showAISuggestion = showAi;
            this.HighLevelScoreCtrl.showAISuggestion = showAi;
            this.AxisScoreCtrl.showAISuggestion = showAi;
            
            if (showAi)
            {
                var aiCity = CityObserver.LastPacket.ai;

                var aiMetrics = new float[]
                {
                    aiCity.objects.metrics.Density.metric,
                    aiCity.objects.metrics.Diversity.metric,
                    aiCity.objects.metrics.Energy.metric,
                    aiCity.objects.metrics.Traffic.metric,
                    aiCity.objects.metrics.Solar.metric
                };
                this.CityMatrixRadarChart.metricsSuggested = aiMetrics;
            }
        }
    }
}