using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Meta : MonoBehaviour
{

    Animator animator;
    private AudioSource musicSource;
    public AudioSource effectsSource;
    bool goalTriggered = false;

    public AudioClip backgroundMusic;
    public AudioClip goalSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        musicSource = gameObject.AddComponent<AudioSource>();
        if (effectsSource == null)
        {
            effectsSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = 0.5f;
        musicSource.Play();

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        animator.SetTrigger("meta");
    }

    public void llegoMeta()
    {
        if (goalTriggered) return; // prevent duplicate triggers
        goalTriggered = true;

        int select_level = PlayerPrefs.GetInt("select_level");
        int current_level = PlayerPrefs.GetInt("current_level");

        if (select_level == 1 && current_level < 1)
        {
            PlayerPrefs.SetInt("current_level", 1);
            PlayerPrefs.Save();
            // Play goal sound, stop other music and then load bonus level after the sound finishes
            StartCoroutine(PlayGoalAndThenLoad(7));
            return;
        }

        if (select_level == 2 && current_level < 2)
        {
            PlayerPrefs.SetInt("current_level", 2);
        }
        else if (select_level == 3 && current_level < 3)
        {
            PlayerPrefs.SetInt("current_level", 3);
        }
        else if (select_level == 4 && current_level < 4)
        {
            PlayerPrefs.SetInt("current_level", 4);
        }

        PlayerPrefs.Save();
        // For normal levels, play the goal sound then return to map
        StartCoroutine(PlayGoalAndThenLoad(1));
    }

    IEnumerator PlayGoalAndThenLoad(int sceneIndex)
    {
        // Stop other music/audio sources so the goal sound is heard clearly
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        foreach (var s in sources)
        {
            if (s == effectsSource) continue; // keep the effects source so it can play the goal sound
            try { s.Stop(); } catch { }
        }

        if (effectsSource != null && goalSound != null)
        {
            effectsSource.PlayOneShot(goalSound);
            yield return new WaitForSeconds(goalSound.length);
        }
        else
        {
            // small delay to allow any short audio to play or animation to finish
            yield return new WaitForSeconds(0.2f);
        }

        SceneManager.LoadScene(sceneIndex);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
