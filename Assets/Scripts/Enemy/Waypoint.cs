using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[Serializable]
public class Waypoint
{
    public string pointName = "Waypoint";
    public Vector2 position;           // Posição X, Y
    public float waitTime = 0f;         // Tempo que espera neste ponto
    public float speed = -1f;           // Velocidade para este trecho (-1 = usa velocidade padrão)

    // Construtor
    public Waypoint(Vector2 pos, float wait = 0f, float spd = -1f)
    {
        position = pos;
        waitTime = wait;
        speed = spd;
    }
}
