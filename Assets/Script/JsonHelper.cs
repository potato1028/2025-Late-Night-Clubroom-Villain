using System;
using UnityEngine;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"positions\": " + json + " }"; // JSON 배열을 감싸는 래퍼 추가
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.positions;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] positions;
    }
}