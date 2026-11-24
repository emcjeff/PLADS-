using System.Collections.Generic;
using UnityEngine;

public class SafeZone : MonoBehaviour
{
    [Header("Drop Point References")]
    [Tooltip("Pangunahing pwesto kung saan magsisimula ang pila sa Evacuation Center")]
    public Transform dropPoint;              

    [Header("Grid / Formation Settings (Kusang Pila)")]
    [Tooltip("Ilang tao bawat linya")]
    public int peoplePerRow = 3;             
    [Tooltip("Agwat ng bawat tao pakaliwa at pakanan")]
    public float spacingX = 1.3f;            
    [Tooltip("Agwat ng bawat linya paharap at paatras")]
    public float spacingZ = 1.3f;            
    [Tooltip("Taas mula sa sahig para lapat ang sapatos")]
    public float standHeightOffset = 0.1f;    

    [Header("Custom Standing Spots (Opsyonal)")]
    [Tooltip("Kung may specific na pwesto/upuan kang gustong pagtayuan nila, i-drag dito.")]
    public Transform[] customStandingSpots;  

    // Naaalala nito ang LAHAT ng survivors na naihatid na sa buong laro
    private List<Transform> allDeliveredSurvivors = new List<Transform>();

    private void OnTriggerEnter(Collider other)
    {
        // 1. Hanapin ang bangka
        BoatController boat = other.GetComponentInParent<BoatController>();

        // 2. Fallback kung player ang tumama
        if (boat == null && (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null))
        {
            boat = Object.FindAnyObjectByType<BoatController>();
        }

        // 3. Ibaba ang pasahero kung meron
        if (boat != null && boat.rescuedPassengers.Count > 0)
        {
            UnloadSurvivors(boat);
        }
    }

    void UnloadSurvivors(BoatController boat)
    {
        List<Transform> newSurvivors = boat.UnloadAllPassengers();
        if (newSurvivors == null || newSurvivors.Count == 0) return;

        Transform dropRef = dropPoint != null ? dropPoint : transform;

        for (int i = 0; i < newSurvivors.Count; i++)
        {
            Transform survivor = newSurvivors[i];
            if (survivor == null) continue;

            // Global Index: binibilang kasama ang mga naunang naihatid sa ibang biyahe
            int globalIndex = allDeliveredSurvivors.Count;
            Vector3 standPos;
            Quaternion standRot;

            // PARAAN 1: Kung may inilagay kang designated spots
            if (customStandingSpots != null && customStandingSpots.Length > 0 && globalIndex < customStandingSpots.Length && customStandingSpots[globalIndex] != null)
            {
                standPos = customStandingSpots[globalIndex].position + (Vector3.up * standHeightOffset);
                standRot = customStandingSpots[globalIndex].rotation;
            }
            // PARAAN 2: Awtomatikong Grid Formation (Pila sa Evacuation Center)
            else
            {
                int col = globalIndex % peoplePerRow; // Pwesto sa kasalukuyang linya (kaliwa/kanan)
                int row = globalIndex / peoplePerRow; // Ilang linya sa likod

                // Kalkulahin ang coordinate base sa rotation ng DropPoint
                Vector3 gridOffset = (dropRef.right * (col * spacingX)) - (dropRef.forward * (row * spacingZ));
                standPos = dropRef.position + gridOffset + (Vector3.up * standHeightOffset);
                standRot = dropRef.rotation;
            }

            // I-pwesto ang survivor
            survivor.position = standPos;
            survivor.rotation = standRot;

            // Siguraduhing naka-IDLE pose (hindi naglalakad)
            Animator anim = survivor.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.SetBool("isWalking", false);
            }

            // I-disable ang colliders para hindi sila magtulakan o mag-ipitan sa safezone
            Collider[] cols = survivor.GetComponentsInChildren<Collider>();
            foreach (var c in cols)
            {
                c.enabled = false;
            }

            // Itala sa listahan
            allDeliveredSurvivors.Add(survivor);

            // I-notify ang RescueManager
            if (RescueManager.instance != null)
            {
                RescueManager.instance.SurvivorDelivered();
            }
        }

        // I-update ang task text
        if (TutorialTaskManager.instance != null && RescueManager.instance != null)
        {
            bool hasMore = RescueManager.instance.deliveredToSafeZone < RescueManager.instance.totalSurvivors;
            TutorialTaskManager.instance.OnSurvivorsDelivered(hasMore);
        }

        Debug.Log($"Naihatid ang {newSurvivors.Count} bagong survivor(s). Kabuuan sa Safezone: {allDeliveredSurvivors.Count}");
    }
}