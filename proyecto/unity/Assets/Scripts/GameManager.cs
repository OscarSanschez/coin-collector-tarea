using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Flujo del juego:
///  1. Obtiene el user_id de la URL (Application.absoluteURL).
///  2. Trae los datos del usuario con GET /game/{user_id}.
///  3. Muestra y actualiza los puntos mientras se juega.
///  4. Guarda el puntaje con PUT /game/{user_id} al pisar la plataforma
///     de guardado, presionar G o pulsar el botón "Guardar".
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referencias")]
    public ApiClient api;
    public Text playerText;
    public Text pointsText;
    public Text savedText;
    public Text statusText;

    [Header("Pruebas en el Editor")]
    [Tooltip("En el Editor no hay URL; se usa este ID para probar.")]
    public int editorUserId = 1;

    public int UserId { get; private set; } = -1;
    public int Points { get; private set; }
    public bool Ready { get; private set; }

    int savedPoints;
    bool saving;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UserId = UrlParams.GetUserId();
#if UNITY_EDITOR
        if (UserId < 0) UserId = editorUserId;
#endif
        if (UserId < 0)
        {
            SetStatus("No se encontró user_id en la URL. Inicia sesión desde la página web.");
            return;
        }

        SetStatus($"Cargando usuario {UserId}...");
        StartCoroutine(api.GetUser(UserId, OnUserLoaded, err => SetStatus("Error al cargar: " + err)));
    }

    void OnUserLoaded(UserData user)
    {
        Points = user.points;
        savedPoints = user.points;
        Ready = true;
        if (playerText) playerText.text = $"Jugador: {user.name} (ID {user.id})";
        RefreshUI();
        SetStatus("¡Listo! Recoge monedas. Guarda en la plataforma verde o con G.");
    }

    void Update()
    {
        if (SavePressed()) SaveScore();
    }

    bool SavePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.G);
#endif
    }

    public void AddPoints(int amount)
    {
        if (!Ready) return;
        Points += amount;
        RefreshUI();
    }

    /// <summary>Guarda el puntaje actual en la base de datos mediante la API.</summary>
    public void SaveScore()
    {
        if (!Ready || saving) return;
        if (Points == savedPoints)
        {
            SetStatus("No hay cambios que guardar.");
            return;
        }

        saving = true;
        SetStatus("Guardando...");
        StartCoroutine(api.SavePoints(UserId, Points,
            user =>
            {
                saving = false;
                savedPoints = user.points;
                RefreshUI();
                SetStatus($"Puntaje guardado: {user.points}");
            },
            err =>
            {
                saving = false;
                SetStatus("Error al guardar: " + err);
            }));
    }

    void RefreshUI()
    {
        if (pointsText) pointsText.text = $"Puntos: {Points}";
        if (savedText) savedText.text = $"Guardado en BD: {savedPoints}" + (Points != savedPoints ? "  (sin guardar)" : "");
    }

    void SetStatus(string msg)
    {
        Debug.Log("[Game] " + msg);
        if (statusText) statusText.text = msg;
    }
}
