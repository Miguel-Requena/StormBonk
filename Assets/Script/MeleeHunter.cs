using UnityEngine;

public class MeleeHunter : MonoBehaviour
{
    public float moveSpeed = 2f;
    public int health = 10; // Vida inicial del enemigo

    Rigidbody2D rb;
    Transform target;
    Vector2 moveDirection;

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
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            moveDirection = direction;
        }
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            rb.linearVelocity = new Vector2(moveDirection.x, moveDirection.y) * moveSpeed;
        }
    }

    // Función que se llama desde Fireball cuando la bola choca con él
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log("¡Impacto! Vida restante del enemigo: " + health);

        if (health <= 0)
        {
            Debug.Log("Enemigo destruido");
            Destroy(gameObject);
        }
    }
}