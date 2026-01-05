using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    // Singleton agar EnemyHealth bisa melapor tanpa ribet
    public static WaveSpawner instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public GameObject enemyPrefab;
        public int count;
        public float rate;
    }

    public enum SpawnState { SPAWNING, WAITING, COUNTING }

    [Header("Settings")]
    public Wave[] waves;
    public Transform[] spawnPoints;
    public float timeBetweenWaves = 5f;

    [Header("Key Item")]
    public GameObject keyItemPrefab; // Drag Item Kunci ke sini

    [Header("Status")]
    public float waveCountdown;
    public SpawnState state = SpawnState.COUNTING;

    private int nextWave = 0;

    // Variable untuk melacak jumlah musuh yang masih hidup/belum mati
    private int enemiesAlive = 0;

    void Start()
    {
        waveCountdown = timeBetweenWaves;
    }

    void Update()
    {
        if (state == SpawnState.WAITING)
        {
            // Kita tidak perlu lagi mengecek setiap frame dengan FindObject
            // Karena musuh akan melapor saat mati (Event Driven)
            if (enemiesAlive == 0)
            {
                WaveCompleted();
            }
            else
            {
                return;
            }
        }

        if (waveCountdown <= 0)
        {
            if (state != SpawnState.SPAWNING)
            {
                StartCoroutine(SpawnWave(waves[nextWave]));
            }
        }
        else
        {
            waveCountdown -= Time.deltaTime;
        }
    }

    // Fungsi ini DIPANGGIL oleh EnemyHealth saat musuh mati
    public void OnEnemyKilled(Vector3 deathPosition)
    {
        // Kurangi jumlah musuh hidup
        enemiesAlive--;

        // LOGIC DROP KEY
        // Jika musuh habis (0) DAN ini adalah Wave Terakhir
        if (enemiesAlive <= 0 && nextWave == waves.Length - 1)
        {
            DropKey(deathPosition);
        }
    }

    void DropKey(Vector3 pos)
    {
        if (keyItemPrefab != null)
        {
            Instantiate(keyItemPrefab, pos, Quaternion.identity);
            Debug.Log("Last Enemy Killed! Key Dropped at " + pos);
        }
    }

    void WaveCompleted()
    {
        Debug.Log("Wave Completed!");
        state = SpawnState.COUNTING;
        waveCountdown = timeBetweenWaves;

        if (nextWave + 1 > waves.Length - 1)
        {
            Debug.Log("ALL WAVES COMPLETE! Level Finished.");
            // nextWave = 0; // Uncomment jika ingin looping dari awal
            enabled = false; // Uncomment jika ingin stop script
        }
        else
        {
            nextWave++;
        }
    }

    IEnumerator SpawnWave(Wave _wave)
    {
        Debug.Log("Spawning Wave: " + _wave.waveName);
        state = SpawnState.SPAWNING;

        // Set jumlah musuh yang harus dibunuh player
        enemiesAlive = _wave.count;

        for (int i = 0; i < _wave.count; i++)
        {
            SpawnEnemy(_wave.enemyPrefab);
            yield return new WaitForSeconds(1f / _wave.rate);
        }

        state = SpawnState.WAITING;
        yield break;
    }

    void SpawnEnemy(GameObject _enemy)
    {
        Transform _sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(_enemy, _sp.position, _sp.rotation);
    }
}