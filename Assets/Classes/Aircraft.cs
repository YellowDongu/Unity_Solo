using UnityEngine;

public class Control
{
    public Vector3 yoke = Vector3.zero;
    public float throttle = 0.2f;
    public bool isAirBrakeOn = false;
    public bool isGearDown = false;
    public float HighGTurn = 1.0f;
    public float velocity = 33.0f;
}

public class Aircraft : Vehicle
{
    //===========================================
    // Initializer/Destructor
    //===========================================

    void Start()
    {
        if (!dummy)
        {
            SystemIntegration();
            animator.Initialize(false);
        }
    }

    public void SystemIntegration()
    {
        if (integrated)
        {
            engineChannel.clip = GameMaster.Instance.Sound.GetSound("Engine_Outside");
            engineChannel.Play();
            engineChannel.loop = true;
            return;
        }

        integrated = true;
        movement.control = control;
        animator.control = control;
        SetIntValue += rader.SetHP;
        rader.SetHP(hp);
        engineChannel.clip = GameMaster.Instance.Sound.GetSound("Engine_Outside");
        engineChannel.Play();
        engineChannel.loop = true;
    }

    public void StandingSet()
    {
        SystemIntegration();
        engineChannel.Stop();
        engineChannel.loop = false;
        engineChannel.enabled = false;
        movement.enabled = false;
        fcs.enabled = false;
        rader.enabled = false;
        engineChannel.enabled = false;
        animator.enabled = true;
        control.isGearDown = true;
        animator.Initialize(true);
    }

    private void OnDisable()
    {
        if (engineChannel != null)
            engineChannel.Stop();
    }
    private void OnEnable()
    {
        if (engineChannel != null)
            engineChannel.Play();
    }
    public void SetSpecial(int value) { fcs.SetSpecial(value); }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public AircraftMovement Movement => movement;
    public AircraftAnimator Animator => animator;
    public FireControlSystem FCS => fcs;
    public Rader Rader => rader;
    public Control Control => control;
    public AudioSource LinkSFXChannel => sfxChannel;

    private bool integrated = false;
    private Control control = new Control();
    [SerializeField] private AudioClip engine_Outside;
    [SerializeField] private bool dummy = false;
    [SerializeField] private AircraftMovement movement;
    [SerializeField] private AircraftAnimator animator;
    [SerializeField] private FireControlSystem fcs;
    [SerializeField] private Rader rader;
    [SerializeField] private AudioSource engineChannel;
    [SerializeField] private AudioSource sfxChannel;
}
