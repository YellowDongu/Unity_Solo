using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using static GaugeUI;

public class FireControlSystem : MonoBehaviour
{
    //===========================================
    // struct/enum
    //===========================================
    [System.Serializable]
    public struct SpecialMissileSlot { public GameObject parent; public List<Missile> slot; } // wrapper

    //===========================================
    // Initializer/Destructor
    //===========================================
    void Start()
    {
        standardCos = Mathf.Cos(standard[0].LockAngle() * Mathf.Deg2Rad);
        refreshTimer = -1.0f;
        gunRPM = 1.0f / (gunRPM / 60.0f);
    }

    public void SetSpecial(int value)
    {
        if (value < 0 || value >= specialSlot.Length)
            return;

        special = value;
        specialSlot[special].parent.SetActive(true);
        specialCos = Mathf.Cos(specialSlot[special].slot[0].LockAngle() * Mathf.Deg2Rad);
        maxCount = specialSlot[special].slot.Count;
        multiShoot = specialSlot[special].slot[0].MultiShoot();
    }

    public void LinkStandard(ReadOnlyCollection<Gauge> gauge)
    {
        float value = standard[0].MaxCoolTime();

        for (int i = 0; i < 2; i++)
        {
            gauge[i].LinkCoolTime(standard[i].CoolTime);
            gauge[i].GetMaxCoolTime(value);
        }
    }

    public void LinkSpecial(ReadOnlyCollection<Gauge> gauge)
    {
        float value = specialSlot[special].slot[0].MaxCoolTime();
        for (int i = 0; i < maxCount; i++)
        {
            gauge[i].LinkCoolTime(specialSlot[special].slot[i].CoolTime);
            gauge[i].GetMaxCoolTime(value);
        }
    }


    //===========================================
    // FrameCycle Methods
    //===========================================
    void Update()
    {
        bulletTime += Time.deltaTime;
        if (bulletTime >= gunRPM)
            bulletTime = gunRPM;
    }

    protected void FixedUpdate()
    {
        if (currentTargets.Count == 0)
            return;

        float raderDistance = rader.RaderDistance();
        raderDistance *= raderDistance;
        raderDistance *= 1.25f;

        if (selectSpecial)
        {
            float distance = specialSlot[special].slot[0].MaxRange();
            float lockSpeed = specialSlot[special].slot[0].LockSpeed() * Time.deltaTime;
            distance *= distance;
            int mask = specialSlot[special].slot[0].AimLayer();
            for (int i = 0; i < currentTargets.Count; i++)
            {
                if (!Lockable(currentTargets[i], mask))
                {
                    lockStatus[i] = 1.0f;
                    continue;
                }

                GameObject current = currentTargets[i].gameObject;
                if (!current.activeInHierarchy)
                {
                    ChangeTarget(false);
                    break;
                }

                Vector3 directionToTarget = current.transform.position - gameObject.transform.position;
                float magnitude = directionToTarget.sqrMagnitude;
                if (magnitude > raderDistance)
                {
                    ChangeTarget(false);
                    break;
                }

                directionToTarget.Normalize();
                if (Vector3.Dot(gameObject.transform.forward, directionToTarget) >= specialCos && magnitude <= distance)
                    lockStatus[i] -= lockSpeed;
                else
                    lockStatus[i] += lockSpeed;

                lockStatus[i] = Mathf.Clamp01(lockStatus[i]);
            }
        }
        else
        {
            float distance = standard[0].MaxRange();
            distance *= distance;

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
            if (Vector3.Dot(gameObject.transform.forward, directionToTarget) >= standardCos && magnitude <= distance)
                lockStatus[0] -= standard[0].LockSpeed() * Time.deltaTime;
            else
                lockStatus[0] += standard[0].LockSpeed() * Time.deltaTime;

            lockStatus[0] = Mathf.Clamp01(lockStatus[0]);
        }
    }


    //===========================================
    // Methods
    //===========================================
    public void Gun(Vehicle vehicle)
    {
        if (bulletTime < gunRPM)
            return;

        bulletTime = 0.0f;
        GameMaster.GetInstance().GetFactory().Shoot(gameObject.transform.position + gameObject.transform.forward * 5.0f, gameObject.transform.rotation, 450.0f, vehicle);
    }

    public bool Missile()
    {
        if (targets.Count == 0)
            return false;

        if (selectSpecial)
        {
            int pointer = 0, shootCount = 0;

            for (int i = 0; i < maxCount; i++)
            {
                if (lockStatus[pointer] >= 0.001f)
                {
                    pointer++;
                    if (pointer >= currentTargets.Count)
                        break;
                    i--;
                    continue;
                }

                if (specialSlot[special].slot[i].Shoot(currentTargets[pointer]))
                {
                    shootCount++;
                    pointer++;
                    if (pointer >= currentTargets.Count || shootCount >= multiShoot)
                        break;
                }

            }
        }
        else
        {
            if (lockStatus[0] >= 0.001f)
                return false;

            foreach (StandardMissile missile in standard)
            {
                if (missile.Shoot(currentTargets[0]))
                    break;
            }
        }
        return true;
    }

    public void ChangeTarget(bool forceRefresh = false)
    {
        BeforeTargetChanged?.Invoke();
        if (targets.Count == 0 || forceRefresh)
            refreshTimer = 0.0f;

        if (refreshTimer <= 0.0f)
        {
            RefreshTarget();
            TargetChanged?.Invoke();
            return;
        }

        int pointerA = 1, pointerB = 0, firstPointer = 0;
        int mask = GetMissileAimLayer();

        while (pointerA < currentTargets.Count)
        {
            if (!currentTargets[pointerA].gameObject.activeInHierarchy || !Lockable(targets[pointerA].Item2, mask))
                currentTargets.RemoveAt(pointerA);
            else
            {
                currentTargets[pointerA - 1] = currentTargets[pointerA];
                pointerA++;
            }
        }

        Vehicle last = currentTargets[currentTargets.Count - 1];
        while (pointerB < targets.Count)
        {
            if (!targets[pointerB].Item2.gameObject.activeInHierarchy)
                targets.RemoveAt(pointerB);
            else
            {
                if (targets[pointerB].Item2 == currentTargets[0])
                    firstPointer = pointerB;

                if (last == targets[pointerB].Item2)
                    break;
                pointerB++;
            }
        }

        HashSet<Vehicle> currentlyAim = new HashSet<Vehicle>(multiShoot);

        foreach (var item in currentTargets)
            currentlyAim.Add(item);

        pointerA--;
        int pointerC = pointerB + 1;
        while (pointerA < maxCount && pointerC != pointerB)
        {
            if (!targets[pointerC].Item2.gameObject.activeInHierarchy)
            {
                targets.RemoveAt(pointerC);
                if (pointerC >= targets.Count)
                    pointerC = 0;
            }
            else
            {
                Vehicle target = targets[pointerC].Item2;
                if (currentlyAim.Contains(target) && Lockable(target, mask))
                {
                    currentlyAim.Add(target);
                    currentTargets.Add(target);
                    pointerA++;
                }

                pointerC = (pointerC + 1) % targets.Count;
            }
        }

        if (lockStatus.Count != currentTargets.Count)
        {
            lockStatus.Clear();
            for (int i = 0; i < currentTargets.Count; i++)
                lockStatus.Add(1.0f);
        }
        else
            for (int i = 0; i < currentTargets.Count; i++)
                lockStatus[i] = 1.0f;

        TargetChanged?.Invoke();
    }


    public void RefreshTarget()
    {
        targets.Clear();

        currentTargets.Clear();
        lockStatus.Clear();
        removeList.Clear();

        float cosThreshold = Mathf.Max(specialCos, standardCos);
        var list = rader.InRangeTarget;
        int max = list.Count;

        refreshTimer = refreshTime;

        for (int i = 0; i < max; i++)
        {
            Vehicle target = list[i];
            if (!target.gameObject.activeInHierarchy)
            {
                removeList.Add(target);
                continue;
            }

            if (target.Team == team)
                continue;

            Vector3 directionToTarget = (target.gameObject.transform.position - gameObject.transform.position);
            targets.Add((directionToTarget.sqrMagnitude * ((Vector3.Dot(gameObject.transform.forward, directionToTarget.normalized) >= cosThreshold) ? 0.5f : 2.0f), target));
        }

        targets.Sort((a, b) => a.Item1.CompareTo(b.Item1));

        int mask = GetMissileAimLayer();
        for (int i = 0; i < multiShoot && i < targets.Count; i++)
        {
            if (!Lockable(targets[i].Item2, mask))
                continue;
            currentTargets.Add(targets[i].Item2);
            lockStatus.Add(1.0f);
        }

        rader.Remove(removeList);
    }

    public void ChangeMissile()
    {
        if (special == -1)
            return;

        selectSpecial = !selectSpecial;
        ChangeState?.Invoke(selectSpecial);

    }

    public bool Lockable(Vehicle vehicle) { return Lockable(vehicle, GetMissileAimLayer()); }
    public bool Lockable(Vehicle vehicle, int mask)
    {
        // bit,    ground/air => 11 air => 1 Ground => 10
        int bit = vehicle.isLand ? 1 : 0;
        bit = 1 << bit;

        return (mask & bit) != 0;
    }


    public delegate void ChangeStateMethod(bool value);
    public delegate void BasicEventMethod();
    public event ChangeStateMethod ChangeState;
    public event BasicEventMethod TargetChanged;
    public event BasicEventMethod BeforeTargetChanged;
    protected void ChangeStateInvoke(bool value) {ChangeState?.Invoke(value);}
    protected void TargetChangedInvoke() {TargetChanged?.Invoke();}
    protected void BeforeTargetChangedInvoke() { BeforeTargetChanged?.Invoke(); }
    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void SetTeam(int value) { team = value; }
    public bool GetSelectState() { return selectSpecial; }
    public virtual int GetMissileAimLayer() { return selectSpecial ? specialSlot[special].slot[0].AimLayer() : standard[0].AimLayer(); }
    public ReadOnlyCollection<Vehicle> Targets => currentTargets.AsReadOnly();
    public ReadOnlyCollection<float> LockStatus => lockStatus.AsReadOnly();
    public GaugeUI.GaugeUIType NeededUIStandard() { return standard[0].NeededUIType(); }
    public GaugeUI.GaugeUIType NeededUISpecial() { if (special == -1) return GaugeUIType.END; return specialSlot[special].slot[0].NeededUIType(); }



    protected bool selectSpecial = false;
    protected int team = 0;
    protected int special = -1;
    protected int maxCount = 1;
    protected int multiShoot = 1;

    protected float standardCos = 0.0f;
    protected float specialCos = 0.0f;

    protected float refreshTime = 0.0f;
    protected float refreshTimer = 0.0f;
    protected float bulletTime = 0.0f;
    [SerializeField] protected float gunRPM = 350.0f;

    protected List<Vehicle> removeList = new List<Vehicle>(20);
    protected List<float> lockStatus = new List<float>(16);
    protected List<Vehicle> currentTargets = new List<Vehicle>(16);
    protected List<(float, Vehicle)> targets = new List<(float, Vehicle)>(16);

    [SerializeField] protected Rader rader = null;
    [SerializeField] protected StandardMissile[] standard;
    [SerializeField] protected SpecialMissileSlot[] specialSlot;
}
