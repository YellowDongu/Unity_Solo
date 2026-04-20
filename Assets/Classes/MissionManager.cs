using System.Collections;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    [System.Serializable]
    public struct Mission
    {
        public string sceneName;
        public string bgmName;
        public float bgmLoopStart;
    }
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        reserved.weaponSelected = 0;
        reserved.selected = VehicleID.END;
        inMission = false;
        mission = -1;
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    private void Update()
    {
        if (!inMission)
            return;

        missionTimer += Time.deltaTime;


    }

    //===========================================
    // Methods
    //===========================================

    public void ReturnToMain() { StartCoroutine(ReturnMain()); }
    public void StartMission() { StartCoroutine(MissionStart()); }
    public void ChangeToSelect(int selectedMission) { MissionSelect(selectedMission); StartCoroutine(ChangeSelect()); }
    public void EndMission(bool sucess) { if(inMission) StartCoroutine(MissionEnd(sucess)); }
    

    private IEnumerator MissionStart()
    {
        GlobalCanvas globalCanvas = GameMaster.GetInstance().GetGlobalCanvas();
        SoundManager sound = GameMaster.GetInstance().Sound();
        SceneChanger scene = GameMaster.GetInstance().GetSceneChanger();
        float timer = 7.5f; // 최소 타이머

        globalCanvas.FadeOut(true, 0.5f);
        while (globalCanvas.Fading()) // wait
            yield return null;

        globalCanvas.LoadingImageActive(true);
        globalCanvas.FadeOut(false, 0.25f);

        while (globalCanvas.Fading()) // wait
            yield return null;

        scene.LoadScene(currentMission.sceneName, true); // 로딩이 너무 느리면 윗 줄로 당기시오

        while (timer > 0.0f && !scene.LoadingDone()) // wait
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        globalCanvas.FadeOut(true, 0.25f);
        while (globalCanvas.Fading()) // wait
            yield return null;

        globalCanvas.LoadingImageActive(false);

        scene.ActiveTrigger();
        while (scene.LoadingProgress() < 1.0f)
            yield return null;

        AudioClip clip = sound.GetSound("ms01");
        sound.PlayLoop("ms01", 17.53f / clip.length);

        globalCanvas.FadeOut(false, 0.25f);
        inMission = true;
        missionTimer = 0.0f;
    }

    private IEnumerator MissionEnd(bool success)
    {
        inMission = false;
        mission = -1;

        GlobalCanvas globalCanvas = GameMaster.GetInstance().GetGlobalCanvas();
        SoundManager sound = GameMaster.GetInstance().Sound();
        SceneChanger scene = GameMaster.GetInstance().GetSceneChanger();
        float timer = 5.0f; // 최소 타이머

        sound.FadeOut(timer * 0.5f);
        while (timer > 0.0f) // wait
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        sound.PlayOnce("MissionSuccess");
        globalCanvas.MissionEndImageActive(true, success);
        timer = 5.0f;

        while (timer > 0.0f) // wait
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        globalCanvas.FadeOut(true, 0.25f);
        while (globalCanvas.Fading()) // wait
            yield return null;
        globalCanvas.MissionEndImageActive(false, success);
        StartCoroutine(ReturnMain());
    }

    private IEnumerator ReturnMain()
    {
        GlobalCanvas globalCanvas = GameMaster.GetInstance().GetGlobalCanvas();
        SceneChanger scene = GameMaster.GetInstance().GetSceneChanger();
        SoundManager sound = GameMaster.GetInstance().Sound();
        float timer = 2.5f;

        globalCanvas.SetMissionText(0);

        sound.FadeOut(4.0f);
        globalCanvas.FadeOut(true, 0.25f);
        while (globalCanvas.Fading()) // wait
            yield return null;

        globalCanvas.LoadingImageActive(true);
        globalCanvas.FadeOut(false, 0.25f);

        while (globalCanvas.Fading()) // wait
            yield return null;


        while (timer > 0.0f) // wait
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        globalCanvas.FadeOut(true, 0.25f);
        while (globalCanvas.Fading()) // wait
            yield return null;

        globalCanvas.LoadingImageActive(false);
        globalCanvas.FadeOut(false, 0.5f);
        GameMaster.GetInstance().GetSceneChanger().ChangeScene("Main");
    }

    private IEnumerator ChangeSelect()
    {
        float timer = 0.5f;
        GlobalCanvas globalCanvas = GameMaster.GetInstance().GetGlobalCanvas();

        globalCanvas.FadeOut(true, 0.25f);
        while (globalCanvas.Fading()) // wait
            yield return null;

        globalCanvas.MissionImageActive(true);
        while (timer > 0.0f) // wait
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        timer = 3.0f;
        globalCanvas.FadeOut(false, 0.5f);
        while (globalCanvas.FadeOutStatus() > 0.0f) // wait
            yield return null;

        while (timer > 0.0f) // wait
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        globalCanvas.FadeOut(true, 0.5f);
        while (globalCanvas.Fading()) // wait
            yield return null;

        globalCanvas.MissionImageActive(false);
        GameMaster.GetInstance().GetSceneChanger().ChangeScene("Select");
        globalCanvas.FadeOut(false, 0.25f);
    }


    public void MissionSelect(int missionNumber)
    {
        if (missionNumber < 0 || missionNumber >= missionList.Length)
            return;

        mission = missionNumber;
        missionTimer = 0.0f;
        currentMission = missionList[mission];
        GameMaster.GetInstance().GetGlobalCanvas().SetMissionText(missionNumber + 1);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    public PlayerSpawnData PlayerSpawnData() { return reserved; }
    public void SetPlayerSpawn(PlayerSpawnData data) { reserved = data; }
    public float missionTime() { return missionTimer; }

    private bool inMission = false;
    private int mission = -1;
    private float missionTimer = 0.0f;
    private Mission currentMission;
    private PlayerSpawnData reserved;
    [SerializeField] Mission[] missionList;
}
