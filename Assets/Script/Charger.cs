using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// HFSM (Hierarchical Finite State Machine)
//
// Nivel 1 (TopState)  →  Alive  |  Dead
// Nivel 2 (AliveState)→  Wander  →  Approach   →  Prepare
//                         └─ Charge ------- Stunned ─┘
// El Blackboard decide si el Charger tiene "turno" de embestir.
[RequireComponent(typeof(Rigidbody2D))]
public class Charger : MonoBehaviour
{
    // Estados
    private enum TopState   { Alive, Dead }
    private enum AliveState { Wander, Approach, Prepare, Charge, Stunned }

    private TopState   _top   = TopState.Alive;
    private AliveState _alive = AliveState.Approach;

    // Inspector
    [Header("Estadísticas")]
    public int   maxHealth      = 40;
    public int   damageToPlayer = 15;
    public int   pointsToGive   = 20;

    [Header("Interfaz de Usuario")]
    [SerializeField] private Slider healthBarSlider;

    [Header("Comportamiento")]
    public float visionRange     = 12f;
    public float prepareTime     = 1f;
    public float chargeDuration  = 3f;
    public float chargeSpeed     = 7f;
    public float stunDuration    = 1f;
    public float walkSpeed       = 1.5f;
    public float chargeOvershoot = 2f;

    [Header("Stun al jugador")]
    public float playerStunDuration = 1f;

    [Header("Aturdimiento del Charger")]
    public float stunDamageMultiplier = 2f;

    [Header("Pathfinding A*")]
    public float pathUpdateInterval = 0.3f;

    // Privados
    private int   _currentHealth;
    private float _stateTimer;

    private Rigidbody2D _rb;
    private Transform   _player;
    private Player      _playerScript;

    // Charge data
    private Vector2 _chargeDirection;
    private float   _chargeMaxDistance;
    private Vector2 _chargeStartPos;
    private bool    _hitPlayerThisCharge;
    private bool    _chargeGranted;

    // A* data
    private List<Vector2> _path;
    private int            _pathIndex;
    private float          _pathTimer;

    // Unity callbacks
    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void Start()
    {
        _currentHealth = maxHealth;
        if (healthBarSlider != null) { healthBarSlider.minValue = 0; healthBarSlider.maxValue = 1; healthBarSlider.value = 1; }

        GameObject p = GameObject.Find("Player");
        if (p != null) { _player = p.transform; _playerScript = p.GetComponent<Player>(); }
    }

    private void Update()
    {
        EnemyBlackboard.Tick();

        switch (_top)
        {
            case TopState.Alive: UpdateAlive(); break;
        }
    }

    // Nivel 1: Alive
    private void UpdateAlive()
    {
        if (_player == null || !_player.gameObject.activeInHierarchy) { _rb.linearVelocity = Vector2.zero; return; }

        EnemyBlackboard.PlayerPosition = _player.position;

        float dist   = Vector2.Distance(transform.position, _player.position);
        _stateTimer -= Time.deltaTime;

        switch (_alive)
        {
            case AliveState.Wander:   UpdateWander(dist);   break;
            case AliveState.Approach: UpdateApproach(dist); break;
            case AliveState.Prepare:  UpdatePrepare();      break;
            case AliveState.Charge:   UpdateCharge();       break;
            case AliveState.Stunned:  UpdateStunned();      break;
        }
    }

    // Nivel 2: sub-estados

    private void UpdateWander(float dist)
    {
        if (dist > visionRange) { _rb.linearVelocity = Vector2.zero; return; }
        EnterAlive(AliveState.Approach);
    }

    private void UpdateApproach(float dist)
    {
        FollowPath(_player.position, walkSpeed);

        if (dist <= visionRange * 0.6f)
        {
            if (EnemyBlackboard.RequestCharge())
            {
                _chargeGranted = true;
                EnterAlive(AliveState.Prepare);
            }
        }
    }

    private void UpdatePrepare()
    {
        _rb.linearVelocity = Vector2.zero;
        if (_stateTimer <= 0f) EnterAlive(AliveState.Charge);
    }

    private void UpdateCharge()
    {
        _rb.linearVelocity = _chargeDirection * chargeSpeed;
        float traveled = Vector2.Distance(transform.position, _chargeStartPos);
        if (traveled >= _chargeMaxDistance || _stateTimer <= 0f)
            EnterAlive(AliveState.Stunned);
    }

    private void UpdateStunned()
    {
        _rb.linearVelocity = Vector2.zero;
        if (_stateTimer <= 0f) EnterAlive(AliveState.Approach);
    }

    // Transiciones
    private void EnterAlive(AliveState next)
    {
        if (next != AliveState.Charge && next != AliveState.Stunned && _chargeGranted)
        {
            EnemyBlackboard.ReleaseCharge();
            _chargeGranted = false;
        }

        _alive = next;

        switch (next)
        {
            case AliveState.Approach:
                _pathTimer = 0f;   // fuerza recálculo inmediato del path
                break;

            case AliveState.Prepare:
                _stateTimer = prepareTime;
                _rb.linearVelocity = Vector2.zero;
                break;

            case AliveState.Charge:
                _stateTimer           = chargeDuration;
                _hitPlayerThisCharge  = false;
                _chargeStartPos       = transform.position;
                Vector2 toPlayer      = (Vector2)_player.position - (Vector2)transform.position;
                _chargeDirection      = toPlayer.normalized;
                _chargeMaxDistance    = toPlayer.magnitude + chargeOvershoot;
                break;

            case AliveState.Stunned:
                _stateTimer = stunDuration;
                _rb.linearVelocity = Vector2.zero;
                if (_chargeGranted) { EnemyBlackboard.ReleaseCharge(); _chargeGranted = false; }
                break;
        }
    }

    // Pathfinding A*
    private void FollowPath(Vector2 destination, float speed)
    {
        _pathTimer -= Time.deltaTime;
        if (_pathTimer <= 0f && PathfindingGrid.Instance != null)
        {
            _path      = PathfindingGrid.Instance.FindPath(transform.position, destination);
            _pathIndex = 0;
            _pathTimer = pathUpdateInterval;
        }

        Vector2 dir;
        if (_path != null && _pathIndex < _path.Count)
        {
            Vector2 wp = _path[_pathIndex];
            dir = (wp - (Vector2)transform.position).normalized;
            if (Vector2.Distance(transform.position, wp) < 0.35f) _pathIndex++;
        }
        else
        {
            dir = ((Vector2)destination - (Vector2)transform.position).normalized;
        }

        _rb.linearVelocity = dir * speed;
    }

    // Colisiones
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_alive != AliveState.Charge) return;

        if (col.gameObject.CompareTag("Player") && !_hitPlayerThisCharge)
        {
            _playerScript?.TakeDamage(damageToPlayer);
            _playerScript?.Stun(playerStunDuration);
            EnemyBlackboard.SetPlayerStunned(playerStunDuration);
            _hitPlayerThisCharge = true;
            EnterAlive(AliveState.Stunned);
            return;
        }

        if (!col.gameObject.CompareTag("Player") && !col.gameObject.CompareTag("Enemy"))
            EnterAlive(AliveState.Stunned);
    }

    // Daño
    public void TakeDamage(int amount)
    {
        if (_top == TopState.Dead) return;

        int dmg = _alive == AliveState.Stunned ? Mathf.RoundToInt(amount * stunDamageMultiplier) : amount;
        _currentHealth -= dmg;

        if (healthBarSlider != null)
            healthBarSlider.value = (float)_currentHealth / maxHealth;

        if (_currentHealth <= 0) Die();
    }

    public bool IsDead() => _top == TopState.Dead;

    private void Die()
    {
        _top = TopState.Dead;
        _rb.linearVelocity = Vector2.zero;

        if (_chargeGranted) { EnemyBlackboard.ReleaseCharge(); _chargeGranted = false; }
        if (healthBarSlider != null) healthBarSlider.gameObject.SetActive(false);

        _playerScript?.AddPoints(pointsToGive);
        LevelManager.Instance?.OnEnemyKilled();
        Destroy(gameObject, 0.1f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange * 0.6f);
        if (Application.isPlaying && _alive == AliveState.Charge)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, _chargeDirection * _chargeMaxDistance);
        }
    }
#endif
}
