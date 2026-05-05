using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeleeHunter : MonoBehaviour
{
    [Header("Estadísticas del Enemigo")]
    public float moveSpeed = 2f;
    public int maxHealth = 10;
    private int currentHealth;
    public int damageToPlayer = 10;
    public int pointsToGive = 5;
    public float damageRate = 1f;

    [Header("Lógica Difusa")]
    public float speedCauteloso = 0.8f;
    public float speedNormal    = 2f;
    public float speedAgresivo  = 3f;
    public float speedFrenético = 5f;
    [Tooltip("Multiplicador cuando el jugador está aturdido (dato leído del Blackboard)")]
    public float stunnedBonus = 1.5f;

    [Header("Pathfinding A*")]
    public float pathUpdateInterval = 0.4f;

    private float nextDamageTime = 0f;

    private Rigidbody2D rb;
    private Transform   target;
    private Player      playerScript;
    private Vector2     moveDirection;
    private Animator    anim;

    // A* state
    private List<Vector2> _path;
    private int            _pathIndex;
    private float          _pathTimer;

    [Header("Interfaz")]
    [SerializeField] private Slider healthBarSlider;

    private bool isDead = false;
    public bool IsDead() => isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        if (healthBarSlider == null)
            healthBarSlider = GetComponentInChildren<Slider>();
    }

    void Start()
    {
        UpdateVisualHealthBar();
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            target       = player.transform;
            playerScript = player.GetComponent<Player>();
        }
    }

    private float FuzzySpeed()
    {
        float h = (float)currentHealth / maxHealth;
        float p = playerScript != null
            ? (float)playerScript.currentHealth / playerScript.maxHealth
            : 1f;

        float muSano     = h;       float muHerido   = 1f - h;
        float muSanoPJ   = p;       float muMuertoPJ = 1f - p;

        float wCauteloso = Mathf.Min(muHerido, muSanoPJ);
        float wNormal    = Mathf.Min(muHerido, muMuertoPJ);
        float wAgresivo  = Mathf.Min(muSano,   muSanoPJ);
        float wFrenetico = Mathf.Min(muSano,   muMuertoPJ);

        float total = wCauteloso + wNormal + wAgresivo + wFrenetico;
        if (total < 0.001f) return moveSpeed;

        float speed = (wCauteloso  * speedCauteloso +
                       wNormal     * speedNormal    +
                       wAgresivo   * speedAgresivo  +
                       wFrenetico  * speedFrenético) / total;

        if (EnemyBlackboard.PlayerIsStunned) speed *= stunnedBonus;
        return speed;
    }

    void Update()
    {
        if (isDead) return;
        if (target == null || !target.gameObject.activeInHierarchy) { moveDirection = Vector2.zero; return; }

        EnemyBlackboard.PlayerPosition    = target.position;
        if (playerScript != null)
            EnemyBlackboard.PlayerHealthRatio = (float)playerScript.currentHealth / playerScript.maxHealth;

        _pathTimer -= Time.deltaTime;
        if (_pathTimer <= 0f)
        {
            _pathTimer = pathUpdateInterval;
            if (PathfindingGrid.Instance != null)
            {
                _path      = PathfindingGrid.Instance.FindPath(transform.position, target.position);
                _pathIndex = 0;
            }
        }

        if (_path != null && _pathIndex < _path.Count)
        {
            Vector2 wp = _path[_pathIndex];
            moveDirection = (wp - (Vector2)transform.position).normalized;
            if (Vector2.Distance(transform.position, wp) < nodeReachThreshold)
                _pathIndex++;
        }
        else
        {
            moveDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
        }
    }

    private const float nodeReachThreshold = 0.35f;

    private void FixedUpdate()
    {
        if (isDead) return;

        if (target != null && target.gameObject.activeInHierarchy)
            rb.linearVelocity = moveDirection * FuzzySpeed();
        else
            rb.linearVelocity = Vector2.zero;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        UpdateVisualHealthBar();
        anim.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            isDead = true;
            anim.SetBool("isDead", true);
            rb.linearVelocity = Vector2.zero;
            GetComponent<Collider2D>().enabled = false;
            playerScript?.AddPoints(pointsToGive);
            LevelManager.Instance?.OnEnemyKilled();
            Destroy(gameObject, 1.2f);
        }
    }

    private void UpdateVisualHealthBar()
    {
        if (healthBarSlider != null)
            healthBarSlider.value = (float)currentHealth / (float)maxHealth;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= nextDamageTime && playerScript != null)
            {
                playerScript.TakeDamage(damageToPlayer);
                nextDamageTime = Time.time + damageRate;
            }
        }
    }
}
