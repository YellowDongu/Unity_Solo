using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public enum VehicleID
{
    None,
    F16C,
    F15E,
    END = 5,
    GroundStart = 99,
    AA = 100,
    SAM
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
        Initialize();
    }
    public void Initialize()
    {
        if (isInitialized)
            return;

        isInitialized = true;
        prefabs = new Dictionary<VehicleID, (GameObject prefab, ObjectPool<Vehicle> pool)>((int)VehicleID.END);
        GameObject[] list = Resources.LoadAll<GameObject>("Prefabs/Aircraft");
        foreach (GameObject item in list)
        {
            Vehicle vehicle = item.GetComponent<Vehicle>();
            if (vehicle == null || prefabs.ContainsKey(vehicle.ID))
                continue;

            ObjectPool<Vehicle> newPool = new ObjectPool<Vehicle>(createFunc: () => Instantiate(item).GetComponent<Vehicle>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => { if ((obj != null && obj.gameObject != null) || !obj.IsDestroyed()) Destroy(obj.gameObject); }, maxSize: 50);

            prefabs.Add(vehicle.ID, (item, newPool));
        }

        list = Resources.LoadAll<GameObject>("Prefabs/Ground");
        foreach (GameObject item in list)
        {
            Vehicle vehicle = item.GetComponent<Vehicle>();
            if (vehicle == null || prefabs.ContainsKey(vehicle.ID))
                continue;

            ObjectPool<Vehicle> newPool = new ObjectPool<Vehicle>(createFunc: () => Instantiate(item).GetComponent<Vehicle>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => { if ((obj != null && obj.gameObject != null) || !obj.IsDestroyed()) Destroy(obj.gameObject); }, maxSize: 50);

            prefabs.Add(vehicle.ID, (item, newPool));
        }


        explosion = Instantiate(explosionPrefab).GetComponent<Explosion>();
        explosion.gameObject.transform.SetParent(gameObject.transform);
        explosionObjectPool = new ObjectPool<Explosion>(createFunc: () => Instantiate(explosionPrefab).GetComponent<Explosion>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => { if ((obj != null && obj.gameObject != null) || !obj.IsDestroyed()) Destroy(obj.gameObject); }, maxSize: 50);
        aiObjectPool = new ObjectPool<AIPilot>(createFunc: () => Instantiate(aircraftAIPrefab).GetComponent<AIPilot>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => { if ((obj != null && obj.gameObject != null) || !obj.IsDestroyed()) Destroy(obj.gameObject); }, maxSize: 50);
        groundAIPool = new ObjectPool<AIDriver>(createFunc: () => Instantiate(groundAIPrefab).GetComponent<AIDriver>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => { if ((obj != null && obj.gameObject != null) || !obj.IsDestroyed()) Destroy(obj.gameObject); }, maxSize: 50);
        bulletObjectPool = new ObjectPool<Bullet>(createFunc: () => Instantiate(bulletPrefab).GetComponent<Bullet>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => { if ((obj != null && obj.gameObject != null) || !obj.IsDestroyed()) Destroy(obj.gameObject); }, maxSize: 1000);


        SMPool = new ObjectPool<StandardMissile>(createFunc: () => Instantiate(SMPrefab).GetComponent<StandardMissile>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => { if ((obj != null && obj.gameObject != null) || !obj.IsDestroyed()) Destroy(obj.gameObject); }, collectionCheck: false, defaultCapacity: 10, maxSize: 100);
        StandardMissile.InjectObjectPool(SMPool);
        FAAMPool = new ObjectPool<FourAirToAirMissile>(createFunc: () => Instantiate(FAAMPrefab).GetComponent<FourAirToAirMissile>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => { if ((obj != null && obj.gameObject != null) || !obj.IsDestroyed()) Destroy(obj.gameObject); }, collectionCheck: false, defaultCapacity: 10, maxSize: 100);
        FourAirToAirMissile.InjectObjectPool(FAAMPool);
        SARMPool = new ObjectPool<SemiActiveRaderMissile>(createFunc: () => Instantiate(SARMPrefab).GetComponent<SemiActiveRaderMissile>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => { if ((obj != null && obj.gameObject != null) || !obj.IsDestroyed()) Destroy(obj.gameObject); }, collectionCheck: false, defaultCapacity: 10, maxSize: 100);
        SemiActiveRaderMissile.InjectObjectPool(SARMPool);
        FAGMPool = new ObjectPool<FourAirToGroundMissile>(createFunc: () => Instantiate(FAGMPrefab).GetComponent<FourAirToGroundMissile>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => { if ((obj != null && obj.gameObject != null) || !obj.IsDestroyed()) Destroy(obj.gameObject); }, collectionCheck: false, defaultCapacity: 10, maxSize: 100);
        FourAirToGroundMissile.InjectObjectPool(FAGMPool);


        FlarePool = new ObjectPool<Flare>(createFunc: () => Instantiate(FlarePrefab).GetComponent<Flare>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => { if ((obj != null && obj.gameObject != null) || !obj.IsDestroyed()) Destroy(obj.gameObject); }, collectionCheck: false, defaultCapacity: 10, maxSize: 100);
    }

    void Start()
    {
        targets = new List<Vehicle>[length];
        for (int i = 0; i < length; i++)
            targets[i] = new List<Vehicle>(30);
    }

    private void OnDestroy()
    {
        isInitialized = false;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        StandardMissile.RemoveObjectPool();
        FourAirToAirMissile.RemoveObjectPool();
        SemiActiveRaderMissile.RemoveObjectPool();
        FourAirToGroundMissile.RemoveObjectPool();
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
                if (targets[i][j] == null || targets[i][j].IsDestroyed() || !targets[i][j].gameObject.activeInHierarchy)
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

        foreach (var list in targets)
            list.Clear();

        explosionObjectPool.Clear();
        aiObjectPool.Clear();
        groundAIPool.Clear();
        bulletObjectPool.Clear();
        SMPool.Clear();
        FAAMPool.Clear();
        SARMPool.Clear();
        FAGMPool.Clear();
        FlarePool.Clear();

        TGTCount = 0;
        player = null;
    }

    public Player CreatePlayer(Pilot.PilotInfo infomation, out Aircraft vehicle)
    {
        vehicle = null;
        if (player != null)
            return null;

        player = Instantiate(playerPrefab).GetComponent<Player>();

        PlayerSpawnData reserved = GameMaster.Instance.PlayerSpawnData();
        if (reserved.selected == VehicleID.END)
            vehicle = null;
        else
        {
            var pool = prefabs[reserved.selected].pool;
            vehicle = pool.Get() as Aircraft;
            vehicle.release += CheckTGT;
            vehicle.release += pool.Release;
            vehicle.SystemIntegration();
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
            vehicle.release += CheckTGT;
            vehicle.release += pool.Release;
            vehicle.SystemIntegration();
            vehicle.SetSpecial(infomation.specialWeapon);
            vehicle.releaseEvent += pilot.Release;
            pilot.SetInfomation(infomation.pilot);
            pilot.Attach(vehicle);
        }

        if (infomation.pilot.isTGT)
            TGTCount++;

        if (leaderSystem != null)
        {
            leaderSystem.Add(pilot, vehicle);
            pilot.SetLeaderSystem(leaderSystem);
        }

        targets[infomation.pilot.team].Add(vehicle);

        return vehicle;
    }


    public Tank Spawn(Spawn_Ground infomation, out AIDriver pilot, GroundLeaderSystem leaderSystem = null)
    {
        Tank vehicle = null;

        pilot = groundAIPool.Get();

        if (vehicle == null)
        {
            var pool = prefabs[infomation.id].pool;
            vehicle = pool.Get() as Tank;
            vehicle.release += CheckTGT;
            vehicle.release += pool.Release;
            vehicle.releaseEvent += pilot.Release;
            pilot.SetInfomation(infomation.pilot);
            pilot.Attach(vehicle);
        }

        if (infomation.isTGT)
            TGTCount++;

        //if (leaderSystem != null)
        //{
        //    leaderSystem.Add(pilot, vehicle);
        //    pilot.SetLeaderSystem(leaderSystem);
        //}

        targets[infomation.pilot.team].Add(vehicle);

        vehicle.gameObject.transform.position = infomation.spawnPoint.transform.position;
        vehicle.gameObject.transform.rotation = infomation.spawnPoint.transform.rotation;

        return vehicle;
    }

    public void Spawn(SpawnPoint point, Spawn_Ground[] spawnTargetList)
    {
        Tank vehicle;
        //GroundLeaderSystem leaderSystem = null;

        //bool leaderExsist = leaderIndex >= 0 && leaderIndex < spawnTargetList.Length;
        //if (leaderExsist)
        //{
        //    leaderSystem = new GroundLeaderSystem();
        //    leaderVehicle = Spawn(spawnTargetList[leaderIndex], out AircraftPilot leader, leaderSystem);
        //
        //    float left = leaderIndex % 2 == 0 ? -1.0f : 1.0f;
        //    float back = (float)(leaderIndex / 2);
        //
        //    leaderVehicle.gameObject.transform.position = (right * 7.5f * back * left) + (forward * 7.5f * back) + point.gameObject.transform.position;
        //    leaderVehicle.gameObject.transform.rotation = point.gameObject.transform.rotation;
        //}

        for (int i = 0; i < spawnTargetList.Length; i++)
        {
            //if (leaderIndex == i) continue;

            vehicle = Spawn(spawnTargetList[i], out AIDriver pilot, null);
        }
    }

    public void Spawn(SpawnPoint point, Spawn_Air[] spawnTargetList)
    {
        Aircraft vehicle, leaderVehicle;
        AviationLeaderSystem leaderSystem = null;

        Vector3 forward = Vector3.Scale(point.gameObject.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 right = Vector3.Scale(point.gameObject.transform.right, new Vector3(1, 0, 1)).normalized;
        int leaderIndex = point.LeaderIndex;
        bool leaderExsist = leaderIndex >= 0 && leaderIndex < spawnTargetList.Length;
        if (leaderExsist)
        {
            leaderSystem = new AviationLeaderSystem();
            leaderVehicle = Spawn(spawnTargetList[leaderIndex], out AircraftPilot leader, leaderSystem);

            leaderVehicle.gameObject.transform.position = point.gameObject.transform.position;
            leaderVehicle.gameObject.transform.rotation = point.gameObject.transform.rotation;
        }

        for (int i = 0; i < spawnTargetList.Length; i++)
        {
            if (leaderIndex == i) continue;

            vehicle = Spawn(spawnTargetList[i], out AircraftPilot pilot, leaderSystem);

            float left = i % 2 == 0 ? -1.0f : 1.0f;
            float back = -(float)((i + 1) / 2);

            vehicle.gameObject.transform.position = (right * 7.5f * back * left) + (forward * 7.5f * back) + point.gameObject.transform.position;
            vehicle.gameObject.transform.rotation = point.gameObject.transform.rotation;
        }
    }

    public Vehicle Create(VehicleID vehicleID) { return prefabs[vehicleID].pool.Get(); }
    public Vehicle Create(Pilot pilot, VehicleID vehicleID)
    {
        Vehicle newInstnace = prefabs[vehicleID].pool.Get();

        pilot.Attach(newInstnace);

        return newInstnace;
    }

    public void ReleaseGroundAI(AIDriver ai)
    {
        ai.transform.parent = null;
        groundAIPool.Release(ai);
    }

    public void ReleaseAI(AIPilot ai)
    {
        ai.transform.parent = null;
        aiObjectPool.Release(ai);
    }

    public void Shoot(Vector3 position, Quaternion rotation, float velocity, Vehicle shooted)
    {
        Bullet newInstnace = bulletObjectPool.Get();
        newInstnace.InjectReleaseMethod(bulletObjectPool.Release);
        newInstnace.Shoot(position, rotation, velocity, shooted, bulletObjectPool);
    }
    public void ShootNonGravity(Vector3 position, Quaternion rotation, float velocity, Vehicle shooted)
    {
        Bullet newInstnace = bulletObjectPool.Get();
        newInstnace.InjectReleaseMethod(bulletObjectPool.Release);
        newInstnace.ShootNonGravity(position, rotation, velocity, shooted, bulletObjectPool);
    }
    
    public Flare ShootFlare(GameObject pod, float velocity = 1.5f) { return ShootFlare(pod.transform.position, pod.transform.rotation, velocity); }
    public Flare ShootFlare(Vector3 position, Quaternion rotation, float velocity = 1.5f)
    {
        Flare newInstnace = FlarePool.Get();
        Rigidbody rigidbody = newInstnace.GetComponent<Rigidbody>();
        newInstnace.InjectReleaseMethod(FlarePool.Release);
        newInstnace.Shoot(position, rotation, velocity);
        return newInstnace;
    }

    public void Explosion(Vector3 worldPos, float size = 5.0f) { explosion.Emit(worldPos, size); }
    public void Explosion(GameObject target)
    {
        if (target == null)
            return;

        Explosion newInstnace = explosionObjectPool.Get();
        newInstnace.Emit(target);
    }

    private void CheckTGT(Vehicle deathTarget)
    {
        if (deathTarget.IsTGT)
            TGTCount--;
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void ReservePlayerVehicle(PlayerSpawnData infomation) { reserved = infomation; }
    public bool IsTGTEmpty() { return TGTCount <= 0; }
    public ReadOnlyCollection<Vehicle> GetAll(int teamID) { return targets[teamID].AsReadOnly(); }

    private bool isInitialized = false;
    private const int length = 4;
    private int TGTCount = 0;

    private PlayerSpawnData reserved;
    private Player player = null;
    private Explosion explosion = null;

    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private GameObject aircraftAIPrefab = null;
    [SerializeField] private GameObject groundAIPrefab = null;
    [SerializeField] private GameObject explosionPrefab = null;
    [SerializeField] private GameObject bulletPrefab = null;
    
    [SerializeField] private GameObject FAAMPrefab;
    [SerializeField] private GameObject SMPrefab;
    [SerializeField] private GameObject SARMPrefab;
    [SerializeField] private GameObject FAGMPrefab;

    [SerializeField] private GameObject FlarePrefab;

    private List<Vehicle>[] targets = null;

    private ObjectPool<AIDriver> groundAIPool = null;
    private ObjectPool<AIPilot> aiObjectPool = null;
    private ObjectPool<Explosion> explosionObjectPool = null;
    private ObjectPool<Bullet> bulletObjectPool = null;

    private ObjectPool<SemiActiveRaderMissile> SARMPool = null;
    private ObjectPool<FourAirToAirMissile> FAAMPool = null;
    private ObjectPool<StandardMissile> SMPool = null;
    private ObjectPool<FourAirToGroundMissile> FAGMPool = null;

    private ObjectPool<Flare> FlarePool = null;

    private Dictionary<VehicleID, (GameObject prefab, ObjectPool<Vehicle> pool)> prefabs = null;



}
