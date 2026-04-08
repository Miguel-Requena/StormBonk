using UnityEngine;
using UnityEngine.UI; // ← ¡NUEVO! Para la UI del enemigo

public class MeleeHunter : MonoBehaviour
{
    [Header("Estadísticas del Enemigo")]
    public float moveSpeed = 2f;
<<<<<<< HEAD
    public int maxHealth = 10;
    private int currentHealth;

    public int damageToPlayer = 10;
    public int pointsToGive = 5;
    public float damageRate = 1f;

    [Header("Interfaz (UI)")]
    public Slider healthBar; // ← ¡NUEVO! Barra de vida encima de su cabeza
=======
    public int health = 10;
    public int damageToPlayer = 10;
    public int pointsToGive = 5;
    public float damageRate = 1f;
>>>>>>> main

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
    }

    void Start()
    {
        currentHealth = maxHealth;

        // Configuramos su barrita de vida al nacer
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    void Update()
    {
<<<<<<< HEAD
=======
        if (isDead) return; 

>>>>>>> main
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
<<<<<<< HEAD
        currentHealth -= damageAmount;
        if (healthBar != null) healthBar.value = currentHealth; // Actualiza su barrita

        if (currentHealth <= 0)
        {
=======
        if (isDead) return;

        health -= damageAmount;

        anim.SetTrigger("Hit");

        if (health <= 0)
        {
            isDead = true;

            anim.SetBool("isDead", true);

            rb.linearVelocity = Vector2.zero;
            GetComponent<Collider2D>().enabled = false;

>>>>>>> main
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
<<<<<<< HEAD
=======
        if (isDead) return;

>>>>>>> main
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