using UnityEngine;
using UnityEngine.UIElements;

public class ShadowPuppet : MonoBehaviour
{
    [SerializeField] int delayFrames = 80;

    private bool isPlaying;

    void Update()
    {
        if (!isPlaying) return;

        var recorder = ShadowRecorder.Instance;

        if (recorder.positions.Count <= delayFrames)
            return;

        Vector3 targetPos = recorder.positions[recorder.positions.Count - delayFrames];
        Quaternion targetRot = recorder.rotations[recorder.rotations.Count - delayFrames];

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            15f * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            15f * Time.deltaTime
        );
    }

    public void StartReplaying()
    {
        isPlaying = true;
    }
    public void StopReplaying()
    {
        isPlaying = false;
    }

    public void ResetReplaying()
    {
        transform.position = new Vector3(-100, -100, -100);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        player.RespawnPlayer();
        ResetReplaying();
        ShadowRecorder.Instance.ResetRecording();
    }
}