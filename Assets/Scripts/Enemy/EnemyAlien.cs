using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAlien : EnemyParent
{
    private Rigidbody2D rb;
    private bool hasChangedDirection = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = new Vector2(0, -speed);
    }

    // Update is called once per frame
    void Update()
    {
        ChangeDirection();

        Shoot(true);

        DestroySelf();
    }

    private void ChangeDirection()
    {
        if (!hasChangedDirection)
        {
            if (transform.position.y < 2.3)
            {
                if (transform.position.x > 0)
                {
                    rb.linearVelocity = new Vector2(-speed, -speed);
                    hasChangedDirection = true;
                }
                else
                {
                    rb.linearVelocity = new Vector2(speed, -speed);
                    hasChangedDirection = true;
                }
            }
        }
    }
}
