using System.Collections;
using TMPro;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject[] enemyPrefabs;
        public int enemyCount = 10;
        public float spawnInterval = 1f;
    }

    public Wave[] waves;
    public TMP_Text waveText;

    public float timeBetweenWaves = 5f;
    public Transform[] spawnPoints;

    public bool allWavesCompleted = false;
    private int currentWaveIndex = 0;
    private bool isSpawning = false;

    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        while (currentWaveIndex < waves.Length)
        {
            Wave wave = waves[currentWaveIndex];

            UpdateWaveText();
            yield return new WaitForSeconds(timeBetweenWaves);

            yield return StartCoroutine(SpawnWave(wave));

            // Wait for all enemies to be cleared
            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length == 0);

            currentWaveIndex++;
        }

        WaveCompleted();
    }

    IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            GameObject prefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            if (prefab != null && spawnPoint != null)
            {
                Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                Debug.LogWarning("Missing enemy prefab or spawn point.");
            }

            yield return new WaitForSeconds(wave.spawnInterval);
        }

        isSpawning = false;
    }

    void UpdateWaveText()
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {currentWaveIndex + 1} / {waves.Length}";
        }
    }

    void WaveCompleted()
    {
        Debug.Log("All waves completed!");
        if (waveText != null)
            waveText.text = "All waves cleared!";

        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.OnWavesCompleted(); 
        }
    }
}

