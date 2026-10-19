using UnityEngine;

/// <summary>Moneda: suma puntos al tocarla y reaparece en otra posición de la arena.</summary>
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    public int value = 10;
    public Vector2 arenaMin = new Vector2(-7.5f, -3.5f);
    public Vector2 arenaMax = new Vector2(7.5f, 3.5f);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || GameManager.Instance == null || !GameManager.Instance.Ready) return;

        GameManager.Instance.AddPoints(value);
        transform.position = new Vector3(
            Random.Range(arenaMin.x, arenaMax.x),
            Random.Range(arenaMin.y, arenaMax.y),
            0f);
    }

    void Update()
    {
        transform.Rotate(0f, 0f, 90f * Time.deltaTime); // pequeña animación
    }
}
