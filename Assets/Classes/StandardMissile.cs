using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class StandardMissile : Missile
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        maxRange = GameMaster.ConvertWorldScale(maxRange);
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
    // FrameCycle Methods
    //===========================================
    private void Update()
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
        flyDistance -= distance;
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
        if (vector.sqrMagnitude < 25.0f)
        {
            GameMaster.Instance.Factory.Explosion(gameObject.transform.position, 2.5f);
            if (targetRader != null)
                targetRader.TraceEnd(this);
            target.TakeDamage(damage);
            Release();
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(vector);
        Vector3 euler = targetRotation.eulerAngles;
        targetRotation = Quaternion.Euler(euler.x, euler.y, 0f);

        rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime));
    }

    protected override Missile ShootTarget(Vehicle targets)
    {
        StandardMissile newInstance = objectPool.Get();
        newInstance.trail.gameObject.SetActive(true);
        return newInstance;
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public static void InjectObjectPool(ObjectPool<StandardMissile> target) { objectPool = target; }
    public static void RemoveObjectPool() { objectPool = null; }

    static private ObjectPool<StandardMissile> objectPool = null;
}
