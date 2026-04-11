using UnityEngine;

public abstract class AircraftPilot : Pilot
{

    public void LinkControl(Control aircraftControl) { control = aircraftControl; }

    public override void Attach(Vehicle target)
    {
        gameObject.transform.SetParent(target.gameObject.transform.GetChild(3).transform); // default attach
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


    protected Aircraft aircraft = null;
    protected Control control = null;
    protected AircraftMovement movement = null;
    protected AircraftAnimator animator = null;
    protected Rader rader = null;
    protected FireControlSystem fcs = null;
}
