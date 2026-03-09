using UnityEngine;

[System.Serializable]
public struct GhostFrame
{
    public Vector3 position;
    public Quaternion rotation;
    public float time;

    public GhostFrame(Vector3 pos, Quaternion rot, float t)
    {
        position = pos;
        rotation = rot;
        time = t;
    }
}