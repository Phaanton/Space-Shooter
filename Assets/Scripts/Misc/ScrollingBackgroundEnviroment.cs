using UnityEngine;

public class ScrollingBackgroundEnviroment : MonoBehaviour
{
    [SerializeField] private GameObject[] backgroundLayers;
    [SerializeField] private float scrollSpeedFar = -0.5f;
    [SerializeField] private float scrollSpeedMid = -1f;
    [SerializeField] private float scrollSpeedClose = -1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        backgroundLayers[0].transform.position += new Vector3(0f, scrollSpeedFar * Time.deltaTime, 0f);
        backgroundLayers[1].transform.position += new Vector3(0f, scrollSpeedMid * Time.deltaTime, 0f);
        backgroundLayers[2].transform.position += new Vector3(0f, scrollSpeedClose * Time.deltaTime, 0f);

        if (backgroundLayers[0].transform.position.y <= -11f)
        {
            backgroundLayers[0].transform.position = new Vector3(0f, 11f, 0f);
        }

        if (backgroundLayers[1].transform.position.y <= -11f)
        {
            backgroundLayers[1].transform.position = new Vector3(0f, 11f, 0f);
        }

        if (backgroundLayers[2].transform.position.y <= -11f)
        {
            backgroundLayers[2].transform.position = new Vector3(0f, 11f, 0f);
        }
    }
}
