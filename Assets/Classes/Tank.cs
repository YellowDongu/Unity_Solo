using UnityEngine;

public class GroundControl
{
    public Vector3 viewAngle = Vector3.zero;
}

public class Tank : Vehicle
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    void Start()
    {
        SystemIntegration();
    }

    public void SystemIntegration()
    {
        hp = maxHp;
        movement.control = control;
        animator.Initialize(control);
        SetIntValue += rader.SetHP;
        rader.SetHP(hp);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void SetSpecial(int value) { fcs.SetSpecial(value); }
    public GroundMovement Movement => movement;
    public TankAnimator Animator => animator;
    public TurretFireControlSystem FCS => fcs;
    public Rader Rader => rader;
    public GroundControl Control => control;

    private GroundControl control = new GroundControl();
    [SerializeField] private GroundMovement movement;
    [SerializeField] private TankAnimator animator;
    [SerializeField] private TurretFireControlSystem fcs;
    [SerializeField] private Rader rader;




}
