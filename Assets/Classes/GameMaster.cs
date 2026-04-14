using UnityEngine;


public class GameMaster : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        if (instance != null)
            Destroy(this);

        instance = this;
        DontDestroyOnLoad(gameObject);
    }


    //===========================================
    // Variable & GetSet Methods
    //===========================================


    public bool IsTGTEmpty() { return factory.IsTGTEmpty(); }
    public static GameMaster GetInstance() { return instance; }
    public Factory GetFactory() { return factory; }
    public SceneChanger GetSceneChanger() { return sceneChanger; }
    public SoundManager Sound() { return soundManager; }
    public void EnlistBaseCanvas(HUDController canvas) { baseCanvas = canvas; }
    public void LinkBaseCanvas(Player player) { player.AttachHUD(baseCanvas); }


    private static GameMaster instance = null;
    private HUDController baseCanvas = null;
    [SerializeField] private SceneChanger sceneChanger = null;
    [SerializeField] private Factory factory = null;
    [SerializeField] private SoundManager soundManager = null;
}
