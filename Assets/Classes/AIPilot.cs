using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using static HorizontalTurnState;


public class AIPilot : AircraftPilot
{
    //===========================================
    // struct/enum
    //===========================================
    public enum Status
    {
        None,
        Attack,
        Chase,
        Follow,
        Override,
        Search,
        Timed,
        END
    }

    //===========================================
    // Initializer/Destructor
    //===========================================
    public void Start()
    {
        currentState = null;
        currentStatus = Status.Attack;
        gunDistance = GameMaster.ConvertWorldScale(gunDistance);
        gunDistance *= gunDistance;
        gunAngleValue = Mathf.Cos(5.0f * Mathf.Deg2Rad);
        if (als != null)
            followLeader.SetOffset(als.GetOffset(this));
    }

    public override void Attach(Vehicle target)
    {
        base.Attach(target);
        rader.SetAutoFlare();
        fcs.TargetChanged += TargetChanged;

        leveling = new LevelingState(gameObject.transform, control);
        horizontalTurn = new FollowState(gameObject.transform, control);
        followLeader = new FollowState(gameObject.transform, control);
        altutudeMatcher = new AltitudeState(gameObject.transform, control);
        evade = new HorizontalTurnState(gameObject.transform, control);

        horizontalTurn.SetTurnValue(movement, TurnType.Deep);
        followLeader.SetTurnValue(movement, TurnType.Deep);
        evade.SetTurnValue(movement, TurnType.Deep);

        targets = fcs.Targets;
        lockState = fcs.LockStatus;
    }

    public override void Release()
    {
        aircraft = null;
        control = null;
        movement = null;
        animator = null;
        rader = null;
        fcs = null;

        GameMaster.Instance.Factory.ReleaseAI(this);
    }

    public override void SetLeaderSystem(LeaderSystem system)
    {
        als = system as AviationLeaderSystem;
        isLeader = als.IsLeader(this);
        followLeader.Initialize(als.Leader, als.GetOffset(this), movement, TurnType.Normal);
    }
    //===========================================
    // FrameCycle Methods
    //===========================================

    void FixedUpdate()
    {
        if(rader.MissileAlart)
            ActiveEvade();

        if (als != null)
            als.Update();

        if (currentState == null)
        {
            if (queue.Count > 0)
            {
                (currentState, currentStatus) = queue.Dequeue();
                if (currentStatus == Status.Timed)
                    timer = currentState.tempStoringTime;
            }
            else if (isLeader)
            {
                currentState = leveling;
                currentStatus = Status.Search;
                state = 0;
            }
            else
            {
                currentState = followLeader;
                currentStatus = Status.Follow;
                state = 0;
            }
        }
        else
        {
            switch (currentStatus)
            {
                case Status.Attack:
                    currentState.Update();
                    Strike();
                    Check();
                    break;
                case Status.Chase:
                    currentState.Update();
                    FindTarget();
                    Check();
                    break;
                case Status.Follow:
                    currentState.Update();
                    if (rader.InRangeTarget.Count != 0)
                    {
                        fcs.ChangeTarget(true);

                        if (targets.Count != 0)
                        {
                            currentStatus = Status.Attack;
                            currentState = horizontalTurn;
                            horizontalTurn.Initialize(targets[0].gameObject);
                            state = -10;
                        }
                        else
                            currentState = null;
                    }
                    break;
                case Status.Override:
                    if (currentState.Update())
                        currentState = null;
                    return;
                case Status.Search:
                    currentState.Update();
                    timer -= Time.deltaTime;
                    if (timer <= 0.0f)
                    {
                        timer = 5.0f;
                        if (rader.InRangeTarget.Count == 0)
                        {
                            chasing = null;
                            ChangeTarget();
                            if (chasing != null)
                                currentStatus = Status.Chase;
                        }
                        else
                        {
                            fcs.ChangeTarget(true);

                            if (targets.Count != 0)
                            {
                                currentStatus = Status.Attack;
                                currentState = horizontalTurn;
                                horizontalTurn.Initialize(targets[0].gameObject);
                                chasing = targets[0];
                                state = -10;
                            }
                            else
                            {
                                chasing = null;
                                ChangeTarget();
                                if (chasing == null)
                                    currentStatus = Status.Chase;
                            }
                        }
                    }
                    break;
                case Status.Timed:
                    currentState.Update();
                    timer -= Time.deltaTime;
                    if (timer <= 0.0f)
                    {
                        timer = 0.0f;
                        currentState = null;
                    }
                    Check();
                    break;
                default:
                    break;
            }
        }
    }

    //===========================================
    // Methods
    //===========================================
    public void Check()
    {
        if (gameObject.transform.position.y < 200.0f)
        {
            altutudeMatcher.SetAltitude(500.0f);
            OverrideOrder(altutudeMatcher);
        }
        else if (gameObject.transform.position.y > 4000.0f)
        {
            altutudeMatcher.SetAltitude(3500.0f);
            OverrideOrder(altutudeMatcher);
        }
        else if (als != null && !isLeader)
        {
            if ((als.Leader.gameObject.transform.position - gameObject.transform.position).sqrMagnitude >= 1000000.0f)
            {
                currentState = followLeader;
                currentStatus = Status.Follow;
                state = 0;
            }
        }

    }

    public void OverrideOrder(FlightState overrideState)
    {
        queue.Enqueue((currentState, currentStatus));
        
        currentState = overrideState;
        currentStatus = Status.Override;
    }

    public void EnqueueTimedOrder(FlightState overrideState, float time)
    {
        overrideState.tempStoringTime = time;
        queue.Enqueue((overrideState, Status.Timed));
    }

    public void TargetChanged()
    {
        if (currentStatus != Status.Attack)
            return;

        if (targets.Count == 0)
        {
            if (isLeader)
            {
                currentStatus = Status.Search;
                currentState = leveling;
            }
            else
            {
                currentStatus = Status.Follow;
                currentState = followLeader;
            }

            state = 0;
        }
        else
        {
            chasing = targets[0];
            horizontalTurn.Initialize(targets[0].gameObject);
        }
    }

    public void ChangeLeader(Aircraft newLeader)
    {
        isLeader = newLeader == this;
        if (!isLeader)
            followLeader.Initialize(als.Leader, als.GetOffset(this), movement, TurnType.Normal);
    }

    public bool BattleStatusUpdate()
    {
        switch (currentStatus)
        {
            case Status.Attack:
                Strike();
                break;
            case Status.Chase:
                FindTarget();
                break;
            case Status.Follow:
                if (rader.InRangeTarget.Count != 0)
                {
                    fcs.ChangeTarget(true);

                    if (targets.Count != 0)
                    {
                        currentStatus = Status.Attack;
                        currentState = horizontalTurn;
                        horizontalTurn.Initialize(targets[0].gameObject);
                        chasing = targets[0];
                        state = 0;
                    }
                    else
                        currentState = null;
                }
                break;
            default:
                break;
        }
        return false;
    }

    public bool FindTarget() // 근접 검색
    {
        switch (state)
        {
            case 0:
                if (rader.InRangeTarget.Count == 0)
                {
                    ChangeTarget();
                    if (chasing != null)
                        currentStatus = Status.Chase;
                    else
                        currentState = null;
                }
                else
                {
                    fcs.ChangeTarget(true);

                    if (targets.Count != 0)
                    {
                        currentStatus = Status.Attack;
                        currentState = horizontalTurn;
                        horizontalTurn.Initialize(targets[0].gameObject);
                        chasing = targets[0];
                        state = -10;
                    }
                }
                break;
            case 1:
                if (!chasing.gameObject.activeInHierarchy)
                {
                    state = 0;
                    break;
                }

                if (rader.InRangeTarget.Count != 0)
                {
                    fcs.ChangeTarget(true);

                    if (targets.Count != 0)
                    {
                        currentStatus = Status.Attack;
                        currentState = horizontalTurn;
                        horizontalTurn.Initialize(targets[0].gameObject);
                        chasing = targets[0];
                        state = -10;
                    }
                    else
                    {
                        currentState = null;
                        state = 0;
                    }
                }
                break;
            default:
                return true;
        }


        return false;
    }

    public void ChangeTarget() // 전역 검색
    {
        if (infomation.team == 0)
            return;

        int index = infomation.team == 2 ? infomation.team - 1 : infomation.team + 1;
        var list = GameMaster.Instance.Factory.GetAll(index);
        float distance = float.MaxValue;
        int selected = -1;

        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].gameObject.activeInHierarchy)
                continue;

            float current = (list[i].transform.position - gameObject.transform.position).sqrMagnitude;
            if (distance > current)
            {
                distance = current;
                selected = i;
            }
        }

        if(selected != -1)
        {
            chasing = list[selected];
            currentState = horizontalTurn;
            horizontalTurn.Initialize(list[selected].gameObject);
            state = 1;
        }
    }

    public bool Strike()
    {
        if (targets.Count == 0)
        {
            fcs.ChangeTarget(true);
            if (targets.Count == 0)
            {
                currentState = null;
                state = 0;
            }
            else
            {
                chasing = targets[0];
                horizontalTurn.Initialize(targets[0].gameObject);
            }

            return targets.Count != 0;
        }

        if(chasing != targets[0])
        {
            chasing = targets[0];
            horizontalTurn.Initialize(targets[0].gameObject);
        }

        Vector3 vector = targets[0].gameObject.transform.position - fcs.gameObject.transform.position;
        if (vector.sqrMagnitude <= gunDistance && Vector3.Dot(fcs.gameObject.transform.forward, Vector3.Normalize(vector)) >= gunAngleValue)
            fcs.Gun(aircraft);

        if (lockState[0] <= 0.0f)
            fcs.Missile();

        return false;
    }

    public void ActiveEvade()
    {
        if (currentStatus == Status.Override)
            return;
        if (currentStatus == Status.Timed && (currentState == evade || currentState == leveling))
            return;

        queue.Clear();
        evade.Initialize(gameObject.transform.position + gameObject.transform.forward * -30.0f);
        EnqueueTimedOrder(evade, 10.0f);
        EnqueueTimedOrder(leveling, 5.0f);
        currentState = null;
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    private Status currentStatus = Status.Chase;
    private bool isLeader = true;
    private int state = 0;
    private float gunAngleValue = 0.0f, gunDistance = 1000.0f;
    private float timer = 0.0f;


    private Vehicle chasing = null;
    private AviationLeaderSystem als = null;

    private FlightState currentState;
    private LevelingState leveling;
    private FollowState horizontalTurn;
    private FollowState followLeader;
    private HorizontalTurnState evade;
    private AltitudeState altutudeMatcher;


    private ReadOnlyCollection<float> lockState;
    private ReadOnlyCollection<Vehicle> targets;
    private Queue<(FlightState, Status)> queue = new Queue<(FlightState, Status)>();

}
