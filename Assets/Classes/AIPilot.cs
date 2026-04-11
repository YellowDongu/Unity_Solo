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
        END
    }

    //===========================================
    // Initializer/Destructor
    //===========================================
    public void Start()
    {
        currentState = null;
        currentStatus = Status.Attack;
    }

    //===========================================
    // FrameCycle Methods
    //===========================================

    void FixedUpdate()
    {
        if (als != null)
            als.Update();

        if (currentState == null)
        {
            if (queue.Count > 0)
            {
                (currentState, currentStatus) = queue.Dequeue();
            }
            else if (isLeader)
            {
                currentState = leveling;
                currentStatus = Status.Chase;
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
                    }
                    break;
                case Status.Override:
                    if(currentState.Update())
                    {
                        currentState = null;

                    }

                    return;
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
            queue.Enqueue((currentState, currentStatus));
            currentState = altutudeMatcher;
            altutudeMatcher.SetAltitude(500.0f);
            currentStatus = Status.Override;
        }
        else if (gameObject.transform.position.y > 4000.0f)
        {
            queue.Enqueue((currentState, currentStatus));
            currentState = altutudeMatcher;
            altutudeMatcher.SetAltitude(3500.0f);
            currentStatus = Status.Override;
        }
        else if (als != null && !isLeader)
        {
            if ((als.GetLeader().gameObject.transform.position - gameObject.transform.position).sqrMagnitude >= 1000.0f * 1000.0f)
            {
                currentState = followLeader;
                currentStatus = Status.Follow;
                state = 0;
            }
        }

    }

    public override void Attach(Vehicle target)
    {
        base.Attach(target);

        leveling = new LevelingState(gameObject.transform, control);
        horizontalTurn = new HorizontalTurnState(gameObject.transform, control);
        followLeader = new HorizontalTurnState(gameObject.transform, control);
        altutudeMatcher = new AltitudeState(gameObject.transform, control);
        horizontalTurn.SetTurnValue(movement, TurnType.Deep);
        targets = fcs.Targets;
        lockState = fcs.LockStatus;
        gunAngleValue = Mathf.Cos(5.0f * Mathf.Deg2Rad);
    }

    //public void ChangeTarget() // 전역 검색
    //{
    //    int index = infomation.team == 2 ? 1 : 2;
    //    var list = GameMaster.GetInstance().GetFactory().GetAll(index);
    //    float distance = float.MaxValue;
    //    int selected = -1;
    //
    //    for (int i = 0; i < list.Count; i++)
    //    {
    //        if (!list[i].gameObject.activeInHierarchy)
    //            continue;
    //
    //        float current = (list[i].transform.position - gameObject.transform.position).sqrMagnitude;
    //        if(distance > current)
    //        {
    //            distance = current;
    //            selected = i;
    //        }
    //    }
    //}

    public void ChangeLeader(Aircraft newLeader)
    {
        isLeader = newLeader == this;
        if (!isLeader)
            followLeader.Initialize(als.GetLeader().gameObject, als.GetOffset(this), movement, TurnType.Normal);
    }

    public override void Release()
    {
        aircraft = null;
        control = null;
        movement = null;
        animator = null;
        rader = null;
        fcs = null;

        GameMaster.GetInstance().GetFactory().ReleaseAI(this);
    }
    public override void SetLeaderSystem(LeaderSystem system)
    {
        als = system as AviationLeaderSystem;
        isLeader = als.IsLeader(this);
        followLeader.Initialize(als.GetLeader().gameObject, als.GetOffset(this), movement, TurnType.Normal);
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
                        chasing = targets[0].gameObject;
                        state = 0;
                    }
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
                }
                else
                {
                    fcs.ChangeTarget(true);

                    if (targets.Count != 0)
                    {
                        currentStatus = Status.Attack;
                        currentState = horizontalTurn;
                        horizontalTurn.Initialize(targets[0].gameObject);
                        chasing = targets[0].gameObject;
                        state = -10;
                    }
                }
                break;
            case 1:
                if (!chasing.activeInHierarchy)
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
                        chasing = targets[0].gameObject;
                        state = -10;
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
        int index = team == 2 ? team - 1 : team + 1;
        var list = GameMaster.GetInstance().GetFactory().GetAll(index);
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
            chasing = list[selected].gameObject;
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
                if (isLeader)
                {
                    currentStatus = Status.Chase;
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
                chasing = targets[0].gameObject;
                horizontalTurn.Initialize(targets[0].gameObject);
            }

            return targets.Count != 0;
        }

        if(chasing != targets[0])
        {
            chasing = targets[0].gameObject;
            horizontalTurn.Initialize(targets[0].gameObject);
        }

        if (Vector3.Dot(fcs.gameObject.transform.forward, Vector3.Normalize(targets[0].gameObject.transform.position - fcs.gameObject.transform.position)) >= gunAngleValue)
            fcs.Gun(aircraft);

        if (lockState[0] <= 0.0f)
            fcs.Missile();

        return false;
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    private Status currentStatus = Status.Chase;
    private int state = 0;
    private int team = 0;
    private float gunAngleValue = 0.0f;
    private GameObject chasing = null;

    private ReadOnlyCollection<float> lockState;
    private ReadOnlyCollection<Vehicle> targets;


    private FlightState currentState;

    private LevelingState leveling;
    private HorizontalTurnState horizontalTurn;
    private HorizontalTurnState followLeader;
    private AltitudeState altutudeMatcher;

    private Queue<(FlightState, Status)> queue = new Queue<(FlightState, Status)>();

    private bool isLeader = true;
    private AviationLeaderSystem als = null;

}
