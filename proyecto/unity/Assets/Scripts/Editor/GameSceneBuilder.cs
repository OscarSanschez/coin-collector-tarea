#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menú: Coin Collector > Crear escena del juego
/// Arma automáticamente la escena completa (cámara, arena, jugador, monedas,
/// punto de guardado, UI y GameManager), la guarda en Assets/Scenes/Game.unity,
/// la agrega a Build Settings y configura WebGL para servirse en local.
/// </summary>
public static class GameSceneBuilder
{
    const string GenDir = "Assets/Generated";
    const string SceneDir = "Assets/Scenes";
    const string ScenePath = SceneDir + "/Game.unity";

    [MenuItem("Coin Collector/Crear escena del juego")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Directory.CreateDirectory(GenDir);
        Directory.CreateDirectory(SceneDir);
        Sprite square = MakeSprite("square", false);
        Sprite circle = MakeSprite("circle", true);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---------- cámara ----------
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color32(18, 24, 38, 255);
        camGo.transform.position = new Vector3(0, 0, -10);

        // ---------- arena (paredes) ----------
        var wallColor = new Color32(60, 74, 104, 255);
        MakeWall("Pared Izq", square, new Vector2(-8f, -0.5f), new Vector2(0.4f, 8.4f), wallColor);
        MakeWall("Pared Der", square, new Vector2(8f, -0.5f), new Vector2(0.4f, 8.4f), wallColor);
        MakeWall("Pared Arriba", square, new Vector2(0f, 3.5f), new Vector2(16.4f, 0.4f), wallColor);
        MakeWall("Pared Abajo", square, new Vector2(0f, -4.5f), new Vector2(16.4f, 0.4f), wallColor);

        // ---------- punto de guardado ----------
        var save = MakeSprite2D("Punto de Guardado", square, new Color32(70, 200, 120, 255), new Vector2(6.6f, -3.6f), new Vector2(1.6f, 1.2f), -1);
        save.AddComponent<BoxCollider2D>().isTrigger = true;
        save.AddComponent<SavePoint>();

        // ---------- jugador ----------
        var player = MakeSprite2D("Jugador", square, new Color32(90, 160, 255, 255), new Vector2(0f, -0.5f), Vector2.one * 0.7f, 2);
        player.tag = "Player";
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        player.AddComponent<BoxCollider2D>();
        player.AddComponent<PlayerController2D>();

        // ---------- monedas ----------
        Vector2[] coinPos =
        {
            new Vector2(-5, 2), new Vector2(-2, -3), new Vector2(3, 2),
            new Vector2(5, -1), new Vector2(-6, -2), new Vector2(1, 0.5f)
        };
        for (int i = 0; i < coinPos.Length; i++)
        {
            var coin = MakeSprite2D($"Moneda {i + 1}", circle, new Color32(255, 204, 51, 255), coinPos[i], Vector2.one * 0.5f, 1);
            coin.AddComponent<CircleCollider2D>().isTrigger = true;
            var c = coin.AddComponent<Coin>();
            c.arenaMin = new Vector2(-7.2f, -3.9f);
            c.arenaMax = new Vector2(7.2f, 2.9f);
        }

        // ---------- UI ----------
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960, 600);
        scaler.matchWidthOrHeight = 0.5f;

        Font font = GetFont();
        Text playerText = MakeText(canvasGo.transform, "Jugador", font, "Jugador: ...", 18, Color.white, new Vector2(16, -8), new Vector2(460, 26), TextAnchor.UpperLeft, true);
        Text pointsText = MakeText(canvasGo.transform, "Puntos", font, "Puntos: 0", 28, new Color32(255, 204, 51, 255), new Vector2(16, -34), new Vector2(460, 36), TextAnchor.UpperLeft, true);
        pointsText.fontStyle = FontStyle.Bold;
        Text savedText = MakeText(canvasGo.transform, "Guardado", font, "Guardado en BD: 0", 16, new Color32(170, 185, 210, 255), new Vector2(16, -72), new Vector2(460, 24), TextAnchor.UpperLeft, true);
        Text statusText = MakeText(canvasGo.transform, "Estado", font, "", 15, new Color32(170, 185, 210, 255), new Vector2(-16, -8), new Vector2(420, 50), TextAnchor.UpperRight, false);

        // botón Guardar
        var btnGo = new GameObject("Boton Guardar", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(canvasGo.transform, false);
        var brt = btnGo.GetComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(1, 1);
        brt.anchoredPosition = new Vector2(-16, -62);
        brt.sizeDelta = new Vector2(170, 36);
        var img = btnGo.GetComponent<Image>();
        img.sprite = square;
        img.color = new Color32(70, 200, 120, 255);
        var btnText = MakeText(btnGo.transform, "Texto", font, "Guardar puntaje (G)", 16, new Color32(10, 30, 15, 255), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, true);
        var trt = btnText.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = trt.offsetMax = Vector2.zero;
        btnText.fontStyle = FontStyle.Bold;

        // EventSystem (para que el botón funcione)
        var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif

        // ---------- GameManager + API ----------
        var gmGo = new GameObject("GameManager");
        var api = gmGo.AddComponent<ApiClient>();
        api.baseUrl = "http://localhost:4000";
        var gm = gmGo.AddComponent<GameManager>();
        gm.api = api;
        gm.playerText = playerText;
        gm.pointsText = pointsText;
        gm.savedText = savedText;
        gm.statusText = statusText;
        UnityEventTools.AddPersistentListener(btnGo.GetComponent<Button>().onClick, gm.SaveScore);

        // ---------- guardar escena y configurar build ----------
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        // Sin compresión: el build se puede servir con cualquier servidor estático.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.runInBackground = true;

        Debug.Log("Escena creada en " + ScenePath + ". Presiona Play para probar (usa GameManager > Editor User Id).");
        EditorUtility.DisplayDialog("Coin Collector",
            "Escena creada en " + ScenePath + ".\n\nPrueba con Play (asegúrate de que la API esté corriendo) y después haz el build WebGL desde File > Build Settings.",
            "OK");
    }

    // ---------------- helpers ----------------
    static GameObject MakeSprite2D(string name, Sprite sprite, Color color, Vector2 pos, Vector2 scale, int order)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(scale.x, scale.y, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        return go;
    }

    static void MakeWall(string name, Sprite sprite, Vector2 pos, Vector2 scale, Color color)
    {
        MakeSprite2D(name, sprite, color, pos, scale, 0).AddComponent<BoxCollider2D>();
    }

    static Text MakeText(Transform parent, string name, Font font, string content, int size, Color color,
                         Vector2 pos, Vector2 sizeDelta, TextAnchor align, bool leftAnchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        Vector2 a = leftAnchor ? new Vector2(0, 1) : new Vector2(1, 1);
        rt.anchorMin = rt.anchorMax = rt.pivot = a;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        var t = go.GetComponent<Text>();
        t.font = font;
        t.text = content;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static Font GetFont()
    {
#if UNITY_2022_2_OR_NEWER
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
    }

    /// <summary>Genera un PNG blanco (cuadrado o círculo) de 64 px = 1 unidad y lo importa como Sprite.</summary>
    static Sprite MakeSprite(string name, bool circle)
    {
        string path = $"{GenDir}/{name}.png";
        if (!File.Exists(path))
        {
            const int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            float r = s / 2f;
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float a = 1f;
                if (circle)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    a = Mathf.Clamp01(r - d);
                }
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != 64)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
#endif
