using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPoint
    {
        public Transform transform;
        public bool isOccupied;
    }

    [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    public static SpawnManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Transform GetRandomSpawnPoint()
    {
        List<SpawnPoint> availablePoints = new List<SpawnPoint>();

        // Ищем свободные точки спавна
        foreach (var point in spawnPoints)
        {
            if (!point.isOccupied)
            {
                availablePoints.Add(point);
            }
        }

        // Если все заняты, возвращаем случайную
        if (availablePoints.Count == 0)
        {
            Debug.LogWarning("Все точки спавна заняты! Возвращаем случайную.");
            return spawnPoints[Random.Range(0, spawnPoints.Count)].transform;
        }

        // Выбираем случайную свободную точку
        SpawnPoint selected = availablePoints[Random.Range(0, availablePoints.Count)];
        selected.isOccupied = true;

        // Через 5 секунд точка снова станет свободной (на случай отключения игрока)
        StartCoroutine(ReleaseSpawnPointRoutine(selected, 5f));

        return selected.transform;
    }

    private IEnumerator ReleaseSpawnPointRoutine(SpawnPoint point, float delay)
    {
        yield return new WaitForSeconds(delay);
        point.isOccupied = false;
    }
}
