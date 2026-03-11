using UnityEngine;

public class MeleeHunter : MonoBehaviour
{
    [Header("Estadísticas del Enemigo")]
    public float moveSpeed = 2f;
    public int health = 10;
    public int damageToPlayer = 10; // Daño que le hace al jugador
    public int pointsToGive = 5;    // Puntos que da al morir
    public float damageRate = 1f;   // Cada cuántos segundos te hace daño si te está tocando

    private float nextDamageTime = 0f;
    private Rigidbody2D rb;
    private Transform target;
    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    void Update()
    {
        // Si el jugador está desactivado (muerto), el objetivo se vuelve null
        if (target != null && target.gameObject.activeInHierarchy)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            moveDirection = direction;
        }
        else
        {
            moveDirection = Vector2.zero; // Se para si el jugador muere
        }
    }

    private void FixedUpdate()
    {
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
        health -= damageAmount;

        if (health <= 0)
        {
            // Antes de morir, buscamos al jugador y le damos los puntos
            if (target != null)
            {
                Player playerScript = target.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.AddPoints(pointsToGive);
                }
            }
            Destroy(gameObject);
        }
    }

    // --- NUEVA FUNCIÓN PARA HACER DAÑO AL JUGADOR ---
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Si lo que estamos tocando tiene la etiqueta "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            // Comprobamos si ya pasó el tiempo para volver a hacerle daño
            if (Time.time >= nextDamageTime)
            {
                Player playerScript = collision.gameObject.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(damageToPlayer);
                    nextDamageTime = Time.time + damageRate; // Reiniciamos el temporizador de daño
                }
            }
        }
    }
}