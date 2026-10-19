using UnityEngine;

/// <summary>Punto de guardado: al pisarlo se guarda el puntaje actual en la BD mediante la API.</summary>
[RequireComponent(typeof(Collider2D))]
public class SavePoint : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && GameManager.Instance != null)
            GameManager.Instance.SaveScore();
    }
}
