using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    public GameObject enemyPrefab;
    public float spawnRate = 1.5f;     // Cada cuántos segundos aparece un enemigo
    public int maxEnemies = 30;        // Límite máximo de enemigos en pantalla
    public float spawnRadius = 10f;    // Distancia a la que aparecen del jugador

    private float nextSpawnTime = 0f;
    private Transform player;

    void Start()
    {
        // Buscamos al jugador al inicio
        GameObject p = GameObject.Find("Player");
        if (p != null)
        {
            player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Contamos cuántos enemigos hay activos usando su Tag
        int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;

        // Si toca spawnear y no hemos superado el límite
        if (Time.time >= nextSpawnTime && currentEnemies < maxEnemies)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnEnemy()
    {
        // Genera una dirección aleatoria en un círculo
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // Calculamos la posición final (posición del jugador + la dirección * el radio)
        Vector2 spawnPosition = (Vector2)player.position + (randomDirection * spawnRadius);

        // Creamos al enemigo
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}