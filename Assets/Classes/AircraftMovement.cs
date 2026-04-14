using UnityEngine;


public class AircraftMovement : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    void Start()
    {
        //MaxSpeed = 130.0f;
        //MinSpeed = 33.0f;
        //
        //enginePower = 100.0f;
        //airbreakPower = 2.0f;
        //
        //rotationSpeed.z = 15.0f;
        //rotationSpeed.x = 35.0f;
        //rotationSpeed.y = 2.5f;

        dragFactor.x = 0.25f;
        dragFactor.y = 0.0001f;
        dragFactor.z = 0.0001f;
        layerMask = LayerMask.NameToLayer("Terrain");
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    void Update()
    {
        if (Time.deltaTime < 0 || Time.deltaTime > 0.1f)
            return;

        VelocityCalculation();

        force = gameObject.transform.forward;
        Vector3 nextPos = transform.position + force * control.velocity * Time.deltaTime;
        rigidbody.MovePosition(nextPos);

        RotationCalculation();

    }

    //===========================================
    // Methods
    //===========================================
    public void RotationCalculation()
    {
        //rotationDelta.x += control.yoke.z * rotationSpeed.z /* * rotationSpeed.z*/ * 5.0f * Time.deltaTime - rotationDelta.x /* * rotationSpeed.z * 0.5f*/ * 2.5f * Time.deltaTime;
        //rotationDelta.y += control.yoke.y * rotationSpeed.y /* * rotationSpeed.y*/ * 5.0f * Time.deltaTime - rotationDelta.y /* * rotationSpeed.y * 0.5f*/ * 2.5f * Time.deltaTime;
        //rotationDelta.z += control.yoke.x * rotationSpeed.x /* * rotationSpeed.x*/ * 5.0f * Time.deltaTime - rotationDelta.z /* * rotationSpeed.x * 0.5f*/ * 2.5f * Time.deltaTime;
        
        //rotationDelta.x = Mathf.Clamp(rotationDelta.x, -rotationSpeed.x, rotationSpeed.x);
        //rotationDelta.y = Mathf.Clamp(rotationDelta.y, -rotationSpeed.y, rotationSpeed.y);
        //rotationDelta.z = Mathf.Clamp(rotationDelta.z, -rotationSpeed.z, rotationSpeed.z);

        rotationDelta.x = Mathf.Clamp(rotationDelta.x + (control.yoke.z * rotationSpeed.z /* * rotationSpeed.z*/ * 5.0f * Time.deltaTime - rotationDelta.x /* * rotationSpeed.z * 0.5f*/ * 2.5f * Time.deltaTime), -rotationSpeed.x, rotationSpeed.x);
        rotationDelta.y = Mathf.Clamp(rotationDelta.y + (control.yoke.y * rotationSpeed.y /* * rotationSpeed.y*/ * 5.0f * Time.deltaTime - rotationDelta.y /* * rotationSpeed.y * 0.5f*/ * 2.5f * Time.deltaTime), -rotationSpeed.y, rotationSpeed.y);
        rotationDelta.z = Mathf.Clamp(rotationDelta.z + (control.yoke.x * rotationSpeed.x /* * rotationSpeed.x*/ * 5.0f * Time.deltaTime - rotationDelta.z /* * rotationSpeed.x * 0.5f*/ * 2.5f * Time.deltaTime), -rotationSpeed.z, rotationSpeed.z);

        if (Mathf.Abs(rotationDelta.x) < 0.5f) rotationDelta.x = 0.0f;
        if (Mathf.Abs(rotationDelta.y) < 0.5f) rotationDelta.y = 0.0f;
        if (Mathf.Abs(rotationDelta.z) < 0.5f) rotationDelta.z = 0.0f;

        rigidbody.MoveRotation(transform.rotation * Quaternion.Euler(rotationDelta * Time.deltaTime * control.HighGTurn));
        //transform.rotation *= Quaternion.Euler(rotationDelta);
        //transform.rotation *= Quaternion.Euler(rotationDelta);
        //transform.rotation *= Quaternion.Euler(yoke.z * rotationSpeed.z, yoke.y * rotationSpeed.y, yoke.x * rotationSpeed.x);
    }

    public float GetLogValueWithK(float value, float min, float max, float power) // 로그 이용, 다만 여기에 맞지 않음
    {
        float normalizedT = (value - 0.2f) / (1.0f - 0.2f);
        //normalizedT = Mathf.Clamp01(normalizedT); // 0.2에서 1까지 강제

        if (Mathf.Abs(power) < 0.001f)
            return min + (max - min) * normalizedT;


        float ratio = (1.0f - Mathf.Exp(-power * normalizedT)) / (1.0f - Mathf.Exp(-power));

        return min + (max - min) * ratio;
    }
    public void VelocityCalculation()
    {
        float rotationFactor = 1.0f + dragFactor.x * Mathf.Abs(control.yoke.x);
        float throttleInput = Mathf.Max(0f, control.throttle - 0.2f);
        float thrust = enginePower * throttleInput + MinSpeed;

        float speedRatio = control.velocity / MaxSpeed;
        float drag = MaxSpeed * (speedRatio * speedRatio) * rotationFactor;

        if (control.isAirBrakeOn)
            drag *= airbrakePower;

        control.velocity += (thrust - drag) * Time.deltaTime;
        control.velocity = Mathf.Max(0f, control.velocity);
    }

    //public void VelocityCalculation()
    //{
    //    //float rotationFactor = 1.0f + (yoke.x != 0 ? dragFactor.x : 0.0f) + (yoke.y != 0 ? dragFactor.y : 0.0f) + (yoke.z != 0 ? dragFactor.z : 0.0f);
    //    float rotationFactor = control.yoke.x != 0 ? dragFactor.x + 1.0f : 1.0f;
    //    float thrust = enginePower * (control.throttle - 0.2f) + MinSpeed;
    //    float drag = MaxSpeed * (1 - (MaxSpeed - control.velocity) / MaxSpeed) * rotationFactor;
    //
    //    if (control.isAirBreakOn)
    //        drag *= airbreakPower;
    //
    //    control.velocity += (thrust - drag) * Time.deltaTime;
    //
    //    //Debug.Log($"thrust{thrust} - drag{drag} * {rotationFactor} => velocity{velocity}");
    //}

    public void OnTriggerEnter(Collider other)
    {
        if (layerMask != other.gameObject.layer)
            return;

        if (Physics.Raycast(transform.position, gameObject.transform.forward, out RaycastHit hit, 10.0f))
        {
            Vector3 normal = hit.normal;
            if (Vector3.Dot(gameObject.transform.forward, normal) < 0.0f)
            {
                Quaternion look = Quaternion.LookRotation(Vector3.Reflect(gameObject.transform.forward, normal));
                //Vector3 direction = rigidbody.linearVelocity.normalized;
                //rigidbody.MoveRotation(look);
                //if (gameObject.transform.forward == direction)
                transform.rotation = look;
                rigidbody.MovePosition(gameObject.transform.position + gameObject.transform.forward * hit.distance);
            }
        }

    }

    //public void OnTriggerStay(Collider other)
    //{
    //    if (layerMask != other.gameObject.layer)
    //        return;
    //
    //    if (Physics.Raycast(transform.position, gameObject.transform.forward, out RaycastHit hit, 10.0f))
    //    {
    //        Vector3 normal = hit.normal;
    //        if (Vector3.Dot(gameObject.transform.forward, normal) < 0.0f)
    //        {
    //            Quaternion look = Quaternion.LookRotation(Vector3.Reflect(gameObject.transform.forward, normal)s);
    //            //Vector3 direction = rigidbody.linearVelocity.normalized;
    //            //rigidbody.MoveRotation(look);
    //            //if (gameObject.transform.forward == direction)
    //            transform.rotation = look;
    //            rigidbody.MovePosition(gameObject.transform.position + gameObject.transform.forward * hit.distance);
    //        }
    //    }
    //
    //}
    public void OnCollisionStay(Collision collision)
    {
        Vector3 normal = collision.contacts[0].normal;

        if (Vector3.Dot(gameObject.transform.forward, normal) < 0f)
        {
            Vector3 reflect = Vector3.Reflect(gameObject.transform.forward, normal);
            rigidbody.MoveRotation(Quaternion.LookRotation(reflect));
            rigidbody.MovePosition(gameObject.transform.position + gameObject.transform.forward * 5.0f);
        }

    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public Vector3 GetRotationSpeed() { return rotationSpeed; }


    [SerializeField] float MaxSpeed, MinSpeed, enginePower, airbrakePower;
    private int layerMask;

    private Vector3 force;
    private Vector3 dragFactor = Vector3.one; // 회전 시 감속, 1.0하면 증폭 없음, 1보다 낮으면 오히려 속도 늘어남
    [SerializeField] private Vector3 rotationDelta = Vector3.zero;
    [SerializeField] private Vector3 rotationSpeed = Vector3.one;

    [SerializeField] public Control control = null;
    [SerializeField] private Rigidbody rigidbody = null;
}
