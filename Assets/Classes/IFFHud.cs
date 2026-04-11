using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IFFHud : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Start()
    {
        isTarget = false;
        nameText.gameObject.SetActive(false);
        distanceText.gameObject.SetActive(false);
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    private void LateUpdate()
    {
        if (!target.gameObject.activeInHierarchy)
        {
            release?.Invoke(this);
            return;
        }
        distance = (target.transform.position - player.gameObject.transform.position).sqrMagnitude;
        if (distance > maxDistance)
        {
            release?.Invoke(this);
            return;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
        if (screenPos.z <= 0)
        {
            image.enabled = false;
            return;
        }

        image.enabled = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(screenTransform, screenPos, null, out Vector2 localPoint);
        uiTransform.localPosition = localPoint;


        //거리비례 스케일 추가 (선택사항)
        //float scale = Mathf.Clamp(10.0f / screenPos.z, 0.5f, 2.0f);
        //uiTransform.localScale = Vector3.one * scale;

        if (isTarget)
            distanceText.text = ((int)(Mathf.Sqrt(distance) * 10.0f)).ToString();
    }

    //===========================================
    // Methods
    //===========================================
    public void Attach(Vehicle _target, Player _player)
    {
        player = _player;
        target = _target;

        distanceText.text = target.VehicleName;
        ChangeColor(_target.Team == 0 ? HUDController.unknown : (_target.Team == _player.Team ? HUDController.ally : HUDController.normal));
        nameText.gameObject.SetActive(_target.Team == _player.Team);
        distanceText.gameObject.SetActive(false);
        screenTransform = uiTransform.parent as RectTransform;
        isTGT = _target.IsTGT;
        TGTText.gameObject.SetActive(isTGT);
    }

    public void SetTarget(bool value)
    {
        isTarget = value;
        nameText.gameObject.SetActive(value);
        distanceText.gameObject.SetActive(value);
        if (isTGT)
            TGTText.gameObject.SetActive(value);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void ChangeImageColor(Color color) { image.color = color; }
    private void ChangeColor(Color color) { image.color = nameText.color = distanceText.color = color; }
    public void SetMaxDistance(float value) { maxDistance = value * value; }
    public bool IsTarget() { return isTarget; }

    public delegate void ReleaseMethod(IFFHud iffHUD);
    public event ReleaseMethod release;
        
    private bool isTarget, isTGT;
    private float distance, maxDistance = 1000.0f * 1000.0f;
    public Vehicle target { get; private set; }
    private Player player;

    [SerializeField] private Image image;
    [SerializeField] private RectTransform screenTransform;
    [SerializeField] private RectTransform uiTransform;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI TGTText;
}
