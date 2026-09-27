using UnityEngine;

public class GamePlayScreen : MonoBehaviour
{
    public static GamePlayScreen Instance;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}

