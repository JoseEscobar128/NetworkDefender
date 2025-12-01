using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class Mapa : MonoBehaviour
{
    public GameObject flag_level1, flag_level2;
    public TextMeshProUGUI texto_level;
    private Animator animator;
    private AudioSource musicSource;
    private AudioSource effectsSource;

    public AudioClip backgroundMusic;
    public AudioClip stepSound;
    public AudioClip enterSound;

    int current_level, selected_level;

    void Start()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = 0.5f;
        musicSource.Play();

        effectsSource = gameObject.AddComponent<AudioSource>();
        animator = GetComponent<Animator>();

        current_level = PlayerPrefs.GetInt("current_level", 0);

        // Mostrar banderas según nivel desbloqueado
        flag_level1.SetActive(true);
        flag_level2.SetActive(current_level >= 1);

        // Posición inicial del selector
        if (current_level == 0)
        {
            selected_level = 1;
            transform.position = new Vector2(-4.3f, 1.5f);
            texto_level.SetText("Level 1");
        }
        else
        {
            selected_level = 2;
            transform.position = new Vector2(-1.6f, 1.5f);
            texto_level.SetText("Level 2");
        }
    }

    void Update()
    {
        bool moved = false;

        if (selected_level == 1)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) && current_level >= 1)
            {
                selected_level = 2;
                transform.position = new Vector2(-1.6f, 1.5f);
                texto_level.SetText("Level 2");
                effectsSource.PlayOneShot(stepSound);
                moved = true;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                PlayerPrefs.SetInt("selected_level", 1);
                StartCoroutine(EnterLevelWithDelay(2f)); // 1 segundo de espera
            }
        }

        else if (selected_level == 2)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selected_level = 1;
                transform.position = new Vector2(-4.3f, 1.5f);
                texto_level.SetText("Level 1");
                effectsSource.PlayOneShot(stepSound);
                moved = true;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                PlayerPrefs.SetInt("selected_level", 2);
                StartCoroutine(EnterLevelWithDelay(2f)); // 1 segundo de espera
            }
        }

        // Activar animación de walk si se movió
        animator.SetBool("Speed", moved);
    }

    IEnumerator EnterLevelWithDelay(float delay)
    {

        yield return new WaitForSeconds(delay);
        effectsSource.PlayOneShot(enterSound);
        gotoEnter();
    }

    public void gotoEnter()
    {
        if (selected_level == 1)
            SceneManager.LoadScene(3);
        else if (selected_level == 2)
            SceneManager.LoadScene(4);
    }
}
