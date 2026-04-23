using System.Collections;
using UnityEngine;

public class GroundLeaderSystem : LeaderSystem
{
}

public class AIDriver : Pilot
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        gunAngle = Mathf.Cos(5.0f * Mathf.Deg2Rad);
    }

    public override void Release()
    {
        vehicle = null;
        control = null;
        movement = null;
        animator = null;
        rader = null;
        fcs = null;

        GameMaster.Instance.Factory.ReleaseGroundAI(this);
    }

    //===========================================
    // FrameCycle Methods
    //===========================================

    void Update()
    {
        if (target != null)
            Attack();
        else
        {
            var targets = rader.InRangeTarget;
            if (targets.Count != 0)
                fcs.ChangeTarget(true);
        }
    }

    //===========================================
    // Methods
    //===========================================
    public override void Attach(Vehicle target)
    {
        gameObject.transform.SetParent(target.FirstView.transform); // default attach
        target.SetVehicleInfo(ref infomation);

        vehicle = target as Tank;
        control = vehicle.Control;
        movement = vehicle.Movement;
        animator = vehicle.Animator;
        rader = vehicle.Rader;
        fcs = vehicle.FCS;
        fcs.TargetChanged += TargetChanged;
        fcs.SetTeam(infomation.team);
        rader.SetTeam(infomation.team);
    }

    public override void SetLeaderSystem(LeaderSystem leaderSystem)
    {

    }

    public void TargetChanged()
    {
        var targets = fcs.Targets;
        if (targets.Count == 0 || target == targets[0])
            return;
        target = targets[0];
        animator.ChangeTarget(target);
    }

    private void Attack()
    {
        if(fcs.Targets.Count == 0)
        {
            target = null;
            return;
        }

        if(!missileDelay)
        {
            if (fcs.Missile())
            {
                StartCoroutine(MissileCoolTime());
            }
        }

        if (fcs.TargetAngle <= gunAngle)
            fcs.Gun(vehicle);
    }

    private IEnumerator MissileCoolTime()
    {
        float time = missileDelayTime;
        missileDelay = true;
        yield return null;
        while (time >= 0.0f)
        {
            time -= Time.deltaTime;
            yield return null;
        }
        missileDelay = false;
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    bool missileDelay = false;
    float gunAngle;
    float missileDelayTime = 1.5f;


    protected Vehicle target = null;
    protected Tank vehicle = null;
    protected Rader rader = null;
    protected GroundControl control = null;
    protected GroundMovement movement = null;
    protected TankAnimator animator = null;
    protected TurretFireControlSystem fcs = null;
}
