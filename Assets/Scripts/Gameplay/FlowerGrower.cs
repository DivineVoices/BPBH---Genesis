using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class FlowerGrower : MonoBehaviour
{
    //[SerializeField] private int appearanceStage;
    [SerializeField] private List<GameObject> flowerStages = new List<GameObject>();

    private void Update()
    {
         flowerStages[((int)ProgressionTracker.progressionLevel)].SetActive(true);
    }
}
