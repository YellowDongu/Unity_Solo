using UnityEngine;

public class Title : MonoBehaviour
{

    private void Awake()
    {
        GameMaster.GetInstance().GetSceneChanger().ChangeScene(nextSceneName, false);
    }

    // Update is called once per frame
    void Update()
    {
        //if (working)
        //    return;
        //if(Keyboard.current.spaceKey.wasPressedThisFrame)
        //{
        //    working = true;
        //    GameMaster.GetInstance().GetSceneChanger().ActiveTrigger();
        //}
    }


    bool working = false;
    [SerializeField] private string nextSceneName;
}
