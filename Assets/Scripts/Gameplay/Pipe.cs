using UnityEngine;

public class Pipe : MonoBehaviour
{
    [SerializeField] private Transform exitPipe;
    [SerializeField] private float activationRange = 2f;

    public bool CanEnter(Transform player)
    {
        return Vector3.Distance(player.position, transform.position) <= activationRange;
    }

    public Transform GetExit()
    {
        return exitPipe;
    }
}