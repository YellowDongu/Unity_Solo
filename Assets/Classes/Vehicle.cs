using UnityEngine;
using static Pilot;

public abstract class Vehicle : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================

    public void Release() { releaseEvent?.Invoke(); releaseEvent = null; release?.Invoke(this); release = null; }

    //===========================================
    // Methods
    //===========================================
    public void SetVehicleInfo(ref PilotInfo infomation)
    {
        team = infomation.team;
        tgt = infomation.isTGT;
        if (infomation.invincible)
            hp = int.MaxValue;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        SetIntValue?.Invoke(hp);
        if (hp <= 0)
        {
            gameObject.SetActive(false);
            GameMaster.GetInstance().GetFactory().Explosion(gameObject.transform.position, 10.0f);
            hp = int.MaxValue;
            Release();
        }
    }


    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void SetRelease(ReleaseMethod listener) { release = listener; }
    public float HPPresentage() { return 1.0f - (float)hp / (float)maxHp; }
    public GameObject FirstView() { return firstView; }
    public GameObject ThirdView() { return thirdView; }
    public VehicleID ID { get { return id; } private set { id = value; } }
    public string VehicleName { get { return vehicleName; } private set { vehicleName = value; } }
    public int VehicleLayer { get { return vehicleLayer; } private set { vehicleLayer = value; } }
    public int Team { get { return team; } private set { team = value; } }
    public bool IsTGT { get { return tgt; } private set { tgt = value; } }
    public bool IsLand { get { return isLand; } private set { isLand = value; } }


    public delegate void ReleaseMethod(Vehicle vehicle);
    public event ReleaseMethod release;
    public delegate void ReleaseEventMethod();
    public event ReleaseEventMethod releaseEvent;
    public delegate void SetIntValueMethod(int value);
    public event SetIntValueMethod SetIntValue;

    [SerializeField] private VehicleID id = VehicleID.END;
    [SerializeField] private string vehicleName = "NULL";
    [SerializeField] private int vehicleLayer = 0;
    [SerializeField] protected int hp = 0;
    [SerializeField] protected int maxHp = 0;
    [SerializeField] protected int team = 0;
    [SerializeField] public bool tgt;
    [SerializeField] public bool isLand;

    [SerializeField] protected GameObject firstView;
    [SerializeField] protected GameObject thirdView;
}
