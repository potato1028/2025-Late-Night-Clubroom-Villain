using System;
using UnityEngine;

public class WorldTime : MonoBehaviour {
    private float timer = 0f;

    void Update() {
        timer += Time.deltaTime;
        if(timer >= 1f) {
            timer = 0f;
            DateTime koreanTime = GetKoreanTimeCrossPlatform();
            Debug.Log("현재 한국 시간 : " + koreanTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

    public static DateTime GetKoreanTimeCrossPlatform() {
        try {
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time"));
            #else
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"));
            #endif
        }
        catch {
            return DateTime.UtcNow.AddHours(9);
        }
    }
}