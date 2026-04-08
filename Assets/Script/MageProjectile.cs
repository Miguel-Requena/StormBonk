using UnityEngine;

/// <summary>
/// Proyectil 2D del mago enemigo.
/// 
/// SETUP en Unity:
///   - Crea un sprite (círculo morado/azul, lo que tengas)
///   - Añade Rigidbody2D (Gravity Scale = 0, Is Kinematic = true)
///   - Añade CircleCollider2D marcado como IS TRIGGER
///   - Añade este script
///   - Guárdalo como prefab y asígnalo en MageEnemy → Projectile Prefab
/// </summary>
public class MageProjectile : MonoBehaviour
{
    [Header("Proyectil")]
    public float speed = 7f;
    public float lifetime = 3f;

    private Vector2 _direction;
    private int _damage;
    private bool _ready;

    /// <summary>Llamado por MageEnemy al instanciar el proyectil.</summary>
    public void Init(Vector2 direction, int damage)
    {
        _direction = direction.normalized;
        _damage    = damage;
        _ready     = true;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!_ready) return;
        transform.Translate(_direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Daña al jugador
        if (collision.CompareTag("Player"))
        {
            Player playerScript = collision.GetComponent<Player>();
            playerScript?.TakeDamage(_damage);
            Destroy(gameObject);
            return;
        }

        // No colisiona con otros enemigos ni con el propio mago
        if (collision.CompareTag("Enemy")) return;

        // Choca con cualquier otra cosa (paredes, etc.)
        Destroy(gameObject);
    }
}
