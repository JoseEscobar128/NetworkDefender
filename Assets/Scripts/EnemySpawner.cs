using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab del enemigo (se asigna en el inspector)
    public float cloneInterval = 5f; // Tiempo entre clones
    public int maxClones = 5; // Máximo de clones permitidos

    private int cloneCount = 0;

    void Start()
    {
        // Empieza a clonarse cada X segundos
        InvokeRepeating("CloneEnemy", cloneInterval, cloneInterval);
    }

    void CloneEnemy()
    {
        // No clonarse infinitamente
        if (cloneCount >= maxClones) return;

        // Crear un nuevo enemigo cerca del original
        Vector3 spawnPos = transform.position + new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0
        );

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        cloneCount++;
    }
}
