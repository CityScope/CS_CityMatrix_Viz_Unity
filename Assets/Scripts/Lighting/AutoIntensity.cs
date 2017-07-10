using UnityEngine;
using System.Collections;

// Source: https://unity3d.com/learn/tutorials/topics/graphics/realtime-global-illumination-daynight-cycle
public class AutoIntensity : MonoBehaviour
{
    public Gradient NightDayColor;

    public float MaxIntensity = 3f;
    public float MinIntensity = 0f;
    public float MinPoint = -0.2f;

    public float MaxAmbient = 1f;
    public float MinAmbient = 0f;
    public float MinAmbientPoint = -0.2f;


    public Gradient NightDayFogColor;
    public AnimationCurve FogDensityCurve;
    public float FogScale = 1f;

    public float DayAtmosphereThickness = 0.4f;
    public float NightAtmosphereThickness = 0.87f;

    public Vector3 DayRotateSpeed;
    public Vector3 NightRotateSpeed;

    float _skySpeed = 1;


    Light _mainLight;
    Skybox _sky;
    Material _skyMat;

    void Start()
    {
        _mainLight = GetComponent<Light>();
        _skyMat = RenderSettings.skybox;
    }

    void Update()
    {
        float tRange = 1 - MinPoint;
        float dot = Mathf.Clamp01((Vector3.Dot(_mainLight.transform.forward, Vector3.down) - MinPoint) / tRange);
        float i = ((MaxIntensity - MinIntensity) * dot) + MinIntensity;

        _mainLight.intensity = i;

        tRange = 1 - MinAmbientPoint;
        dot = Mathf.Clamp01((Vector3.Dot(_mainLight.transform.forward, Vector3.down) - MinAmbientPoint) / tRange);
        i = ((MaxAmbient - MinAmbient) * dot) + MinAmbient;
        RenderSettings.ambientIntensity = i;

        _mainLight.color = NightDayColor.Evaluate(dot);
        RenderSettings.ambientLight = _mainLight.color;

        RenderSettings.fogColor = NightDayFogColor.Evaluate(dot);
        RenderSettings.fogDensity = FogDensityCurve.Evaluate(dot) * FogScale;

        i = ((DayAtmosphereThickness - NightAtmosphereThickness) * dot) + NightAtmosphereThickness;
        _skyMat.SetFloat("_AtmosphereThickness", i);

        if (dot > 0)
            transform.Rotate(DayRotateSpeed * Time.deltaTime * _skySpeed);
        else
            transform.Rotate(NightRotateSpeed * Time.deltaTime * _skySpeed);

        if (Input.GetKeyDown(KeyCode.Q)) _skySpeed *= 0.5f;
        if (Input.GetKeyDown(KeyCode.E)) _skySpeed *= 2f;
    }
}