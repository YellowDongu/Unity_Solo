using UnityEngine;

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public struct LayerDistance
    {
        public LayerMask layer; // 레이어 선택
        public float distance;  // 가시 거리
    }

    public void Awake()
    {
        Camera cam = camera;
        float[] distances = new float[32];

        foreach (var item in customDistances)
        {
            // LayerMask에서 실제 레이어 인덱스(0~31)를 추출
            int layerIndex = 0;
            int layerVal = item.layer.value;
            while (layerVal > 1)
            {
                layerVal >>= 1;
                layerIndex++;
            }

            distances[layerIndex] = item.distance;
        }

        cam.layerCullDistances = distances;
    }

    public void Attach(GameObject target, bool thirdProspective)
    {
        if(thirdProspective)
            ThirdProspective(target);
        else
            FirstProspective(target);

        transform.SetParent(attachedPoint.transform);
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        camera.gameObject.transform.localPosition = Vector3.zero;
        camera.gameObject.transform.localEulerAngles = Vector3.zero;
    }

    public void FirstProspective(GameObject target)
    {
        camera.fieldOfView = 45.0f;
        attachedPoint = target.transform.GetChild(3);
    }
    public void ThirdProspective(GameObject target)
    {
        camera.fieldOfView = 60.0f;
        attachedPoint = target.transform.GetChild(2);
    }

    private Transform attachedPoint = null;
    [SerializeField] private Camera camera = null;
    [SerializeField] private LayerDistance[] customDistances;
}
