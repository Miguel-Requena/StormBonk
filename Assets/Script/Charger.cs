using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Charger : MonoBehaviour
{
    private enum State { Idle, Prepare, Charge, Stunned, Dead }
    private State _state = State.Idle;

    [Header("Estadísticas")]
    public int health = 40;
    public int damageToPlayer = 15;
    public int pointsToGive = 20;

    [Header("Comportamiento")]
    public float visionRange = 12f;
    public float prepareTime = 1f;
    public float chargeDuration = 3f;
    public float chargeSpeed = 7f;
    public float stunDuration = 1f;
    public float walkSpeed = 1.5f;
    public float chargeOvershoot = 2f;

    [Header("Stun al jugador")]
    [Tooltip("Segundos que el jugador no puede moverse tras recibir la embestida.")]
    public float playerStunDuration = 1f;

    [Header("Aturdimiento del Charger")]
    public float stunDamageMultiplier = 2f;

    private Rigidbody2D _rb;
    private Transform   _player;

    private float   _stateTimer;
    private Vector2 _chargeDirection;
    private float   _chargeMaxDistance;
    private Vector2 _chargeStartPos;
    private bool    _hitPlayerThisCharge;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void Start()
    {
        GameObject p = GameObject.Find("Player");
        if (p != null) _player = p.transform;
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
        _stateTimer -= Time.deltaTime;

        switch (_state)
        {
            case State.Idle:    UpdateIdle(dist); break;
            case State.Prepare: UpdatePrepare();  break;
            case State.Charge:  UpdateCharge();   break;
            case State.Stunned: UpdateStunned();  break;
        }
    }

    private void UpdateIdle(float dist)
    {
        if (dist > visionRange) { _rb.linearVelocity = Vector2.zero; return; }

        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        _rb.linearVelocity = dir * walkSpeed;

        if (dist <= visionRange * 0.6f) EnterPrepare();
    }

    private void UpdatePrepare()
    {
        _rb.linearVelocity = Vector2.zero;
        if (_stateTimer <= 0f) EnterCharge();
    }

    private void UpdateCharge()
    {
        _rb.linearVelocity = _chargeDirection * chargeSpeed;

        float traveled = Vector2.Distance((Vector2)transform.position, _chargeStartPos);
        if (traveled >= _chargeMaxDistance || _stateTimer <= 0f)
            EnterStunned();
    }

    private void UpdateStunned()
    {
        _rb.linearVelocity = Vector2.zero;
        if (_stateTimer <= 0f) _state = State.Idle;
    }

    private void EnterPrepare()
    {
        _state      = State.Prepare;
        _stateTimer = prepareTime;
        _rb.linearVelocity = Vector2.zero;
    }

    private void EnterCharge()
    {
        _state               = State.Charge;
        _stateTimer          = chargeDuration;
        _hitPlayerThisCharge = false;
        _chargeStartPos      = transform.position;

        Vector2 toPlayer   = (Vector2)_player.position - (Vector2)transform.position;
        _chargeDirection   = toPlayer.normalized;
        _chargeMaxDistance = toPlayer.magnitude + chargeOvershoot;
    }

    private void EnterStunned()
    {
        _state      = State.Stunned;
        _stateTimer = stunDuration;
        _rb.linearVelocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_state != State.Charge) return;

        if (collision.gameObject.CompareTag("Player") && !_hitPlayerThisCharge)
        {
            Player playerScript = collision.gameObject.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damageToPlayer);
                playerScript.Stun(playerStunDuration); // ← jugador no se puede mover
            }
            _hitPlayerThisCharge = true;
            EnterStunned();
            return;
        }

        if (!collision.gameObject.CompareTag("Player") &&
            !collision.gameObject.CompareTag("Enemy"))
        {
            EnterStunned();
        }
    }

    public void TakeDamage(int amount)
    {
        if (_state == State.Dead) return;

        int finalDamage = _state == State.Stunned
            ? Mathf.RoundToInt(amount * stunDamageMultiplier)
            : amount;

        health -= finalDamage;
        if (health <= 0) Die();
    }

    private void Die()
    {
        _state = State.Dead;
        _rb.linearVelocity = Vector2.zero;
        _player?.GetComponent<Player>()?.AddPoints(pointsToGive);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange * 0.6f);
        if (Application.isPlaying && _state == State.Charge)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, _chargeDirection * _chargeMaxDistance);
        }
    }
#endif
}
