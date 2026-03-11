using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Estadísticas")]
    public int maxHealth = 100;
    public int currentHealth;
    public int score = 0;

    [Header("Movimiento")]
    public float speed = 3f;

    [Header("Bola de Fuego")]
    public GameObject fireballPrefab;
    public float fireRate = 1.0f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float nextFireTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth; // Empezamos con la vida al máximo
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;

        if (Time.time >= nextFireTime && fireballPrefab != null)
        {
            Transform target = GetClosestEnemy();
            if (target != null)
            {
                ShootFireball(target);
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * speed;
    }

    void ShootFireball(Transform target)
    {
        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);

        Fireball fireballScript = fireball.GetComponent<Fireball>();
        if (fireballScript != null)
        {
            fireballScript.SetTarget(target);
        }
    }

    Transform GetClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        Transform closest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(enemy.transform.position, currentPos);
            if (dist < minDist)
            {
                closest = enemy.transform;
                minDist = dist;
            }
        }
        return closest;
    }

    // --- NUEVAS FUNCIONES DE VIDA Y PUNTOS ---

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("¡Auch! Vida del jugador: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("¡HAS MUERTO! Fin de la partida.");
            gameObject.SetActive(false); // Desactiva al jugador (simula que muere)
            // Aquí más adelante llamaremos a la pantalla de Game Over
        }
    }

    public void AddPoints(int pointsToAdd)
    {
        score += pointsToAdd;
        Debug.Log("¡Puntos ganados! Puntuación total: " + score);
    }
}