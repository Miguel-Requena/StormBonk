using UnityEngine;

/// <summary>
/// Enemigo mago 2D. Compatible con Player.cs, MeleeHunter.cs y Fireball.cs del proyecto.
/// 
/// SETUP en Unity:
///   - Añade este script al prefab del mago
///   - Añade Rigidbody2D (Gravity Scale = 0, Freeze Rotation Z)
///   - Añade un Collider2D (CircleCollider2D va bien)
///   - Ponle el Tag "Enemy"
///   - Asigna el prefab del proyectil en el Inspector (ver MageProjectile.cs)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MageEnemy : MonoBehaviour
{
    // ─── Estados ───────────────────────────────────────────────────────────────
    private enum State { Idle, Chase, Attack, Retreat, Dead }
    private State _state = State.Idle;

    // ─── Stats ─────────────────────────────────────────────────────────────────
    [Header("Estadísticas del Mago")]
    public int health = 30;
    public int damageToPlayer = 8;      // Daño de cada proyectil
    public int pointsToGive = 15;       // Más puntos que el melee por ser más difícil

    // ─── Distancias ────────────────────────────────────────────────────────────
    [Header("Distancias de comportamiento")]
    [Tooltip("A esta distancia el mago detecta al jugador.")]
    public float detectionRange = 12f;

    [Tooltip("Distancia máxima desde la que dispara.")]
    public float attackRange = 9f;

    [Tooltip("Distancia ideal a la que quiere quedarse.")]
    public float preferredRange = 7f;

    [Tooltip("Si el jugador se acerca más de esto, el mago huye.")]
    public float retreatDistance = 4f;

    // ─── Movimiento ────────────────────────────────────────────────────────────
    [Header("Movimiento")]
    public float chaseSpeed = 1.8f;
    public float retreatSpeed = 2.8f;

    // ─── Disparo ───────────────────────────────────────────────────────────────
    [Header("Disparo")]
    [Tooltip("Prefab del proyectil del mago (MageProjectile.cs).")]
    public GameObject projectilePrefab;

    [Tooltip("Segundos entre disparos.")]
    public float attackCooldown = 2.5f;

    // ─── Privados ──────────────────────────────────────────────────────────────
    private Rigidbody2D _rb;
    private Transform _player;
    private float _attackTimer = 0f;

    // ──────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Buscamos al jugador igual que hace MeleeHunter
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
            _player = playerObj.transform;
    }

    private void Update()
    {
        if (_state == State.Dead) return;

        // Si el jugador muere (se desactiva), nos paramos
        if (_player == null || !_player.gameObject.activeInHierarchy)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float dist = Vector2.Distance(transform.position, _player.position);
        _attackTimer -= Time.deltaTime;

        UpdateStateMachine(dist);
    }

    // ─── Máquina de estados ───────────────────────────────────────────────────

    private void UpdateStateMachine(float dist)
    {
        switch (_state)
        {
            // ── Idle: espera hasta detectar al jugador ─────────────────────────
            case State.Idle:
                _rb.linearVelocity = Vector2.zero;
                if (dist <= detectionRange)
                    _state = dist <= attackRange ? State.Attack : State.Chase;
                break;

            // ── Chase: avanza hacia el jugador hasta entrar en rango ───────────
            case State.Chase:
                if (dist > detectionRange) { _state = State.Idle;    break; }
                if (dist < retreatDistance) { _state = State.Retreat; break; }
                if (dist <= attackRange)    { _state = State.Attack;  break; }

                MoveToward(_player.position, chaseSpeed);
                break;

            // ── Attack: se queda en preferredRange y dispara ───────────────────
            case State.Attack:
                if (dist > detectionRange)  { _state = State.Idle;    break; }
                if (dist > attackRange)     { _state = State.Chase;   break; }
                if (dist < retreatDistance) { _state = State.Retreat; break; }

                AdjustPreferredDistance(dist);

                if (_attackTimer <= 0f)
                {
                    ShootAtPlayer();
                    _attackTimer = attackCooldown;
                }
                break;

            // ── Retreat: huye del jugador ──────────────────────────────────────
            case State.Retreat:
                if (dist > detectionRange)  { _state = State.Idle;   break; }
                if (dist >= preferredRange) { _state = State.Attack; break; }

                Vector2 fleeDir = ((Vector2)transform.position - (Vector2)_player.position).normalized;
                _rb.linearVelocity = fleeDir * retreatSpeed;

                // Puede seguir disparando mientras huye
                if (_attackTimer <= 0f)
                {
                    ShootAtPlayer();
                    _attackTimer = attackCooldown;
                }
                break;
        }
    }

    // ─── Helpers de movimiento ────────────────────────────────────────────────

    private void MoveToward(Vector3 targetPos, float speed)
    {
        Vector2 dir = ((Vector2)targetPos - (Vector2)transform.position).normalized;
        _rb.linearVelocity = dir * speed;
    }

    /// <summary>Ajusta posición para quedarse en preferredRange.</summary>
    private void AdjustPreferredDistance(float dist)
    {
        float tolerance = 1.2f;

        if (dist > preferredRange + tolerance)
        {
            MoveToward(_player.position, chaseSpeed * 0.6f);
        }
        else if (dist < preferredRange - tolerance)
        {
            Vector2 backDir = ((Vector2)transform.position - (Vector2)_player.position).normalized;
            _rb.linearVelocity = backDir * chaseSpeed * 0.6f;
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    // ─── Disparo ──────────────────────────────────────────────────────────────

    private void ShootAtPlayer()
    {
        if (projectilePrefab == null || _player == null) return;

        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        if (proj.TryGetComponent<MageProjectile>(out var mp))
            mp.Init(dir, damageToPlayer);
    }

    // ─── Daño y muerte ────────────────────────────────────────────────────────

    /// <summary>
    /// Recibe daño. Fireball.cs actualizado llama enemy.TakeDamage(amount) directamente.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (_state == State.Dead) return;

        health -= amount;

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        _state = State.Dead;
        _rb.linearVelocity = Vector2.zero;

        // Da puntos al jugador, igual que MeleeHunter
        if (_player != null)
        {
            Player playerScript = _player.GetComponent<Player>();
            playerScript?.AddPoints(pointsToGive);
        }

        Destroy(gameObject);
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, preferredRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
#endif
}
