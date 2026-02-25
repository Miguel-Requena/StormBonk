using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 3f;

    [Header("Bola de Fuego")]
    public GameObject fireballPrefab;
    public float fireForce = 15f;
    public float fireRate = 1.0f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float nextFireTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
                ShootFireball(target.position);
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * speed;
    }

    void ShootFireball(Vector3 targetPos)
    {
        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        Rigidbody2D fireballRb = fireball.GetComponent<Rigidbody2D>();
        if (fireballRb != null)
        {
            Vector2 direction = (targetPos - transform.position).normalized;
            fireballRb.AddForce(direction * fireForce, ForceMode2D.Impulse);
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

}
