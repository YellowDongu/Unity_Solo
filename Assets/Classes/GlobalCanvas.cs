using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalCanvas : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================

    private void Awake()
    {
        SetMissionText(0);
    }

    //===========================================
    // Methods
    //===========================================

    public void FadeOut(bool active, float speed = 1.0f)
    {
        if (fadeOut == active || fading)
            return;
        fadeOut = active;
        StartCoroutine(FadeOut(speed));

    }

    private IEnumerator FadeOut(float speed)
    {
        fading = true;
        Color color = fadeOutImage.color;
        float target = fadeOut ? 1.0f : 0.0f;
        float delta = fadeOut ? 1.0f : -1.0f;

        yield return null;
        while (color.a != target)
        {
            color.a = Mathf.Clamp01(color.a + delta * Time.deltaTime);
            fadeOutImage.color = color;
            yield return null;
        }
        fading = false;
    }

    public void SetMissionText(int missionNumber)
    {
        switch (missionNumber)
        {
            case 1:
                loadingTitleText.text = missionTitleText.text = ms01TitleText;
                loadingSideText.text = ms01SideText;
                break;
            case 2:
                loadingTitleText.text = missionTitleText.text = ms02TitleText;
                loadingSideText.text = ms02SideText;
                break;
            default:
                loadingTitleText.text = missionTitleText.text = "";
                loadingSideText.text = "";
                break;
        }
    }

    public void MissionEndImageActive(bool active, bool sucess)
    {
        if (sucess)
            MissionSucessPanel.SetActive(active);
        else
            MissionFailedPanel.SetActive(active);
    }

    public void LoadingImageActive(bool active)
    {
        if (isLoadingActive == active)
            return;

        isLoadingActive = active;
        loadingPanel.SetActive(active);
    }


    public void MissionImageActive(bool active)
    {
        if (isMissionActive == active)
            return;

        isMissionActive = active;
        missionPanel.SetActive(active);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    public float FadeOutStatus => fadeOutImage.color.a;
    public bool IsFading => fading;

    private bool fadeOut = false, fading = false;
    private bool isLoadingActive = false, isMissionActive = false;
    private string ms01TitleText = "MISSION01";
    private string ms02TitleText = "MISSION02";
    private string ms01SideText = "15.May.2019   1614   Fort Grays Island   7°58'25\"S 9°25'50\"W   Cloud Cover: Scattered";
    private string ms02SideText = "19.Aug.2019   1009   Stonehenge (Hatties.D)   9° 18'52\"N 49° 31'51\"W   Cloud Cover: Few Clouds"; // 배열로 해서 관리할 순 있지만 지금은 일단 유지

    [SerializeField] private Image fadeOutImage;
    [SerializeField] private TextMeshProUGUI missionTitleText;
    [SerializeField] private TextMeshProUGUI loadingTitleText;
    [SerializeField] private TextMeshProUGUI loadingSideText;
    [SerializeField] private GameObject missionPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject MissionSucessPanel;
    [SerializeField] private GameObject MissionFailedPanel;
}
