using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private Light sunlight;
    [SerializeField] private float cycleTime = 5f;
    [SerializeField] private float sunsetStartAngle = 15f;
    [SerializeField] private float sunsetIntensity = 4f;
    [SerializeField] private Color noonColor = Color.white;
    [SerializeField] private Color midnightColor = Color.black;

    [SerializeField] private Color dayFog = Color.white;
    [SerializeField] private float dayFogDensity = 0;
    [SerializeField] private float nightFogDensity = 0.05f;

    void Update()
    {
        // rotate around the sun’s up 
        // 360/ cycle time is the degrees per second 
        sunlight.transform.Rotate(sunlight.transform.up, 360 / cycleTime * Time.deltaTime, Space.World);
        
        // project to the plane 
        Vector3 plane = new Vector3(sunlight.transform.forward.x, 0, sunlight.transform.forward.z);

        // angle to the plane 
        float theta = Vector3.Angle(sunlight.transform.forward, plane);

        // fix the sign 
        if (plane.y < sunlight.transform.forward.y)
            theta = -theta;

        float percentage = (theta) / (sunsetStartAngle);
        float intensity = Mathf.Lerp(sunsetIntensity, 1, percentage);
        RenderSettings.skybox.SetFloat("_AtmosphereThickness", intensity);

        Color sunBrightness = Color.Lerp(midnightColor, noonColor, percentage);
        sunlight.color = sunBrightness;

        Color fogColor = Color.Lerp(midnightColor, dayFog, percentage);
        RenderSettings.fogColor = fogColor;
        float distance = Mathf.Lerp(nightFogDensity, dayFogDensity, percentage);
        RenderSettings.fogDensity = distance;
    }

    void OnApplicationQuit()
    {
        RenderSettings.skybox.SetFloat("_AtmosphereThickness", 1);
    }
}
