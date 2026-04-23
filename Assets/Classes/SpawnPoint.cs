using System;
using System.Collections;
using UnityEngine;

//===========================================
// struct/enum
[System.Serializable]
public struct Spawn_Air
{
    public bool player;
    public int specialWeapon;
    public VehicleID id;
    public Pilot.PilotInfo pilot;
}

[System.Serializable]
public struct Spawn_Ground
{
    public bool isTGT;
    public VehicleID id;
    public GameObject spawnPoint;
    public Pilot.PilotInfo pilot;
}

[System.Serializable]
public struct ActiveCondition
{
    enum Condition
    {
        None,
        Time,
        TGT,
        Disabled,
        END
    }

    public bool MetCondition()
    {
        switch (condition)
        {
            case Condition.Time:
                time -= Time.deltaTime;
                return time <= 0.0f;
            case Condition.TGT:
                return GameMaster.Instance.IsTGTEmpty();
            case Condition.Disabled:
                return false;
            default:
                return true;
        }
    }
    [SerializeField] private Condition condition;
    [SerializeField] private float time;
}

//===========================================

public class SpawnPoint : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        if (activeOnStart)
            Active();
    }

    //===========================================
    // Methods
    //===========================================
    private void ActiveNextSpawnPoint()
    {
        foreach (SpawnPoint point in linkedPoint)
            point.Active();
    }

    public void Active()
    {
        if (isActive)
            return;

        isActive = true;
        StartCoroutine(CheckActiveStatus());
    }

    private IEnumerator CheckActiveStatus()
    {
        while (gameObject.activeInHierarchy)
        {
            if (activeCondition.MetCondition())
            {
                if (endFlag)
                {
                    GameMaster.Instance.EndMission(true);
                }
                else
                {
                    Spawn();
                    ActiveNextSpawnPoint();
                }

                Destroy(gameObject);
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void Spawn()
    {
        if (airforce)
            GameMaster.Instance.Factory.Spawn(this, spawnTargetList);
        else
            GameMaster.Instance.Factory.Spawn(this, spawnGroundList);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public int TeamID { get { return teamID; } private set { teamID = value; } }
    public int LeaderIndex { get { return leaderIndex; } private set { leaderIndex = value; } }

    [SerializeField] protected ActiveCondition activeCondition;
    [SerializeField] protected bool activeOnStart = false;
    [SerializeField] protected bool isActive = false;
    [SerializeField] protected bool airforce = true;
    [SerializeField] protected bool endFlag = false;
    [SerializeField] protected int teamID;
    [SerializeField] protected int leaderIndex;
    [SerializeField] protected Spawn_Air[] spawnTargetList;
    [SerializeField] protected Spawn_Ground[] spawnGroundList;
    [SerializeField] protected SpawnPoint[] linkedPoint;
}
