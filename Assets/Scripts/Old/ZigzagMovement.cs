using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZigzagMovement : MonoBehaviour
{
    [SerializeField] private float amplitude = 2f;
    [SerializeField] private float frequency = 5f;
    [SerializeField] private float direction = 1f;
    [SerializeField] private float verticalSpeed = 10f;

    private Rigidbody2D rb;
    private float zigzagTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        zigzagTimer += Time.deltaTime;
        float zigzagX = Mathf.Sin(zigzagTimer * frequency) * amplitude * direction;
        rb.linearVelocity = new Vector2(zigzagX, verticalSpeed);
    }

    public void Configure(float newAmplitude, float newFrequency, float newDirection)
    {
        amplitude = newAmplitude;
        frequency = newFrequency;
        direction = newDirection;
    }
}
