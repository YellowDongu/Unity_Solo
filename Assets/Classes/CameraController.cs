using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    //===========================================
    // struct/enum
    //===========================================
    [System.Serializable]
    public struct LayerDistance
    {
        public LayerMask layer; // 레이어 선택
        public float distance;  // 가시 거리
    }

    //===========================================
    // Initializer/Destructor
    //===========================================
    public void Awake()
    {
        Camera cam = camera;
        float[] distances = new float[32];

        foreach (var item in customDistances)
        {
            int layerIndex = 0;
            int layerVal = item.layer.value;
            while (layerVal > 1)
            {
                layerVal >>= 1;
                layerIndex++;
            }

            distances[layerIndex] = GameMaster.ConvertWorldScale(item.distance);
        }

        cam.layerCullDistances = distances;
    }

    public void Attach(Vehicle _target, bool thirdProspective)
    {
        thirdView = thirdProspective;
        target = _target;
        if (thirdView)
            ThirdProspective();
        else
            FirstProspective();

        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localEulerAngles = Vector3.zero;
        camera.gameObject.transform.localPosition = Vector3.zero;
        camera.gameObject.transform.localEulerAngles = Vector3.zero;
    }

    public void DetachCamera()
    {
        camera.gameObject.transform.SetParent(null);
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    private void Update()
    {
        if (Keyboard.current.vKey.isPressed)
        {
            thirdView = !thirdView;

            if (thirdView)
                ThirdProspective();
            else
                FirstProspective();

        }
        if (Keyboard.current.numpad5Key.isPressed)
            attachedPoint.rotation = Quaternion.LookRotation(target.transform.forward);
        if (Keyboard.current.numpad4Key.isPressed)
            attachedPoint.rotation *= Quaternion.Euler(0.0f, -rotationSpeed * Time.deltaTime, 0.0f);
        if (Keyboard.current.numpad6Key.isPressed)
            attachedPoint.rotation *= Quaternion.Euler(0.0f, rotationSpeed * Time.deltaTime, 0.0f);
        if (Keyboard.current.numpad8Key.isPressed)
            attachedPoint.rotation *= Quaternion.Euler(-rotationSpeed * Time.deltaTime, 0.0f, 0.0f);
        if (Keyboard.current.numpad2Key.isPressed)
            attachedPoint.rotation *= Quaternion.Euler(rotationSpeed * Time.deltaTime, 0.0f, 0.0f);
    }

    //===========================================
    // Methods
    //===========================================

    public void FirstProspective()
    {
        camera.fieldOfView = 45.0f;
        attachedPoint = target.FirstView().transform;
        transform.SetParent(attachedPoint);
    }
    public void ThirdProspective()
    {
        camera.fieldOfView = 60.0f;
        transform.SetParent(target.ThirdView().transform);
        attachedPoint = transform.parent.parent;
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    private bool thirdView = true;
    private float rotationSpeed = 45.0f;

    private Vehicle target;
    private Transform attachedPoint = null;
    [SerializeField] private Camera camera = null;
    [SerializeField] private LayerDistance[] customDistances;
}
