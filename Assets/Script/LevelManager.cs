using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Estructura de niveles")]
    public int wavesPerLevel = 5;
    public int totalLevels   = 3;

    [Header("Enemigos por oleada (kills necesarios)")]
    public int[] waveKillQuota = { 3, 6, 10, 15, 20 };        // más asequible al inicio

    [Header("Segundos entre spawns por oleada")]
    public float[] waveSpawnRate = { 3f, 2.5f, 2f, 1.5f, 1f }; // más lento al inicio

    [Header("UI (asignar en Inspector)")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI levelText;

    private int          _wave;
    private int          _level = 1;
    private int          _killed;
    private int          _quota;
    private EnemySpawner _spawner;
    private Player       _player;

    public int CurrentWave  => _wave + 1;
    public int CurrentLevel => _level;

    private void Awake() => Instance = this;

    private void Start()
    {
        _spawner = FindFirstObjectByType<EnemySpawner>();
        _player  = FindFirstObjectByType<Player>();
        BeginWave(0);
    }

    public void OnEnemyKilled()
    {
        _killed++;
        if (_killed >= _quota) NextWave();
    }

    private void BeginWave(int index)
    {
        _wave   = index;
        _killed = 0;
        _quota  = waveKillQuota[Mathf.Min(index, waveKillQuota.Length - 1)];

        if (_spawner != null)
            _spawner.spawnRate = waveSpawnRate[Mathf.Min(index, waveSpawnRate.Length - 1)];

        RefreshUI();
    }

    private void NextWave()
    {
        // Mejora al jugador al completar cada oleada
        _player?.ApplyWaveProgression(_wave + 1, _level);

        if (_wave + 1 >= wavesPerLevel)
        {
            _level++;
            if (_level > totalLevels) { Debug.Log("¡Juego completado!"); return; }
            BeginWave(0);
        }
        else
        {
            BeginWave(_wave + 1);
        }
    }

    private void RefreshUI()
    {
        if (waveText  != null) waveText.text  = $"WAVE {_wave + 1}/{wavesPerLevel}";
        if (levelText != null) levelText.text = $"LEVEL {_level}";
    }
}
