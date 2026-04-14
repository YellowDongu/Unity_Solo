using UnityEngine;

public class GroundLeaderSystem : LeaderSystem
{
}

public class AIDriver : Pilot
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Attach(Vehicle target)
    {
        gameObject.transform.SetParent(target.FirstView().transform); // default attach
        target.SetVehicleInfo(ref infomation);

        vehicle = target as Tank;
        control = vehicle.Control();
        movement = vehicle.Movement();
        animator = vehicle.Animator();
        rader = vehicle.Rader();
        fcs = vehicle.FCS();
        fcs.SetTeam(infomation.team);
        rader.SetTeam(infomation.team);
    }

    public override void SetLeaderSystem(LeaderSystem leaderSystem)
    {

    }

    public override void Release()
    {
        vehicle = null;
        control = null;
        movement = null;
        animator = null;
        rader = null;
        fcs = null;

        GameMaster.GetInstance().GetFactory().ReleaseGroundAI(this);
    }

    protected Tank vehicle = null;
    protected GroundControl control = null;
    protected GroundMovement movement = null;
    protected TankAnimator animator = null;
    protected Rader rader = null;
    protected FireControlSystem fcs = null;
}
