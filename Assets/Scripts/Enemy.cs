using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Disparo")]
    public GameObject proyectilePrefab;
    public Transform firePoint;
    public float shootInterval = 0.5f;

    [Header("Patrullaje")]
    public float patrolSpeed = 3f;
    public Transform player;

    [HideInInspector] public bool playerDetected = false;


    bool isDead = false;
    Rigidbody2D rb;
    SpriteRenderer sprite;
    Animator animator;

    AudioSource audioSource;
    [Header("Sonido de muerte")]
    public AudioClip deathSound;

    float nextShootTime = 0f;

    // Patrullaje
    float time_move;
    int direction = 1; // 1 = derecha, -1 = izquierda

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = deathSound;
        audioSource.spatialBlend = 0f;  // 🔥 esto lo hace 100% 2D


        time_move = Time.time + 2f;
    }

    void Update()
    {
        Patrol();

        if (playerDetected)
        {
            
            TryShoot();
        }
    }

    void Patrol()
    {
        if (!playerDetected)
        {

            rb.linearVelocity = new Vector2(direction * patrolSpeed, 0);


            if (Time.time > time_move)
            {
                direction *= -1;
                sprite.flipX = direction < 0;
                time_move = Time.time + 2f;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
        }
    }

    void TryShoot()
    {
        if (Time.time > nextShootTime)
        {
            
            Shoot();
            nextShootTime = Time.time + shootInterval;
        }
    }

    public void Shoot()
    {
        if (player == null) return;



        GameObject proyectil = Instantiate(proyectilePrefab, firePoint.position, Quaternion.identity);
        float directionProj = (player.position.x > transform.position.x) ? 1f : -1f;
        proyectil.transform.rotation = Quaternion.Euler(0, directionProj < 0 ? 180 : 0, 0);

        

        Rigidbody2D rbProy = proyectil.GetComponent<Rigidbody2D>();
        rbProy.linearVelocity = new Vector2(directionProj * 6f, 0);
        animator.SetTrigger("Attack");

        sprite.flipX = directionProj < 0;
        
    }

    // ---------------------------
    // COLISIÓN CON JUGADOR (usando corazones)
    // ---------------------------

    /*
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Alex playerScript = collision.gameObject.GetComponent<Alex>();
            if (playerScript != null)
            {
                // Si el jugador está atacando o con power-up, el enemigo muere
                if (playerScript.isAttacking || playerScript.isPoweredUp)
                {
                    Destroy(gameObject);
                }
                else
                {
                    // Usar el sistema de vidas/corazones del jugador
                    if (!playerScript.isInvulnerable)
                        playerScript.TakeDamage(1, transform.position);
                }
            }
        }
    }
    */

    // -------------------------------
    // COLISIÓN CON JUGADOR
    // -------------------------------
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Alex player = collision.gameObject.GetComponent<Alex>();
            if (player != null)
            {
                if (player.isAttacking || player.isPoweredUp)
                {
                    Die();
                }
                else
                {
                    if (!player.isInvulnerable)
                        player.TakeDamage(1, transform.position);
                }
            }
        }
    }
    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Desactivar colisiones inmediatamente
        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("IsMoving", false);

        //AudioSource.PlayClipAtPoint(audioSource.clip, transform.position); // 🔊 No se destruye

        audioSource.PlayOneShot(deathSound, 0.5f);
        Destroy(gameObject, 0.6f);
    }






    /*
    void Die()
    {
        animator.SetTrigger("Death");        // Activar animación de muerte
        rb.linearVelocity = Vector2.zero;      // Detener movimiento
        animator.SetBool("IsMoving", false);
        Destroy(gameObject, 0.4f);       // Destruir objeto después de animación
    }
    */
}
