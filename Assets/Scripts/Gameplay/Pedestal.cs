using Unity.Cinemachine;
using UnityEngine;

public class Pedestal : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject teleportLocation;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private CamSwitcher camSwitcher;
    [SerializeField] private ShadowPuppet shadowPuppet;

    public void OnInteract()
    {
        Debug.Log("Interact Logged");
        CharacterController controller = playerObject.GetComponent<CharacterController>();
        controller.enabled = false;
        playerObject.transform.position = teleportLocation.transform.position;
        controller.enabled = true;
        camSwitcher.SwitchToCam(1, true);

        if (shadowPuppet != null)
        {
            ShadowRecorder.Instance.StartRecording();
            shadowPuppet.StartReplaying();
        }
    }
}
