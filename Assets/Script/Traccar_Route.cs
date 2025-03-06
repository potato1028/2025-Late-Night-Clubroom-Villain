using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using ExcelDataReader;
using System.Collections.Generic;

public class Traccar_Route : MonoBehaviour
{
    private string serverUrl = "http://172.16.108.81:8082/api/reports/route"; // Traccar API
    private string username = "gamja";  // Traccar 사용자 이름
    private string password = "admin";  // Traccar 비밀번호

    void Start()
    {
        StartCoroutine(DownloadTraccarRoute());
    }

    IEnumerator DownloadTraccarRoute()
    {
        string auth = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username + ":" + password));

        // 요청 URL (디바이스 ID와 시간 범위 설정 필요)
        string requestUrl = $"{serverUrl}?deviceId=2&from=2025-03-06T00:00:00Z&to=2025-03-06T23:59:59Z";

        UnityWebRequest request = UnityWebRequest.Get(requestUrl);
        request.SetRequestHeader("Authorization", "Basic " + auth);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Traccar 엑셀 데이터 다운로드 성공!");

            // 엑셀 데이터 저장 (로컬 파일)
            string filePath = Path.Combine(Application.persistentDataPath, "route.xlsx");
            File.WriteAllBytes(filePath, request.downloadHandler.data);

            Debug.Log($"📂 엑셀 파일 저장 완료: {filePath}");

            // 엑셀 파일에서 경로 데이터 읽기
            ReadExcelData(filePath);
        }
        else
        {
            Debug.LogError($"🚨 Traccar 경로 다운로드 실패! 에러 코드: {request.responseCode}");
            Debug.LogError($"에러 메시지: {request.error}");
        }
    }

    void ReadExcelData(string filePath)
    {
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            // ✅ `.NET Standard 2.1`에서는 CodePagesEncodingProvider가 필요 없음
            // System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                List<string[]> rows = new List<string[]>(); // 데이터를 저장할 리스트

                while (reader.Read()) // `AsDataSet()`을 사용하지 않고 직접 읽기
                {
                    string[] row = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[i] = reader.GetValue(i)?.ToString() ?? "N/A";
                    }
                    rows.Add(row);
                }

                Debug.Log($"📊 엑셀 데이터 로드 완료! 총 {rows.Count} 개의 행");

                // 5번째 줄부터 경로 데이터 시작 (Traccar 보고서 포맷 기준)
                for (int i = 5; i < rows.Count; i++)
                {
                    string time = rows[i][1];
                    string latitude = rows[i][2];
                    string longitude = rows[i][3];
                    string speed = rows[i][4];

                    Debug.Log($"🛰️ 시간: {time}, 위도: {latitude}, 경도: {longitude}, 속도: {speed}");
                }
            }
        }
    }
}