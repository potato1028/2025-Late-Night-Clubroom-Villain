using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Collections;

public class TimeAndWeatherUI : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_Text timeText;
    public TMP_Text weatherText;

    [Header("OpenWeatherMap 설정")]
    public string apiKey = "8cf84a776067a09b71171f342ef00efa";

    private float timer = 0f;

    void Start()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // Use hardcoded coordinates (Seoul) for desktop/macOS testing
        StartCoroutine(GetWeather(37.5665f, 126.9780f)); // Seoul
#else
        StartCoroutine(GetLocationAndWeather());
#endif
    }

    void Update()
    {
#if !UNITY_EDITOR
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            timer = 0f;
            DateTime koreanTime = GetKoreanTime();
            timeText.text = "현재 시간: " + koreanTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
#endif
    }

    DateTime GetKoreanTime()
    {
        try
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time"));
#else
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"));
#endif
        }
        catch
        {
            return DateTime.UtcNow.AddHours(9); // Fallback
        }
    }

    IEnumerator GetLocationAndWeather()
    {
        if (!Input.location.isEnabledByUser)
        {
            weatherText.text = "위치 서비스 꺼짐";
            yield break;
        }

        Input.location.Start();
        int maxWait = 20;

        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            weatherText.text = "위치 정보를 가져올 수 없습니다.";
            yield break;
        }

        float lat = Input.location.lastData.latitude;
        float lon = Input.location.lastData.longitude;

        Input.location.Stop();

        yield return StartCoroutine(GetWeather(lat, lon));
    }

    IEnumerator GetWeather(float lat, float lon)
    {
        string url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={apiKey}&units=metric&lang=kr";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                weatherText.text = "날씨 정보를 불러오지 못했습니다.";
            }
            else
            {
                WeatherResponse weather = JsonUtility.FromJson<WeatherResponse>(request.downloadHandler.text);
                weatherText.text = $"현재 위치: {weather.name}\n" +
                                   $"날씨: {weather.weather[0].description}\n" +
                                   $"온도: {weather.main.temp}°C";
            }
        }
    }

    [Serializable]
    public class WeatherResponse
    {
        public Weather[] weather;
        public Main main;
        public string name;
    }

    [Serializable]
    public class Weather
    {
        public string description;
    }

    [Serializable]
    public class Main
    {
        public float temp;
    }
}