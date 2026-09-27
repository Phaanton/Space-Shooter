using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipShooting : MonoBehaviour
{
    public AudioSource shootingSound;

    void Shoot()
    {
        // Só dá Play - o volume é controlado pelo AudioMixer!
        shootingSound.Play();
    }
}
