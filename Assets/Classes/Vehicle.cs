using UnityEngine;
using static Pilot;

public abstract class Vehicle : MonoBehaviour
{
    public void Awake()
    {
        hp = maxHp;
    }
    public void SetVehicleInfo(ref PilotInfo infomation)
    {
        team = infomation.team;
        tgt = infomation.tgt;
        if (infomation.invincible)
            hp = int.MaxValue;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            gameObject.SetActive(false);
            GameMaster.GetInstance().GetFactory().Explosion(gameObject.transform.position, 10.0f);
            hp = int.MaxValue;
            Release();
        }
    }

    public void Release() { releaseEvent?.Invoke(); releaseEvent = null; release.Invoke(this); }
    public void SetRelease(ReleaseMethod listener) { release = listener; }

    public float HPPresentage() { return (float)hp / (float)maxHp; }
    public VehicleID ID { get { return id; } private set { id = value; } }
    public string VehicleName { get { return vehicleName; } private set { vehicleName = value; } }
    public int Team { get { return team; } private set { team = value; } }
    public bool IsTGT { get { return tgt; } private set { tgt = value; } }
    public bool IsLand { get { return isLand; } private set { isLand = value; } }


    public delegate void ReleaseMethod(Vehicle vehicle);
    private event ReleaseMethod release; // 나중에 고침
    public delegate void ReleaseEventMethod();
    public event ReleaseEventMethod releaseEvent; // 나중에 고침

    [SerializeField] private VehicleID id = VehicleID.END;
    [SerializeField] private string vehicleName = "NULL";
    [SerializeField] protected int hp = 0;
    [SerializeField] protected int maxHp = 0;
    [SerializeField] protected int team = 0;
    [SerializeField] public bool tgt;
    [SerializeField] public bool isLand;
}
