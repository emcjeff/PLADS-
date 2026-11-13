using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Engine & Speed")]
    public float motorForce = 1500f;
    public float steerSpeed = 60f;
    public float maxSpeed = 20f;
    public float waterDrag = 1.2f;

    [Header("Interaction Settings")]
    public float interactDistance = 4f;
    public Vector3 seatOffset = new Vector3(0f, 1f, 0f);
    public float exitDistance = 2.5f;

    [Header("Random Spawning")]
    public Transform[] spawnPoints; // MGA RANDOM POSITIONS KUNG SAAN PWEDENG LUMITAW ANG BANGKA

    [Header("Passengers / Rescued")]
    public List<Transform> rescuedPassengers = new List<Transform>();

    [Header("UI Prompts")]
    public GameObject ridePromptUI;
    public GameObject exitPromptUI;

    private GameObject player;
    private CharacterController playerController;
    private MonoBehaviour movementScript;
    private bool isRiding = false;

    private float verticalInput;
    private float horizontalInput;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        if (ridePromptUI != null) ridePromptUI.SetActive(false);
        if (exitPromptUI != null) exitPromptUI.SetActive(false);

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<CharacterController>();

            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (var s in scripts)
            {
                if (s != this && s.GetType().Name != "ThrowLifebuoy")
                {
                    movementScript = s;
                    break;
                }
            }
        }

        rb.linearDamping = waterDrag;
        rb.angularDamping = 2.5f;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (!isRiding)
        {
            bool isNear = distanceToPlayer <= interactDistance;
            if (ridePromptUI != null) ridePromptUI.SetActive(isNear);
            if (exitPromptUI != null) exitPromptUI.SetActive(false);
        }
        else
        {
            if (ridePromptUI != null) ridePromptUI.SetActive(false);
            if (exitPromptUI != null) exitPromptUI.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isRiding && distanceToPlayer <= interactDistance)
            {
                EnterBoat();
            }
            else if (isRiding)
            {
                ExitBoat();
            }
        }

        if (isRiding)
        {
            verticalInput = Input.GetAxis("Vertical");
            horizontalInput = Input.GetAxis("Horizontal");
        }
        else
        {
            verticalInput = 0f;
            horizontalInput = 0f;
        }
    }

    void FixedUpdate()
    {
        if (isRiding)
        {
            MoveBoat();
            SteerBoat();
        }
    }

    void LateUpdate()
    {
        // 1. Manatiling nakapwesto ang player sa loob ng bangka
        if (isRiding && player != null)
        {
            player.transform.position = transform.TransformPoint(seatOffset);
            player.transform.rotation = transform.rotation;
        }

        // 2. Pasahero sa likod
        for (int i = 0; i < rescuedPassengers.Count; i++)
        {
            if (rescuedPassengers[i] != null)
            {
                float xOffset = (i % 2 == 0) ? -0.5f : 0.5f;
                float zOffset = -1.2f - ((i / 2) * 0.9f);
                Vector3 passengerOffset = new Vector3(xOffset, seatOffset.y, zOffset);

                rescuedPassengers[i].position = transform.TransformPoint(passengerOffset);
                rescuedPassengers[i].rotation = transform.rotation;
            }
        }
    }

    public void AddPassenger(Transform passenger)
    {
        if (!rescuedPassengers.Contains(passenger))
        {
            rescuedPassengers.Add(passenger);
        }
    }

    public List<Transform> UnloadAllPassengers()
    {
        List<Transform> delivered = new List<Transform>(rescuedPassengers);
        rescuedPassengers.Clear();
        return delivered;
    }

    void MoveBoat()
    {
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            Vector3 force = transform.forward * verticalInput * motorForce * Time.fixedDeltaTime;
            rb.AddForce(force, ForceMode.Acceleration);
        }
    }

    void SteerBoat()
    {
        float speedRatio = Mathf.Clamp01(rb.linearVelocity.magnitude / 5f);
        float turn = horizontalInput * steerSpeed * speedRatio * Time.fixedDeltaTime;

        if (verticalInput < 0) turn = -turn;

        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    void EnterBoat()
    {
        isRiding = true;

        if (playerController != null) playerController.enabled = false;
        if (movementScript != null) movementScript.enabled = false;

        // --- RANDOM SPAWN LOGIC ---
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Pumili ng random index mula sa listahan
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform randomPoint = spawnPoints[randomIndex];

            if (randomPoint != null)
            {
                // I-teleport ang bangka sa napiling pwesto
                transform.position = randomPoint.position;
                transform.rotation = randomPoint.rotation;

                // I-reset ang velocity para hindi bumulusok pagka-teleport
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        // Agad ding ilagay ang player sa tamang upuan ng bangka
        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.OnPlayerBoardedBoat();
        }
        else if (RescueManager.instance != null)
        {
            // Fallback kung walang tutorial
            RescueManager.instance.TriggerMissionStart();
        }
    }

    void ExitBoat()
    {
        isRiding = false;

        Vector3 dropPosition = transform.position + (transform.right * exitDistance) + (Vector3.up * 0.5f);
        player.transform.position = dropPosition;

        if (playerController != null) playerController.enabled = true;
        if (movementScript != null) movementScript.enabled = true;

        if (exitPromptUI != null) exitPromptUI.SetActive(false);
    }
}