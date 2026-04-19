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
    private Animator anim;           // de main3
    private Vector2 moveDirection;
    private float nextFireTime = 0f;
    private bool isDead = false;     // de main3

    // ─── Stun (de HEAD) ───────────────────────────────────────────────────────
    private float _stunTimer = 0f;
    public bool IsStunned => _stunTimer > 0f;

    void Start()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // de main3
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return; // de main3

        // Cuenta regresiva del stun (de HEAD)
        if (_stunTimer > 0f)
            _stunTimer -= Time.deltaTime;

        // Si está stuneado, no lee input ni dispara (de HEAD)
        if (IsStunned) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;

        // Animación de caminar (de main3)
        anim.SetFloat("Velocidad", moveDirection.magnitude);

        // Girar el sprite (de main3)
        if (moveX > 0)      transform.localScale = new Vector3( 3f, 3f, 1);
        else if (moveX < 0) transform.localScale = new Vector3(-3f, 3f, 1);

        // Disparo
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
        if (isDead) { rb.linearVelocity = Vector2.zero; return; } // de main3

        // Si está stuneado, para completamente al jugador (de HEAD)
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
        if (fireballScript != null) fireballScript.SetTarget(target);
    }

    // ─── Daño y muerte ────────────────────────────────────────────────────────

    public void TakeDamage(int damage)
    {
        if (isDead) return; // de main3

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            anim.SetTrigger("Hurt"); // de main3
            Debug.Log("Vida del jugador: " + currentHealth);
        }
        else
        {
            MuerteJugador();
        }
    }

    void MuerteJugador() // de main3
    {
        isDead = true;
        currentHealth = 0;
        anim.SetTrigger("Die");
        Debug.Log("¡HAS MUERTO! Fin de la partida.");
        GetComponent<Collider2D>().enabled = false;
    }

    // ─── Stun (de HEAD) ───────────────────────────────────────────────────────

    /// <summary>Deja al jugador sin poder moverse ni disparar durante 'duration' segundos.</summary>
    public void Stun(float duration)
    {
        if (isDead) return;
        _stunTimer = duration;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("[Player] ¡Stuneado por " + duration + "s!");
    }

    // ─── Puntos ───────────────────────────────────────────────────────────────

    public void AddPoints(int pointsToAdd)
    {
        score += pointsToAdd;
        Debug.Log("¡Puntos ganados! Puntuación total: " + score);
    }

    // ─── Enemigo más cercano ──────────────────────────────────────────────────

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
            MeleeHunter enemyScript = enemy.GetComponent<MeleeHunter>();
            if (enemyScript != null && enemyScript.IsDead()) continue;
            if (dist < minDist) { closest = enemy.transform; minDist = dist; }
        }
        return closest;
    }
}
