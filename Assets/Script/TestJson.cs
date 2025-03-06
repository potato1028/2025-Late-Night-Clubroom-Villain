using UnityEngine;
using Newtonsoft.Json.Linq;

public class TestJson : MonoBehaviour
{
    void Start()
    {
        string jsonString = "{\"name\": \"Unity\", \"version\": \"2025\"}";
        JObject jsonObj = JObject.Parse(jsonString);

        Debug.Log("📌 JSON 데이터: " + jsonObj.ToString());
        Debug.Log("📌 이름: " + jsonObj["name"]);
        Debug.Log("📌 버전: " + jsonObj["version"]);
    }
}