using Unity.VisualScripting;
using UnityEngine;

public class Control
{
    public Vector3 yoke = Vector3.zero;
    public float throttle = 0.2f;
    public bool isAirBreakOn = false;
    public bool isGearDown = false;
    public float HighGTurn = 1.0f;
    public float velocity = 33.0f;
}



public class Aircraft : Vehicle
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SystemIntegration();
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void Aboard(Pilot pilot)
    {

    }

    public void SystemIntegration()
    {
        movement.control = control;
        animator.control = control;
    }

    public void SetSpecial(int value) { fcs.SetSpecial(value); }
    public AircraftMovement Movement() { return movement; }
    public AircraftAnimator Animator() { return animator; }
    public FireControlSystem FCS() { return fcs; }
    public Rader Rader() { return rader; }
    public Control Control() { return control; }

    private Control control = new Control();
    [SerializeField] private AircraftMovement movement;
    [SerializeField] private AircraftAnimator animator;
    [SerializeField] private FireControlSystem fcs;
    [SerializeField] private Rader rader;
}
