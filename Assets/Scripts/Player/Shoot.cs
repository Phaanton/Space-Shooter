using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    //[SerializeField] private GameObject impactVfx;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<Enemy>()?.TakeDamage(damage);
        }

        if (collision.CompareTag("Boss"))
        {
            collision.GetComponent<BossDrone>()?.TakeDamage(damage);
        }

        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>()?.TakeDamage(damage);
        }

        //Instantiate(impactVfx, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
