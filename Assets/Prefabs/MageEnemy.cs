using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private enum State { Chase, Attack, Retreat, Dead }
    private State _state = State.Chase;   // persigue desde el primer frame

    // ─── Stats ─────────────────────────────────────────────────────────────────
    [Header("Estadísticas del Mago")]
    public int maxHealth      = 30;
    private int _currentHealth;
    public int damageToPlayer = 8;
    public int pointsToGive   = 15;

    [Header("Interfaz de Usuario")]
    [SerializeField] private Slider healthBarSlider;

    // ─── Distancias ────────────────────────────────────────────────────────────
    [Header("Distancias de comportamiento")]
    public float attackRange     = 9f;
    public float preferredRange  = 7f;
    public float retreatDistance = 4f;

    // ─── Movimiento ────────────────────────────────────────────────────────────
    [Header("Movimiento")]
    public float chaseSpeed   = 1.8f;
    public float retreatSpeed = 2.8f;

    [Header("Pathfinding A*")]
    public float pathUpdateInterval = 0.4f;

    // ─── Disparo ───────────────────────────────────────────────────────────────
    [Header("Disparo")]
    public GameObject projectilePrefab;
    public float attackCooldown = 2.5f;

    // ─── Privados ──────────────────────────────────────────────────────────────
    private Rigidbody2D _rb;
    private Transform   _player;
    private Player      _playerScript;
    private float       _attackTimer = 0f;

    // A* data
    private List<Vector2> _path;
    private int            _pathIndex;
    private float          _pathTimer;
    private Vector2        _currentPathTarget;

    // ──────────────────────────────────────────────────────────────────────────
    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void Start()
    {
        _currentHealth = maxHealth;

        if (healthBarSlider != null)
        {
            healthBarSlider.minValue = 0f;
            healthBarSlider.maxValue = 1f;
            healthBarSlider.value    = 1f;
        }

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            _player       = playerObj.transform;
            _playerScript = playerObj.GetComponent<Player>();
        }

        // Fuerza recálculo de path en el primer frame
        _pathTimer = 0f;
    }

    private void Update()
    {
        if (_state == State.Dead) return;
        if (_player == null || !_player.gameObject.activeInHierarchy)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float dist = Vector2.Distance(transform.position, _player.position);
        _attackTimer -= Time.deltaTime;

        // Actualiza path A* cada intervalo
        _pathTimer -= Time.deltaTime;
        if (_pathTimer <= 0f && PathfindingGrid.Instance != null)
        {
            RecalculatePath(dist);
            _pathTimer = pathUpdateInterval;
        }

        UpdateStateMachine(dist);
    }

    // ─── Recálculo de ruta A* ─────────────────────────────────────────────────
    private void RecalculatePath(float dist)
    {
        Vector2 pathTarget;

        if (_state == State.Retreat)
        {
            Vector2 fleeDir = ((Vector2)transform.position - (Vector2)_player.position).normalized;
            pathTarget = (Vector2)transform.position + fleeDir * (preferredRange + 2f);
        }
        else
        {
            pathTarget = _player.position;
        }

        _currentPathTarget = pathTarget;
        _path      = PathfindingGrid.Instance.FindPath(transform.position, pathTarget);
        _pathIndex = 0;
    }

    // ─── Movimiento por A* ────────────────────────────────────────────────────
    private void FollowCurrentPath(float speed)
    {
        if (_path != null && _pathIndex < _path.Count)
        {
            Vector2 wp  = _path[_pathIndex];
            Vector2 dir = (wp - (Vector2)transform.position).normalized;
            _rb.linearVelocity = dir * speed;
            if (Vector2.Distance(transform.position, wp) < 0.35f) _pathIndex++;
        }
        else
        {
            Vector2 dir = (_currentPathTarget - (Vector2)transform.position).normalized;
            _rb.linearVelocity = dir * speed;
        }
    }

    // ─── Máquina de estados ───────────────────────────────────────────────────
    private void UpdateStateMachine(float dist)
    {
        switch (_state)
        {
            // Chase: siempre persigue al jugador hasta entrar en rango
            case State.Chase:
                if (dist < retreatDistance) { _state = State.Retreat; _pathTimer = 0f; break; }
                if (dist <= attackRange)    { _state = State.Attack;  _pathTimer = 0f; break; }
                FollowCurrentPath(chaseSpeed);
                break;

            // Attack: mantiene preferredRange y dispara
            case State.Attack:
                if (dist > attackRange)     { _state = State.Chase;   _pathTimer = 0f; break; }
                if (dist < retreatDistance) { _state = State.Retreat; _pathTimer = 0f; break; }
                AdjustPreferredDistance(dist);
                if (_attackTimer <= 0f) { ShootAtPlayer(); _attackTimer = attackCooldown; }
                break;

            // Retreat: huye del jugador
            case State.Retreat:
                if (dist >= preferredRange) { _state = State.Attack; _pathTimer = 0f; break; }
                FollowCurrentPath(retreatSpeed);
                if (_attackTimer <= 0f) { ShootAtPlayer(); _attackTimer = attackCooldown; }
                break;
        }
    }

    // ─── Ajuste de distancia en ataque (A*) ──────────────────────────────────
    private void AdjustPreferredDistance(float dist)
    {
        float tolerance = 1.2f;

        if (dist > preferredRange + tolerance)
        {
            FollowCurrentPath(chaseSpeed * 0.6f);
        }
        else if (dist < preferredRange - tolerance)
        {
            if (_path == null || _pathIndex >= _path.Count)
            {
                Vector2 backDir = ((Vector2)transform.position - (Vector2)_player.position).normalized;
                _currentPathTarget = (Vector2)transform.position + backDir * 3f;
                if (PathfindingGrid.Instance != null)
                {
                    _path      = PathfindingGrid.Instance.FindPath(transform.position, _currentPathTarget);
                    _pathIndex = 0;
                }
            }
            FollowCurrentPath(chaseSpeed * 0.6f);
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
    public void TakeDamage(int amount)
    {
        if (_state == State.Dead) return;

        _currentHealth -= amount;

        if (healthBarSlider != null)
            healthBarSlider.value = (float)_currentHealth / (float)maxHealth;

        if (_currentHealth <= 0) Die();
    }

    private void Die()
    {
        _state = State.Dead;
        _rb.linearVelocity = Vector2.zero;

        _playerScript?.AddPoints(pointsToGive);
        LevelManager.Instance?.OnEnemyKilled();
        Destroy(gameObject);
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, preferredRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
#endif
}
