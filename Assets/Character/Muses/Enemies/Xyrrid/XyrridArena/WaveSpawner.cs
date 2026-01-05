using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    // Class kecil untuk mengatur settingan per wave di Inspector
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public GameObject enemyPrefab;
        public int count;
        public float rate; // Jarak waktu spawn antar musuh
    }

    public enum SpawnState { SPAWNING, WAITING, COUNTING }

    [Header("Settings")]
    public Wave[] waves; // List wave yang dinamis
    public Transform[] spawnPoints; // Titik-titik spawn musuh
    public float timeBetweenWaves = 5f;

    [Header("Item Drop")]
    public GameObject keyItemPrefab; // Drag prefab Kunci/Item ke sini

    [Header("Status (Don't Edit)")]
    public float waveCountdown;
    public SpawnState state = SpawnState.COUNTING;
    private int nextWave = 0;
    private float searchCountdown = 1f;

    void Start()
    {
        waveCountdown = timeBetweenWaves;
    }

    void Update()
    {
        // Cek apakah musuh sudah habis (hanya saat status WAITING)
        if (state == SpawnState.WAITING)
        {
            if (!EnemyIsAlive())
            {
                WaveCompleted();
            }
            else
            {
                return; // Masih ada musuh, jangan lanjut code di bawah
            }
        }

        // Hitung mundur untuk wave berikutnya
        if (waveCountdown <= 0)
        {
            if (state != SpawnState.SPAWNING)
            {
                // Mulai spawn wave
                StartCoroutine(SpawnWave(waves[nextWave]));
            }
        }
        else
        {
            waveCountdown -= Time.deltaTime;
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
            // Logic untuk menang level bisa ditaruh di sini
            // enabled = false; // Matikan spawner
        }
        else
        {
            nextWave++;
        }
    }

    bool EnemyIsAlive()
    {
        // Cek musuh setiap 1 detik agar tidak berat performanya
        searchCountdown -= Time.deltaTime;
        if (searchCountdown <= 0f)
        {
            searchCountdown = 1f;
            if (GameObject.FindGameObjectWithTag("Enemy") == null)
            {
                return false;
            }
        }
        return true;
    }

    IEnumerator SpawnWave(Wave _wave)
    {
        Debug.Log("Spawning Wave: " + _wave.waveName);
        state = SpawnState.SPAWNING;

        // Loop sebanyak jumlah musuh di wave tersebut
        for (int i = 0; i < _wave.count; i++)
        {
            // Cek Logic Kunci:
            // Apakah ini Wave Terakhir? DAN Apakah ini Musuh Terakhir di loop?
            bool isLastWave = (nextWave == waves.Length - 1);
            bool isLastEnemy = (i == _wave.count - 1);
            bool shouldDropKey = (isLastWave && isLastEnemy);

            SpawnEnemy(_wave.enemyPrefab, shouldDropKey);

            yield return new WaitForSeconds(1f / _wave.rate);
        }

        state = SpawnState.WAITING;
        yield break;
    }

    void SpawnEnemy(GameObject _enemy, bool dropsKey)
    {
        // Pilih spawn point secara acak
        Transform _sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Instantiate musuh
        GameObject newEnemy = Instantiate(_enemy, _sp.position, _sp.rotation);

        // Jika ini musuh penentu, kita inject item kuncinya ke script EnemyHealth
        if (dropsKey)
        {
            EnemyHealth eHealth = newEnemy.GetComponent<EnemyHealth>();
            if (eHealth != null)
            {
                eHealth.itemToDrop = keyItemPrefab;
                Debug.Log("Key has been injected to the final enemy!");
            }
            else
            {
                Debug.LogWarning("Enemy tidak punya script EnemyHealth!");
            }
        }
    }
}