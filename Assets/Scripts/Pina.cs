using UnityEngine;

public class Pina : MonoBehaviour
{
    public AudioSource effectsSource;
    public AudioClip collectSound;

    void Start()
    {
        if (effectsSource == null)
            effectsSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detectar si el objeto que toca es Alex
        Alex alex = collision.GetComponent<Alex>();
        if (alex == null) return;

        // Verificar TAG correcto (Player con mayúscula)
        if (!collision.gameObject.CompareTag("Player")) return;

        // Reproducir sonido
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound,
                Camera.main ? Camera.main.transform.position : transform.position);

        // Sumar piñas en Alex
        alex.CollectPina();

        // Destruir piña
        Destroy(gameObject);
    }
}
