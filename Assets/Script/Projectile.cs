using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 7f;
    public int damage = 10;
    public float lifeTime = 3f;

    void Start()
    {
        // Se mueve hacia adelante 
        GetComponent<Rigidbody2D>().linearVelocity = transform.right * speed;
        Destroy(gameObject, lifeTime); // Se destruye solo si no choca
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}