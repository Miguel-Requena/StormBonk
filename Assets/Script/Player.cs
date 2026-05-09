using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Estadísticas")]
    public int   maxHealth    = 100;
    public int   currentHealth;
    public int   score        = 0;

    [Header("Movimiento")]
    public float speed        = 3f;

    [Header("Bola de Fuego")]
    public GameObject fireballPrefab;
    public float      fireRate       = 0.65f;
    public int        fireballDamage = 10;

    [Header("Pulso Arcano (automático / defensivo)")]
    public float pulseRadius        = 2.5f;
    public int   pulseDamage        = 20;
    public float pulseCooldown      = 4f;
    public int   minEnemiesForPulse = 2;

    private float _nextFireTime;
    private float _nextPulseTime;
    private bool  isDead = false;

    private Rigidbody2D rb;
    private Animator    anim;
    private Vector2     moveDirection;

    // Stun
    private float _stunTimer;
    public  bool  IsStunned => _stunTimer > 0f;

    void Start()
    {
        rb            = GetComponent<Rigidbody2D>();
        anim          = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        if (_stunTimer > 0f) _stunTimer -= Time.deltaTime;
        if (IsStunned) return;

        // Movimiento
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;

        anim.SetFloat("Velocidad", moveDirection.magnitude);

        if      (moveX > 0) transform.localScale = new Vector3( 3f, 3f, 1);
        else if (moveX < 0) transform.localScale = new Vector3(-3f, 3f, 1);

        // Disparo automático (bola de fuego)
        if (Time.time >= _nextFireTime && fireballPrefab != null)
        {
            Transform target = GetClosestEnemy();
            if (target != null)
            {
                ShootFireball(target);
                _nextFireTime = Time.time + fireRate;
            }
        }

        // Pulso Arcano automático (defensivo)
        if (Time.time >= _nextPulseTime && CountEnemiesInPulseRange() >= minEnemiesForPulse)
        {
            PerformPulse();
            _nextPulseTime = Time.time + pulseCooldown;
        }
    }

    void FixedUpdate()
    {
        if (isDead) { rb.linearVelocity = Vector2.zero; return; }
        if (IsStunned) { rb.linearVelocity = Vector2.zero; return; }
        rb.linearVelocity = moveDirection * speed;
    }

    // Disparo
    void ShootFireball(Transform target)
    {
        Vector2 toTarget = ((Vector2)target.position - (Vector2)transform.position);
        Vector2 spawnPos = (Vector2)transform.position + toTarget.normalized * 0.5f;
        GameObject fireball = Instantiate(fireballPrefab, (Vector3)spawnPos, Quaternion.identity);
        Fireball fb = fireball.GetComponent<Fireball>();
        if (fb != null)
        {
            fb.SetTarget(target);
            fb.damage = fireballDamage;
        }
    }

    // Pulso Arcano
    int CountEnemiesInPulseRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pulseRadius);
        int count = 0;
        foreach (Collider2D hit in hits)
            if (hit.CompareTag("Enemy")) count++;
        return count;
    }

    void PerformPulse()
    {
        anim.SetTrigger("Attack");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pulseRadius);
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            MeleeHunter mh = hit.GetComponent<MeleeHunter>();
            if (mh != null) { mh.TakeDamage(pulseDamage); continue; }

            Charger c = hit.GetComponent<Charger>();
            if (c != null) { c.TakeDamage(pulseDamage); continue; }

            MageEnemy me = hit.GetComponent<MageEnemy>();
            if (me != null) me.TakeDamage(pulseDamage);
        }
    }

    // Progresión por oleada
    public void ApplyWaveProgression(int wave, int level)
    {
        fireRate        = Mathf.Max(0.25f, fireRate * 0.90f);
        fireballDamage += 2;

        if (wave == 0)
        {
            // Subida de nivel: aumenta vida máxima y curación completa
            speed     = Mathf.Min(6f, speed + 0.3f);
            maxHealth += 10;
            currentHealth = maxHealth;
        }
        else
        {
            // Entre oleadas: curación parcial
            currentHealth = Mathf.Min(maxHealth, currentHealth + 20);
        }
    }

    // Daño y muerte
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
        currentHealth = 0;
        anim.SetTrigger("Die");
        Debug.Log("¡HAS MUERTO! Fin de la partida.");
        GetComponent<Collider2D>().enabled = false;
    }

    // Stun
    public void Stun(float duration)
    {
        if (isDead) return;
        _stunTimer = duration;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("[Player] ¡Stuneado por " + duration + "s!");
    }

    // Puntos
    public void AddPoints(int pointsToAdd)
    {
        score += pointsToAdd;
        Debug.Log("¡Puntos ganados! Puntuación total: " + score);
    }

    // Enemigo más cercano
    Transform GetClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        Transform closest  = null;
        float     minDist  = Mathf.Infinity;
        Vector3   selfPos  = transform.position;

        foreach (GameObject enemy in enemies)
        {
            MeleeHunter mh = enemy.GetComponent<MeleeHunter>();
            if (mh != null && mh.IsDead()) continue;

            Charger c = enemy.GetComponent<Charger>();
            if (c != null && c.IsDead()) continue;

            float dist = Vector3.Distance(enemy.transform.position, selfPos);
            if (dist < minDist) { closest = enemy.transform; minDist = dist; }
        }
        return closest;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, pulseRadius);
    }
#endif
}
