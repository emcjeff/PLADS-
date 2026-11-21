using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Level 2 Configuration")]
    public bool autoBoardOnStart = false;

    [Header("Engine & Speed")]
    public float motorForce = 1500f;
    public float steerSpeed = 60f;
    public float maxSpeed = 20f;
    public float waterDrag = 1.2f;

    [Header("Hazard / Slowdown System")]
    private float currentMotorForce;
    private float currentMaxSpeed;
    private bool isSlowed = false;

    [Header("Interaction Settings")]
    public float interactDistance = 4f;
    public Vector3 seatOffset = new Vector3(0f, 0.1f, 0f);
    public float exitDistance = 2.5f;

    [Header("Random Spawning")]
    public Transform[] spawnPoints;

    [Header("Passengers / Rescued")]
    public List<Transform> rescuedPassengers = new List<Transform>();
    public float passengerZOffset = -0.6f;
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

        currentMotorForce = motorForce;
        currentMaxSpeed = maxSpeed;

        if (ridePromptUI != null) ridePromptUI.SetActive(false);
        if (exitPromptUI != null) exitPromptUI.SetActive(false);

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            CharacterController cc = Object.FindAnyObjectByType<CharacterController>();
            if (cc != null) player = cc.gameObject;
        }

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

        if (autoBoardOnStart)
        {
            Invoke(nameof(EnterBoat), 0.2f);
        }
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
        if (isRiding && player != null)
        {
            player.transform.position = transform.TransformPoint(seatOffset);
            player.transform.rotation = transform.rotation;
        }

        for (int i = 0; i < rescuedPassengers.Count; i++)
        {
            if (rescuedPassengers[i] != null)
            {
                float x = (i % 2 == 0) ? -passengerXOffset : passengerXOffset;
                float z = passengerZOffset - ((i / 2) * 0.7f);

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
        // Gagamitin ang currentMaxSpeed at currentMotorForce para kapag slowed, ramdam agad
        if (rb.linearVelocity.magnitude < currentMaxSpeed)
        {
            Vector3 force = transform.forward * verticalInput * currentMotorForce * Time.fixedDeltaTime;
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

    // --- BAGONG HAZARD REACTION FUNCTIONS ---

    // 1. Kapag nabangga ang basag na bote (Slowdown Penalty)
    public void ApplySlowdown(float duration, float penaltyMultiplier)
    {
        if (!isSlowed)
        {
            StartCoroutine(SlowdownRoutine(duration, penaltyMultiplier));
        }
    }

    private IEnumerator SlowdownRoutine(float duration, float penaltyMultiplier)
    {
        isSlowed = true;
        currentMotorForce = motorForce * penaltyMultiplier; // Bawas sa lakas
        currentMaxSpeed = maxSpeed * penaltyMultiplier;     // Bawas sa max speed

        // I-update ang UI task text pansamantala bilang babala
        if (TutorialTaskManager.instance != null && TutorialTaskManager.instance.taskText != null)
        {
            TutorialTaskManager.instance.taskText.text = "⚠️ NABUTAS ANG BANGKA SA BASAG NA BOTE! BUMABAGAL KA!";
            TutorialTaskManager.instance.taskText.color = Color.red;
        }

        yield return new WaitForSeconds(duration);

        // Ibalik sa normal ang bilis
        currentMotorForce = motorForce;
        currentMaxSpeed = maxSpeed;
        isSlowed = false;

        if (TutorialTaskManager.instance != null)
        {
            TutorialTaskManager.instance.taskText.color = Color.white;
        }
    }

    // 2. Kapag bumangga sa malaking harang (Kahoy / Barrel Knockback)
    public void ApplyKnockback(Vector3 bumpDirection, float bumpForce)
    {
        rb.linearVelocity = Vector3.zero; // Hinto agad
        rb.AddForce(bumpDirection * bumpForce, ForceMode.Impulse); // Talsik pabalik
    }

    public void EnterBoat()
    {
        isRiding = true;

        if (playerController != null) playerController.enabled = false;
        if (movementScript != null) movementScript.enabled = false;

        if (player != null)
        {
            Animator anim = player.GetComponentInChildren<Animator>();
            if (anim != null) anim.SetBool("isWalking", false);
        }

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

        if (player != null)
        {
            player.transform.position = transform.TransformPoint(seatOffset);
            player.transform.rotation = transform.rotation;
        }

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
        if (player != null) player.transform.position = dropPosition;

        if (playerController != null) playerController.enabled = true;
        if (movementScript != null) movementScript.enabled = true;

        if (exitPromptUI != null) exitPromptUI.SetActive(false);
    }
}