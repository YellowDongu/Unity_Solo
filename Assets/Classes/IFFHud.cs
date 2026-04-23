using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IFFHud : MonoBehaviour
{
    //===========================================
    // struct/enum
    //===========================================
    [System.Serializable]
    private struct ImageSet
    {
        public GameObject container;
        public Image image;
        public Image secondImage; // not
    }

    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Start()
    {
        isTarget = false;
        nameText.gameObject.SetActive(false);
        distanceText.gameObject.SetActive(false);
        TGTText.gameObject.SetActive(false);
    }

    public void Attach(Vehicle _target, Player _player, Aircraft playerAircraft)
    {
        player = _player;
        Target = _target;

        nameText.gameObject.SetActive(_target.Team == _player.Team);
        distanceText.gameObject.SetActive(false);
        GetLayer = playerAircraft.FCS.GetMissileAimLayer;
        screenTransform = uiTransform.parent as RectTransform;
        ImageInitialize(_target);
        ChangeColor(_target.Team == 0 ? HUDController.unknown : (_target.Team == _player.Team ? HUDController.ally : HUDController.normal));
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    private void LateUpdate()
    {
        if (Target == null || !Target.gameObject.activeInHierarchy)
        {
            release?.Invoke(this);
            return;
        }
        distance = (Target.transform.position - player.gameObject.transform.position).sqrMagnitude;
        if (distance > maxDistance)
        {
            release?.Invoke(this);
            return;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(Target.transform.position);
        if (screenPos.z <= 0)
        {
            ImageActive(false);
            return;
        }

        ImageActive(true);
        GetAimMask();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(screenTransform, screenPos, null, out Vector2 localPoint);
        uiTransform.localPosition = localPoint;


        //거리비례 스케일 추가 (선택사항)
        //float scale = Mathf.Clamp(10.0f / screenPos.z, 0.5f, 2.0f);
        //uiTransform.localScale = Vector3.one * scale;

        if (isTarget)
            distanceText.text = ((int)GameMaster.ConvertInGameScale(Mathf.Sqrt(distance))).ToString();
    }

    //===========================================
    // Methods
    //===========================================
    public void SetTarget(bool value)
    {
        isTarget = value;
        nameText.gameObject.SetActive(value);
        distanceText.gameObject.SetActive(value);
    }

    public void ChangeImage(int mask)
    {
        aimMask = mask;
        int bit = isAir ? 0 : 1;

        if ((aimMask & (1 << bit)) != 0)
        {
            imageSet[preset].image.gameObject.SetActive(true);
            imageSet[preset].secondImage.gameObject.SetActive(false);
        }
        else
        {
            imageSet[preset].image.gameObject.SetActive(false);
            imageSet[preset].secondImage.gameObject.SetActive(true);
        }
    }

    public void ImageInitialize(Vehicle target)
    {
        isAir = !target.IsLand;
        preset = target.VehicleLayer;
        isTGT = target.IsTGT;
        nameText.text = target.VehicleName;
        TGTText.gameObject.SetActive(isTGT);

        foreach (var item in imageSet)
            item.container.SetActive(false);

        imageSet[preset].container.SetActive(true);
        isActive = true;
        ChangeImage(aimMask);
    }

    public void ImageActive(bool active)
    {
        if (isActive == active)
            return;
        isActive = active;
        imageSet[preset].container.SetActive(isActive);
        TGTText.gameObject.SetActive(active ? isTGT : false);
    }

    public void GetAimMask()
    {
        int layer = GetLayer();
        if (aimMask == layer)
            return;
        aimMask = layer;
        ChangeImage(aimMask);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void ChangeImageColor(Color color) { imageSet[preset].secondImage.color = imageSet[preset].image.color = color; }
    private void ChangeColor(Color color) { imageSet[preset].secondImage.color = imageSet[preset].image.color = nameText.color = distanceText.color = color; }
    public void SetMaxDistance(float value) { maxDistance = value * value; }
    public bool IsTarget() { return isTarget; }
    public Vehicle Target { get { return target; } private set { target = value; } }

    public delegate void ReleaseMethod(IFFHud iffHUD);
    public event ReleaseMethod release;
    private bool isTarget, isTGT, isActive = true;
    private float distance, maxDistance = 1000.0f * 1000.0f;
    private int aimMask = 0, preset = 0;
    private Func<int> GetLayer;
    public Vehicle target = null;
    private Player player;
    private RectTransform screenTransform;

    [SerializeField] private bool isAir;
    [SerializeField] private ImageSet[] imageSet;
    [SerializeField] private RectTransform uiTransform;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI TGTText;
}
