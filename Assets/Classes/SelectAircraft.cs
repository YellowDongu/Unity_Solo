using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;

public struct PlayerSpawnData
{
    public int weaponSelected;
    public VehicleID selected;
}

public class SelectAircraft : MonoBehaviour
{
    //Sound
	//if(FAILED(::Sound()->LoadSound("../Bin/Resources/Sounds/Effects/", "TurnToMainMenu.wav", TurnToMainMenu))) return E_FAIL;
	//if(FAILED(::Sound()->LoadSound("../Bin/Resources/Sounds/Effects/", "MenuSelected.wav", MenuSelected))) return E_FAIL;
	//if(FAILED(::Sound()->LoadSound("../Bin/Resources/Sounds/Effects/", "MenuChange.wav", MenuChange))) return E_FAIL;
	//if(FAILED(::Sound()->LoadSound("../Bin/Resources/Sounds/Effects/", "MenuCancel.wav", MenuCancel))) return E_FAIL;
	//if(FAILED(::Sound()->LoadSound("../Bin/Resources/Sounds/Effects/", "AircraftSelected.wav", aircraftSelected))) return E_FAIL;
	//if(FAILED(::Sound()->LoadSound("../Bin/Resources/Sounds/BGMs/", "main.wav", MainMenuBGM))) return E_FAIL;
	//if(FAILED(::Sound()->LoadSound("../Bin/Resources/Sounds/BGMs/", "Select.wav", SelectMenuBGM))) return E_FAIL; 

    //===========================================
    // struct/enum
    //===========================================
    [System.Serializable]
    public struct AircraftData
    {
        public VehicleID id;
        public Vector3 hangerPosition;
    }

    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        selectedList = new List<VehicleID>(data.Length);
        Factory factory = GameMaster.Instance.Factory;
        Quaternion eulerAngle = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        preLoaded.Add(VehicleID.None, null);
        selectedList.Add(VehicleID.None);

        foreach (AircraftData item in data)
        {
            Aircraft newInstnace = factory.Create(item.id) as Aircraft;
            newInstnace.StandingSet();

            MonoBehaviour[] behaviours = newInstnace.gameObject.GetComponents<MonoBehaviour>(); // all Off
            foreach (MonoBehaviour component in behaviours)
                component.enabled = false;

            newInstnace.gameObject.GetComponent<AircraftAnimator>().enabled = true;
            newInstnace.gameObject.transform.localScale = Vector3.one * 2.0f;
            newInstnace.gameObject.transform.position = item.hangerPosition;
            newInstnace.gameObject.transform.rotation = eulerAngle;
            newInstnace.SystemIntegration();
            newInstnace.Control.isGearDown = true;
            newInstnace.gameObject.SetActive(false);
            preLoaded.Add(item.id, newInstnace.gameObject);
            selectedList.Add(item.id);
        }

        vehicleSelector.SetActive(true);
        vehicleSelector.gameObject.SetActive(true);
        weaponSelector.SetActive(false);
        weaponSelector.gameObject.SetActive(false);
        currentSelector = vehicleSelector;
        phase = 0;

        SoundManager sound = GameMaster.Instance.Sound;
        MenuSelected = sound.GetSound("MenuSelected");
        MenuChange = sound.GetSound("MenuChange");
        returnSound = sound.GetSound("MenuCancel");
        //sound.Play("main", true, true);
    }
    private void Start()
    {
        GameMaster.Instance.Sound.Play("Select");
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    private void Update()
    {
        if (deactive)
            return;

        switch (phase)
        {
            case 0:
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    selected = currentSelector.Selected + 1;
                    if (selectedList[selected] != VehicleID.None && selectedList[selected] != VehicleID.END)
                    {
                        vehicleSelector.SetActive(false);
                        vehicleSelector.gameObject.SetActive(false);
                        weaponSelector.SetActive(true);
                        weaponSelector.gameObject.SetActive(true);
                        currentSelector = weaponSelector;
                        phase = 1;
                        GameMaster.Instance.Sound.PlayOnce(MenuSelected);
                    }
                }
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    GameMaster.Instance.Sound.PlayOnce(returnSound);
                    GameMaster.Instance.ReturnToMain();
                }

                break;
            case 1:
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    deactive = true;
                    PlayerSpawnData newData;
                    newData.weaponSelected = weaponSelector.Selected;
                    newData.selected = selectedList[selected];

                    SoundManager sound = GameMaster.Instance.Sound;
                    sound.PlayOnce("AircraftSelected");
                    sound.FadeOut(4.0f);

                    GameMaster.Instance.StartMission(newData);
                    return;
                }

                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    vehicleSelector.SetActive(true);
                    vehicleSelector.gameObject.SetActive(true);
                    weaponSelector.SetActive(false);
                    weaponSelector.gameObject.SetActive(false);
                    weaponSelected = currentSelector.Selected;
                    currentSelector = vehicleSelector;
                    phase = 0;
                    GameMaster.Instance.Sound.PlayOnce(returnSound);
                }

                break;
            default:
                phase = 0;
                break;
        }

        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            currentSelector.SelectNext();
            Change();
            GameMaster.Instance.Sound.PlayOnce(MenuChange);
        }
        else if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            currentSelector.SelectPrevious();
            Change();
            GameMaster.Instance.Sound.PlayOnce(MenuChange);
        }
    }

    //===========================================
    // Methods
    //===========================================

    private void Change()
    {
        if (phase != 0)
            return;

        if(preLoaded.TryGetValue(selectedList[currentSelector.Selected + 1], out GameObject next))
        {
            if (current != null)
                current.SetActive(false);

            current = next;
            current.SetActive(true);
        }
    }


    //===========================================
    // Variable & GetSet Methods
    //===========================================
    private bool deactive = false;
    private int phase = 0;
    private int selected = 0, weaponSelected = 0;

    private AudioClip MenuSelected;
    private AudioClip MenuChange;
    private AudioClip returnSound;
    private UISelector currentSelector;
    private GameObject current;
    
    [SerializeField] private string nextScene;
    [SerializeField] private UISelector vehicleSelector;
    [SerializeField] private UISelector weaponSelector;

    private List<VehicleID> selectedList;
    private Dictionary<VehicleID, GameObject> preLoaded = new Dictionary<VehicleID, GameObject>((int)VehicleID.END);
    [SerializeField] private AircraftData[] data;
}
