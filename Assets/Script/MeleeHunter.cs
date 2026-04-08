using UnityEngine;
using UnityEngine.UI; // Imprescindible para manejar el Slider

public class MeleeHunter : MonoBehaviour
{
    [Header("Estad�sticas del Enemigo")]
    public float moveSpeed = 2f;
    public int maxHealth = 10;   // Usamos maxHealth para el c�lculo
    private int currentHealth;   // Salud actual del enemigo
    public int damageToPlayer = 10;
    public int pointsToGive = 5;
    public float damageRate = 1f;

    private float nextDamageTime = 0f;
    private Rigidbody2D rb;
    private Transform target;
    private Vector2 moveDirection;

    private Animator anim;

    private bool isDead = false;
    public bool IsDead()
    {
        return isDead;
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
    [SerializeField] private Slider healthBarSlider; // Arrastra aqu� el Slider directamente

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth; // Inicializamos vida al 100%

        // Intentamos buscar el Slider autom�ticamente si se nos olvid� asignarlo
        if (healthBarSlider == null)
        {
            healthBarSlider = GetComponentInChildren<Slider>();
        }
    }

    void Start()
    {
        UpdateVisualHealthBar(); // Ponemos la barra al m�ximo al empezar

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    void Update()
    {
        if (isDead) return; 

        if (target != null && target.gameObject.activeInHierarchy)
        {
            moveDirection = (target.position - transform.position).normalized;
        }
        else
        {
            moveDirection = Vector2.zero;
        }

    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (target != null && target.gameObject.activeInHierarchy)
        {
            // Nota: En versiones nuevas se usa 'linearVelocity', en antiguas 'velocity'
            rb.linearVelocity = moveDirection * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;

        anim.SetTrigger("Hit");

        if (health <= 0)
        {
            isDead = true;

            anim.SetBool("isDead", true);

            rb.linearVelocity = Vector2.zero;
            GetComponent<Collider2D>().enabled = false;

        currentHealth -= damageAmount;

        UpdateVisualHealthBar(); // Actualizamos la barra cada vez que recibe da�o

        if (currentHealth <= 0)
        {
            if (target != null)
            {
                Player playerScript = target.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.AddPoints(pointsToGive);
                }
            }

            Destroy(gameObject, 1.2f);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

    // Funci�n interna para actualizar el Slider sin necesidad de otra clase
    private void UpdateVisualHealthBar()
    {
        if (healthBarSlider != null)
        {
            // Convertimos a float para que la divisi�n sea decimal (ej: 5/10 = 0.5)
            healthBarSlider.value = (float)currentHealth / (float)maxHealth;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= nextDamageTime)
            {
                Player playerScript = collision.gameObject.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(damageToPlayer);
                    nextDamageTime = Time.time + damageRate;
                }
            }
        }
    }
}