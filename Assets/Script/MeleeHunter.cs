using UnityEngine;
using UnityEngine.UI; // Necesario para trabajar con Sliders

public class MeleeHunter : MonoBehaviour
{
    [Header("Estadísticas del Enemigo")]
    public float moveSpeed = 2f;
    public int health = 10;
    public int maxHealth = 10; // Salud máxima original
    private int currentHealth; // Salud actual
    public int damageToPlayer = 10;
    public int pointsToGive = 5;
    public float damageRate = 1f;

    private float nextDamageTime = 0f;
    private Rigidbody2D rb;
    private Transform target;
    private Vector2 moveDirection;

    private Animator anim;

    [Header("Interfaz")]
    [SerializeField] private Slider healthBarSlider;

    private bool isDead = false;
    public bool IsDead()
    {
        return isDead;
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth; // Inicializamos la salud
        anim = GetComponent<Animator>();

        // Buscamos la barra de vida en los hijos para evitar el NullReferenceException
        if (healthBarSlider == null)
        {
            healthBarSlider = GetComponentInChildren<Slider>();
        }
    }

    void Start()
    {
        UpdateVisualHealthBar(); // Ponemos la barra al máximo al empezar

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
            Vector3 direction = (target.position - transform.position).normalized;
            moveDirection = direction;
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
            rb.linearVelocity = new Vector2(moveDirection.x, moveDirection.y) * moveSpeed;
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
        currentHealth -= damageAmount;
        UpdateVisualHealthBar();

        anim.SetTrigger("Hit");

        if (health <= 0)
        {
            isDead = true;

            anim.SetBool("isDead", true);

            rb.linearVelocity = Vector2.zero;
            GetComponent<Collider2D>().enabled = false;

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

    // Función interna para actualizar el Slider sin necesidad de otra clase
    private void UpdateVisualHealthBar()
    {
        if (healthBarSlider != null)
        {
            // Convertimos a float para que la división sea decimal
            healthBarSlider.value = (float)currentHealth / (float)maxHealth;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

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