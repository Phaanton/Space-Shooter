using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpawnPointMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private Vector2 moveDirection = Vector2.down;

    [Header("Configuração")]
    [SerializeField] private string spawnPointTag = "SpawnPoint";
    [SerializeField] private bool findAutomatically = true;

    private List<Transform> spawnPoints = new List<Transform>();

    void Start()
    {
        if (findAutomatically)
        {
            FindAllSpawnPoints();
        }
    }

    void FindAllSpawnPoints()
    {
        spawnPoints.Clear();

        // Encontra todos os objetos com a tag especificada
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(spawnPointTag);

        foreach (GameObject obj in taggedObjects)
        {
            spawnPoints.Add(obj.transform);
        }

        Debug.Log($"Encontrados {spawnPoints.Count} spawn points com a tag '{spawnPointTag}'");
    }

    void Update()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return;

        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                point.Translate(moveDirection * moveSpeed * Time.deltaTime);
            }
        }
    }

    // Para visualização no Editor
    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.cyan;
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }
    }
}
