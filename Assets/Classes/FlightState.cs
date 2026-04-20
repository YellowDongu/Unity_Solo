using System.Collections.Generic;
using UnityEngine;

//===========================================
// struct/enum
public enum TurnType
{
    Shallow,
    Normal,
    Deep,
    END
}
//===========================================

public abstract class FlightState
{
    //===========================================
    // Methods
    //===========================================
    public abstract bool Update();

    public float AngleCalibration(float value) { while (value < -180.0f) { value += 360.0f; } while (value > 180.0f) { value -= 360.0f; } return value; }
    public float GetMinAngleDifference(float angle, float myAngle)
    {
        myAngle = AngleCalibration(myAngle);
        angle = AngleCalibration(angle);

        float origin = angle - myAngle;
        float minus = origin - 360.0f;
        float plus = origin + 360.0f;

        float difference = Mathf.Abs(origin) > Mathf.Abs(minus) ? minus : origin;
        difference = Mathf.Abs(difference) > Mathf.Abs(plus) ? plus : difference;

        return difference;
    }

    public bool Roll(float angle) { return Roll(angle, GetMinAngleDifference(angle, transform.eulerAngles.z)); }
    public bool Roll(float angle, float difference)
    {
        float abs = Mathf.Abs(difference);

        if (abs > 5.0f)
            control.yoke.x = Mathf.Sign(difference);
        else
            control.yoke.x = difference / 5.0f;

        return abs < 3.0f;
    }

    public bool Pitch(float angle) { return Pitch(angle, GetMinAngleDifference(angle, transform.eulerAngles.x)); }
    public bool Pitch(float angle, float difference)
    {
        float abs = Mathf.Abs(difference);

        if (abs > 5.0f)
            control.yoke.z = Mathf.Sign(difference);
        else
            control.yoke.z = difference / 5.0f;

        return abs < 3.0f;
    }


    public bool Yaw(float angle) { return Yaw(angle, GetMinAngleDifference(angle, transform.eulerAngles.y)); }
    public bool Yaw(float angle, float difference)
    {
        float abs = Mathf.Abs(difference);

        if (abs > 5.0f)
            control.yoke.y = Mathf.Sign(difference);
        else
            control.yoke.y = difference / 5.0f;

        return abs < 3.0f;

    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    protected Transform transform;
    protected Control control;
    protected Vector3 targetAngle;
    public float tempStoringTime;
}

public class LevelingState : FlightState
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    public LevelingState(Transform _transform, Control _control)
    {
        transform = _transform;
        control = _control;
    }

    //===========================================
    // Methods
    //===========================================
    public override bool Update()
    {
        bool reutrnValue = Roll(0.0f);
        reutrnValue |= Pitch(0.0f);
        return reutrnValue;
    }
}

public class AltitudeState : FlightState
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    public AltitudeState(Transform _transform, Control _control)
    {
        transform = _transform;
        control = _control;
    }

    //===========================================
    // Methods
    //===========================================
    public override bool Update()
    {
        Roll(0.0f);
        float difference = targetAltitude - transform.position.y;
        float targetAngle = Mathf.Clamp(difference * 5.0f, -45.0f, 45.0f) * -1.0f;
        Pitch(targetAngle);
        return difference < 2.5f;
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void SetAltitude(float value) { targetAltitude = value; }

    private float targetAltitude;
}

public class HorizontalTurnState : FlightState
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    public HorizontalTurnState(Transform _transform, Control _control)
    {
        transform = _transform;
        control = _control;
    }

    public void Initialize(Vector3 _targetPosition, AircraftMovement movement, TurnType turnType = TurnType.Normal)
    {
        targetPosition = _targetPosition;
        mode = 1;

        SetTurnValue(movement, turnType);
    }

    public void Initialize(Quaternion quaternion, AircraftMovement movement, TurnType turnType = TurnType.Normal)
    {
        targetAngle = quaternion.eulerAngles;
        targetAngle.x = AngleCalibration(targetAngle.x);
        targetAngle.y = AngleCalibration(targetAngle.y);
        targetAngle.z = AngleCalibration(targetAngle.z);
        mode = 0;
        SetTurnValue(movement, turnType);
    }

    public void Initialize(Vector3 _targetPosition)
    {
        targetPosition = _targetPosition;
        mode = 1;
    }

    public void Initialize(Quaternion quaternion)
    {
        targetAngle = quaternion.eulerAngles;
        targetAngle.x = AngleCalibration(targetAngle.x);
        targetAngle.y = AngleCalibration(targetAngle.y);
        targetAngle.z = AngleCalibration(targetAngle.z);
        mode = 0;
    }

    public void SetTurnValue(AircraftMovement movement, TurnType turnType)
    {
        Vector3 rotationSpeed = movement.GetRotationSpeed();

        switch (turnType)
        {
            case TurnType.Shallow:
                bankAngle = 15.0f;
                maxX = 7.5f;
                minX = -7.5f;
                zAngle = 4.0f;
                break;
            case TurnType.Deep:
                bankAngle = 70.0f;
                maxX = 45.0f;
                minX = -45.0f;
                zAngle = 2.0f;
                break;
            default: // case TurnType.Normal:
                bankAngle = 45.0f;
                maxX = 12.5f;
                minX = -12.5f;
                zAngle = 1.0f;
                break;
        }
        yaw = Mathf.Clamp01(rotationSpeed.z * Mathf.Tan(maxX * Mathf.Deg2Rad) / rotationSpeed.y);
    }

    //===========================================
    // Methods
    //===========================================

    public override bool Update()
    {
        Vector3 vector = Vector3.zero;

        switch (mode)
        {
            case 0:
                vector = targetPosition - transform.position;
                break;
            case 1:
                vector = targetPosition - transform.position;
                targetAngle = Quaternion.LookRotation(vector).eulerAngles;
                targetAngle.y = AngleCalibration(targetAngle.y);
                break;
            case 2:
                {
                    Vector3 forward = Vector3.Scale(target.gameObject.transform.forward, new Vector3(1, 0, 1)).normalized;
                    Vector3 right = Vector3.Scale(target.gameObject.transform.right, new Vector3(1, 0, 1)).normalized;
                    vector = (right * offset.x) + (forward * offset.z) + target.gameObject.transform.position;
                    vector -= transform.position;
                    targetAngle = Quaternion.LookRotation(vector).eulerAngles;
                    targetAngle.y = AngleCalibration(targetAngle.y);
                }
                break;
            case 3:
                vector = target.gameObject.transform.position - transform.position;
                targetAngle = Quaternion.LookRotation(vector).eulerAngles;
                targetAngle.y = AngleCalibration(targetAngle.y);
                break;
            default:
                return true;
        }


        float y = AngleCalibration(transform.eulerAngles.y);
        y = GetMinAngleDifference(targetAngle.y, y);

        float absY = Mathf.Abs(y);
        float signY = Mathf.Sign(y);
        float angleX = Mathf.Clamp(vector.y / zAngle, minX, maxX);

        if (absY < 15.0f)
        {
            Roll(0.0f);
            Pitch(Mathf.Clamp(vector.y, -45.0f, 45.0f) * -1.0f);
            Yaw(targetAngle.y, y);
            control.throttle = 0.2f;
        }
        else
        {
            if (absY > 165.0f)
            {
                if (vector.sqrMagnitude < 1375.0f)
                {
                    control.throttle = 0.1f;
                    control.isAirBrakeOn = true;
                    y = (180.0f - absY) * -signY;

                    Roll(0.0f);
                    Pitch(Mathf.Clamp(vector.y, -45.0f, 45.0f) * -1.0f);
                    Yaw(targetAngle.y, y);

                    return false;
                }
            }

            if (Roll(bankAngle * -signY))
            {
                Pitch(-angleX);
                control.yoke.y = signY * yaw;
            }

            control.throttle = 1.0f;
            control.isAirBrakeOn = false;
        }

        return vector.sqrMagnitude < 100.0f;
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    public void SetOffset(Vector3 position) { offset = position; }

    private int mode = 0;
    private Vector3 targetPosition;

    private GameObject target;
    private Vector3 offset;

    private float bankAngle = 30.0f, yaw = 0.5f;
    private float minX = -10.0f, maxX = 10.0f, zAngle = 5.0f;
}

public class FollowState : FlightState
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    public FollowState(Transform _transform, Control _control)
    {
        transform = _transform;
        control = _control;
    }

    public void Initialize(GameObject _target, Vector3 _offset, AircraftMovement movement, TurnType turnType = TurnType.Normal)
    {
        target = _target;
        mode = 0;
        offset = _offset;
        SetTurnValue(movement, turnType);
    }

    public void Initialize(Aircraft _targetVehicle, Vector3 _offset, AircraftMovement movement, TurnType turnType = TurnType.Normal)
    {
        target = _targetVehicle.gameObject;
        mode = 0;
        offset = _offset;
        targetVehicle = _targetVehicle;
        SetTurnValue(movement, turnType);
    }
    public void Initialize(GameObject _target, AircraftMovement movement, TurnType turnType = TurnType.Normal)
    {
        target = _target;
        mode = 1;

        SetTurnValue(movement, turnType);
    }

    public void Initialize(GameObject _target, Vector3 _offset)
    {
        target = _target;
        offset = _offset;
        mode = 0;
    }

    public void Initialize(GameObject _target)
    {
        target = _target;
        mode = 1;
    }


    public void SetTurnValue(AircraftMovement movement, TurnType turnType)
    {
        Vector3 rotationSpeed = movement.GetRotationSpeed();

        switch (turnType)
        {
            case TurnType.Shallow:
                bankAngle = 15.0f;
                maxX = 7.5f;
                minX = -7.5f;
                zAngle = 4.0f;
                break;
            case TurnType.Deep:
                bankAngle = 70.0f;
                maxX = 45.0f;
                minX = -45.0f;
                zAngle = 2.0f;
                break;
            default: // case TurnType.Normal:
                bankAngle = 45.0f;
                maxX = 12.5f;
                minX = -12.5f;
                zAngle = 1.0f;
                break;
        }
        yaw = Mathf.Clamp01(rotationSpeed.z * Mathf.Tan(maxX * Mathf.Deg2Rad) / rotationSpeed.y);
    }

    //===========================================
    // Methods
    //===========================================

    public override bool Update()
    {
        Vector3 vector = Vector3.zero;

        switch (mode)
        {
            case 0:
                {
                    Vector3 forward = Vector3.Scale(target.gameObject.transform.forward, new Vector3(1, 0, 1)).normalized;
                    Vector3 right = Vector3.Scale(target.gameObject.transform.right, new Vector3(1, 0, 1)).normalized;
                    vector = (forward * offset.x) + (right * offset.z) + target.gameObject.transform.position - transform.position;
                    targetAngle = Quaternion.LookRotation(vector).eulerAngles;
                    targetAngle.y = AngleCalibration(targetAngle.y);
                }
                break;
            case 1:
                vector = target.gameObject.transform.position - transform.position;
                targetAngle = Quaternion.LookRotation(vector).eulerAngles;
                targetAngle.y = AngleCalibration(targetAngle.y);
                break;
            default:
                return true;
        }


        float y = AngleCalibration(transform.eulerAngles.y);
        y = GetMinAngleDifference(targetAngle.y, y);

        float absY = Mathf.Abs(y);
        float signY = Mathf.Sign(y);
        float angleX = Mathf.Clamp(vector.y / zAngle, minX, maxX);

        if (absY < 15.0f)
        {
            Roll(0.0f);
            Pitch(Mathf.Clamp(vector.y, -45.0f, 45.0f) * -1.0f);
            Yaw(targetAngle.y, y);
            SetThrottle(vector);

        }
        else
        {
            if (absY > 165.0f)
            {
                if (vector.sqrMagnitude < 1375.0f)
                {
                    control.throttle = 0.1f;
                    control.isAirBrakeOn = true;
                    y = (180.0f - absY) * -signY;

                    Roll(0.0f);
                    Pitch(Mathf.Clamp(vector.y, -45.0f, 45.0f) * -1.0f);
                    Yaw(targetAngle.y, y);

                    return false;
                }
            }

            if (Roll(bankAngle * -signY))
            {
                Pitch(-angleX);
                control.yoke.y = signY * yaw;
            }

            control.isAirBrakeOn = false;
            control.throttle = 1.0f;
        }

        return vector.sqrMagnitude < 100.0f;
    }

    private void SetThrottle(Vector3 vector)
    {
        if (targetVehicle != null)
        {
            float distanceFactor = Mathf.Clamp01(vector.sqrMagnitude / 60000.0f);
            float targetVelocity = targetVehicle.Control().velocity * (distanceFactor + 1.0f);

            if (control.velocity > targetVelocity)
            {
                control.isAirBrakeOn = true;
                control.throttle = 0.1f;
            }
            else
            {
                control.throttle = Mathf.Clamp01(distanceFactor + 0.2f);
                control.isAirBrakeOn = false;
            }

        }
        else
            control.throttle = 1.0f;
    }
    //===========================================
    // Variable & GetSet Methods
    //===========================================

    public void SetOffset(Vector3 position) { offset = position; }

    private int mode = 0;
    private float bankAngle = 30.0f, yaw = 0.5f;
    private float minX = -10.0f, maxX = 10.0f, zAngle = 5.0f;

    private GameObject target;
    private Aircraft targetVehicle;
    private Vector3 offset;
}









public abstract class LeaderSystem
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    public LeaderSystem() { }

}
public class AviationLeaderSystem : LeaderSystem
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    public AviationLeaderSystem() { }

    //===========================================
    // Methods
    //===========================================
    public void Update()
    {
        for (int i = 0; i < wings.Count; i++)
        {
            if (wings[i].aircraft.gameObject.activeInHierarchy)
                continue;

            wings.RemoveAt(i);
            if (i == 0)
                ChangeLeader?.Invoke(wings[0].aircraft);
        }
    }

    public Vector3 GetOffset(AircraftPilot pilot)
    {
        int i = 0;
        for (; i < wings.Count; i++)
        {
            if (wings[i].pilot == pilot)
                break;
        }

        if (i == wings.Count) return Vector3.zero;

        float left = i % 2 == 0 ? -1.0f : 1.0f;
        float back = -(float)((i + 1) / 2);

        return new Vector3(7.5f * back, 0.0f, 7.5f * back * left);
    }

    public void Add(AircraftPilot pilot, Aircraft aircraft) { wings.Add((pilot, aircraft)); }

    public delegate void ChangeLeaderMethod(Aircraft newLeader);
    public event ChangeLeaderMethod ChangeLeader;

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public bool IsLeader(AircraftPilot pilot) { return pilot == wings[0].pilot; }
    public bool IsLeader(Aircraft aircraft) { return aircraft == wings[0].aircraft; }

    public Aircraft GetLeader() { return wings[0].aircraft; }

    List<(AircraftPilot pilot, Aircraft aircraft)> wings = new List<(AircraftPilot, Aircraft)>();
}

