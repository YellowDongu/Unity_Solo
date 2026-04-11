using UnityEngine;

[ExecuteAlways]
public class VolumetricLinedCloud : MonoBehaviour
{
    void Update()
    {
        //if (fogMaterial == null) return;

        fogMaterial.SetVector("_LineStart", lineStart);
        fogMaterial.SetVector("_LineEnd", lineEnd);
        fogMaterial.SetFloat("_RadiusStart", radiusStart);
        fogMaterial.SetFloat("_RadiusEnd", radiusEnd);

        fogMaterial.SetFloat("_Density", density);
        fogMaterial.SetFloat("_Scattering", scattering);
        fogMaterial.SetColor("_CloudColor", cloudColor);
    }


    [Header("Settings")]
    public Vector3 lineStart;
    public Vector3 lineEnd;
    public float radiusStart;
    public float radiusEnd;

    public float density = 5.0f;
    public float scattering = 1.0f;
    public Color cloudColor = Color.white;

    [Header("Resources")]
    public Material fogMaterial;
}
