using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Lifebuoy : MonoBehaviour
{
    public float lifetime = 15f;       // Mawawala matapos ang 15 segundo para malinis ang eksena
    public float waterLevel = 0f;      // Y-coordinate ng tubig mo sa laro
    public float floatStrength = 8f;   // Lakas ng pag-angat kapag lumubog sa tubig

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Kusang sirain ang salbabida pagkalipas ng oras
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        // Kapag mas mababa sa tubig, paangatin pataas (Buoyancy effect)
        if (transform.position.y < waterLevel)
        {
            float depth = waterLevel - transform.position.y;
            rb.AddForce(Vector3.up * depth * floatStrength, ForceMode.Acceleration);

            // Lagyan ng damping sa tubig para huminahon ang alon nito
            rb.linearDamping = 2f; // Sa lumang Unity: rb.drag = 2f;
        }
    }
}