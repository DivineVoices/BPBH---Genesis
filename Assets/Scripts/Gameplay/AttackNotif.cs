using UnityEngine;
using UnityEngine.Events;

public class AttackNotif : MonoBehaviour
{
    [System.Serializable]
    public class AttackAnnounceEvent : UnityEvent { }

    public AttackAnnounceEvent OnAttackAnnounced;

    public void AnnounceAttack()
    {
        //Debug.Log("Event Announced");
        OnAttackAnnounced?.Invoke();
    }
}
