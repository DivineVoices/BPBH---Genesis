using UnityEngine;

public class WinPoint : MonoBehaviour
{
    [SerializeField] private GameObject worldSpawn;
    [SerializeField] private ShadowPuppet shadowPuppet;
    [SerializeField] private CamSwitcher camSwitcher;


    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            player.ChangeCheckPoint(worldSpawn);
            player.RespawnPlayer();
            ShadowRecorder.Instance.StopRecording(); 
            shadowPuppet.StopReplaying();
            ProgressionTracker.progressionLevel += 1;
            camSwitcher.SwitchToCam(0, true);
        }
    }
}
