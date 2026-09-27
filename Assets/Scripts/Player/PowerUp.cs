using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = new Vector2(0, -speed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroySelf()
    {
        if (transform.position.y < -6)
        {
            Destroy(gameObject);
        }
    }
}
