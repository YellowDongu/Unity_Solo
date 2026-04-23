using System.Collections;
using UnityEngine;

public class TurretFireControlSystem : FireControlSystem
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    void Start()
    {
        refreshTimer = -1.0f;

        if(gunRPM == 0)
        {
            bulletTime = 1.0f;
        }
        else
        {
            gunRPM = 1.0f / (gunRPM / 60.0f);
            StartCoroutine(UpdateBulletCoolTime());
        }

        if (standard != null && standard.Length != 0)
        {
            standardCos = Mathf.Cos(standard[0].LockAngle * Mathf.Deg2Rad);

            raderDistance = rader.RaderDistance;
            raderDistance *= raderDistance;
            raderDistance *= 1.25f;

            distance = standard[0].MaxRange;
            distance *= distance;
        }
        else
            noMissile = true;
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    protected void Update()
    {
        bulletTime = Mathf.Clamp(bulletTime - Time.deltaTime, 0.0f, gunRPM);
    }

    protected new void FixedUpdate()
    {
        if (currentTargets.Count == 0)
            return;

        GameObject current = currentTargets[0].gameObject;
        if (!current.activeInHierarchy)
        {
            ChangeTarget(false);
            return;
        }

        Vector3 directionToTarget = current.transform.position - gameObject.transform.position;
        float magnitude = directionToTarget.sqrMagnitude;
        if (magnitude > raderDistance)
        {
            ChangeTarget(false);
            return;
        }

        directionToTarget.Normalize();
        targetAngleDot = Vector3.Dot(gameObject.transform.forward, directionToTarget);

        if (noMissile)
            return;

        //if (targetAngleDot >= standardCos && magnitude <= distance)
        if (magnitude <= distance)
            lockStatus[0] -= standard[0].LockSpeed * Time.deltaTime;
        else
            lockStatus[0] += standard[0].LockSpeed * Time.deltaTime;

        lockStatus[0] = Mathf.Clamp01(lockStatus[0]);
    }

    //===========================================
    // Methods
    //===========================================

    private IEnumerator UpdateBulletCoolTime()
    {
        bulletTime = gunRPM;

        while (bulletTime >= 0.0f)
        {
            bulletTime -= Time.deltaTime;
            yield return null;
        }
        bulletTime = 0.0f;
    }

    public new void Gun(Vehicle vehicle)
    {
        if (bulletTime >= 0.0f)
            return;

        bulletTime = gunRPM;
        GameMaster.Instance.Factory.ShootNonGravity(gameObject.transform.position + gunBarrel.transform.forward * 5.0f, gunBarrel.transform.rotation, 450.0f, vehicle);
    }

    public new bool Missile()
    {
        if (noMissile)
            return true;

        return base.Missile();
    }

    //public new void ChangeTarget(bool forceRefresh = false)
    //{
    //    base.BeforeTargetChangedInvoke();
    //    if (targets.Count == 0 || forceRefresh)
    //        refreshTimer = 0.0f;
    //
    //    if (refreshTimer <= 0.0f)
    //    {
    //        RefreshTarget();
    //        TargetChangedInvoke();
    //        return;
    //    }
    //
    //    int pointerA = 1, pointerB = 0, firstPointer = 0;
    //    int mask = GetMissileAimLayer();
    //
    //    while (pointerA < currentTargets.Count)
    //    {
    //        if (!currentTargets[pointerA].gameObject.activeInHierarchy || !Lockable(targets[pointerA].Item2, mask))
    //            currentTargets.RemoveAt(pointerA);
    //        else
    //        {
    //            currentTargets[pointerA - 1] = currentTargets[pointerA];
    //            pointerA++;
    //        }
    //    }
    //
    //    Vehicle last = currentTargets[currentTargets.Count - 1];
    //    while (pointerB < targets.Count)
    //    {
    //        if (!targets[pointerB].Item2.gameObject.activeInHierarchy)
    //            targets.RemoveAt(pointerB);
    //        else
    //        {
    //            if (targets[pointerB].Item2 == currentTargets[0])
    //                firstPointer = pointerB;
    //
    //            if (last == targets[pointerB].Item2)
    //                break;
    //            pointerB++;
    //        }
    //    }
    //
    //    HashSet<Vehicle> currentlyAim = new HashSet<Vehicle>(multiShoot);
    //
    //    foreach (var item in currentTargets)
    //        currentlyAim.Add(item);
    //
    //    pointerA--;
    //    int pointerC = pointerB + 1;
    //    while (pointerA < maxCount && pointerC != pointerB)
    //    {
    //        if (!targets[pointerC].Item2.gameObject.activeInHierarchy)
    //        {
    //            targets.RemoveAt(pointerC);
    //            if (pointerC >= targets.Count)
    //                pointerC = 0;
    //        }
    //        else
    //        {
    //            Vehicle target = targets[pointerC].Item2;
    //            if (currentlyAim.Contains(target) && Lockable(target, mask))
    //            {
    //                currentlyAim.Add(target);
    //                currentTargets.Add(target);
    //                pointerA++;
    //            }
    //
    //            pointerC = (pointerC + 1) % targets.Count;
    //        }
    //    }
    //
    //    if (lockStatus.Count != currentTargets.Count)
    //    {
    //        lockStatus.Clear();
    //        for (int i = 0; i < currentTargets.Count; i++)
    //            lockStatus.Add(1.0f);
    //    }
    //    else
    //        for (int i = 0; i < currentTargets.Count; i++)
    //            lockStatus[i] = 1.0f;
    //
    //    TargetChangedInvoke();
    //}

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public override int GetMissileAimLayer() { return noMissile ? 3 : standard[0].AimLayer; }
    public float TargetAngle { get { return targetAngleDot; } private set { targetAngleDot = value; } }


    private bool noMissile;

    private float distance;
    private float raderDistance;
    private float targetAngleDot;

    [SerializeField] private GameObject gunBarrel;
}
