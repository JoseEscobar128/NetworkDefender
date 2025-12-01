using UnityEngine;

public class PowerUp : MonoBehaviour
{


    public float duration = 5f; // Duración del poder en segundos

    [Header("Audio")]
    public AudioSource effectsSource;
    public AudioClip collectSound;

    void Start()
    {
        
        if (effectsSource == null)
        {
            effectsSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Alex player = collision.GetComponent<Alex>();
        if (player != null)
        {
            // Reproducir sonido
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, Camera.main ? Camera.main.transform.position : transform.position);

            // Activar power-up
            player.ActivatePowerUp(duration);

            // Destruir objeto
            Destroy(gameObject);
        }
    }

}