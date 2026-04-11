using System.Collections;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class SemiActiveRaderMissile : Missile
{
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

        if (target == null || lockStatus[index] <= 0.0f)
        {
            rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(gameObject.transform.eulerAngles.x, 0.0f, 0.0f), rotationSpeed * Time.deltaTime));
            return;
        }

        if (target.IsDestroyed() || target.gameObject.activeInHierarchy)
        {
            target = null;
            return;
        }

        Vector3 vector = target.transform.position - gameObject.transform.position;
        if (vector.sqrMagnitude < 9.0f)
        {
            GameMaster.GetInstance().GetFactory().Explosion(gameObject.transform.position, 2.5f);
            targetRader.TraceEnd(this);
            target.TakeDamage(damage);
            Release();
            return;
        }
        //float fps = 1.0f / Time.deltaTime;
        Vector3 vectorDelta = (target.transform.position - PreviousPosition) / Time.deltaTime;
        PreviousPosition = target.transform.position;
        Vector3 interceptPosition = target.transform.position + (vectorDelta * Mathf.Sqrt(vector.sqrMagnitude / (velocity * velocity)));
        interceptPosition -= gameObject.transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(interceptPosition);
        Vector3 euler = targetRotation.eulerAngles;
        targetRotation = Quaternion.Euler(euler.x, euler.y, 0f);

        rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime));
    }

    protected override Missile ShootTarget(Vehicle targets)
    {
        SemiActiveRaderMissile newInstance = objectPool.Get();
        newInstance.PreviousPosition = targets.transform.position;
        newInstance.trail.gameObject.SetActive(true);
        newInstance.lockStatus = fcs.LockStatus;
        newInstance.fcs = null;
        return newInstance;
    }

    protected override void Release()
    {
        isProjectile = false;
        mesh.SetActive(false);
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
    public static void InjectObjectPool(ObjectPool<SemiActiveRaderMissile> target) { objectPool = target; }
    public static void RemoveObjectPool() { objectPool = null; }

    [SerializeField] private int index;
    private Vector3 PreviousPosition;
    private ReadOnlyCollection<float> lockStatus;
    static private ObjectPool<SemiActiveRaderMissile> objectPool = null;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private FireControlSystem fcs;
}
