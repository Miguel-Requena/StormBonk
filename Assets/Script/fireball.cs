using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float velocidad = 10f;
    public float tiempoVida = 2.5f;
    public int damage = 5; // Cantidad de daño que hace la bola

    private Transform target;

    // Esta función es llamada desde el Player para indicarle a quién perseguir
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Start()
    {
        Destroy(gameObject, tiempoVida); // Autodestrucción de seguridad
    }

    void Update()
    {
        if (target != null)
        {
            // Movimiento teledirigido hacia la posición exacta del enemigo
            transform.position = Vector2.MoveTowards(transform.position, target.position, velocidad * Time.deltaTime);
        }
        else
        {
            // Si el enemigo muere mientras la bola viaja, la bola avanza hacia la derecha
            transform.Translate(Vector2.right * velocidad * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Buscamos el script del enemigo para hacerle daño
            MeleeHunter enemy = collision.GetComponent<MeleeHunter>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject); // La bola desaparece tras golpear
        }
    }
}