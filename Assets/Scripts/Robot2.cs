using UnityEngine;

public class Robot2 : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 3f;
    public float detectionRange = 8f;
    public float stopDistance = 1f;

    [Header("Referencias")]
    public Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;

    bool isDead = false;
    AudioSource audioSource;
    [Header("Sonido de muerte")]
    public AudioClip deathSound;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = deathSound;
        audioSource.spatialBlend = 0f;  // 🔥 esto lo hace 100% 2D

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRange && distance > stopDistance)
        {
            FollowPlayer();
            anim.SetBool("IsMoving", true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("IsMoving", false);
        }
    }

    void FollowPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
        sprite.flipX = direction.x < 0;
    }
    /*
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Alex player = collision.gameObject.GetComponent<Alex>();
                if (player == null) return;

                // Si el jugador está atacando o tiene power-up → muere el enemigo
                if (player.isAttacking || player.isPoweredUp)
                {
                    anim.SetTrigger("Death");
                    rb.linearVelocity = Vector2.zero;
                    anim.SetBool("IsMoving", false);
                    Destroy(gameObject, 0.5f);
                    return;
                }

                // Si el jugador no está atacando → recibir daño
                if (!player.isInvulnerable)
                    player.TakeDamage(1, transform.position);

                rb.linearVelocity = Vector2.zero;
                anim.SetBool("IsMoving", false);
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

        anim.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("IsMoving", false);

        //AudioSource.PlayClipAtPoint(audioSource.clip, transform.position); // 🔊 No se destruye

        audioSource.PlayOneShot(deathSound, 0.5f);
        Destroy(gameObject, 0.6f);
    }



}
