using UnityEngine;

public class DynamicCam : MonoBehaviour
{

    [Header("Camera")]
    [SerializeField] GameObject camB;



    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "CamTrigger":
                camB.SetActive(true);
                break;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "CamTrigger":
                camB.SetActive(false);
                break;
        }
    }

}
