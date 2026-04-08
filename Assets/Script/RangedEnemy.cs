using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 3f;
    public float safeDistance = 5f; // Distancia a la que intentará mantenerse del jugador

    [Header("Configuración de Ataque")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackRate = 2f;
    private float nextAttackTime = 0f;

    [Header("Estadísticas")]
    public int health = 5;
    public int pointsToGive = 10;

    private Transform target;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isDead || target == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        // LÓGICA DE ALEJARSE
        if (distanceToPlayer < safeDistance)
        {
            // Calcula la dirección opuesta al jugador
            Vector2 escapeDirection = (transform.position - target.position).normalized;
            rb.linearVelocity = escapeDirection * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // Se queda quieto si está a salvo
        }

        // LÓGICA DE DISPARAR
        if (Time.time >= nextAttackTime)
        {
            Shoot();
            nextAttackTime = Time.time + attackRate;
        }
    }

    void Shoot()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetTrigger("Die");

        // Dar puntos (asumiendo que tienes el script Player con AddPoints)
        target.GetComponent<Player>()?.AddPoints(pointsToGive);

        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 1.5f);
    }
}