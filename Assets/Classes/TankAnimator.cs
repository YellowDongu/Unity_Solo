using UnityEngine;
using UnityEngine.UIElements;

public class TankAnimator : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    public void Initialize(GroundControl mainControl)
    {
        turretYawBone.transform.rotation = Quaternion.identity;
        turretPitchBone.transform.rotation = Quaternion.identity;
        control = mainControl;
    }


    //===========================================
    // FrameCycle Methods
    //===========================================
    void Update()
    {
        if(target == null)
        {
            turretYawBone.transform.localRotation = (Quaternion.RotateTowards(turretYawBone.transform.localRotation, Quaternion.identity, rotationSpeed * Time.deltaTime));
            turretPitchBone.transform.localRotation = (Quaternion.RotateTowards(turretPitchBone.transform.localRotation, Quaternion.identity, rotationSpeed * Time.deltaTime));
        }
        else
            TurretRotation(target.gameObject.transform.position);
    }

    //===========================================
    // Methods
    //===========================================

    public void TurretRotation(Vector3 position)
    {
        Vector3 targetAngle = Quaternion.LookRotation(position - gameObject.transform.position).eulerAngles;

        float yaw = Mathf.Clamp((AngleCalibration(targetAngle.y) - AngleCalibration(turretYawBone.transform.localEulerAngles.z)) / 5.0f, -1.0f , 1.0f);
        float pitch = Mathf.Clamp((AngleCalibration(-targetAngle.x) - AngleCalibration(turretPitchBone.transform.localEulerAngles.y)) / 5.0f, -1.0f , 1.0f);

        turretYawBone.transform.localEulerAngles = new Vector3(0.0f, 0.0f, turretYawBone.transform.localEulerAngles.z + yaw * rotationSpeed * Time.deltaTime);
        turretPitchBone.transform.localEulerAngles = new Vector3(0.0f, turretPitchBone.transform.localEulerAngles.y + pitch * rotationSpeed * Time.deltaTime, 0f);
    }
    public float AngleCalibration(float value) { while (value < -180.0f) { value += 360.0f; } while (value > 180.0f) { value -= 360.0f; } return value; }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public Vector3 forward  => turretPitchBone.transform.forward;
    public Quaternion rotation  => turretPitchBone.transform.rotation;
    public Vector3 yawForward  => turretYawBone.transform.forward;
    public Quaternion yawRotation  => turretYawBone.transform.rotation;

    public void ChangeTarget(Vehicle next) { target = next; }

    private GroundControl control;
    private Vehicle target;
    [SerializeField] private float rotationSpeed = 60.0f;
    [SerializeField] private GameObject turretYawBone;
    [SerializeField] private GameObject turretPitchBone;
}
