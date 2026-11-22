using System.Collections;
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
    public float motorForce = 1800f;        // Lakas ng makina (Paharap)
    public float steerSpeed = 65f;          // Bilis ng pagliko ng timon
    public float maxSpeed = 18f;            // Pinakamataas na bilis
    public float waterDrag = 1.8f;          // Preho ng tubig kapag binitawan ang W
    public float reverseMultiplier = 0.4f;  // Bawas-lakas kapag umaatras (40% lang)

    [Header("Boat Realism (Water Grip)")]
    [Tooltip("Ito ang pumipigil sa pag-slide sa gilid! Taasan (hal. 6 hanggang 10) para humiwa sa tubig tulad ng totoong bangka.")]
    public float waterGrip = 7.5f;

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

        // Hanapin ang Player
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
            ApplyWaterGrip(); // PUMIPIGIL SA DRIFT AT ICE-SLIDING EFFECT!
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

    // --- REAKSYON NG MAKINA SA TUBIG ---
    void MoveBoat()
    {
        if (verticalInput != 0)
        {
            // Bawas-lakas kapag paatras (Reverse)
            float powerMultiplier = verticalInput > 0 ? 1f : reverseMultiplier;

            if (rb.linearVelocity.magnitude < currentMaxSpeed)
            {
                Vector3 force = transform.forward * verticalInput * (currentMotorForce * powerMultiplier) * Time.fixedDeltaTime;
                rb.AddForce(force, ForceMode.Acceleration);
            }
        }
    }

    void SteerBoat()
    {
        // Mas epektibo lumiko kapag may forward motion
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float speedRatio = Mathf.Clamp(Mathf.Abs(forwardSpeed) / 4f, 0.25f, 1f);

        float turn = horizontalInput * steerSpeed * speedRatio * Time.fixedDeltaTime;

        // Kapag umaatras, babaligtad ang direksyon ng pag-ikot
        if (forwardSpeed < -0.1f) turn = -turn;

        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    // --- ITO ANG PUMAPATAY SA SLIDING (HYDRODYNAMIC KEEL FORCE) ---
    void ApplyWaterGrip()
    {
        // Kuhanin kung gaano kabilis ang takbo paharap
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        Vector3 desiredForwardVel = transform.forward * forwardSpeed;

        // Panatilihin ang Y velocity
        desiredForwardVel.y = rb.linearVelocity.y;

        // Hilahin ang buong velocity papunta sa direksyon ng bangka para mawala ang dulas sa gilid
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredForwardVel, waterGrip * Time.fixedDeltaTime);
    }

    public void AddPassenger(Transform passenger)
    {
        if (!rescuedPassengers.Contains(passenger))
        {
            rescuedPassengers.Add(passenger);

            Collider[] passengerCols = passenger.GetComponentsInChildren<Collider>();
            Collider[] boatCols = GetComponentsInChildren<Collider>();

            foreach (var pCol in passengerCols)
            {
                pCol.enabled = false;
                foreach (var bCol in boatCols)
                {
                    Physics.IgnoreCollision(bCol, pCol);
                }
            }

            Rigidbody passengerRb = passenger.GetComponentInChildren<Rigidbody>();
            if (passengerRb != null)
            {
                passengerRb.isKinematic = true;
            }
        }
    }

    public List<Transform> UnloadAllPassengers()
    {
        List<Transform> delivered = new List<Transform>(rescuedPassengers);
        rescuedPassengers.Clear();
        return delivered;
    }

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
        currentMotorForce = motorForce * penaltyMultiplier;
        currentMaxSpeed = maxSpeed * penaltyMultiplier;

        if (TutorialTaskManager.instance != null && TutorialTaskManager.instance.taskText != null)
        {
            TutorialTaskManager.instance.taskText.text = "⚠️ NABUTAS ANG BANGKA SA BASURA! BUMABAGAL KA!";
            TutorialTaskManager.instance.taskText.color = Color.red;
        }

        yield return new WaitForSeconds(duration);

        currentMotorForce = motorForce;
        currentMaxSpeed = maxSpeed;
        isSlowed = false;

        if (TutorialTaskManager.instance != null && TutorialTaskManager.instance.taskText != null)
        {
            TutorialTaskManager.instance.taskText.color = Color.white;
        }
    }

    public void ApplyKnockback(Vector3 bumpDirection, float bumpForce)
    {
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(bumpDirection * bumpForce, ForceMode.Impulse);
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