using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class FourAirToGroundMissile : Missile
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        maxRange = GameMaster.ConvertWorldScale(maxRange);
        flyDistance = float.MaxValue;
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    void Update()
    {
        Fly();
    }

    //===========================================
    // Methods
    //===========================================
    private void Fly()
    {
        if (!isProjectile)
            return;

        if (flyDistance < 0.0f)
        {
            Release();
            return;
        }

        float distance = velocity * Time.deltaTime;
        rigidbody.MovePosition(gameObject.transform.position + gameObject.transform.forward * distance);

        if (target == null)
        {
            rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(gameObject.transform.eulerAngles.x, 0.0f, 0.0f), rotationSpeed * Time.deltaTime));
            return;
        }
        if (target.IsDestroyed() || !target.gameObject.activeInHierarchy)
        {
            target = null;
            return;
        }

        Vector3 vector = target.transform.position - gameObject.transform.position;
        SendDistance(vector.sqrMagnitude);

        Vector3 xzVector = vector;
        xzVector.y = 0.0f;
        Quaternion targetRotation;

        if (xzVector.sqrMagnitude < 2500.0f)
        {
            if (vector.sqrMagnitude < 25.0f)
            {
                GameMaster.GetInstance().GetFactory().Explosion(gameObject.transform.position, 2.5f);
                if (targetRader != null)
                    targetRader.TraceEnd(this);
                target.TakeDamage(damage);
                Release();
                return;
            }

            if (vector.sqrMagnitude > flyDistance)
            {
                Release();
                return;
            }

            flyDistance = vector.sqrMagnitude;

            Vector3 euler = Quaternion.LookRotation(vector).eulerAngles;
            targetRotation = Quaternion.Euler(euler.x, euler.y, 0f);
        }
        else
        {
            vector = target.transform.position;
            vector.y = 150.0f;
            vector -= gameObject.transform.position;

            Vector3 euler = Quaternion.LookRotation(vector).eulerAngles;
            targetRotation = Quaternion.Euler(euler.x, euler.y, 0f);
        }


        rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime));
    }

    protected override Missile ShootTarget(Vehicle targets)
    {
        FourAirToGroundMissile newInstance = objectPool.Get();
        newInstance.trail.gameObject.SetActive(true);
        return newInstance;
    }

    protected override void Release()
    {
        isProjectile = false;
        mesh.SetActive(false);
        ReleaseEvent();
        StartCoroutine(DelayRelease());
    }

    protected IEnumerator DelayRelease()
    {
        float time = trail.time;

        while (time > 0.0f)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        objectPool.Release(this);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public static void InjectObjectPool(ObjectPool<FourAirToGroundMissile> target) { objectPool = target; }
    public static void RemoveObjectPool() { objectPool = null; }

    static private ObjectPool<FourAirToGroundMissile> objectPool = null;
}
