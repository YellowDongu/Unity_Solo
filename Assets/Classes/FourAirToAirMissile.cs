using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class FourAirToAirMissile : Missile
{
    void Update()
    {
        Fly();
    }

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

        if (target.IsDestroyed())
            target = null;

        if (target == null)
        {
            rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(gameObject.transform.eulerAngles.x, 0.0f, 0.0f), rotationSpeed * Time.deltaTime));
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

        Quaternion targetRotation = Quaternion.LookRotation(vector);
        Vector3 euler = targetRotation.eulerAngles;
        targetRotation = Quaternion.Euler(euler.x, euler.y, 0f);

        rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime));
    }

    protected override Missile ShootTarget(Vehicle targets)
    {
        FourAirToAirMissile newInstance = objectPool.Get();
        newInstance.trail.gameObject.SetActive(true);
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

    public static void InjectObjectPool(ObjectPool<FourAirToAirMissile> target) { objectPool = target; }
    public static void RemoveObjectPool() { objectPool = null; }

    static private ObjectPool<FourAirToAirMissile> objectPool = null;
    [SerializeField] private TrailRenderer trail = null;
}
