using UnityEngine;

public class Proyectile : MonoBehaviour
{
    public float speed = 8f;       // Velocidad de la bala
    public float lifetime = 3f;    // Tiempo de vida antes de autodestruirse

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Alex player = other.GetComponent<Alex>();

            if (player != null)
            {
                if (player.isAttacking || player.isPoweredUp)
                {
                    // 🔥 Si el jugador está atacando o con power-up, destruye la bala
                    Destroy(gameObject);
                }
                else
                {
                    // 💀 Usar sistema de vidas y corazones
                    if (!player.isInvulnerable)
                        player.TakeDamage(1, transform.position);

                    Destroy(gameObject);
                }
            }
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            // Destruir la bala si choca con el entorno
            Destroy(gameObject);
        }
    }
}
