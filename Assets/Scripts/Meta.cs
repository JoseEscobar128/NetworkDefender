using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Meta : MonoBehaviour
{
    private AudioSource musicSource;
    public AudioSource effectsSource;

    public AudioClip backgroundMusic;
    public AudioClip goalSound;

    bool goalTriggered = false;

    void Start()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        if (effectsSource == null)
            effectsSource = gameObject.AddComponent<AudioSource>();

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = 0.5f;
        musicSource.Play();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // En vez de usar animación → llamamos directo
        llegoMeta();
    }

    public void llegoMeta()
    {
        if (goalTriggered) return;
        goalTriggered = true;

        int select_level = PlayerPrefs.GetInt("select_level");

        // Nivel 1 → Bonus 1 (ej: escena 7)
        if (select_level == 1)
        {
            StartCoroutine(PlayGoalAndThenLoad(5));
            return;
        }

        // Nivel 2 → Bonus 2 (ej: escena 8)
        if (select_level == 2)
        {
            StartCoroutine(PlayGoalAndThenLoad(8));
            return;
        }

        // Cualquier otro caso → regresar al mapa
        StartCoroutine(PlayGoalAndThenLoad(1));
    }

    IEnumerator PlayGoalAndThenLoad(int sceneIndex)
    {
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        foreach (var s in sources)
        {
            if (s == effectsSource) continue;
            try { s.Stop(); } catch { }
        }

        if (effectsSource != null && goalSound != null)
        {
            effectsSource.PlayOneShot(goalSound);
            yield return new WaitForSeconds(goalSound.length);
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        SceneManager.LoadScene(sceneIndex);
    }
}
