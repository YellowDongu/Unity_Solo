using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using static Rader;

public enum VehicleID
{
    None,
    F16C,
    F15E,
    END
}

public class Factory : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        reserved.weaponSelected = 0;
        reserved.selected = VehicleID.END;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        //GameObject newInstance = Instantiate(playerPrefab);
        //player = newInstance.GetComponent<Player>();
        Initialize();
    }
    public void Initialize()
    {
        if (prefabs != null)
            return;

        prefabs = new Dictionary<VehicleID, (GameObject prefab, ObjectPool<Vehicle> pool)>((int)VehicleID.END);
        GameObject[] list = Resources.LoadAll<GameObject>("Prefabs/Aircraft");
        foreach (GameObject item in list)
        {
            Vehicle vehicle = item.GetComponent<Vehicle>();
            if (vehicle == null || prefabs.ContainsKey(vehicle.ID))
                continue;

            ObjectPool<Vehicle> newPool = new ObjectPool<Vehicle>(createFunc: () => Instantiate(item).GetComponent<Vehicle>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj), maxSize: 50);

            prefabs.Add(vehicle.ID, (item, newPool));
        }


        explosion = Instantiate(explosionPrefab).GetComponent<Explosion>();
        explosion.gameObject.transform.SetParent(gameObject.transform);
        explosionObjectPool = new ObjectPool<Explosion>(createFunc: () => Instantiate(explosionPrefab).GetComponent<Explosion>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj), maxSize: 50);
        aiObjectPool = new ObjectPool<AIPilot>(createFunc: () => Instantiate(aircraftAIPrefab).GetComponent<AIPilot>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj), maxSize: 50);

        bulletObjectPool = new ObjectPool<Bullet>(createFunc: () => Instantiate(bulletPrefab).GetComponent<Bullet>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj), maxSize: 1000);


        SMPool = new ObjectPool<StandardMissile>(createFunc: () => Instantiate(SMPrefab).GetComponent<StandardMissile>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj.gameObject), collectionCheck: false, defaultCapacity: 10, maxSize: 100);
        StandardMissile.InjectObjectPool(SMPool);
        FAAMPool = new ObjectPool<FourAirToAirMissile>(createFunc: () => Instantiate(FAAMPrefab).GetComponent<FourAirToAirMissile>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj.gameObject), collectionCheck: false, defaultCapacity: 10, maxSize: 100);
        FourAirToAirMissile.InjectObjectPool(FAAMPool);
        SARMPool = new ObjectPool<SemiActiveRaderMissile>(createFunc: () => Instantiate(SARMPrefab).GetComponent<SemiActiveRaderMissile>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj.gameObject), collectionCheck: false, defaultCapacity: 10, maxSize: 100);
        SemiActiveRaderMissile.InjectObjectPool(SARMPool);

    }

    void Start()
    {
        targets = new List<Vehicle>[length];
        for (int i = 0; i < length; i++)
            targets[i] = new List<Vehicle>(30);

    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        StandardMissile.RemoveObjectPool();
        FourAirToAirMissile.RemoveObjectPool();
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    private void FixedUpdate()
    {
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < targets[i].Count; j++)
            {
                if (!targets[i][j].gameObject.activeInHierarchy)
                {
                    targets[i].RemoveAt(j);
                }
            }

        }
    }

    //===========================================
    // Methods
    //===========================================
    private void OnSceneUnloaded(Scene current)
    {
        foreach ((VehicleID id, (GameObject prefab, ObjectPool<Vehicle> pool)) in prefabs)
            pool.Clear();

        aiObjectPool.Clear();
        explosionObjectPool.Clear();

        bulletObjectPool.Clear();
        SMPool.Clear();
        FAAMPool.Clear();
        
        player = null;
    }

    public Player CreatePlayer(Pilot.PilotInfo infomation, out Aircraft vehicle)
    {
        vehicle = null;
        if (player != null)
            return null;

        player = Instantiate(playerPrefab).GetComponent<Player>();

        if (reserved.selected == VehicleID.END)
            vehicle = null;
        else
        {
            var pool = prefabs[reserved.selected].pool;
            vehicle = pool.Get() as Aircraft;
            vehicle.SetRelease(pool.Release);
            vehicle.SetSpecial(reserved.weaponSelected);
            vehicle.releaseEvent += player.Release;
            player.SetInfomation(infomation);
            player.Attach(vehicle);

            reserved.selected = VehicleID.END;
        }

        return player;
    }

    public AIPilot CreateAircraftAI() { return aiObjectPool.Get(); }


    public Aircraft Spawn(Spawn_Air infomation, out AircraftPilot pilot, AviationLeaderSystem leaderSystem = null)
    {
        Aircraft vehicle = null;

        pilot = infomation.player ? CreatePlayer(infomation.pilot, out vehicle) : CreateAircraftAI();

        if (vehicle == null)
        {
            var pool = prefabs[infomation.id].pool;
            vehicle = pool.Get() as Aircraft;
            vehicle.SetSpecial(infomation.specialWeapon);
            vehicle.SetRelease(pool.Release);
            vehicle.releaseEvent += pilot.Release;
            pilot.SetInfomation(infomation.pilot);
            pilot.Attach(vehicle);
        }

        if (infomation.isTGT)
            TGTCount++;

        if (leaderSystem != null)
        {
            leaderSystem.Add(pilot, vehicle);
            pilot.SetLeaderSystem(leaderSystem);
        }

        targets[infomation.pilot.team].Add(vehicle);

        return vehicle;
    }

    public void Spawn(SpawnPoint point, int teamID, int leaderIndex, Spawn_Air[] spawnTargetList)
    {
        Aircraft vehicle, leaderVehicle;
        AviationLeaderSystem leaderSystem = null;

        Vector3 forward = Vector3.Scale(point.gameObject.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 right = Vector3.Scale(point.gameObject.transform.right, new Vector3(1, 0, 1)).normalized;

        bool leaderExsist = leaderIndex >= 0 && leaderIndex < spawnTargetList.Length;
        if (leaderExsist)
        {
            leaderSystem = new AviationLeaderSystem();
            leaderVehicle = Spawn(spawnTargetList[leaderIndex], out AircraftPilot leader, leaderSystem);

            float left = leaderIndex % 2 == 0 ? -1.0f : 1.0f;
            float back = (float)(leaderIndex / 2);

            leaderVehicle.gameObject.transform.position = (right * 7.5f * back * left) + (forward * 7.5f * back) + point.gameObject.transform.position;
            leaderVehicle.gameObject.transform.rotation = point.gameObject.transform.rotation;
        }

        for (int i = 0; i < spawnTargetList.Length; i++)
        {
            if (leaderIndex == i) continue;

            vehicle = Spawn(spawnTargetList[i], out AircraftPilot pilot, leaderSystem);

            float left = i % 2 == 0 ? -1.0f : 1.0f;
            float back = (float)(i / 2) + 1;

            vehicle.gameObject.transform.position = (right * 7.5f * back * left) + (forward * 7.5f * back) + point.gameObject.transform.position;
            vehicle.gameObject.transform.rotation = point.gameObject.transform.rotation;
        }
    }

    public Vector3 GetPosition(SpawnPoint point, int index)
    {
        float left = index % 2 == 0 ? -1.0f : 1.0f;
        float back = (float)(index / 2);

        Vector3 forward = Vector3.Scale(point.gameObject.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 right = Vector3.Scale(point.gameObject.transform.right, new Vector3(1, 0, 1)).normalized;

        return (right * 7.5f * back * left) + (forward * 7.5f * back) + point.gameObject.transform.position;
    }

    public Vehicle Create(VehicleID vehicleID) { return prefabs[vehicleID].pool.Get(); }

    public Vehicle Create(Pilot pilot, VehicleID vehicleID)
    {
        Vehicle newInstnace = prefabs[vehicleID].pool.Get();

        pilot.Attach(newInstnace);

        return newInstnace;
    }

    public void ReleaseAI(AIPilot ai)
    {
        ai.transform.parent = null;
        aiObjectPool.Release(ai);
    }

    public void Shoot(Vector3 position, Quaternion rotation, float velocity, Vehicle shooted)
    {
        bulletObjectPool.Get().Shoot(position, rotation, velocity, shooted, bulletObjectPool);
    }


    public void Explosion(GameObject target)
    {
        if (target == null)
            return;

        Explosion newInstnace = explosionObjectPool.Get();
        newInstnace.Emit(target);
    }

    public void Explosion(Vector3 worldPos, float size = 5.0f) { explosion.Emit(worldPos, size); }


    //===========================================
    // Variable & GetSet Methods
    //===========================================

    public void EnlistRaderUI(RaderUI target) { raderUI = target; }
    public void ReservePlayerVehicle(PlayerSpawnData infomation) { reserved = infomation; }
    public bool IsTGTEmpty() { return TGTCount <= 0; }
    public ReadOnlyCollection<Vehicle> GetAll(int teamID) { return targets[teamID].AsReadOnly(); }


    private PlayerSpawnData reserved;
    private Player player = null;
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private GameObject aircraftAIPrefab = null;
    [SerializeField] private GameObject explosionPrefab = null;
    [SerializeField] private GameObject bulletPrefab = null;

    private Explosion explosion = null;


    const int length = 4;
    private List<Vehicle>[] targets;
    private int TGTCount = 0;

    private RaderUI raderUI = null;
    private ObjectPool<AIPilot> aiObjectPool = null;
    private ObjectPool<Explosion> explosionObjectPool = null;
    private ObjectPool<Bullet> bulletObjectPool = null;


    [SerializeField] GameObject FAAMPrefab;
    [SerializeField] GameObject SMPrefab;
    [SerializeField] GameObject SARMPrefab;
    private ObjectPool<SemiActiveRaderMissile> SARMPool;
    private ObjectPool<FourAirToAirMissile> FAAMPool;
    private ObjectPool<StandardMissile> SMPool;



    private Dictionary<VehicleID, (GameObject prefab, ObjectPool<Vehicle> pool)> prefabs;



}
