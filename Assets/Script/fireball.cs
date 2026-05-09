using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float velocidad = 10f;
    public float tiempoVida = 2.5f;
    public int damage = 5;

    private Transform target;
    private Vector2   _fallbackDir;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Start()
    {
        Destroy(gameObject, tiempoVida);
        if (target != null)
        {
            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            _fallbackDir = dir != Vector2.zero ? dir : Vector2.right;
        }
        else
        {
            _fallbackDir = Vector2.right;
        }
    }

    void Update()
    {
        if (target != null)
        {
            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            if (dir != Vector2.zero) _fallbackDir = dir;

            transform.position = Vector2.MoveTowards(
                transform.position,
                target.position,
                velocidad * Time.deltaTime
            );
        }
        else
        {
            transform.Translate(_fallbackDir * velocidad * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        // MeleeHunter
        MeleeHunter melee = collision.GetComponent<MeleeHunter>();
        if (melee != null) { melee.TakeDamage(damage); Destroy(gameObject); return; }

        // MageEnemy
        MageEnemy mage = collision.GetComponent<MageEnemy>();
        if (mage != null) { mage.TakeDamage(damage); Destroy(gameObject); return; }

        // Charger
        Charger charger = collision.GetComponent<Charger>();
        if (charger != null) { charger.TakeDamage(damage); Destroy(gameObject); return; }
    }
}
