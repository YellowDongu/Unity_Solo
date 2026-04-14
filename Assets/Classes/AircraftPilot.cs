using UnityEngine;

public abstract class AircraftPilot : Pilot
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    public void LinkControl(Control aircraftControl) { control = aircraftControl; }

    public override void Attach(Vehicle target)
    {
        gameObject.transform.SetParent(target.FirstView().transform); // default attach
        target.SetVehicleInfo(ref infomation);

        aircraft = target as Aircraft;
        control = aircraft.Control();
        movement = aircraft.Movement();
        animator = aircraft.Animator();
        rader = aircraft.Rader();
        fcs = aircraft.FCS();
        fcs.SetTeam(infomation.team);
        rader.SetTeam(infomation.team);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    protected Aircraft aircraft = null;
    protected Control control = null;
    protected AircraftMovement movement = null;
    protected AircraftAnimator animator = null;
    protected Rader rader = null;
    protected FireControlSystem fcs = null;
}
