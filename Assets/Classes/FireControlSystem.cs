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
        gunRPM /= 3600.0f;
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

    private void FixedUpdate()
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

            for (int i = 0; i < currentTargets.Count; i++)
            {
                GameObject current = currentTargets[i].gameObject;
                if (!current.activeInHierarchy)
                {
                    ChangeTarget();
                    break;
                }

                Vector3 directionToTarget = current.transform.position - gameObject.transform.position;
                float magnitude = directionToTarget.sqrMagnitude;
                if (magnitude > raderDistance)
                {
                    ChangeTarget();
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
                ChangeTarget();
                return;
            }

            Vector3 directionToTarget = current.transform.position - gameObject.transform.position;
            float magnitude = directionToTarget.sqrMagnitude;
            if (magnitude > raderDistance)
            {
                ChangeTarget();
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
        GameMaster.GetInstance().GetFactory().Shoot(gameObject.transform.position + gameObject.transform.forward * 5.0f, gameObject.transform.rotation, 600.0f, vehicle);
    }

    public void Missile()
    {
        if (targets.Count == 0)
            return;

        if (selectSpecial)
        {
            int pointerA = 0, pointerB = 0;

            for (int i = 0; i < multiShoot; i++)
            {
                if (lockStatus[pointerB] >= 0.001f)
                {
                    pointerB++;
                    if (pointerB >= currentTargets.Count)
                        break;
                    i--;
                    continue;
                }

                if (specialSlot[special].slot[pointerA].Shoot(currentTargets[pointerB]))
                {
                    pointerB++;
                    if (pointerB >= currentTargets.Count)
                        break;
                }

                pointerA++;
                if (pointerA >= maxCount)
                    break;
            }
        }
        else
        {
            if (lockStatus[0] >= 0.001f)
                return;

            foreach (StandardMissile missile in standard)
            {
                if (missile.Shoot(currentTargets[0]))
                    break;
            }
        }
    }

    public void ChangeTarget(bool forceRefresh = false)
    {
        int pointerA = 1, pointerB = 0;

        if (targets.Count == 0 || forceRefresh)
            refreshTimer = 0.0f;

        if (refreshTimer <= 0.0f)
        {
            RefreshTarget();
            return;
        }

        while (pointerA < currentTargets.Count)
        {
            if (!currentTargets[pointerA].gameObject.activeInHierarchy)
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
                if (last == targets[pointerB].Item2)
                    break;
                pointerB++;
            }
        }

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
                currentTargets.Add(targets[pointerC].Item2);
                pointerA++;
                pointerC = (pointerC + 1) % targets.Count;
            }
        }

        if (lockStatus.Count != currentTargets.Count)
        {
            lockStatus.Clear();
            for (int i = 0; i < currentTargets.Count; i++)
                lockStatus.Add(0.0f);
        }
        else
            for (int i = 0; i < currentTargets.Count; i++)
                lockStatus[0] = 0.0f;
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
                removeList.Add(i);
                continue;
            }

            if (target.Team == team)
                continue;

            Vector3 directionToTarget = (target.gameObject.transform.position - gameObject.transform.position);
            targets.Add((directionToTarget.sqrMagnitude * ((Vector3.Dot(gameObject.transform.forward, directionToTarget.normalized) >= cosThreshold) ? 0.5f : 2.0f), target));
        }

        targets.Sort((a, b) => a.Item1.CompareTo(b.Item1));

        for (int i = 0; i < maxCount && i < targets.Count; i++)
        {
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
        for (int i = 0; i < lockStatus.Count; i++)
            lockStatus[i] = 0.0f;
        ChangeState?.Invoke(selectSpecial);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void SetTeam(int value) { team = value; }
    public bool GetSelectState() { return selectSpecial; }
    public ReadOnlyCollection<Vehicle> Targets => currentTargets.AsReadOnly();
    public ReadOnlyCollection<float> LockStatus => lockStatus.AsReadOnly();
    public GaugeUI.GaugeUIType NeededUIStandard() { return standard[0].NeededUIType(); }
    public GaugeUI.GaugeUIType NeededUISpecial() { if (special == -1) return GaugeUIType.END; return specialSlot[special].slot[0].NeededUIType(); }

    public delegate void ChangeStateMethod(bool value);
    public event ChangeStateMethod ChangeState;

    private bool selectSpecial = false;

    private int team = 0;
    private int special = -1;
    private int maxCount = 1;
    private int multiShoot = 1;

    private float standardCos = 0.0f;
    private float specialCos = 0.0f;

    private float refreshTime = 0.0f;
    private float refreshTimer = 0.0f;
    private float bulletTime = 0.0f;
    private float gunRPM = 350.0f;

    private List<int> removeList = new List<int>(20);
    private List<float> lockStatus = new List<float>(16);
    private List<Vehicle> currentTargets = new List<Vehicle>(16);
    private List<(float, Vehicle)> targets = new List<(float, Vehicle)>(16);

    [SerializeField] private Rader rader = null;
    [SerializeField] private StandardMissile[] standard;
    [SerializeField] private SpecialMissileSlot[] specialSlot;
}
