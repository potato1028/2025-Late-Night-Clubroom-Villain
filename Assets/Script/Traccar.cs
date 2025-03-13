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

public class Traccar : MonoBehaviour {
    private string serverUrl = "http://172.16.112.98:8082/api/positions"; // Traccar API URL
    private string username = "gamja";  // Traccar 사용자 이름
    private string password = "admin";  // Traccar 비밀번호

    public float latitude;
    public float longitude;

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
                latitude = positions[0].latitude;
                longitude = positions[0].longitude;
                Debug.Log($"📍 현재 위치: 위도 {latitude}, 경도 {longitude}");
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

        yield return new WaitForSeconds(5f);
        StartCoroutine(CheckTraccarConnection()); // 5초마다 갱신
    }
}