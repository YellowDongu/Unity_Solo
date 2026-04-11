using System.Collections;
using UnityEngine;

//===========================================
// struct/enum
[System.Serializable]
public struct Spawn_Air
{
    public VehicleID id;
    public Pilot.PilotInfo pilot;
    public bool player;
    public bool isTGT;
    public int specialWeapon;
}

public struct Spawn_Ground
{
    public VehicleID id;
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
                return GameMaster.GetInstance().GetFactory().IsTGTEmpty();
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
        {
            Spawn();
            ActiveNextSpawnPoint();
            Destroy(gameObject);
        }
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    void Update()
    {
        //ActiveNextSpawnPoint();
        Spawn();
        Destroy(gameObject);
    }


    //===========================================
    // Methods
    //===========================================
    private void ActiveNextSpawnPoint()
    {
        foreach (SpawnPoint point in linkedPoint)
            point.gameObject.SetActive(true);
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
        while (true)
        {
            if(activeCondition.MetCondition())
            {
                ActiveNextSpawnPoint();
                Destroy(gameObject);
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void Spawn()
    {
        GameMaster.GetInstance().GetFactory().Spawn(this, teamID, leaderIndex, spawnTargetList);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    [SerializeField] private ActiveCondition activeCondition; // deactive when activeOnStart is true
    [SerializeField] private bool activeOnStart = false;
    [SerializeField] private bool isActive = false;
    [SerializeField] private int teamID;
    [SerializeField] private int leaderIndex;
    [SerializeField] private Spawn_Air[] spawnTargetList;
    [SerializeField] private Spawn_Ground[] spawnGroundList;
    [SerializeField] private SpawnPoint[] linkedPoint;
}
