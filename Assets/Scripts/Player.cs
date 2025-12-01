using Unity.VisualScripting;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    Rigidbody2D rigidbody;
    SpriteRenderer spriterenderer;
    BoxCollider2D boxcollider;
    Animator animator;

    bool control, doublejump, hit, water;

    public LayerMask layermask;

    public static int num_pina, num_vidas, num_corazones=3, num_cherry;

    // Audio Sources
    private AudioSource musicSource;
    public AudioSource effectsSource;

    // Audio Clips
    public AudioClip backgroundMusic;
    public AudioClip jumpSound;
    public AudioClip damageSound;
    public AudioClip collectSound;
    public AudioClip killEnemySound;
    public AudioClip deathSound;

    public TextMeshProUGUI texto_timer, texto_vidas;

    public Image corazon1, corazon2, corazon3;

    float timer;

    int level_timer;
    // Prevent multiple damage events in a short time (e.g. bouncing spikehead)
    bool recentlyDamaged = false;
    public float damageCooldown = 1f; // seconds of invulnerability after taking damage

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        spriterenderer = GetComponent<SpriteRenderer>();
        boxcollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        if (effectsSource == null)
        {
            effectsSource = gameObject.AddComponent<AudioSource>();
        }


        if (PlayerPrefs.GetInt("num_vidas") > 0)
        {
            num_vidas = PlayerPrefs.GetInt("num_vidas");
            texto_vidas.SetText("vidas: " + num_vidas);
        } else {
            num_vidas = 5;
            texto_vidas.SetText("vidas: " + num_vidas);
        }

        control = true;
        doublejump = false;
        hit = false;
        water = false;

        num_pina = 0;
        num_cherry = 0;

        timer = Time.time;

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex == 7)
        {
            level_timer = 20;
        }
        else
        {
            level_timer = 250;
        }
        texto_timer.SetText("" + level_timer);
    }

    // Update is called once per frame
    void Update()
    {
        if (num_pina >= 5 && num_vidas < 5)
        {
            int extraLives = num_pina / 5;
            num_vidas += extraLives;
            num_pina = num_pina % 5;
            texto_vidas.SetText("vidas: " + num_vidas);
            PlayerPrefs.SetInt("num_vidas", num_vidas);
        }
        if (num_cherry >= 5)
        {
            switch(num_corazones)
            {
                case 1:
                    corazon2.enabled = true;
                    num_corazones++;
                    break;
                case 2:
                    corazon3.enabled = true;
                    num_corazones++;
                    break;
            }
            num_cherry = 0;
        }
        //Timer
        if (Time.time > timer)
        {
            level_timer--;
            texto_timer.SetText("" + level_timer);
            timer++;

            if (level_timer == 0)
            {
                int select_level = PlayerPrefs.GetInt("select_level");
                QuitarVida();

             switch (select_level)
                {
                    case 1:
                        SceneManager.LoadScene(3);
                        break;

                    case 2:
                        SceneManager.LoadScene(4);
                        break;

                    case 3:
                        SceneManager.LoadScene(5);
                        break;

                    case 4:
                        SceneManager.LoadScene(6);
                        break;
                }
            }

        }

        //Control
        if (control)
        {
            if (Input.GetKey(KeyCode.A))
            {
                spriterenderer.flipX = true;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    rigidbody.linearVelocity = new Vector2(-8f, rigidbody.linearVelocity.y);
                }
                else
                {
                    rigidbody.linearVelocity = new Vector2(-4f, rigidbody.linearVelocity.y);
                }
            }
            if (Input.GetKey(KeyCode.D))
            {
                spriterenderer.flipX = false;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    rigidbody.linearVelocity = new Vector2(8f, rigidbody.linearVelocity.y);
                }
                else
                {
                    rigidbody.linearVelocity = new Vector2(4f, rigidbody.linearVelocity.y);
                }
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (water)
                {
                    rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 2f);
                }
                else if (isGrounded())
                {
                    doublejump = true;
                    effectsSource.PlayOneShot(jumpSound);
                    rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 20f);
                }
                else if (doublejump)
                {
                    doublejump = false;
                    effectsSource.PlayOneShot(jumpSound);
                    rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 20f);
                }
            }
        }

        //Animator
        if (!hit)
        {
            if (isGrounded() && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            {
                animator.SetInteger("estados", 0);
            }
            else if (isGrounded() && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
            {
                animator.SetInteger("estados", 1);
            }
            else if (rigidbody.linearVelocity.y > 0)
            {
                if (water)
                {
                    animator.SetInteger("estados", 2);
                }
                else if (doublejump)
                {
                    animator.SetInteger("estados", 2);
                }
                else
                {
                    animator.SetInteger("estados", 4);
                }
            }
            else if (rigidbody.linearVelocity.y < 0)
            {
                animator.SetInteger("estados", 3);
            }
        }

    }


    bool isGrounded()
    {
        return Physics2D.BoxCast(boxcollider.bounds.center, boxcollider.bounds.size, 0f, Vector2.down, 0.1f, layermask);
    }

    public void gotoIdle()
    {
        animator.SetInteger("estados", 0);
        hit = false;
        control = true;
    }


    public void enterDestroy()
    {
        if(num_vidas == 0)
        {
            PlayerPrefs.SetInt("select_level", 1);
            PlayerPrefs.SetInt("current_level", 0);
            SceneManager.LoadScene(1);
        } else {
            int select_level = PlayerPrefs.GetInt("select_level");

             switch (select_level)
                {
                    case 1:
                        SceneManager.LoadScene(3);
                        break;

                    case 2:
                        SceneManager.LoadScene(4);
                        break;

                    case 3:
                        SceneManager.LoadScene(5);
                        break;

                    case 4:
                        SceneManager.LoadScene(6);
                        break;
                }
            }
        }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "meta")
        {
            control = false;
            hit = true;
            animator.SetInteger("estados", 0);
        }
        if(collision.gameObject.tag == "destroy")
        {
            control = false;
            QuitarVida();
            animator.SetTrigger("destroy");
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "spikehead")
        {
            collider.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            collider.gameObject.GetComponent<Rigidbody2D>().mass = 20;
            collider.gameObject.GetComponent<Rigidbody2D>().freezeRotation = true;
            collider.gameObject.GetComponent<Rigidbody2D>().gravityScale = 5;
        }

        if (collider.gameObject.tag == "trampoline")
        {
            collider.gameObject.GetComponent<Animator>().SetBool("trampoline", true);
            rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 30f);
        }

        if (collider.gameObject.tag == "fan")
        {
            rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 10f);
        }


        if (collider.gameObject.tag == "water")
        {
            water = true;
            rigidbody.gravityScale = 0.5f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "trampoline")
        {
            collision.gameObject.GetComponent<Animator>().SetBool("trampoline", false);
        }

        if (collision.gameObject.tag == "water")
        {
            water = false;
            rigidbody.gravityScale = 4f;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "plataforma")
        {
            transform.SetParent(collision.gameObject.transform); //este hace que self (player ahorita) se haga hijo del que colisiona (plataforma)
        }

        if (collision.gameObject.tag == "falling")
        {
            collision.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            collision.gameObject.GetComponent<Rigidbody2D>().freezeRotation = true;
            collision.gameObject.GetComponent<Rigidbody2D>().gravityScale = 20;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "plataforma")
        {
            transform.SetParent(null);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "destroy" || collision.gameObject.tag == "spikehead")
        {
            if (recentlyDamaged) return;
            recentlyDamaged = true;
            StartCoroutine(ResetDamageCooldown());

            control = false;
            animator.SetTrigger("destroy");
            QuitarVida();
        }

        if (collision.gameObject.tag == "hit")
        {
            control = false;
            hit = true;
            animator.SetInteger("estados", 5);
            effectsSource.PlayOneShot(damageSound);

            if (num_corazones > 1)
            {
                switch (num_corazones)
                {
                    case 3:
                        corazon3.enabled = false;
                        break;
                    case 2:
                        corazon2.enabled = false;
                        break;
                }
                num_corazones--;
            }
            else
            {
                QuitarVida();
                animator.SetTrigger("destroy");
            }

            if (spriterenderer.flipX)
            {
                rigidbody.linearVelocity = new Vector2(10f, 5f);

            }
            else
            {
                rigidbody.linearVelocity = new Vector2(-10f, 5f);
            }
        }

        if (collision.gameObject.tag == "rino")
        {
            if (isGrounded())
            {
                control = false;
                QuitarVida();
                animator.SetTrigger("destroy");
            }
            else
            {
                rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 20f);
                effectsSource.PlayOneShot(killEnemySound);
                collision.gameObject.GetComponent<Animator>().SetTrigger("destroyrino");
            }
        }

        if (collision.gameObject.tag == "Spider")
        {
            
                animator.SetTrigger("destroy");
            }
            else
            {
                rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 20f);
                effectsSource.PlayOneShot(killEnemySound);
                collision.gameObject.GetComponent<Animator>().SetTrigger("destroyspider");
            }
        

        if (collision.gameObject.tag == "slime")
        {
                control = false;
                QuitarVida();
                animator.SetTrigger("destroy");
        }
        if (num_vidas <= 0)
        {
            SceneManager.LoadScene(2);
        }
    }

    public void QuitarVida()
    {
        corazon1.enabled = false;
        corazon2.enabled = false;
        corazon3.enabled = false;
        num_corazones = 3;
        num_vidas--;
        if (num_vidas <= 0)
        {
            PlayerPrefs.SetInt("num_vidas", 0);
            PlayerPrefs.SetInt("select_level", 1);
            PlayerPrefs.SetInt("current_level", 0);
            PlayerPrefs.Save();

            if (effectsSource != null && deathSound != null)
                effectsSource.PlayOneShot(deathSound);

            SceneManager.LoadScene(2);
            return;
        }
        else
        {
            if (effectsSource != null && damageSound != null)
                effectsSource.PlayOneShot(damageSound);
        }

        texto_vidas.SetText("vidas: " + num_vidas);
        PlayerPrefs.SetInt("num_vidas", num_vidas);
        PlayerPrefs.Save();
    }
    
    IEnumerator ResetDamageCooldown()
    {
        yield return new WaitForSeconds(damageCooldown);
        recentlyDamaged = false;
    }
  

}
