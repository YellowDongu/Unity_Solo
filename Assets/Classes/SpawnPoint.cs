using UnityEngine;

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

public class SpawnPoint : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (activeOnStart)
        {

            Spawn();
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //ActiveNextSpawnPoint();
        Spawn();
        Destroy(gameObject);
    }


    private void ActiveNextSpawnPoint()
    {
        foreach (SpawnPoint point in linkedPoint)
            point.gameObject.SetActive(true);
    }

    public void Spawn()
    {
        GameMaster.GetInstance().GetFactory().Spawn(this, teamID, leaderIndex, spawnTargetList);
    }

    [SerializeField] private bool activeOnStart = false;
    [SerializeField] private int teamID;
    [SerializeField] private int leaderIndex;
    [SerializeField] private Spawn_Air[] spawnTargetList;
    [SerializeField] private Spawn_Ground[] spawnGroundList;
    [SerializeField] private SpawnPoint[] linkedPoint;
}
