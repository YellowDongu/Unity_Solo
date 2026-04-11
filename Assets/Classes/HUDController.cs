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
        ChangeColor(greenHUD);
        PitchHalf = Instantiate(PitchHalfPrefab, attitudeIndicator.transform).GetComponent<Image>();
        PitchNEGPool = new ObjectPool<Image>(createFunc: () => Instantiate(PitchNEGPrefab, attitudeIndicator.transform).GetComponent<Image>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj), maxSize: 15);
        PitchNEGHPool = new ObjectPool<Image>(createFunc: () => Instantiate(PitchNEG_HPrefab, attitudeIndicator.transform).GetComponent<Image>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj), maxSize: 15);
        PitchPOSPool = new ObjectPool<Image>(createFunc: () => Instantiate(PitchPOSPrefab, attitudeIndicator.transform).GetComponent<Image>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj), maxSize: 15);
        PitchPOSHPool = new ObjectPool<Image>(createFunc: () => Instantiate(PitchPOS_HPrefab, attitudeIndicator.transform).GetComponent<Image>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj), maxSize: 15);
    }

    private void FixedUpdate()
    {
        foreach ((Image target, int value) in currentActive)
        {
            switch (value)
            {
                case 1:
                    PitchNEGPool.Release(target);
                    break;
                case 2:
                    PitchNEGHPool.Release(target);
                    break;
                case 3:
                    PitchPOSPool.Release(target);
                    break;
                case 4:
                    PitchPOSHPool.Release(target);
                    break;
                default:
                    target.gameObject.SetActive(false);
                    break;
            }
        }
        currentActive.Clear();

        altitudeIndicator.text = ((int)(player.gameObject.transform.position.y * 10.0f)).ToString();
        speedIndicator.text = ((int)(player.control.velocity * 10.0f)).ToString();

        Vector3 angle = player.gameObject.transform.eulerAngles;
        attitudeIndicator.transform.rotation = Quaternion.Euler(0.0f, 0.0f, AngleCalibration(angle.z));

        float y = AngleCalibration(angle.x) / -2.5f;
        int ySpace = (int)y;
        float yLeft = (y - (float)ySpace) * 2.5f;
        float baseValue = -50.0f * (yLeft / 2.5f);
        ySpace -= 4;

        for (int i = 0; i < 9; i++)
        {
            Image pitch = null;
            if (ySpace == 0)
            {
                PitchHalf.gameObject.SetActive(true);
                pitch = PitchHalf;
                currentActive.Add((PitchHalf, 5));
            }
            else if (ySpace < 0)
            {
                if (i % 2 == 0)
                {
                    pitch = PitchNEGPool.Get();
                    currentActive.Add((pitch, 1));

                }
                else
                {
                    pitch = PitchNEGHPool.Get();
                    currentActive.Add((pitch, 2));
                }
            }
            else
            {
                if (i % 2 == 0)
                {
                    pitch = PitchPOSPool.Get();
                    currentActive.Add((pitch, 3));
                }
                else
                {
                    pitch = PitchPOSHPool.Get();
                    currentActive.Add((pitch, 4));
                }

            }
            ySpace++;
            if (ySpace > 72)
                ySpace *= -1;

            pitch.color = currentColor;
            RectTransform rectTransform = pitch.gameObject.transform as RectTransform;
            rectTransform.anchoredPosition = new Vector2(0.0f, -200.0f + (float)i * 50.0f + baseValue);
        }



    }


    //===========================================
    // Methods
    //===========================================
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

    public delegate void ChangeUIColor(Vector4 color);
    public event ChangeUIColor changeColor;


    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void BoundPlayer(AircraftMovement playerComponent) { player = playerComponent; }
    public LockHUD LinkLockHUD() { return lockHUD; }
    public RaderUI LinkRaderUI() { return raderUI; }
    public IFFUIController LinkIFFUIController() { return iFFUIController; }
    public GaugeUIController LinkGaugeUIController() { return gaugeUIController; }
    public Color GetColor() { return currentColor; }


    static public Color ally = new Color(0.0f, 1.0f, 1.0f, 200.0f / 255.0f);
    static public Color normal = new Color(0.0f, 200.0f / 255.0f, 0.0f, 200.0f / 255.0f);
    static public Color unknown = new Color(1.0f, 1.0f, 0.0f, 200.0f / 255.0f);

    static public Color greenHUD = normal;
    static public Color redHUD = new Color(180.0f / 255.0f, 0.0f, 0.0f, 200.0f / 255.0f);

    private Color currentColor = greenHUD;
    private AircraftMovement player;
    [SerializeField] private AircraftMovement debug;

    [SerializeField] private LockHUD lockHUD = null;
    [SerializeField] private RaderUI raderUI = null;
    [SerializeField] private IFFUIController iFFUIController = null;
    [SerializeField] private GaugeUIController gaugeUIController = null;

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

    [SerializeField] private List<(Image, int)> currentActive = new List<(Image, int)>(10);
    [SerializeField] private Image PitchHalf;
    [SerializeField] private ObjectPool<Image> PitchNEGPool;
    [SerializeField] private ObjectPool<Image> PitchNEGHPool;
    [SerializeField] private ObjectPool<Image> PitchPOSPool;
    [SerializeField] private ObjectPool<Image> PitchPOSHPool;
}
