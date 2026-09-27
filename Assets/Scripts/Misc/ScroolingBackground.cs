using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScroolingBackground : MonoBehaviour
{
    [SerializeField] private float scroolSpeed = 0.5f;
    [SerializeField] private Renderer backgroundRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        backgroundRenderer.material.mainTextureOffset += new Vector2(0f, scroolSpeed * Time.deltaTime);
    }
}
