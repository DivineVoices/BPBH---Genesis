using System.Collections.Generic;
using UnityEngine;

public class ShadowRecorder : MonoBehaviour
{
    public static ShadowRecorder Instance;

    public List<Vector3> positions = new List<Vector3>();
    public List<Quaternion> rotations = new List<Quaternion>();

    [SerializeField] float recordInterval = 0.02f;

    float timer;
    private bool isRecording;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isRecording) return;

        timer += Time.deltaTime;

        if (timer >= recordInterval)
        {
            timer = 0f;

            positions.Add(transform.position);
            rotations.Add(transform.rotation);
        }

        if (positions.Count > 10000)
        {
            positions.RemoveAt(0);
            rotations.RemoveAt(0);
        }
    }

    public void StartRecording()
    {
        isRecording = true;
    }

    public void StopRecording()
    {
        isRecording = false;
    }

    public void ResetRecording()
    {
        positions.Clear(); rotations.Clear();
    }
}