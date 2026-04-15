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
    private Animator anim; 
    private Vector2 moveDirection;
    private float nextFireTime = 0f;
    private bool isDead = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return; 

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;

        // --- ANIMACIÓN DE CAMINAR ---
        anim.SetFloat("Velocidad", moveDirection.magnitude);

        // --- GIRAR EL SPRITE ---
        if (moveX > 0) transform.localScale = new Vector3(3f, 3f, 1);
        else if (moveX < 0) transform.localScale = new Vector3(-3f, 3f, 1);

        // --- DISPARO ---
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
        if (isDead) { rb.linearVelocity = Vector2.zero; return; }
        rb.linearVelocity = moveDirection * speed;
    }

    void ShootFireball(Transform target)
    {
        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        Fireball fireballScript = fireball.GetComponent<Fireball>();
        if (fireballScript != null) fireballScript.SetTarget(target);
    }

    // --- SISTEMA DE DAÑO Y ANIMACIONES ---

    public void TakeDamage(int damage)
    {
        if (isDead) return; 

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            anim.SetTrigger("Hurt");
            Debug.Log("Vida del jugador: " + currentHealth);
        }
        else
        {
            MuerteJugador();
        }
    }

    void MuerteJugador()
    {
        isDead = true;
        anim.SetTrigger("Die"); // Activa la animación de muerte
        Debug.Log("¡HAS MUERTO! Fin de la partida.");

        GetComponent<Collider2D>().enabled = false;
    }


    public void AddPoints(int pointsToAdd)
    {
        score += pointsToAdd;
        Debug.Log("¡Puntos ganados! Puntuación total: " + score);
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
            MeleeHunter enemyScript = enemy.GetComponent<MeleeHunter>();
            if (enemyScript != null && enemyScript.IsDead()) continue;
            if (dist < minDist)
            {
                closest = enemy.transform;
                minDist = dist;
            }
        }
        return closest;
    }
}