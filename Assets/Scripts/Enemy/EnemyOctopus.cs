using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyOctopus : EnemyParent
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = new Vector2(0, -speed);
    }

    // Update is called once per frame
    void Update()
    {
        Shoot();

        DestroySelf();
    }
}
