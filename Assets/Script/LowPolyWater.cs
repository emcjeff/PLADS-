using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class LowPolyWater : MonoBehaviour
{
    [Header("Wave Settings")]
    public float waveScale = 0.3f;      // Laki ng bawat tatsulok na alon
    public float waveHeight = 0.12f;    // Taas ng pag-angat at pagbaba ng alon
    public float waveSpeed = 1.5f;      // Bilis ng pag-alon

    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3[] animatedVertices;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        // 1. Gawing "Flat-Shaded" ang Mesh para lumitaw ang mga Low-Poly Triangles
        mesh = MakeFlatShadedMesh(mf.sharedMesh);
        mf.mesh = mesh;

        baseVertices = mesh.vertices;
        animatedVertices = new Vector3[baseVertices.Length];
    }

    void Update()
    {
        if (baseVertices == null) return;

        float time = Time.time * waveSpeed;

        // 2. Pagalawin ang bawat kanto ng tatsulok
        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 v = baseVertices[i];

            float pX = (transform.position.x + v.x) * waveScale;
            float pZ = (transform.position.z + v.z) * waveScale;

            // Wave math gamit ang Sine at Perlin Noise
            float wave = Mathf.Sin(time + pX + pZ) * waveHeight 
                       + Mathf.PerlinNoise(pX + time * 0.2f, pZ + time * 0.2f) * waveHeight;

            animatedVertices[i] = new Vector3(v.x, v.y + wave, v.z);
        }

        mesh.vertices = animatedVertices;
        // Ito ang sekreto: Iko-compute nito ang flat lighting sa bawat polygon!
        mesh.RecalculateNormals();
    }

    // Kinakaltas ang dugtungan ng vertices para maging matalas ang bawat tatsulok
    Mesh MakeFlatShadedMesh(Mesh originalMesh)
    {
        Mesh newMesh = new Mesh();
        newMesh.name = "LowPolyWater_Mesh";

        Vector3[] origVerts = originalMesh.vertices;
        int[] origTris = originalMesh.triangles;

        Vector3[] newVerts = new Vector3[origTris.Length];
        int[] newTris = new int[origTris.Length];
        Vector2[] newUVs = new Vector2[origTris.Length];

        for (int i = 0; i < origTris.Length; i++)
        {
            newVerts[i] = origVerts[origTris[i]];
            newTris[i] = i;
            if (originalMesh.uv.Length > 0)
            {
                newUVs[i] = originalMesh.uv[origTris[i]];
            }
        }

        newMesh.vertices = newVerts;
        newMesh.triangles = newTris;
        newMesh.uv = newUVs;
        newMesh.RecalculateNormals();

        return newMesh;
    }
}