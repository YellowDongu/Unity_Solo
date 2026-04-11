using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GaugeUIController : MonoBehaviour
{
    //===========================================
    // struct/enum
    //===========================================
    [System.Serializable]
    public struct SpriteInfomation
    {
        public VehicleID vehicleID;
        public Sprite container;
        public Sprite progress;
    }

    [System.Serializable]
    public struct UIInfomation
    {
        public GaugeUI.GaugeUIType GaugeUIType;
        public GameObject prefab;
    }

    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (initialized)
            return;

        initialized = true;
        hpUIPrefabs = new Dictionary<VehicleID, SpriteInfomation>();
        uiPrefabs = new Dictionary<GaugeUI.GaugeUIType, GameObject>();

        foreach (var item in hpUIPrefabList)
            hpUIPrefabs.Add(item.vehicleID, item);
        foreach (var item in uiPrefabList)
            uiPrefabs.Add(item.GaugeUIType, item.prefab);
    }

    public void BoundPlayer(Aircraft vehicle)
    {
        Initialize();
        SpriteInfomation infomation = hpUIPrefabs[vehicle.ID];

        Image image = gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>();
        image.sprite = infomation.container;
        image = gameObject.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Image>();
        image.sprite = infomation.progress;

        hp.GaugeList[0].GetCoolTime += vehicle.HPPresentage;
        hp.GaugeList[0].GetMaxCoolTime(0);

        FireControlSystem fcs = vehicle.FCS();
        standardGauge = Instantiate(uiPrefabs[fcs.NeededUIStandard()], gameObject.transform).GetComponent<GaugeUI>();
        fcs.LinkStandard(standardGauge.GaugeList);
        standardGauge.gameObject.SetActive(!fcs.GetSelectState());

        if (fcs.NeededUISpecial() != GaugeUI.GaugeUIType.END)
        {
            specialGauge = Instantiate(uiPrefabs[fcs.NeededUISpecial()], gameObject.transform).GetComponent<GaugeUI>();
            fcs.LinkSpecial(specialGauge.GaugeList);
            specialGauge.gameObject.SetActive(fcs.GetSelectState());
        }

        fcs.ChangeState += ChangeState;
    }

    //===========================================
    // Methods
    //===========================================
    public void ChangeState(bool value)
    {
        standardGauge.gameObject.SetActive(!value);
        if (specialGauge != null)
            specialGauge.gameObject.SetActive(value);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    private bool initialized = false;

    [SerializeField] private Image hpContainer;
    [SerializeField] private Image hpProgress;

    private GaugeUI standardGauge;
    private GaugeUI specialGauge;
    [SerializeField] private GaugeUI hp;

    private Dictionary<VehicleID, SpriteInfomation> hpUIPrefabs;
    private Dictionary<GaugeUI.GaugeUIType, GameObject> uiPrefabs;
    [SerializeField] private List<SpriteInfomation> hpUIPrefabList;
    [SerializeField] private List<UIInfomation> uiPrefabList;
}
