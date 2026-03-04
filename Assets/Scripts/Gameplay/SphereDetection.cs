using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class SphereDetection : MonoBehaviour
{
    public GameObject SphereDetectClosest(LayerMask layers, float detectionRange)
    {
        Vector3 origin = transform.position;

        //Debug.Log("Interact!");
        Collider[] colliders = Physics.OverlapSphere(origin, detectionRange, layers);

        if (colliders.Length == 0)
        {
            Debug.Log("Nothing in Range");
            return null;
        }

        Collider closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            float dist = Vector3.Distance(origin, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = col;
            }
        }

        if (closest != null)
        {
            return closest.gameObject;
        }

        return null;
    }

    public List<GameObject> SphereCastAhead(LayerMask layers, float detectionRange, int range)
    {
        Vector3 origin = transform.position + transform.forward * range;
        List<GameObject> ObjectList = new List<GameObject>();

        //Debug.Log("Interact!");
        Collider[] colliders = Physics.OverlapSphere(origin, detectionRange, layers);

        if (colliders.Length == 0)
        {
            Debug.Log("Nothing in Range");
            return null;
        }
        else
        {
            foreach (Collider col in colliders)
                ObjectList.Add(col.gameObject);
            return ObjectList;
        }
    }

    public List<GameObject> SphereDetectAll(LayerMask layers, float detectionRange)
    {
        Vector3 origin = transform.position;
        List<GameObject> ObjectList = new List<GameObject>();

        //Debug.Log("Interact!");
        Collider[] colliders = Physics.OverlapSphere(origin, detectionRange, layers);

        if (colliders.Length == 0)
        {
            Debug.Log("Nothing in Range");
            return null;
        }
        else
        {
            foreach (Collider col in colliders)
                ObjectList.Add(col.gameObject);
            return ObjectList;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        //Gizmos.DrawWireSphere(origin, 5f);
        Gizmos.DrawWireSphere(origin + transform.forward * 2, 2f);
    }
}

