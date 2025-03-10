using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class TraccarPosition {
    public int id;
    public int deviceId;
    public string protocol;
    public string serverTime;
    public string deviceTime;
    public string fixTime;
    public float latitude;
    public float longitude;
    public float altitude;
    public float speed;
    public float course;
}

[System.Serializable]
public class TraccarPositionList {
    public List<TraccarPosition> positions;
}

[System.Serializable]
public class AddressData
{
    public string borough;
}

[System.Serializable]
public class LocationData
{
    public string display_name;
    public AddressData address;
}

public class Traccar : MonoBehaviour {
    private string serverUrl = "http://172.16.110.69:8082/api/positions"; // Traccar API URL
    private string username = "gamja";  // Traccar 사용자 이름
    private string password = "admin";  // Traccar 비밀번호

    private string baseUrl = "https://nominatim.openstreetmap.org/reverse?format=json";

    void Start()
    {
        StartCoroutine(CheckTraccarConnection());
    }

    IEnumerator CheckTraccarConnection()
    {
        string auth = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username + ":" + password));
        UnityWebRequest request = UnityWebRequest.Get(serverUrl);
        request.SetRequestHeader("Authorization", "Basic " + auth);
        request.SetRequestHeader("User-Agent", "UnityTraccarClient");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("✅ Traccar 서버 응답: " + jsonResponse);

            // JSON 데이터 파싱
            TraccarPosition[] positions = JsonHelper.FromJson<TraccarPosition>(jsonResponse);

            if (positions.Length > 0)
            {
                float latitude = positions[0].latitude;
                float longitude = positions[0].longitude;
                Debug.Log($"📍 현재 위치: 위도 {latitude}, 경도 {longitude}");
                GetLocationName(latitude, longitude);
            }
            else
            {
                Debug.LogWarning("🚨 위치 데이터 없음!");
            }
        }
        else
        {
            Debug.LogError("🚨 Traccar 서버 접속 실패! 에러 코드: " + request.responseCode);
            Debug.LogError("에러 메시지: " + request.error);
        }
    }


    public void GetLocationName(float latitude, float longitude) {
        StartCoroutine(GetLocationCoroutine(latitude, longitude));
    }

    IEnumerator GetLocationCoroutine(float latitude, float longitude) {
        string url = $"{baseUrl}&lat={latitude}&lon={longitude}&accept-language=ko"; // 한글 주소 요청 가능
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("User-Agent", "UnityGeocoder"); // User-Agent 설정 필요

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("✅ 위치 데이터 (전체): " + jsonResponse);

            // JSON에서 address.borough 추출
            LocationData location = JsonUtility.FromJson<LocationData>(jsonResponse);

            if (location != null && location.address != null && !string.IsNullOrEmpty(location.address.borough))
            {
                Debug.Log($"📍 현재 구 (Borough): {location.address.borough}");
            }
            else
            {
                Debug.LogWarning("🚨 '구' 정보를 찾을 수 없음! JSON 구조 확인 필요.");
            }
        }
        else
        {
            Debug.LogError("🚨 위치 정보 가져오기 실패! " + request.error);
        }
    }

    string ExtractBorough(string displayName) {
        string[] parts = displayName.Split(' '); // 공백 기준으로 분할

        foreach (string part in parts)
        {
            if (part.EndsWith("구")) // "구"로 끝나는 단어 찾기
            {
                return part;
            }
        }
        return null; // 찾지 못하면 null 반환
    }
}