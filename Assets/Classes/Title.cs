using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Title : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================

    private void Awake()
    {
        if (fistTime)
        {
            fistTime = false;
            disable = false;
            breath = false;
            TitlePanel.SetActive(true);
            mainMenuPanel.SetActive(false);
            missionSelector.SetInitialSelectPosition(0);
            mainMenuSelector.SetInitialSelectPosition(0);
        }
        else
        {
            phase = 1;
            TitlePanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            mainMenuSelector.SetActive(true);
            missionSelector.SetActive(false);
            Calculate();
            mainMenuSelector.MovePosition(0.0f, outerZ, 0.75f);
            missionSelector.MovePosition(outerZ * -0.5f, outerZ + outerZ * -0.5f, 0.75f);
            SoundManager sound = GameMaster.GetInstance().Sound();
            MenuSelected = sound.GetSound("MenuSelected");
            MenuChange = sound.GetSound("MenuChange");
            sound.Play("main", true, true);
            TitleText.text = "MAIN MENU";
            secondPhase = 0;
            currentSelector = mainMenuSelector;
        }
        //GameMaster.GetInstance().GetSceneChanger().ChangeScene("Select", true);
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    private void Update()
    {
        if (disable)
            return;
        switch (phase)
        {
            case 0:
                InTitle();
                break;
            case 1:
                MainMenu();
                break;
            default:
                phase = 0;
                TitlePanel.SetActive(true);
                mainMenuPanel.SetActive(false);
                break;
        }
    }

    //===========================================
    // Methods
    //===========================================

    private void InTitle()
    {
        timer += Time.deltaTime * (breath ? -0.5f : 0.5f);

        if (timer <= 0.0f)
        {
            breath = false;
            timer = 0.0f;
        }
        else if(timer >= 1.0f)
        {
            breath = true;
            timer = 1.0f;
        }

        Color color = TitleText.color;
        color.a = timer;
        TitleText.color = color;
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Calculate();
            StartCoroutine(ChangeToMainMenu());
        }
    }

    private IEnumerator ChangeToMainMenu()
    {
        disable = true;
        SoundManager sound = GameMaster.GetInstance().Sound();
        AudioClip clip = sound.GetSound("TurnToMainMenu");
        MenuSelected = sound.GetSound("MenuSelected");
        MenuChange = sound.GetSound("MenuChange");
        sound.PlayOnce(clip);
        float soundLength = clip.length;
        Color color = TitleText.color;
        color.a = 0.0f;
        TitleText.color = color;

        while (soundLength >= 0.0f)
        {
            soundLength -= Time.deltaTime;
            yield return null;
        }

        phase = 1;
        TitlePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        mainMenuSelector.SetActive(true);
        missionSelector.SetActive(false);
        mainMenuSelector.MovePosition(0.0f, outerZ, 0.75f);
        missionSelector.MovePosition(outerZ * -0.5f, outerZ + outerZ * -0.5f, 0.75f);
        disable = false;
        TitleText.text = "MAIN MENU";
        secondPhase = 0;
        currentSelector = mainMenuSelector;
        sound.Play("main", true, true);
    }

    private void MainMenu()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
        {
            switch (secondPhase)
            {
                case 0:
                    if (currentSelector.Selected == 0)
                    {
                        mainMenuSelector.SetActive(false);
                        missionSelector.SetActive(true);
                        mainMenuSelector.MovePosition(outerZ, 0.75f);
                        missionSelector.MovePosition(0.0f, 0.75f);
                        currentSelector = missionSelector;
                        secondPhase = 1;
                        TitleText.text = "SELECT MISSION";
                        GameMaster.GetInstance().Sound().PlayOnce(MenuSelected);
                    }
                    else if (currentSelector.Selected == 1)
                    {
                        GameMaster.GetInstance().QuitApplication();
                        GameMaster.GetInstance().Sound().PlayOnce(MenuSelected);
                    }
                    break;
                case 1:
                    {
                        disable = true;
                        SoundManager sound = GameMaster.GetInstance().Sound();
                        sound.Stop();
                        GameMaster.GetInstance().Sound().PlayOnce(MenuSelected);
                        GameMaster.GetInstance().ChangeToSelect(currentSelector.Selected);
                    }
                    break;
                default:
                    break;
            }
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            switch (secondPhase)
            {
                case 0:
                    GameMaster.GetInstance().Sound().PlayOnce(MenuSelected);
                    break;
                case 1:
                    mainMenuSelector.SetActive(true);
                    missionSelector.SetActive(false);
                    mainMenuSelector.MovePosition(0.0f, 0.75f);
                    missionSelector.MovePosition(outerZ * -0.5f, 0.75f);
                    currentSelector = mainMenuSelector;
                    secondPhase = 0;
                    TitleText.text = "MAIN MENU";
                    GameMaster.GetInstance().Sound().PlayOnce(MenuSelected);
                    break;
                default:
                    break;
            }
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            GameMaster.GetInstance().Sound().PlayOnce(MenuChange);
            currentSelector.SelectNext();
        }
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            currentSelector.SelectPrevious();
            GameMaster.GetInstance().Sound().PlayOnce(MenuChange);
        }
    }

    void Calculate() { outerZ = -(targetCanvas.planeDistance - targetCamera.nearClipPlane) / targetCanvas.transform.localScale.z; }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public static string GetSelectedMission() { return selectedMission; }

    private static string selectedMission;
    private static bool fistTime = true;
    private bool disable = false, breath = false;
    private int phase = 0, secondPhase = 0;
    private float timer = 0.0f;
    private float outerZ = 0.0f;

    private AudioClip MenuSelected;
    private AudioClip MenuChange;
    private UISelector currentSelector;

    [SerializeField] private string nextSceneName;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private TextMeshProUGUI TitleText;
    [SerializeField] private UISelector mainMenuSelector;
    [SerializeField] private UISelector missionSelector;
    [SerializeField] private GameObject TitlePanel;
    [SerializeField] private GameObject mainMenuPanel;
}
