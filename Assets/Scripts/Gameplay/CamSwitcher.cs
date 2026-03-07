using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CamSwitcher : MonoBehaviour
{
    List<CinemachineCamera> allCams;

    CinemachineCamera activeCam;

    [SerializeField] CinemachineCamera FreelookCam;
    [SerializeField] CinemachineCamera FollowCam;

    private void Awake()
    {
        if (allCams == null || allCams.Count == 0)
        {
            allCams = new List<CinemachineCamera> { FreelookCam, FollowCam };
            activeCam = FreelookCam;
        }
    }

    public void SwitchToCam(int cam, bool debug)
    {
        if (cam < 0 || cam >= allCams.Count)
        {
            if (debug) Debug.LogWarning("Invalid camera index.");
            return;
        }

        if (allCams[cam] == activeCam)
        {
            if (debug) Debug.Log("New Cam is Active Cam!");
            return;
        }

        activeCam.Priority = 0;
        allCams[cam].Priority = 1;
        activeCam = allCams[cam];

        if (debug) Debug.Log("Swapped to " + activeCam);
    }


    public void SwitchToCam(CinemachineCamera targetCam)
    {
        int index = allCams.IndexOf(targetCam);
        if (index == -1)
        {
            Debug.LogWarning("Camera not found.");
            return;
        }

        SwitchToCam(index, false);
    }

    public void SetFollowTarget(Transform newTarget, bool debug = false)
    {
        if (FreelookCam == null)
        {
            if (debug) Debug.LogError("Follow Camera reference is missing!");
            return;
        }

        FreelookCam.Follow = newTarget;
        FreelookCam.LookAt = newTarget;

        if (debug)
            Debug.Log("Follow Camera now tracks: " + newTarget.name);
    }

    public void NextCamera()
    {
        int next = (allCams.IndexOf(activeCam) + 1) % allCams.Count;
        SwitchToCam(next, false);
    }

    public void PreviousCamera()
    {
        int prev = (allCams.IndexOf(activeCam) - 1 + allCams.Count) % allCams.Count;
        SwitchToCam(prev, false);
    }

    private void OnDrawGizmos()
    {
        if (activeCam != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(activeCam.transform.position, 0.5f);
        }
    }
}
