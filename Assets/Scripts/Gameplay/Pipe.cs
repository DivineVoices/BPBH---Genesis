using UnityEngine;

public class Pipe : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform exitPipe;
    [SerializeField] private GameObject playerObject;

    public void OnInteract()
    {
        CharacterController controller = playerObject.GetComponent<CharacterController>();
        controller.enabled = false;
        playerObject.transform.position = exitPipe.position;
        controller.enabled = true;
    }

    public Transform GetExit()
    {
        return exitPipe;
    }
}