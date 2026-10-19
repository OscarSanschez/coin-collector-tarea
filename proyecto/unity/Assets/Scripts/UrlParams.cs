using System;
using UnityEngine;

/// <summary>
/// Lee parámetros de la URL donde está cargado el juego WebGL.
/// La página web carga el iframe como:  unity/index.html?user_id=5
/// y Application.absoluteURL devuelve exactamente esa URL.
/// test thing
/// </summary>
public static class UrlParams
{
    public static string Get(string key)
    {
        string url = Application.absoluteURL;          // vacío en el Editor
        if (string.IsNullOrEmpty(url)) return null;

        int q = url.IndexOf('?');
        if (q < 0) return null;

        string query = url.Substring(q + 1);
        int hash = query.IndexOf('#');
        if (hash >= 0) query = query.Substring(0, hash);

        foreach (string pair in query.Split('&'))
        {
            string[] kv = pair.Split(new[] { '=' }, 2);
            if (kv.Length == 2 && Uri.UnescapeDataString(kv[0]) == key)
                return Uri.UnescapeDataString(kv[1]);
        }
        return null;
    }

    /// <summary>Devuelve el user_id de la URL o -1 si no existe / no es válido.</summary>
    public static int GetUserId()
    {
        string raw = Get("user_id");
        return int.TryParse(raw, out int id) ? id : -1;
    }
}
