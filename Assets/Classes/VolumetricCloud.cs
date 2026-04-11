using UnityEngine;

[ExecuteAlways]
public class VolumetricCloud : MonoBehaviour
{
    void Update()
    {
        if (fogMaterial == null) return;

        fogMaterial.SetVector("_SpherePos", transform.position);
        fogMaterial.SetFloat("_Radius", radius);
        //float radius = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) * 0.5f;

        fogMaterial.SetFloat("_Density", density);
        fogMaterial.SetFloat("_Scattering", scattering);
        fogMaterial.SetColor("_CloudColor", cloudColor);

    }

    [Header("Settings")]
    public float radius = 5.0f;
    public float density = 5.0f;
    public float scattering = 1.0f;
    public Color cloudColor = Color.white;

    [Header("Resources")]
    public Material fogMaterial; // 우리가 만든 VolumetricFog 메테리얼

}
