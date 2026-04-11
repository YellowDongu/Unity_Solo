using System.Collections;
using UnityEngine;

public abstract class Missile : MonoBehaviour
{
    //===========================================
    // Methods
    //===========================================
    public bool Shoot(Vehicle locked)
    {
        if (isCoolTime)
            return false;

        Missile newInstance = ShootTarget(locked);
        newInstance.target = locked;
        newInstance.transform.position = gameObject.transform.position;
        newInstance.transform.rotation = gameObject.transform.rotation;
        newInstance.isProjectile = true;
        newInstance.flyDistance = maxRange;

        targetRader = locked.gameObject.GetComponent<Rader>();
        if(targetRader != null)
            targetRader.Trace(newInstance);

        StartCoroutine(ActiveCoolTime());
        return true;
    }

    protected abstract Missile ShootTarget(Vehicle targets);
    protected abstract void Release();

    protected IEnumerator ActiveCoolTime()
    {
        isCoolTime = true;
        mesh.SetActive(false);
        time = coolTime;

        while (time >= 0.0f)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        isCoolTime = false;
        mesh.SetActive(true);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public float LockAngle()  { return lockAngle; }
    public float MaxRange()  { return maxRange; }
    public int TargetCount() { return targetCount; }
    public int MultiShoot() { return multiShoot; }
    public float LockSpeed() { return lockSpeed; }
    public float CoolTime() { return time; }
    public float MaxCoolTime() { return coolTime; }
    public GaugeUI.GaugeUIType NeededUIType() { return neededUI; }


    protected bool isCoolTime = false;
    protected bool isProjectile = false;
    protected float time = 0.0f;
    protected float flyDistance = 0.0f;

    [SerializeField] protected int damage = 70;
    [SerializeField] protected int targetCount = 1;
    [SerializeField] protected float coolTime = 30.0f;
    [SerializeField] protected float maxRange = 1000.0f;
    [SerializeField] protected float velocity = 30.0f;
    [SerializeField] protected float rotationSpeed = 3.5f;
    [SerializeField] protected float lockAngle = 45.0f;
    [SerializeField] protected float lockSpeed = 1.0f;
    [SerializeField] protected int multiShoot = 1;
    [SerializeField] protected GaugeUI.GaugeUIType neededUI;

    protected Rader targetRader = null;
    protected Vehicle target = null;
    [SerializeField] protected GameObject mesh;
    [SerializeField] protected Rigidbody rigidbody = null;
}
