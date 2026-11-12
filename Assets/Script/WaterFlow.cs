using UnityEngine;

public class WaterFlow : MonoBehaviour
{
    [Header("Flow Speed")]
    public float scrollSpeedX = 0.1f; // Tinaasan natin nang kaunti para kitang-kita
    public float scrollSpeedY = 0.1f;

    private Renderer rend;
    private Material mat;
    private Vector2 offset;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
        }
    }

    void Update()
    {
        if (mat == null) return;

        // Bilis ng galaw
        offset.x += scrollSpeedX * Time.deltaTime;
        offset.y += scrollSpeedY * Time.deltaTime;

        // Gumagana pareho sa Standard at URP (_BaseMap)
        mat.SetTextureOffset("_BaseMap", offset);
        mat.SetTextureOffset("_MainTex", offset);
        mat.SetTextureOffset("_BumpMap", offset); // Kung may Normal map para sa alon
    }
}