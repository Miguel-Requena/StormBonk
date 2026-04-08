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

    // ─── Stun ──────────────────────────────────────────────────────────────────
    private float _stunTimer = 0f;
    public bool IsStunned => _stunTimer > 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        // Cuenta regresiva del stun
        if (_stunTimer > 0f)
            _stunTimer -= Time.deltaTime;

        // Si está stuneado, no lee input ni dispara
        if (IsStunned) return;

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
        // Si está stuneado, para completamente al jugador
        if (IsStunned)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveDirection * speed;
    }

    void ShootFireball(Transform target)
    {
        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        Fireball fireballScript = fireball.GetComponent<Fireball>();
        if (fireballScript != null)
            fireballScript.SetTarget(target);
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("¡Auch! Vida del jugador: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("¡HAS MUERTO! Fin de la partida.");
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Deja al jugador sin poder moverse ni disparar durante 'duration' segundos.
    /// Llamado por Charger al impactar.
    /// </summary>
    public void Stun(float duration)
    {
        _stunTimer = duration;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("[Player] ¡Stuneado por " + duration + "s!");
    }

    public void AddPoints(int pointsToAdd)
    {
        score += pointsToAdd;
        Debug.Log("¡Puntos ganados! Puntuación total: " + score);
    }
}
