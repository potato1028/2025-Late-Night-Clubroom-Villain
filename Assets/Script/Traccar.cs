using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TraccarManager : MonoBehaviour
{
    private string serverUrl = "http://172.16.108.81:8082/api/positions"; // Traccar API URL
    private string username = "gamja";  // Traccar 사용자 이름
    private string password = "admin";  // Traccar 비밀번호

    void Start()
    {
        StartCoroutine(CheckTraccarConnection());
    }

    IEnumerator CheckTraccarConnection()
    {
        string auth = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username + ":" + password));
        UnityWebRequest request = UnityWebRequest.Get(serverUrl);
        request.SetRequestHeader("Authorization", "Basic " + auth);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Traccar 서버 응답: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("🚨 Traccar 서버 접속 실패! 에러 코드: " + request.responseCode);
            Debug.LogError("에러 메시지: " + request.error);
        }
    }
}