using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        GameMaster.GetInstance().EnlistBaseCanvas(this);
        warningSound = GameMaster.GetInstance().Sound().GetSound("MissileWarn");
        ChangeColor(greenHUD);
        PitchHalf = Instantiate(PitchHalfPrefab, attitudeIndicator.transform).GetComponent<PitchUI>();
        PitchNEGPool = new ObjectPool<PitchUI>(createFunc: () => Instantiate(PitchNEGPrefab, attitudeIndicator.transform).GetComponent<PitchUI>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj), maxSize: 15);
        PitchNEGHPool = new ObjectPool<PitchUI>(createFunc: () => Instantiate(PitchNEG_HPrefab, attitudeIndicator.transform).GetComponent<PitchUI>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj), maxSize: 15);
        PitchPOSPool = new ObjectPool<PitchUI>(createFunc: () => Instantiate(PitchPOSPrefab, attitudeIndicator.transform).GetComponent<PitchUI>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj), maxSize: 15);
        PitchPOSHPool = new ObjectPool<PitchUI>(createFunc: () => Instantiate(PitchPOS_HPrefab, attitudeIndicator.transform).GetComponent<PitchUI>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj), maxSize: 15);

    }

    private void InitializeHUD()
    {
        Vector3 angle = player.gameObject.transform.eulerAngles;
        attitudeIndicator.transform.rotation = Quaternion.Euler(0.0f, 0.0f, AngleCalibration(angle.z));

        float y = -AngleCalibration(angle.x);
        y /= 2.5f;
        int ySpace = yHeight = (int)y;
        float yLeft = (y - (float)ySpace) * 2.5f;
        float baseValue = -50.0f * (yLeft / 2.5f);
        ySpace -= 4;

        for (int i = 0; i < 9; i++)
        {
            currentActive.Add(GetImage(ySpace));

            ySpace++;
            if (ySpace > 72)
                ySpace *= -1;

            PitchUI image = currentActive[i].Item1;
            image.ChangeColor(currentColor);
            image.CalibratePosition(-200.0f + (float)i * 50.0f + baseValue);
        }
    }

    public void BoundPlayer(Aircraft vehicle)
    {
        player = vehicle.Movement();
        playerRader = vehicle.Rader();
        playerRader.uiWarningEvent += Warning;
        InitializeHUD();
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    private void Update()
    {
        gameTimer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        altitudeIndicator.text = ((int)(player.gameObject.transform.position.y * 10.0f)).ToString();
        speedIndicator.text = ((int)(player.control.velocity * 10.0f)).ToString();

        UpdateAttitude();
    }


    //===========================================
    // Methods
    //===========================================
    private void UpdateAttitude()
    {
        Vector3 angle = player.gameObject.transform.eulerAngles;
        attitudeIndicator.transform.rotation = Quaternion.Euler(0.0f, 0.0f, AngleCalibration(angle.z));

        float y = -AngleCalibration(angle.x);
        int ySpace = (int)(y / 2.5f);
        float difference = 50.0f * (y / 2.5f - (float)ySpace);

        if (ySpace != yHeight)
        {
            bool back = ySpace > yHeight;

            Dequeue(ySpace > yHeight, ySpace);
            yHeight = ySpace;
        }

        for (int i = 0; i < 9; i++)
        {
            PitchUI pitch = currentActive[i].Item1;
            pitch.ChangeColor(currentColor);
            pitch.CalibratePosition(200.0f - (float)i * 50.0f - difference);
        }
    }

    private (PitchUI image, int value) GetImage(int value)
    {
        if (value == 0)
        {
            PitchHalf.gameObject.SetActive(true);
            return (PitchHalf, 5);
        }
        else if (value < 0)
        {
            if (value % 2 == 0)
            {
                PitchUI newInstnce = PitchNEGPool.Get();
                newInstnce.ChangeText((int)((float)value * 2.5f));
                return (newInstnce, 1);
            }
            else
                return (PitchNEGHPool.Get(), 2);
        }
        else
        {
            if (value % 2 == 0)
            {
                PitchUI newInstnce = PitchPOSPool.Get();
                newInstnce.ChangeText((int)((float)value * 2.5f));
                return (newInstnce, 3);
            }
            else
                return (PitchPOSHPool.Get(), 4);
        }
    }

    private void Dequeue(bool isBack, int pitchValue = 255)
    {
        (PitchUI image, int value) target;
        if (isBack)
            target = currentActive[currentActive.Count - 1];
        else
            target = currentActive[0];

        switch (target.value)
        {
            case 1:
                PitchNEGPool.Release(target.image);
                break;
            case 2:
                PitchNEGHPool.Release(target.image);
                break;
            case 3:
                PitchPOSPool.Release(target.image);
                break;
            case 4:
                PitchPOSHPool.Release(target.image);
                break;
            default:
                target.image.gameObject.SetActive(false);
                break;
        }

        if (Mathf.Abs(pitchValue) <= 100)
        {
            if (isBack)
            {
                target = GetImage(pitchValue + 4);
                currentActive.Insert(0, target);
                currentActive.RemoveAt(currentActive.Count - 1);
            }
            else
            {
                target = GetImage(pitchValue - 4);
                currentActive.RemoveAt(0);
                currentActive.Add(target);
            }
        }
    }

    public float AngleCalibration(float value) { while (value < -180.0f) { value += 360.0f; } while (value > 180.0f) { value -= 360.0f; } return value; }

    public void ChangeColor(Vector4 color)
    {
        currentColor = color;
        speedIndicator.color = color;
        altitudeIndicator.color = color;

        foreach (Image hud in huds)
            hud.color = color;
        foreach (TextMeshProUGUI text in texts)
            text.color = color;

        changeColor?.Invoke(color);
    }

    public void Warning(bool _urgent) { warning = true; urgent |= _urgent; if (!inWarning) StartCoroutine(Warning()); }
    public void StartWarning() { StartCoroutine(Warning()); }

    private IEnumerator Warning()
    {
        inWarning = true;
        float Timer = 0.0f;
        int count = 3;
        ChangeColor(redHUD);

        while (count > 0)
        {
            Timer -= Time.deltaTime;

            if (warning)
                count = 3;
            else
                count--;

            warning = false;

            if (Timer >= 0.0f)
            {
                yield return new WaitForFixedUpdate();
                continue;
            }

            if (urgent)
            {
                Timer = 0.5f;
                urgent = false;
            }
            else
                Timer = 2.0f;

            GameMaster.GetInstance().Sound().PlayOnce(warningSound);
            yield return new WaitForFixedUpdate();
        }
        ChangeColor(greenHUD);
        inWarning = false;
    }

    public delegate void ChangeUIColor(Vector4 color);
    public event ChangeUIColor changeColor;

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public float GetGameTimer() { return gameTimer; }
    public LockHUD LinkLockHUD() { return lockHUD; }
    public RaderUI LinkRaderUI() { return raderUI; }
    public IFFUIController LinkIFFUIController() { return iFFUIController; }
    public GaugeUIController LinkGaugeUIController() { return gaugeUIController; }
    public Color GetColor() { return currentColor; }


    private bool initialized = false;
    private bool inWarning = false, warning = false, urgent = false;
    private int yHeight = 0;
    private float gameTimer = 0.0f;

    private Color currentColor = greenHUD;
    private AircraftMovement player = null;
    private Rader playerRader = null;

    private AudioClip warningSound = null;

    private List<(PitchUI, int)> currentActive = new List<(PitchUI, int)>(10);
    private PitchUI PitchHalf = null;
    private ObjectPool<PitchUI> PitchNEGPool = null;
    private ObjectPool<PitchUI> PitchNEGHPool = null;
    private ObjectPool<PitchUI> PitchPOSPool = null;
    private ObjectPool<PitchUI> PitchPOSHPool = null;

    [SerializeField] private LockHUD lockHUD;
    [SerializeField] private RaderUI raderUI;
    [SerializeField] private IFFUIController iFFUIController;
    [SerializeField] private GaugeUIController gaugeUIController;

    [SerializeField] private GameObject attitudeIndicator;
    [SerializeField] private TextMeshProUGUI speedIndicator;
    [SerializeField] private TextMeshProUGUI altitudeIndicator;
    [SerializeField] private List<Image> huds;
    [SerializeField] private List<TextMeshProUGUI> texts;

    [SerializeField] private GameObject PitchHalfPrefab;
    [SerializeField] private GameObject PitchNEGPrefab;
    [SerializeField] private GameObject PitchNEG_HPrefab;
    [SerializeField] private GameObject PitchPOSPrefab;
    [SerializeField] private GameObject PitchPOS_HPrefab;


    /*Color Presets*/
    static public Color ally = new Color(0.0f, 1.0f, 1.0f, 220.0f / 255.0f);
    static public Color normal = new Color(0.0f, 200.0f / 255.0f, 0.0f, 220.0f / 255.0f);
    static public Color unknown = new Color(1.0f, 1.0f, 0.0f, 220.0f / 255.0f);
    static public Color greenHUD = normal;
    static public Color redHUD = new Color(180.0f / 255.0f, 0.0f, 0.0f, 200.0f / 255.0f);
}
