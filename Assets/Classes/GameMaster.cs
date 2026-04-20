using UnityEditor;
using UnityEngine;


public class GameMaster : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }


        instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }

    public void StartMission(PlayerSpawnData spawnData) { SetPlayerSpawn(spawnData); missionManager.StartMission(); }
    public void EndMission(bool success) { missionManager.EndMission(success); }
    public void ReturnToMain() { missionManager.ReturnToMain(); }
    public void ChangeToSelect(int selectedMission) { missionManager.ChangeToSelect(selectedMission); }
    public void Explosion(Vector3 worldPos, float size = 5.0f) { soundManager.PlayOnce("Explosion", worldPos); factory.Explosion(worldPos, size); }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public float GameTime() { return missionManager.missionTime(); }
    public bool IsTGTEmpty() { return factory.IsTGTEmpty(); }
    public Factory GetFactory() { return factory; }
    public SceneChanger GetSceneChanger() { return sceneChanger; }
    public GlobalCanvas GetGlobalCanvas() { return globalCanvas; }
    public SoundManager Sound() { return soundManager; }
    public void EnlistBaseCanvas(HUDController canvas) { baseCanvas = canvas; }
    public void LinkBaseCanvas(Player player) { player.AttachHUD(baseCanvas); }
    public PlayerSpawnData PlayerSpawnData() { return missionManager.PlayerSpawnData(); }
    public void SetPlayerSpawn(PlayerSpawnData data) { missionManager.SetPlayerSpawn(data); }


    private HUDController baseCanvas = null;
    [SerializeField] private SceneChanger sceneChanger = null;
    [SerializeField] private Factory factory = null;
    [SerializeField] private SoundManager soundManager = null;
    [SerializeField] private MissionManager missionManager = null;
    [SerializeField] private GlobalCanvas globalCanvas = null;
    //===========================================
    // Global Variable & Methods
    //===========================================
    public static GameMaster GetInstance() { return instance; }
    public static float ConvertWorldScale(float value) { return value / gameDistanceScale; }
    public static float ConvertInGameScale(float value) { return value * gameDistanceScale; }

    private static GameMaster instance = null;
    static float gameDistanceScale = 5.0f;
}
