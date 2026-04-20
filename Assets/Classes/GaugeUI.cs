using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class GaugeUI : MonoBehaviour
{
    //===========================================
    // struct/enum
    //===========================================
    public enum GaugeUIType
    {
        stm,
        faam,
        sarm,
        fagm,
        END
    }

    [System.Serializable]
    public class Gauge
    {
        //===========================================
        // Methods
        //===========================================
        public void UpdateProgress()
        {
            progress = GetCoolTime();
            gauge.fillAmount = 1 - progress / progressMax;
        }

        //===========================================
        // Variable & GetSet Methods
        //===========================================
        public void GetMaxCoolTime(float value) { progressMax = value; }
        public void LinkCoolTime(GetCoolTimeMethod method) { GetCoolTime = method; }

        public delegate float GetCoolTimeMethod();
        private GetCoolTimeMethod GetCoolTime;

        private float progressMax;
        private float progress;
        [SerializeField] private Image container;
        [SerializeField] private Image gauge;
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    void FixedUpdate()
    {
        foreach (Gauge gauge in gaugeList)
        {
            gauge.UpdateProgress();
        }
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public ReadOnlyCollection<Gauge> GaugeList => gaugeList.AsReadOnly();
    public GaugeUIType GetUIType() { return type; }

    [SerializeField] private GaugeUIType type;
    [SerializeField] private List<Gauge> gaugeList;
}
