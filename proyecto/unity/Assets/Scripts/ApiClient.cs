using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class UserData
{
    public int id;
    public string name;
    public int points;
}

[Serializable]
public class PointsPayload
{
    public int points;
}

/// <summary>
/// Cliente de la REST-API.
///   GET  {baseUrl}/game/{user_id}           -> {id, name, points}
///   PUT  {baseUrl}/game/{user_id}  {points} -> {id, name, points}
/// </summary>
public class ApiClient : MonoBehaviour
{
    [Tooltip("URL base de la API (proyecto /api)")]
    public string baseUrl = "http://localhost:4000";

    public IEnumerator GetUser(int userId, Action<UserData> onOk, Action<string> onError)
    {
        string url = $"{baseUrl}/game/{userId}";
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"GET {url} -> {req.responseCode} {req.error}");
                yield break;
            }
            onOk?.Invoke(JsonUtility.FromJson<UserData>(req.downloadHandler.text));
        }
    }

    public IEnumerator SavePoints(int userId, int points, Action<UserData> onOk, Action<string> onError)
    {
        string url = $"{baseUrl}/game/{userId}";
        string json = JsonUtility.ToJson(new PointsPayload { points = points }); // {"points":N}

        using (UnityWebRequest req = new UnityWebRequest(url, "PUT"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"PUT {url} -> {req.responseCode} {req.error}");
                yield break;
            }
            onOk?.Invoke(JsonUtility.FromJson<UserData>(req.downloadHandler.text));
        }
    }
}
