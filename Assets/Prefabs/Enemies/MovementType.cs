using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType
{
    Simple,        // Movimento simples para baixo
    Diagonal,      // Movimento diagonal
    CurveExit,     // Padrão 1: Desce e sai com curva
    Arc,           // Padrão 2: Movimento em arco
    LShape,        // Padrão 3: Movimento em "L"
    ZigZag,        // Padrão 4: Zig-zag
    Decelerate,    // Padrão 5: Desacelera e para
    Train          // Padrão 6: Movimento de trem/centopeia
}
