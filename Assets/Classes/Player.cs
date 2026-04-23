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

    public override void Attach(Vehicle target)
    {
        CameraController camera = gameObject.GetComponent<CameraController>();
        camera.Attach(target, true);

        target.SetVehicleInfo(ref infomation);

        aircraft = target as Aircraft;
        sfxChannel = aircraft.LinkSFXChannel;
        control = aircraft.Control;
        movement = aircraft.Movement;
        animator = aircraft.Animator;
        rader = aircraft.Rader;
        fcs = aircraft.FCS;
        fcs.SetTeam(infomation.team);
        rader.SetTeam(infomation.team);
        rader.SetFlareCoolTime(2);
        fcs.TargetChanged += TargetChanged;
        fcs.BeforeTargetChanged += BeforeTargetChanged;

        GameMaster.Instance.LinkBaseCanvas(this);
    }

    public void AttachHUD(HUDController baseHUD)
    {
        baseHUD.BoundPlayer(aircraft);
        baseHUD.GaugeController.BoundPlayer(aircraft);

        iffHud = baseHUD.IFFController;
        iffHud.SetMaxDistance(rader.RaderDistance);
        IFFAttach = iffHud.AttachIFF;
        SelectIFF = iffHud.Select;
        rader.enterEvent += AttachIFF;

        LockHUD lockHUD = baseHUD.LockUI;
        lockHUD.BoundPlayer(fcs);
        fcs.TargetChanged += lockHUD.TargetChanged;
        fcs.BeforeTargetChanged += lockHUD.BeforeTargetChanged;

        RaderUI raderUI = baseHUD.Rader;
        raderUI.BoundPlayer(aircraft);
        RaderChange = raderUI.ChangeState;
    }

    public override void Release()
    {
        aircraft = null;
        control = null;
        movement = null;
        animator = null;
        rader = null;
        fcs = null;

        gameObject.transform.SetParent(null);
        gameObject.GetComponent<CameraController>().DetachCamera();
        GameMaster.Instance.EndMission(false);
        gameObject.SetActive(false);
        iffHud.gameObject.SetActive(false);
    }

    public override void SetLeaderSystem(LeaderSystem system)
    {
        //als = system as AviationLeaderSystem;
        //isLeader = als.IsLeader(this);
        //followLeader.Initialize(als.GetLeader().gameObject, als.GetOffset(this), movement, TurnType.Normal);
    }


    //===========================================
    // FrameCycle Methods
    //===========================================
    private void Update()
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
            GameMaster.Instance.Sound.Play(loopSFXChannel, "Valcan20mm", true, false);
        }
        else
            loopSFXChannel.Stop();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            fcs.ChangeMissile();
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if(fcs.Missile())
                GameMaster.Instance.Sound.PlayOnce(sfxChannel, fcs.SelectState ? "MissileFired2" : "MissileFired");
        }
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            rader.DeployFlare(sfxChannel, GameMaster.Instance.Sound.GetSound("Flare_Temp"));
        }
        if (Keyboard.current.gKey.wasPressedThisFrame)
            control.isGearDown = !control.isGearDown;
        if (Keyboard.current.rKey.wasPressedThisFrame)
            RaderChange();
    }


    //===========================================
    // Methods
    //===========================================

    public void AttachIFF(Vehicle target) { IFFAttach(target, this, aircraft); }

    public void ChangeTarget()
    {
        fcs.ChangeTarget();
    }

    public void BeforeTargetChanged()
    {
        foreach (var target in fcs.Targets)
        {
            SelectIFF(target, false);
        }

    }

    public void TargetChanged()
    {
        var targets = fcs.Targets;
        if (targets.Count == 0)
            return;

        if (fcs.SelectState)
        {
            foreach (var target in targets)
            {
                SelectIFF(target, true);
            }
        }
        else
        {
            SelectIFF(targets[0], true);
        }
    }

    public void SwitchWeapon()
    {
        fcs.ChangeMissile();

        var targets = fcs.Targets;
        if (targets.Count == 0)
            return;

        if (fcs.SelectState)
        {
            foreach (var target in targets)
            {
                SelectIFF(target, true);
            }
        }
        else
        {
            foreach (var target in targets)
            {
                SelectIFF(target, false);
            }

            SelectIFF(targets[0], true);
        }
    }


    //===========================================
    // Variable & GetSet Methods
    //===========================================

    private IFFUIController iffHud;

    private Func<Vehicle, Player, Aircraft, IFFHud> IFFAttach;
    private Action<Vehicle, bool> SelectIFF;
    private Action RaderChange;

    private AudioSource sfxChannel;
    [SerializeField] private AudioSource loopSFXChannel;
}
