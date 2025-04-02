using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class WeatherManager : MonoBehaviour
{
    public string apiKey = "8cf84a776067a09b71171f342ef00efa"; // 🔐 여기에 너의 API 키 넣기

    void Start()
    {
        StartCoroutine(GetLocationAndWeather());
    }

    IEnumerator GetLocationAndWeather()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("위치 서비스가 꺼져있음");
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
            Debug.LogError("위치 정보를 가져올 수 없음");
            yield break;
        }

        float latitude = Input.location.lastData.latitude;
        float longitude = Input.location.lastData.longitude;

        Input.location.Stop();

        yield return StartCoroutine(GetWeather(latitude, longitude));
    }

    IEnumerator GetWeather(float lat, float lon)
    {
        string url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={apiKey}&units=metric&lang=kr";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("날씨 요청 실패: " + request.error);
            }
            else
            {
                string json = request.downloadHandler.text;
                Debug.Log("날씨 응답: " + json);

                WeatherResponse weather = JsonUtility.FromJson<WeatherResponse>(json);
                Debug.Log($"🌤️ 날씨: {weather.weather[0].description}, 🌡️ {weather.main.temp}°C, 📍 {weather.name}");
            }
        }
    }

    // JSON 파싱용 클래스
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