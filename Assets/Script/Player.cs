using UnityEngine;
using UnityEngine.UI; // ← ¡NUEVO! Necesario para la UI
using UnityEngine.SceneManagement; // ← ¡NUEVO! Para reiniciar el nivel

public class Player : MonoBehaviour
{
    [Header("Estadísticas")]
    public int maxHealth = 100;
    public int currentHealth;
    public int score = 0;

    [Header("Interfaz (UI)")]
    public Text scoreText;              // El texto de los puntos
    public Slider playerHealthBar;      // La barra de vida del jugador
    public GameObject gameOverPanel;    // El panel de Game Over

    [Header("Movimiento")]
    public float speed = 3f;

    [Header("Bola de Fuego")]
    public GameObject fireballPrefab;
    public float fireRate = 1.0f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float nextFireTime = 0f;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        // Configuramos la UI inicial
        if (playerHealthBar != null)
        {
            playerHealthBar.maxValue = maxHealth;
            playerHealthBar.value = currentHealth;
        }
        UpdateScoreUI();

        // Nos aseguramos de ocultar el Game Over y que el tiempo corra
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (isDead) return; // Si estás muerto, no puedes hacer nada

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
        if (!isDead)
        {
            rb.linearVelocity = moveDirection * speed;
        }
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
            MeleeHunter enemyScript = enemy.GetComponent<MeleeHunter>();
            //Ignorar enemigos muertos
            if (enemyScript != null && enemyScript.IsDead())
                continue;
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
        if (isDead) return;

        currentHealth -= damage;
        if (playerHealthBar != null) playerHealthBar.value = currentHealth; // Actualiza la barra

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void AddPoints(int pointsToAdd)
    {
        if (isDead) return;
        score += pointsToAdd;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "PUNTOS: " + score;
        }
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // Muestra la pantalla de Game Over
        }

        Time.timeScale = 0f; // Pausa el juego por completo
    }

    // Esta función la pondremos en un botón para reiniciar
    public void RestartGame()
    {
        Time.timeScale = 1f; // Vuelve a la normalidad el tiempo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recarga la escena
    }
}