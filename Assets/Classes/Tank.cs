using UnityEngine;

public class GroundControl
{
    public Vector3 viewAngle = Vector3.zero;
}

public class Tank : Vehicle
{
    void Start()
    {
        SystemIntegration();
    }

    public void SystemIntegration()
    {
        movement.control = control;
        animator.Initialize(control);
        SetIntValue += rader.GetHP;
        rader.GetHP(hp);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void SetSpecial(int value) { fcs.SetSpecial(value); }
    public GroundMovement Movement() { return movement; }
    public TankAnimator Animator() { return animator; }
    public FireControlSystem FCS() { return fcs; }
    public Rader Rader() { return rader; }
    public GroundControl Control() { return control; }


    private GroundControl control = new GroundControl();

    [SerializeField] private GroundMovement movement;
    [SerializeField] private TankAnimator animator;
    [SerializeField] private FireControlSystem fcs;
    [SerializeField] private Rader rader;




}
