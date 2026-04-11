using UnityEngine;
using UnityEngine.InputSystem;

public class F16CAnimator : AircraftAnimator
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        elevator = AddAnimationData("elevator", "Take 001", "Elevator");
        aileronL = AddAnimationData("aileronL", "Take 001", "AileronL");
        aileronR = AddAnimationData("aileronR", "Take 001", "AileronR");
        rudder = AddAnimationData("rudder", "Take 001", "Rudder");
        gear = AddAnimationData("gear", "Take 001", "Base Layer", false);
        gear.SetSpeed(0.1f);
        gear.SetMiddleTime(0.8f);
        gear.SetMotionTime(0.0f);
        airBreak = AddAnimationData("airBreak", "Take 001", "AirBreak", -1);
        airBreak.SetSpeed(0.5f);
        airBreak.SetMotionTime(0.0f);
        baseAnimation = AddAnimationData("speedFactor", "Take 001", "Base Layer", -1);
        baseAnimation.SetSpeed(0.5f);
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    private void Update()
    {
        PrimarySurfaceControl();
        GearControl();
        SpeedBreakControl();
        BaseAnimationControl();
    }

    //===========================================
    // Methods
    //===========================================
    public void PrimarySurfaceControl()
    {
        aileronL.Update((int)control.yoke.x);
        aileronR.Update((int)-control.yoke.x);
        rudder.Update((int)-control.yoke.y);
        elevator.Update((int)-control.yoke.z);
    }

    public void GearControl()
    {
        gear.Update(control.isGearDown ? 0 : -1);
    }

    public void SpeedBreakControl()
    {
        airBreak.Update(control.isAirBreakOn ? 0 : -1);
    }

    public void BaseAnimationControl()
    {
        baseAnimation.Update(control.velocity > 300.0f ? 1 : 0);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    private RotationAnimationData elevator;
    private RotationAnimationData aileronL;
    private RotationAnimationData aileronR;
    private RotationAnimationData rudder;
    private RotationAnimationData gear;
    private PartsAnimationData airBreak;
    private PartsAnimationData baseAnimation;
}