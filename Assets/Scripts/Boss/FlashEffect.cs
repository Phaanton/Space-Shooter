using UnityEngine;

public class FlashEffect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var particle = GetComponentInChildren<ParticleSystem>();
        if (particle.isStopped)
        {
            Destroy(gameObject);
        }
    }
}
