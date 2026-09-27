using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum para tipos de formação
public enum FormationType
{
    Single,
    Line,           // Fila indiana (centopeia)
    DoubleLine,     // Duas duplas paralelas
    Triangle,       // Formação triangular
    StaggeredLine,  // Linha com delay entre unidades
    V,              // Formação em V
    HorizontalLine, // Linha horizontal
    TrainStack,     // Formação de trem/centopeia (inimigos se seguem)
    Circle          // Circular
}

// Enum para posições de spawn predefinidas
public enum SpawnPosition
{
    Top, TopLeft, TopRight,
    MiddleLeft, MiddleRight,
    Bottom, BottomLeft, BottomRight,
    Custom  // Para posições específicas
}

// Classe para representar um inimigo no padrão
[System.Serializable]
public class PatternEnemy
{
    public GameObject enemyPrefab;
    public float delayAfterSpawn = 0f;  // Delay após este inimigo
    public Vector2 offsetFromBase;       // Offset da posição base
    public bool useCustomSpeed = false;
    public float customSpeed = 5f;
}
