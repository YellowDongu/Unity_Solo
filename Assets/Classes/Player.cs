using UnityEngine;
using UnityEngine.InputSystem;


public class Player : AircraftPilot
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        infomation.team = 1;
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    void Update()
    {
        control.yoke.x = (Keyboard.current.leftArrowKey.isPressed ? 1.0f : 0.0f) - (Keyboard.current.rightArrowKey.isPressed ? 1.0f : 0.0f);
        control.yoke.y = (Keyboard.current.dKey.isPressed ? 1.0f : 0.0f) - (Keyboard.current.aKey.isPressed ? 1.0f : 0.0f);
        control.yoke.z = (Keyboard.current.upArrowKey.isPressed ? 1.0f : 0.0f) - (Keyboard.current.downArrowKey.isPressed ? 1.0f : 0.0f);


        if (Keyboard.current.wKey.isPressed)
            control.throttle = 1.0f;
        if (Keyboard.current.sKey.isPressed)
        {
            control.throttle = 0.2f;
            control.isAirBreakOn = true;
            if (Keyboard.current.wKey.isPressed)
            {
                control.HighGTurn = Mathf.Clamp(control.HighGTurn + 6.0f * Time.deltaTime, 1.0f, 3.0f);
                control.throttle = 0.2f;
            }
        }
        else
        {
            control.isAirBreakOn = false;
            control.HighGTurn = Mathf.Clamp(control.HighGTurn - 6.0f * Time.deltaTime, 1.0f, 3.0f);
        }

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            ChangeTarget();
        }
        if (Keyboard.current.leftCtrlKey.isPressed)
        {
            fcs.Gun(aircraft);
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            fcs.ChangeMissile();
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            fcs.Missile();
        }
        if (Keyboard.current.gKey.wasPressedThisFrame)
            control.isGearDown = !control.isGearDown;

    }


    //===========================================
    // Methods
    //===========================================
    public override void Release()
    {
        aircraft = null;
        control = null;
        movement = null;
        animator = null;
        rader = null;
        fcs = null;

        gameObject.transform.SetParent(null);

    }
    public override void SetLeaderSystem(LeaderSystem system)
    {
        //als = system as AviationLeaderSystem;
        //isLeader = als.IsLeader(this);
        //followLeader.Initialize(als.GetLeader().gameObject, als.GetOffset(this), movement, TurnType.Normal);
    }

    public override void Attach(Vehicle target)
    {
        CameraController camera = gameObject.GetComponent<CameraController>();
        camera.Attach(target.gameObject, true);

        target.SetVehicleInfo(ref infomation);

        aircraft = target as Aircraft;

        control = aircraft.Control();
        movement = aircraft.Movement();
        animator = aircraft.Animator();
        rader = aircraft.Rader();
        fcs = aircraft.FCS();
        fcs.SetTeam(infomation.team);
        rader.SetTeam(infomation.team);

        GameMaster.GetInstance().LinkBaseCanvas(this);
    }

    public void AttachHUD(HUDController baseHUD) // 굳이 여기서 할 필요가 있을까
    {
        baseHUD.BoundPlayer(movement);
        baseHUD.LinkLockHUD().BoundPlayer(fcs);
        baseHUD.LinkRaderUI().BoundPlayer(aircraft);
        baseHUD.LinkGaugeUIController().BoundPlayer(aircraft);
        iffHud = baseHUD.LinkIFFUIController();
        iffHud.SetMaxDistance(rader.RaderDistance());
        rader.enterEvent += AttachIFF;
    }

    public void AttachIFF(Vehicle target) { iffHud.AttachIFF(target, this); }

    public void ChangeTarget()
    {
        foreach (var target in fcs.Targets)
        {
            iffHud.Select(target, false);
        }

        fcs.ChangeTarget();

        if (fcs.Targets.Count == 0)
            return;

        if(fcs.GetSelectState())
        {
            foreach (var target in fcs.Targets)
            {
                iffHud.Select(target, true);
            }
        }
        else
        {
            iffHud.Select(fcs.Targets[0], true);
        }
    }
    public void SwitchWeapon()
    {
        fcs.ChangeMissile();

        if (fcs.Targets.Count == 0)
            return;

        if (fcs.GetSelectState())
        {
            foreach (var target in fcs.Targets)
            {
                iffHud.Select(target, true);
            }
        }
        else
        {
            foreach (var target in fcs.Targets)
            {
                iffHud.Select(target, false);
            }

            iffHud.Select(fcs.Targets[0], true);
        }
    }


    //===========================================
    // Variable & GetSet Methods
    //===========================================

    private IFFUIController iffHud;

}
