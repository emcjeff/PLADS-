using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Level 2 Configuration")]
    [Tooltip("I-check ito sa Level 2 para simula pa lang ay nakasakay na agad ang player sa bangka!")]
    public bool autoBoardOnStart = false;

    [Header("Engine & Speed")]
    public float motorForce = 1500f;
    public float steerSpeed = 60f;
    public float maxSpeed = 20f;
    public float waterDrag = 1.2f;

    [Header("Interaction Settings")]
    public float interactDistance = 4f;
    public Vector3 seatOffset = new Vector3(0f, 0.1f, 0f); // Lapat ang sapatos ng 3D character
    public float exitDistance = 2.5f;

    [Header("Random Spawning")]
    public Transform[] spawnPoints;

    [Header("Passengers / Rescued")]
    public List<Transform> rescuedPassengers = new List<Transform>();
    [Tooltip("Pwesto sa likod o harap ng player sa bangka (hal. -0.6)")]
    public float passengerZOffset = -0.6f;
    [Tooltip("Agwat pakaliwa at pakanan ng mga pasahero")]
    public float passengerXOffset = 0.4f;

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

        // 1. Hanapin ang Player gamit ang Tag
        player = GameObject.FindGameObjectWithTag("Player");

        // 2. Fallback: Hanapin gamit ang CharacterController kung sakaling hindi naka-tag
        if (player == null)
        {
            CharacterController cc = Object.FindAnyObjectByType<CharacterController>();
            if (cc != null)
            {
                player = cc.gameObject;
            }
        }

        if (player != null)
        {
            playerController = player.GetComponent<CharacterController>();

            // Hanapin ang movement script pero huwag idadamay ang ThrowLifebuoy
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

        // Water drag settings (Unity 6 damping)
        rb.linearDamping = waterDrag;
        rb.angularDamping = 2.5f;

        // KUNG LEVEL 2: Awtomatikong isakay agad ang player pagkabukas ng eksena
        if (autoBoardOnStart)
        {
            Invoke(nameof(EnterBoat), 0.2f);
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // UI Prompt logic
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

        // Pagsakay at Pagbaba (Pindot ng E)
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

        // WASD Input para sa bangka
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
        // 1. Pwesto ng player habang nagda-drive
        if (isRiding && player != null)
        {
            player.transform.position = transform.TransformPoint(seatOffset);
            player.transform.rotation = transform.rotation;
        }

        // 2. Pwesto ng mga na-rescue na pasahero sa deck ng bangka
        for (int i = 0; i < rescuedPassengers.Count; i++)
        {
            if (rescuedPassengers[i] != null)
            {
                // Salit-salit sa kaliwa at kanan
                float x = (i % 2 == 0) ? -passengerXOffset : passengerXOffset;
                // Agwat paatras sa likod ng bawat pares
                float z = passengerZOffset - ((i / 2) * 0.7f);

                // NORMALIZED VECTORS: Hindi apektado ng Scale ng bangka mo!
                Vector3 targetPos = transform.position 
                    + (transform.right.normalized * x) 
                    + (transform.forward.normalized * z) 
                    + (Vector3.up * seatOffset.y);

                rescuedPassengers[i].position = targetPos;
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

    public void EnterBoat()
    {
        isRiding = true;

        if (playerController != null) playerController.enabled = false;
        if (movementScript != null) movementScript.enabled = false;

        // SIGURADUHING IDLE POSE ANG CHARACTER (Bawal mag-walk habang nasa bangka)
        if (player != null)
        {
            Animator anim = player.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.SetBool("isWalking", false);
            }
        }

        // Random Spawning kapag sumakay
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform randomPoint = spawnPoints[randomIndex];

            if (randomPoint != null)
            {
                transform.position = randomPoint.position;
                transform.rotation = randomPoint.rotation;

                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        // I-pwesto agad ang player sa tamang coordinate
        if (player != null)
        {
            player.transform.position = transform.TransformPoint(seatOffset);
            player.transform.rotation = transform.rotation;
        }

        // Simulan ang Tutorial o Emergency Mission
        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.OnPlayerBoardedBoat();
        }
        else if (RescueManager.instance != null)
        {
            RescueManager.instance.TriggerMissionStart();
        }
    }

    public void ExitBoat()
    {
        isRiding = false;

        Vector3 dropPosition = transform.position + (transform.right * exitDistance) + (Vector3.up * 0.5f);
        if (player != null)
        {
            player.transform.position = dropPosition;
        }

        if (playerController != null) playerController.enabled = true;
        if (movementScript != null) movementScript.enabled = true;

        if (exitPromptUI != null) exitPromptUI.SetActive(false);
    }
}