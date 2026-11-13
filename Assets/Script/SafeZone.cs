using System.Collections.Generic;
using UnityEngine;

public class SafeZone : MonoBehaviour
{
    [Header("Drop Point")]
    public Transform dropPoint;              // Pwesto sa dock kung saan tatayo ang survivors
    public float standHeightOffset = 1f;     // Taas para lumapat ang talampakan (Default: 1 meter)

    private void OnTriggerEnter(Collider other)
    {
        BoatController boat = other.GetComponentInParent<BoatController>();

        if (boat == null && (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null))
        {
            boat = Object.FindAnyObjectByType<BoatController>();
        }

        if (boat != null && boat.rescuedPassengers.Count > 0)
        {
            UnloadSurvivors(boat);
        }
    }

    void UnloadSurvivors(BoatController boat)
    {
        List<Transform> survivors = boat.UnloadAllPassengers();

        Vector3 basePos = dropPoint != null ? dropPoint.position : transform.position;

        for (int i = 0; i < survivors.Count; i++)
        {
            if (survivors[i] != null)
            {
                // INAYOS DITO: Nagdagdag ng (Vector3.up * standHeightOffset) para nakatayo sa ibabaw ng sahig!
                Vector3 standPos = basePos + (transform.right * (i * 1.2f)) + (Vector3.up * standHeightOffset);
                survivors[i].position = standPos;
                survivors[i].rotation = transform.rotation;

                if (RescueManager.instance != null)
                {
                    RescueManager.instance.SurvivorDelivered();
                }

                
            }

        }
            if (TutorialTaskManager.instance != null && RescueManager.instance != null)
        {
            bool hasMore = RescueManager.instance.deliveredToSafeZone < RescueManager.instance.totalSurvivors;
            TutorialTaskManager.instance.OnSurvivorsDelivered(hasMore);
        }

        Debug.Log($"SUCCESS: Naihatid ang {survivors.Count} survivor(s) nang maayos sa Safe Zone!");
    }
}