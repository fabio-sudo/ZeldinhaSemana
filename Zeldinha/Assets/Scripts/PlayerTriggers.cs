using UnityEngine;

public class PlayerTriggers : MonoBehaviour
{
    private GameManager gameManager;


    private void Start()
    {
        gameManager = FindFirstObjectByType(typeof(GameManager))as GameManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coletavel"))
        {
            gameManager.setGemas(1);
            Destroy(other.gameObject);
        }
    }



}
