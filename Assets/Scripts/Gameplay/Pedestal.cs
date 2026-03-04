using UnityEngine;

public class Pedestal : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject teleportLocation;
    [SerializeField] private GameObject playerObject;
    public void OnInteract()
    {
        Debug.Log("Interact Logged");
        CharacterController controller = playerObject.GetComponent<CharacterController>();
        controller.enabled = false;
        playerObject.transform.position = teleportLocation.transform.position;
        controller.enabled = true;
    }
}
