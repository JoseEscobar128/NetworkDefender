using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{

    private AudioSource musicSource;
    public AudioClip backgroundMusic;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = 0.5f;
        musicSource.Play();
    }

    public void BotonNewGame()
    {
        PlayerPrefs.SetInt("current_level", 0);
        PlayerPrefs.SetInt("num_vidas", 5);
        SceneManager.LoadScene(1);
    }

    public void BotonContinue()
    {
        if (PlayerPrefs.HasKey("current_level"))
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            PlayerPrefs.SetInt("current_level", 0);
            PlayerPrefs.SetInt("num_vidas", 5);
            SceneManager.LoadScene(1);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
