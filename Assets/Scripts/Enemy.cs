using UnityEngine;

/// <summary>
/// Controla el comportamiento del enemigo:
/// - Patrullaje automático
/// - Detección del jugador
/// - Disparo hacia el jugador
/// - Interacción al colisionar con el jugador
/// - Animaciones y sonidos de muerte
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Disparo")]
    public GameObject proyectilePrefab; // Prefab del proyectil que dispara el enemigo
    public Transform firePoint;         // Punto desde donde se dispara
    public float shootInterval = 0.5f;  // Intervalo entre disparos

    [Header("Patrullaje")]
    public float patrolSpeed = 3f;      // Velocidad del enemigo al patrullar
    public Transform player;            // Referencia al jugador para detectar y disparar

    [HideInInspector] public bool playerDetected = false; // Si el jugador está detectado

    // Estado del enemigo
    bool isDead = false;                // Si el enemigo ya ha muerto
    Rigidbody2D rb;                     // Rigidbody2D para mover al enemigo
    SpriteRenderer sprite;              // SpriteRenderer para voltear el sprite
    Animator animator;                  // Animator para animaciones

    AudioSource audioSource;            // Componente de audio
    [Header("Sonido de muerte")]
    public AudioClip deathSound;         // Sonido que se reproduce al morir

    float nextShootTime = 0f;           // Temporizador para el próximo disparo

    // Patrullaje automático
    float time_move;                     // Temporizador para cambiar de dirección
    int direction = 1;                   // Dirección de movimiento: 1 = derecha, -1 = izquierda

    void Start()
    {
        // Referencias a componentes
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Configuración del audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;    // No reproducir al iniciar
        audioSource.clip = deathSound;      // Asignar clip de muerte
        audioSource.spatialBlend = 0f;      // Sonido 100% 2D

        // Configurar temporizador de movimiento inicial
        time_move = Time.time + 2f;
    }

    void Update()
    {
        // Ejecutar patrullaje automático
        Patrol();

        // Si el jugador está detectado, intentar disparar
        if (playerDetected)
        {
            TryShoot();
        }
    }

    /// <summary>
    /// Patrullaje automático del enemigo
    /// </summary>
    void Patrol()
    {
        if (!playerDetected)
        {
            // Mover enemigo en la dirección actual
            rb.linearVelocity = new Vector2(direction * patrolSpeed, 0);

            // Cambiar de dirección cada 2 segundos
            if (Time.time > time_move)
            {
                direction *= -1;
                sprite.flipX = direction < 0;  // Voltear sprite según dirección
                time_move = Time.time + 2f;
            }
        }
        else
        {
            // Detener movimiento si se detecta al jugador
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
        }
    }

    /// <summary>
    /// Intenta disparar hacia el jugador si ha pasado el intervalo
    /// </summary>
    void TryShoot()
    {
        if (Time.time > nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + shootInterval; // Actualizar temporizador
        }
    }

    /// <summary>
    /// Dispara un proyectil hacia la posición del jugador
    /// </summary>
    public void Shoot()
    {
        if (player == null) return;

        // Instanciar proyectil en el firePoint
        GameObject proyectil = Instantiate(proyectilePrefab, firePoint.position, Quaternion.identity);

        // Determinar dirección del proyectil: derecha (1) o izquierda (-1)
        float directionProj = (player.position.x > transform.position.x) ? 1f : -1f;
        proyectil.transform.rotation = Quaternion.Euler(0, directionProj < 0 ? 180 : 0, 0);

        // Asignar velocidad al proyectil
        Rigidbody2D rbProy = proyectil.GetComponent<Rigidbody2D>();
        rbProy.linearVelocity = new Vector2(directionProj * 6f, 0);

        // Activar animación de ataque
        animator.SetTrigger("Attack");

        // Voltear sprite según dirección del disparo
        sprite.flipX = directionProj < 0;
    }

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
                // Si el jugador está atacando o con power-up, enemigo muere
                if (player.isAttacking || player.isPoweredUp)
                {
                    Die();
                }
                else
                {
                    // Si jugador no es invulnerable, recibir daño
                    if (!player.isInvulnerable)
                        player.TakeDamage(1, transform.position);
                }
            }
        }
    }

    /// <summary>
    /// Mata al enemigo, reproduce animación y sonido, y destruye el objeto
    /// </summary>
    void Die()
    {
        if (isDead) return;  // Evitar muerte doble
        isDead = true;

        // Desactivar colisiones inmediatamente
        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        // Animaciones y detener movimiento
        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("IsMoving", false);

        // Reproducir sonido de muerte
        audioSource.PlayOneShot(deathSound, 0.5f);

        // Destruir objeto después de 0.6s para dejar animación visible
        Destroy(gameObject, 0.6f);
    }
}
