using UnityEngine;

public class DayNightController : MonoBehaviour
{
    [Header("Scene Lighting")]
    public Light sun;
    public Light[] lampLights;

    [Header("Lighting Presets")]
    public float dayIntensity = 1f;
    public float nightIntensity = 0.05f;
    [SerializeField] private float dayAmbientIntensity = 1f;
    [SerializeField] private float nightAmbientIntensity = 0.12f;
    [SerializeField, Min(0.1f)] private float transitionDuration = 1.25f;

    public bool IsNight { get; private set; }

    private float targetSunIntensity;
    private float targetAmbientIntensity;

    private void Start()
    {
        if (dayAmbientIntensity <= 0f)
        {
            dayAmbientIntensity = 1f;
        }

        if (sun == null)
        {
            sun = RenderSettings.sun;
        }

        SetLighting(false, true);
    }

    private void Update()
    {
        if (!UIManager.IsPaused && Input.GetKeyDown(KeyCode.N))
        {
            SetLighting(!IsNight, false);
        }

        float speed = 1f / Mathf.Max(0.1f, transitionDuration);
        if (sun != null)
        {
            sun.intensity = Mathf.MoveTowards(sun.intensity, targetSunIntensity, speed * Time.deltaTime);
        }

        RenderSettings.ambientIntensity = Mathf.MoveTowards(
            RenderSettings.ambientIntensity,
            targetAmbientIntensity,
            speed * Time.deltaTime);
    }

    public void SetDay()
    {
        SetLighting(false, false);
    }

    public void SetNight()
    {
        SetLighting(true, false);
    }

    private void SetLighting(bool night, bool immediate)
    {
        IsNight = night;
        targetSunIntensity = night ? nightIntensity : dayIntensity;
        targetAmbientIntensity = night ? nightAmbientIntensity : dayAmbientIntensity;

        if (lampLights != null)
        {
            foreach (Light lamp in lampLights)
            {
                if (lamp != null)
                {
                    lamp.enabled = night;
                }
            }
        }

        if (immediate)
        {
            if (sun != null)
            {
                sun.intensity = targetSunIntensity;
            }

            RenderSettings.ambientIntensity = targetAmbientIntensity;
        }

        if (!immediate && UIManager.Instance != null)
        {
            UIManager.Instance.ShowToast(night ? "Night mode  •  Park lights on" : "Day mode  •  Park lights off");
        }
    }
}
