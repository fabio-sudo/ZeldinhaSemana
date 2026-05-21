using UnityEngine;

public class RainManager : MonoBehaviour
{
    private GameManager gameManager;
    public bool isRain;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = FindFirstObjectByType(typeof(GameManager))as GameManager;  
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            gameManager.onOfRain(isRain);
        }
    }
}
