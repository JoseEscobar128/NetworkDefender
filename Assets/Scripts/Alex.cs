using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla el comportamiento del jugador "Alex":
/// - Movimiento y corrida
/// - Ataque
/// - Vidas, corazones y recolección de items (cherries y piñas)
/// - Knockback al recibir daño
/// - Invulnerabilidad temporal
/// - Power-ups
/// - UI de corazones, vidas, cherries y timer
/// </summary>
public class Alex : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Sonidos")]
    public AudioClip damageSound; // Sonido al recibir daño
    public AudioClip AttackSound; // Sonido al atacar

    [Header("Movimiento")]
    public float speed = 5f;        // Velocidad de movimiento base
    public float runMultiplier = 1.8f; // Multiplicador de velocidad al correr

    private Rigidbody2D rb;         // Referencia al Rigidbody2D para física
    private Vector2 moveInput;      // Entrada de movimiento horizontal y vertical
    private Animator anim;          // Controlador de animaciones
    private SpriteRenderer sr;      // SpriteRenderer para invertir la dirección
    private Vector2 lastDir = Vector2.down; // Última dirección usada para ataques/animación

    private float knockbackTimer = 0f; // Temporizador de knockback
    private Vector2 knockbackDir;      // Dirección de knockback

    [Header("Ataque")]
    public bool isAttacking = false;   // Si actualmente se está atacando
    public bool canAttack = true;      // Si puede atacar
    public int attackDamage = 2;       // Daño que causa el ataque
    public float attackRange = 10f;    // Alcance del ataque
    public LayerMask enemyLayer;       // Capa de enemigos a detectar

    [Header("Vidas y Corazones")]
    public static int numVidas = 5;    // Número total de vidas del jugador
    public static int numCorazones = 3; // Corazones actuales
    public static int maxCorazones = 3; // Máximo de corazones
    public static int numCherry = 0;    // Contador de cherries recogidos
    public TextMeshProUGUI textoVidas;  // UI que muestra número de vidas
    public UnityEngine.UI.Image corazon1; // Imagen del corazón 1
    public UnityEngine.UI.Image corazon2; // Imagen del corazón 2
    public UnityEngine.UI.Image corazon3; // Imagen del corazón 3

    [Header("Piñas")]
    public static int numPina = 0;       // Contador global de piñas
    public TextMeshProUGUI textoPina;    // UI de piñas (si aplica)
    public int maxVidas = 5;             // Vida máxima

    [Header("UI")]
    public TextMeshProUGUI textoCherry; // UI de cherries

    [Header("Knockback")]
    public float knockbackForce = 10f;   // Fuerza aplicada al recibir daño
    public float knockbackTime = 0.3f;   // Duración del knockback
    private bool knockbackActive = false; // Estado de knockback activo

    [Header("Invulnerabilidad")]
    public float invulnerableTime = 0.5f; // Tiempo que dura la invulnerabilidad tras recibir daño
    public bool isInvulnerable = false;   // Estado de invulnerabilidad actual

    [Header("Power-Up")]
    public bool isPoweredUp = false;      // Estado de power-up activo

    [Header("Hitbox")]
    public GameObject hitbox;             // Hitbox usada al atacar

    [Header("Timer de Nivel")]
    public float levelTime = 250f;        // Tiempo total del nivel en segundos
    public TextMeshProUGUI textoTimer;    // UI del timer
    private float timerTick = 0f;         // Para contar tiempo real

    void Start()
    {
        // Inicializar vidas y corazones
        numVidas = 5;
        numCorazones = maxCorazones;

        // Agregar componente de audio si no existe
        audioSource = gameObject.AddComponent<AudioSource>();

        // Obtener referencias a componentes
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Actualizar la UI al iniciar
        UpdateHeartsUI();
        UpdateVidasUI();

        if (textoCherry != null)
            textoCherry.SetText("" + numCherry);

        timerTick = Time.time;
        if (textoTimer != null)
            textoTimer.SetText(levelTime.ToString());
    }

    void Update()
    {
        // Leer input del jugador (WASD / Flechas)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Si hay movimiento, actualizar la última dirección
        if (moveInput != Vector2.zero)
        {
            moveInput.Normalize();
            lastDir = moveInput;
        }

        // Actualizar animaciones según dirección
        if (moveInput.sqrMagnitude > 0.01f)
        {
            moveInput.Normalize();
            anim.SetFloat("Horizontal", moveInput.x);
            anim.SetFloat("Vertical", moveInput.y);
        }

        anim.SetFloat("Speed", moveInput.magnitude);

        // Detectar ataque con barra espaciadora
        if (canAttack && Input.GetKeyDown(KeyCode.Space))
            Attack();

        // Si no puede atacar, asegurar que no quede en ataque
        if (!canAttack)
            isAttacking = false;

        // Timer de nivel (comentado)
        /*
        levelTime -= Time.deltaTime;
        if (textoTimer != null)
            textoTimer.SetText(Mathf.Ceil(levelTime).ToString());

        if (levelTime <= 0)
        {
            levelTime = 0;
            Die();
        }
        */
    }

    /// <summary>
    /// Método que inicia el ataque del jugador
    /// </summary>
    void Attack()
    {
        isAttacking = true;
        audioSource.PlayOneShot(AttackSound);

        // Actualizar animación según última dirección
        anim.SetFloat("Horizontal", lastDir.x);
        anim.SetFloat("Vertical", lastDir.y);
        anim.SetTrigger("Attack");

        // Activar hitbox temporalmente
        hitbox.SetActive(true);

        // Desactivar hitbox al final del ataque
        Invoke(nameof(EndAttack), 0.6f);
    }

    /// <summary>
    /// Termina el ataque y desactiva la hitbox
    /// </summary>
    void EndAttack()
    {
        isAttacking = false;
        hitbox.SetActive(false);
    }

    void FixedUpdate()
    {
        // Aplicar knockback si está activo
        if (knockbackTimer > 0f)
        {
            rb.linearVelocity = knockbackDir;
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        bool hasInput = moveInput.sqrMagnitude > 0.01f;
        bool runningNow = Input.GetKey(KeyCode.LeftShift) && hasInput;

        // Movimiento físico
        rb.linearVelocity = moveInput * speed * (runningNow ? runMultiplier : 1f);

        // Guardar última dirección solo si hay input
        if (hasInput)
            lastDir = moveInput.normalized;

        // Animación según velocidad o última dirección
        Vector2 animDir = rb.linearVelocity.sqrMagnitude > 0.01f ? rb.linearVelocity.normalized : lastDir;
        anim.SetFloat("Horizontal", animDir.x);
        anim.SetFloat("Vertical", animDir.y);
        anim.SetFloat("Speed", rb.linearVelocity.magnitude);
        anim.SetBool("IsRunning", runningNow);
    }

    /// <summary>
    /// Actualiza la UI de corazones
    /// </summary>
    void UpdateHeartsUI()
    {
        corazon1.enabled = numCorazones >= 1;
        corazon2.enabled = numCorazones >= 2;
        corazon3.enabled = numCorazones >= 3;
    }

    /// <summary>
    /// Actualiza la UI de vidas
    /// </summary>
    void UpdateVidasUI()
    {
        if (textoVidas != null)
            textoVidas.text = "Vidas: " + numVidas;
    }

    /// <summary>
    /// Aplica daño al jugador, knockback e invulnerabilidad temporal
    /// </summary>
    /// <param name="damage">Cantidad de daño</param>
    /// <param name="attackerPos">Posición del atacante</param>
    public void TakeDamage(int damage, Vector3 attackerPos)
    {
        if (isInvulnerable) return;

        // Iniciar invulnerabilidad
        isInvulnerable = true;
        StartCoroutine(ResetInvulnerability());

        // Aplicar knockback
        knockbackDir = (transform.position - attackerPos).normalized * knockbackForce;
        knockbackTimer = knockbackTime;

        // Reproducir animación y sonido de golpe
        if (Mathf.Abs(lastDir.x) > Mathf.Abs(lastDir.y))
        {
            audioSource.PlayOneShot(damageSound);
            anim.Play(lastDir.x > 0 ? "Alex_Hit_Right" : "Alex_Hit_Left");
        }
        else
        {
            audioSource.PlayOneShot(damageSound);
            anim.Play(lastDir.y > 0 ? "Alex_Hit_Up" : "Alex_hit_Down");
        }

        // Reducir corazones o vidas
        if (numCorazones > damage)
        {
            numCorazones -= damage;
            UpdateHeartsUI();
        }
        else
        {
            numVidas--;
            numCorazones = maxCorazones;
            UpdateHeartsUI();
            UpdateVidasUI();

            if (numVidas <= 0)
                Die();
        }
    }

    /// <summary>
    /// Coroutine para terminar la invulnerabilidad
    /// </summary>
    IEnumerator ResetInvulnerability()
    {
        yield return new WaitForSeconds(invulnerableTime);
        isInvulnerable = false;
    }

    /// <summary>
    /// Muerte del jugador
    /// </summary>
    void Die()
    {
        anim.SetTrigger("Death");
        canAttack = false;
        rb.linearVelocity = Vector2.zero;

        // Esperar animación y luego cargar GameOver
        Invoke(nameof(LoadGameOver), 1f);

        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// Carga la escena de GameOver
    /// </summary>
    void LoadGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }

    /// <summary>
    /// Recolecta un cherry y regenera un corazón si no está al máximo
    /// </summary>
    public void CollectCherry()
    {
        if (numCorazones < maxCorazones)
        {
            numCorazones++;
            UpdateHeartsUI();
        }
    }

    /// <summary>
    /// Recolecta una piña y regenera una vida si no está al máximo
    /// </summary>
    public void CollectPina()
    {
        if (numVidas < maxVidas)
        {
            numVidas++;
            UpdateVidasUI();
        }
    }

    /// <summary>
    /// Activa un power-up por un tiempo determinado
    /// </summary>
    /// <param name="time">Duración en segundos del power-up</param>
    public void ActivatePowerUp(float time)
    {
        isPoweredUp = true;
        StartCoroutine(DeactivatePowerUpAfterTime(time));
    }

    /// <summary>
    /// Coroutine que desactiva el power-up después de cierto tiempo
    /// </summary>
    IEnumerator DeactivatePowerUpAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        isPoweredUp = false;
    }
}
