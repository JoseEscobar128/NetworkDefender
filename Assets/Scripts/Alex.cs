using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


/// <summary>
/// Controla el comportamiento del jugador: movimiento, ataque, vidas, corazones y knockback.
/// </summary>
public class Alex : MonoBehaviour
{

    private AudioSource audioSource;

    [Header("Sonidos")]
    public AudioClip damageSound;
    public AudioClip AttackSound;



    [Header("Movimiento")]
    public float speed = 5f;                 // Velocidad de movimiento
    public float runMultiplier = 1.8f;


    private Rigidbody2D rb;                    // Referencia al Rigidbody2D
    private Vector2 moveInput;                 // Entrada de movimiento (Horizontal/Vertical)
    private Animator anim;                     // Controlador de animaciones
    private SpriteRenderer sr;                 // Sprite para invertir dirección
    private Vector2 lastDir = Vector2.down;    // Última dirección de movimiento usada para ataque/animación

    private float knockbackTimer = 0f;
    private Vector2 knockbackDir;

    [Header("Ataque")]
    public bool isAttacking = false;           // Si el jugador está atacando
    public bool canAttack = true;              // Si puede atacar
    public int attackDamage = 2;               // Daño del ataque
    public float attackRange = 10f;            // Alcance del ataque
    public LayerMask enemyLayer;               // Capa donde están los enemigos

    [Header("Vidas y Corazones")]
    public static int numVidas = 5;                   // Número de vidas del jugador
    public static int numCorazones = 3;               // Corazones actuales
    public static int maxCorazones = 3;               // Máximo de corazones
    public static int numCherry = 0;                  // Contador de "cherries" recogidos
    public TextMeshProUGUI textoVidas;         // Texto que muestra vidas
    public UnityEngine.UI.Image corazon1;      // Imagen del primer corazón
    public UnityEngine.UI.Image corazon2;      // Imagen del segundo corazón
    public UnityEngine.UI.Image corazon3;      // Imagen del tercer corazón

    [Header("piñas")]
    public static int numPina = 0;  // contador global
    public TextMeshProUGUI textoPina; // si tienes texto de piñas
    public int maxVidas = 5;         // vida máxima


    [Header("UI")]
    public TextMeshProUGUI textoCherry;




    [Header("Knockback")]
    public float knockbackForce = 10f;         // Fuerza de retroceso al recibir daño
    public float knockbackTime = 0.3f;        // Duración del retroceso
    private bool knockbackActive = false;      // Si actualmente se está aplicando knockback


    [Header("Invulnerabilidad")]
    public float invulnerableTime = 0.5f; // Tiempo durante el cual no puede recibir daño
    public bool isInvulnerable = false;  // Estado actual de invulnerabilidad

    [Header("Power-Up")]
    public bool isPoweredUp = false; // Estado de poder activo

    [Header("Hitbox")]
    public GameObject hitbox;

    [Header("Timer de Nivel")]
    public float levelTime = 250f; // tiempo total del nivel en segundos
    public TextMeshProUGUI textoTimer;
    private float timerTick = 0f;// para contar segundos reales




    void Start()
    {
        numVidas = 5;
        numCorazones = maxCorazones;

        audioSource = gameObject.AddComponent<AudioSource>();

        // Referencias a componentes
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Actualizar UI al iniciar
        UpdateHeartsUI();
        UpdateVidasUI();

        if (textoCherry != null)
        {
            textoCherry.SetText("" + numCherry);
        }

        timerTick = Time.time;
        if (textoTimer != null)
            textoTimer.SetText(levelTime.ToString());


    }

    void Update()
    {
        // Leer input del jugador (teclas WASD / flechas)
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

        // Si no puede atacar, asegurarse que no quede en ataque
        if (!canAttack)
        {
            isAttacking = false;
        }


        // Timer de nivel
        // Timer de nivel
        /*
        levelTime -= Time.deltaTime; // descontar segundos reales
        if (textoTimer != null)
            textoTimer.SetText(Mathf.Ceil(levelTime).ToString()); // mostrar segundos enteros

        if (levelTime <= 0)
        {
            levelTime = 0; // asegurar que no sea negativo
                          
        
                Die();
        }
        */


    }

    /// <summary>
    /// Método para atacar a enemigos cercanos
    /// </summary>
    void Attack()
    {
        isAttacking = true;
        audioSource.PlayOneShot(AttackSound);
        // Animación de ataque según la última dirección
        anim.SetFloat("Horizontal", lastDir.x);
        anim.SetFloat("Vertical", lastDir.y);
        anim.SetTrigger("Attack");

        // Activar hitbox temporalmente
        hitbox.SetActive(true);

        // Desactivar hitbox al final del ataque
        Invoke(nameof(EndAttack), 0.6f); // 0.3f = duración del ataque
    }

    void EndAttack()
    {
        isAttacking = false;
        hitbox.SetActive(false);
    }

    private void FixedUpdate()
    {
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

        // Guardar última dirección solo si hay input (para ataques)
        if (hasInput)
            lastDir = moveInput.normalized;

        // --- ANIMACIÓN ---
        // Usar la dirección real de velocidad si nos movemos, sino lastDir
        Vector2 animDir = rb.linearVelocity.sqrMagnitude > 0.01f ? rb.linearVelocity.normalized : lastDir;

        anim.SetFloat("Horizontal", animDir.x);
        anim.SetFloat("Vertical", animDir.y);
        anim.SetFloat("Speed", rb.linearVelocity.magnitude);

        anim.SetBool("IsRunning", runningNow);
    }






    // Actualiza UI de corazones
    void UpdateHeartsUI()
    {
        corazon1.enabled = numCorazones >= 1;
        corazon2.enabled = numCorazones >= 2;
        corazon3.enabled = numCorazones >= 3;
    }

    // Actualiza UI de vidas
    void UpdateVidasUI()
    {
        if (textoVidas != null)
            textoVidas.text = "Vidas: " + numVidas;
    }

    /// <summary>
    /// Método que aplica daño al jugador
    /// </summary>
    public void TakeDamage(int damage, Vector3 attackerPos)
    {


        if (isInvulnerable) return; // Si está invulnerable, no hace nada

        // Iniciar invulnerabilidad
        isInvulnerable = true;
        StartCoroutine(ResetInvulnerability());

        // Iniciar knockback
        knockbackDir = (transform.position - attackerPos).normalized * knockbackForce;
        knockbackTimer = knockbackTime;

        // Animación de golpe según dirección
        if (Mathf.Abs(lastDir.x) > Mathf.Abs(lastDir.y))
        {
            audioSource.PlayOneShot(damageSound);
            if (lastDir.x > 0)
                anim.Play("Alex_Hit_Right");
            else
                anim.Play("Alex_Hit_Left");
        }
        else
        {
            audioSource.PlayOneShot(damageSound);
            if (lastDir.y > 0)
                anim.Play("Alex_Hit_Up");
            else
                anim.Play("Alex_hit_Down");
        }

        // Reducir corazones / vidas
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
                Die(); // Muerte del jugador
        }
    }

    // Coroutine para terminar invulnerabilidad
    IEnumerator ResetInvulnerability()
    {
        yield return new WaitForSeconds(invulnerableTime);
        isInvulnerable = false;
    }

    // Muerte del jugador
    void Die()
    {
        anim.SetTrigger("Death");
        canAttack = false;
        rb.linearVelocity = Vector2.zero;

        // Esperar 1 segundo antes de cambiar de escena para que se vea la animación
        Invoke(nameof(LoadGameOver), 1f);

        // Destruir objeto jugador
        Destroy(gameObject, 1f);
    }

    // Método que carga la escena de GameOver
    void LoadGameOver()
    {
        SceneManager.LoadScene("GameOver"); // Asegúrate que la escena se llame exactamente "GameOver"
    }


    // Método para recolectar cherries y regenerar corazones
    // Cada cherry = +1 corazón
    public void CollectCherry()
    {
        if (numCorazones < maxCorazones)
        {
            numCorazones++;
            UpdateHeartsUI();
        }
        else
        {
            // Si ya tienes corazones al máximo, no hace nada o puedes decidir dar extra si quieres
        }
    }

    // Cada piña = +1 vida
    public void CollectPina()
    {
        if (numVidas < maxVidas)
        {
            numVidas++;
            UpdateVidasUI();
        }
    }




    public void ActivatePowerUp(float time)
    {
        isPoweredUp = true;
        StartCoroutine(DeactivatePowerUpAfterTime(time));
    }

    private IEnumerator DeactivatePowerUpAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        isPoweredUp = false;
    }
}
