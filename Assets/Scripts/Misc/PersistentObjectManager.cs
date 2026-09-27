using System.Collections.Generic;
using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private static Dictionary<string, GameObject> _instances = new Dictionary<string, GameObject>();
    [SerializeField] private string objectKey; // Chave única, ex: "Player", "AudioManager"

    void Awake()
    {
        if (string.IsNullOrEmpty(objectKey))
        {
            objectKey = gameObject.name;
        }

        // Verifica se já existe uma instância deste tipo
        if (_instances.ContainsKey(objectKey))
        {
            Debug.Log($"Duplicata de {objectKey} detectada, destruindo...");
            Destroy(gameObject);
        }
        else
        {
            _instances[objectKey] = gameObject;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"Objeto persistente ativado: {objectKey}");
        }
    }

    void OnDestroy()
    {
        if (_instances.ContainsKey(objectKey) && _instances[objectKey] == gameObject)
        {
            _instances.Remove(objectKey);
        }
    }
}
