using UnityEngine;
using System.Collections;

/// <summary>
/// Script para el comportamiento de una araña enemiga.
/// Controla movimiento, ataque, clonación y recepción de daño.
/// </summary>
public class Spider : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 3f;          // Velocidad de movimiento de la araña
    public float detectionRange = 8f;     // Distancia a la que detecta al jugador
    public float stopDistance = 1f;       // Distancia mínima para detenerse frente al jugador

    [Header("Multiplicación")]
    public GameObject spiderPrefab;       // Prefab usado para crear clones
    public float cloneInterval = 5f;      // Tiempo entre clonaciones
    public int maxClones = 3;             // Número máximo de clones por araña
    private int cloneCount = 0;           // Contador de clones actuales
    private float cloneTimer = 0f;        // Temporizador para clonación

    [Header("Referencias")]
    public Transform player;              // Referencia al jugador
    private Rigidbody2D rb;               // Referencia al Rigidbody2D de la araña
    private SpriteRenderer sprite;        // Referencia al SpriteRenderer para invertir sprite
    private Animator anim;                // Referencia al Animator para animaciones

    [Header("Ataque")]
    public float attackCooldown = 1f;     // Tiempo entre ataques
    private float lastAttackTime = 0f;    // Momento del último ataque

    [Header("Vida")]
    public int health = 1;                // Vida de la araña

    AudioSource audioSource;
    [Header("Sonido de muerte")]
    public AudioClip deathSound;

    bool isDead = false;

    void Start()
    {
        // Obtener referencias de componentes
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = deathSound;
        audioSource.spatialBlend = 0f;  // 🔥 esto lo hace 100% 2D

        // Si no se asignó el jugador en el Inspector, buscarlo por Tag
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // Inicia la clonación periódica de la araña
       // InvokeRepeating(nameof(CloneSpider), cloneInterval, cloneInterval);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Si el jugador está dentro del rango de detección y fuera de la distancia mínima
        if (distance <= detectionRange && distance > stopDistance)
        {
            FollowPlayer();           // Seguir al jugador
            anim.SetBool("IsMoving", true);

            cloneTimer += Time.deltaTime;

            if (cloneTimer >= cloneInterval && cloneCount < maxClones)
            {
                CloneSpider();
                cloneTimer = 0f;
            }
        }
        else
        {
            // Detener movimiento
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("IsMoving", false);

            cloneTimer = 0f;
        }
    }

    /// <summary>
    /// Movimiento de la araña hacia el jugador
    /// </summary>
    void FollowPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // Invertir sprite según dirección
        sprite.flipX = direction.x < 0;
    }

    // -------------------------------
    // DAÑO RECIBIDO
    // -------------------------------
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
            Die();
    }

    // -------------------------------
    // CLONACIÓN
    // -------------------------------
    void CloneSpider()
    {
        if (cloneCount >= maxClones) return;

        // Generar posición aleatoria cercana para el clon
        Vector3 spawnPos = transform.position + new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0
        );

        Instantiate(spiderPrefab, spawnPos, Quaternion.identity);
        cloneCount++;
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
                if (player.isAttacking || player.isPoweredUp)
                {
                    Die(); // Muere al tocar al jugador si está atacando o con power-up
                }
                else
                {
                    if (!player.isInvulnerable)
                        player.TakeDamage(1, transform.position);
                }
            }
        }
    }






    // -------------------------------
    // MUERTE DE LA ARAÑA
    // -------------------------------
    void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.SetTrigger("Death");        // Activar animación de muerte
        rb.linearVelocity = Vector2.zero;      // Detener movimiento
        anim.SetBool("IsMoving", false);

        audioSource.PlayOneShot(deathSound, 0.5f);
        Destroy(gameObject, 0.3f);       // Destruir objeto después de animación
    }
}
