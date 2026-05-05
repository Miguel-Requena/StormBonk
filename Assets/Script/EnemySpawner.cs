using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab de enemigo")]
    public GameObject enemyPrefab;

    [Header("Ritmo de spawn")]
    public float spawnRate  = 3f;   // LevelManager lo sobreescribe por oleada
    public int   maxEnemies = 15;   // cap inicial; más bajo = más manejable

    [Header("Posicionamiento en el tilemap")]
    [Tooltip("Distancia mínima al jugador para que aparezca un enemigo")]
    public float minSpawnDistance = 7f;
    [Tooltip("Distancia máxima al jugador para que aparezca un enemigo")]
    public float maxSpawnDistance = 14f;

    private float     _nextSpawnTime;
    private Transform _player;

    private void Start()
    {
        GameObject p = GameObject.Find("Player");
        if (p != null) _player = p.transform;
    }

    private void Update()
    {
        if (_player == null || enemyPrefab == null) return;
        if (Time.time < _nextSpawnTime) return;
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies) return;

        Instantiate(enemyPrefab, GetSpawnPosition(), Quaternion.identity);
        _nextSpawnTime = Time.time + spawnRate;
    }

    private Vector2 GetSpawnPosition()
    {
        // Prioridad: nodo walkable del PathfindingGrid dentro del rango de distancia
        if (PathfindingGrid.Instance != null)
        {
            List<Vector2> candidates = PathfindingGrid.Instance.GetSpawnCandidates(
                _player.position, minSpawnDistance, maxSpawnDistance);

            if (candidates != null && candidates.Count > 0)
                return candidates[Random.Range(0, candidates.Count)];
        }

        // Fallback sin grid: dirección aleatoria a distancia mínima
        return (Vector2)_player.position + Random.insideUnitCircle.normalized * minSpawnDistance;
    }
}
