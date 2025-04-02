using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

public class GeoJsonLoader : MonoBehaviour
{
    public string fileName = "seoul_data.geojson"; // GeoJSON 파일
    public GameObject linePrefab; // LineRenderer 프리팹
    public Transform mapParent; // 지도 오브젝트를 포함할 부모
    public GameObject playerMarkerPrefab; // 현재 위치 마커 프리팹
    private GameObject playerMarkerInstance; // 생성된 마커 오브젝트

    public Traccar traccar; // Traccar 스크립트 참조

    [System.Serializable]
    public class PolygonData
    {
        public List<Vector3> points;
    }

    public List<PolygonData> polygons = new List<PolygonData>();

    void Start()
    {
        LoadGeoJson();
        DrawWorldBorders();

        // 플레이어 마커 인스턴스 생성
        if (playerMarkerPrefab != null)
        {
            playerMarkerInstance = Instantiate(playerMarkerPrefab, mapParent);
        }
    }

    void Update()
    {
        if (traccar != null && playerMarkerInstance != null)
        {
            Vector3 playerPosition = LatLonToWorldPosition(traccar.latitude, traccar.longitude);
            playerMarkerInstance.transform.position = playerPosition;
        }
    }

    void LoadGeoJson()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"🚨 GeoJSON 파일을 찾을 수 없습니다: {filePath}");
            return;
        }

        string json = File.ReadAllText(filePath);
        JObject geoJson = JObject.Parse(json);
        JArray features = (JArray)geoJson["features"];

        foreach (JObject feature in features)
        {
            if (feature["geometry"]["type"].ToString() == "Polygon")
            {
                PolygonData polygon = new PolygonData();
                polygon.points = new List<Vector3>();

                JArray coordinates = (JArray)feature["geometry"]["coordinates"][0];

                foreach (JArray coord in coordinates)
                {
                    float lon = (float)coord[0];
                    float lat = (float)coord[1];

                    Vector3 worldPos = LatLonToWorldPosition(lat, lon);
                    polygon.points.Add(worldPos);
                }

                polygons.Add(polygon);
            }
        }

        Debug.Log($"✅ GeoJSON에서 {polygons.Count}개의 폴리곤 데이터를 로드했습니다.");
    }

    void DrawWorldBorders()
    {
        foreach (var polygon in polygons)
        {
            GameObject lineObj = Instantiate(linePrefab, mapParent);
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();

            lineRenderer.positionCount = polygon.points.Count;
            lineRenderer.SetPositions(polygon.points.ToArray());
            lineRenderer.loop = true;
        }
    }

    Vector3 LatLonToWorldPosition(float lat, float lon)
    {
        float minLatitude = 37.4283f;
        float maxLatitude = 37.7011f;
        float minLongitude = 126.7644f;
        float maxLongitude = 127.1836f;

        float latPercent = (lat - minLatitude) / (maxLatitude - minLatitude); // Y축 반전 수정
        float lonPercent = (lon - minLongitude) / (maxLongitude - minLongitude);

        float xPos = (lonPercent - 0.5f) * 100f;
        float yPos = (latPercent - 0.5f) * 100f;

        return new Vector3(xPos, yPos, 0);
    }
}