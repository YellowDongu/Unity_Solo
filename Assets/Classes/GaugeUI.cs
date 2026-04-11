using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class GaugeUI : MonoBehaviour
{
    public enum GaugeUIType
    {
        stm,
        faam,
        END
    }

    [System.Serializable]
    public struct Gauge
    {
        public void UpdateProgress()
        {
            progress = GetCoolTime.Invoke();
            gauge.fillAmount = 1 - progress / progressMax;
        }
        public void GetMaxCoolTime(float value) { progressMax = value; }


        public delegate float GetCoolTimeMethod();
        public event GetCoolTimeMethod GetCoolTime;

        private float progressMax;
        private float progress;
        [SerializeField] private Image container;
        [SerializeField] private Image gauge;
    }


    void FixedUpdate()
    {
        foreach (Gauge gauge in gaugeList)
        {
            gauge.UpdateProgress();
        }
    }

    public ReadOnlyCollection<Gauge> GaugeList => gaugeList.AsReadOnly();
    public GaugeUIType GetUIType() { return type; }

    [SerializeField] private GaugeUIType type;
    [SerializeField] private List<Gauge> gaugeList;
}
