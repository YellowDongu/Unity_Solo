using System;
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
        else
            control.throttle = 0.2f;

        if (Keyboard.current.sKey.isPressed)
        {
            control.isAirBrakeOn = true;
            control.throttle = 0.0f;
            if (Keyboard.current.wKey.isPressed)
            {
                control.HighGTurn = Mathf.Clamp(control.HighGTurn + 4.0f * Time.deltaTime, 1.0f, 2.0f);
                control.throttle = 0.2f;
            }
        }
        else
        {
            control.isAirBrakeOn = false;
            control.HighGTurn = Mathf.Clamp(control.HighGTurn - 4.0f * Time.deltaTime, 1.0f, 2.0f);
        }

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            ChangeTarget();
        }
        if (Keyboard.current.leftCtrlKey.isPressed)
        {
            fcs.Gun(aircraft);
            GameMaster.GetInstance().Sound().PlayOnce(sfxChannel, "Valcan20mm");
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            fcs.ChangeMissile();
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if(fcs.Missile())
                GameMaster.GetInstance().Sound().PlayOnce(sfxChannel, fcs.GetSelectState() ? "MissileFired2" : "MissileFired");
        }
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            rader.DeployFlare(sfxChannel, GameMaster.GetInstance().Sound().GetSound("Flare_Temp"));
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
        camera.Attach(target, true);

        target.SetVehicleInfo(ref infomation);

        aircraft = target as Aircraft;
        sfxChannel = aircraft.LinkSFXChannel();
        control = aircraft.Control();
        movement = aircraft.Movement();
        animator = aircraft.Animator();
        rader = aircraft.Rader();
        fcs = aircraft.FCS();
        fcs.SetTeam(infomation.team);
        rader.SetTeam(infomation.team);
        rader.SetFlareCoolTime(2);
        GameMaster.GetInstance().LinkBaseCanvas(this);
    }

    public void AttachHUD(HUDController baseHUD) // 굳이 여기서 할 필요가 있을까
    {
        baseHUD.BoundPlayer(aircraft);
        baseHUD.LinkLockHUD().BoundPlayer(fcs);
        baseHUD.LinkRaderUI().BoundPlayer(aircraft);
        baseHUD.LinkGaugeUIController().BoundPlayer(aircraft);
        iffHud = baseHUD.LinkIFFUIController();
        iffHud.SetMaxDistance(rader.RaderDistance());
        IFFAttach = iffHud.AttachIFF;
        SelectIFF = iffHud.Select;
        rader.enterEvent += AttachIFF;
    }

    public void AttachIFF(Vehicle target) { IFFAttach(target, this, aircraft); }

    public void ChangeTarget()
    {
        foreach (var target in fcs.Targets)
        {
            SelectIFF(target, false);
        }

        fcs.ChangeTarget();

        if (fcs.Targets.Count == 0)
            return;

        if(fcs.GetSelectState())
        {
            foreach (var target in fcs.Targets)
            {
                SelectIFF(target, true);
            }
        }
        else
        {
            SelectIFF(fcs.Targets[0], true);
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
                SelectIFF(target, true);
            }
        }
        else
        {
            foreach (var target in fcs.Targets)
            {
                SelectIFF(target, false);
            }

            SelectIFF(fcs.Targets[0], true);
        }
    }


    //===========================================
    // Variable & GetSet Methods
    //===========================================

    private IFFUIController iffHud;
    private Func<Vehicle, Player, Aircraft, IFFHud> IFFAttach;
    private Action<Vehicle, bool> SelectIFF;
    private AudioSource sfxChannel;
    private AudioSource engineChannel;
}
