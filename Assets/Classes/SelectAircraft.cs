using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
        Factory factory = GameMaster.GetInstance().GetFactory();
        Quaternion eulerAngle = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        preLoaded.Add(VehicleID.None, null);
        selectedList.Add(VehicleID.None);

        foreach (AircraftData item in data)
        {
            Aircraft newInstnace = factory.Create(item.id) as Aircraft;
            newInstnace.StandingSet();
            MonoBehaviour[] behaviours = newInstnace.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in behaviours)
                component.enabled = false;
            newInstnace.gameObject.GetComponent<AircraftAnimator>().enabled = true;
            newInstnace.gameObject.transform.localScale = Vector3.one * 2.5f;
            newInstnace.gameObject.transform.position = item.hangerPosition;
            newInstnace.gameObject.transform.rotation = eulerAngle;
            newInstnace.SystemIntegration();
            newInstnace.Control().isGearDown = true;
            newInstnace.gameObject.SetActive(false);
            preLoaded.Add(item.id, newInstnace.gameObject);
            selectedList.Add(item.id);
        }

        vehicleSelectedHighlight.gameObject.SetActive(false);

        vehicleSelectPanel.SetActive(true);
        weaponSelectPanel.SetActive(false);
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    private void Update()
    {
        Selected();
    }

    //===========================================
    // Methods
    //===========================================
    private void Selected()
    {
        switch (phase)
        {
            case 0:
                if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
                    ChangeSelectVehicle(selected + 1);
                else if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
                    ChangeSelectVehicle(selected - 1);
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    if (selectedList[selected] != VehicleID.None && selectedList[selected] != VehicleID.END)
                    {
                        ChangePhase(1);
                    }
                }
                break;
            case 1:
                if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
                    ChangeSelectWeapon(weaponSelected + 1);
                else if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
                    ChangeSelectWeapon(weaponSelected - 1);
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                {
                        ReserveSpawn();
                        GameMaster.GetInstance().GetSceneChanger().ChangeScene(nextScene, false);
                }
                break;
            default:
                phase = 0;
                break;
        }

    }

    public void ChangePhase(int next)
    {
        switch (next)
        {
            case 0:
                phase = next;
                vehicleSelectPanel.SetActive(true);
                weaponSelectPanel.SetActive(false);
                break;
            case 1:
                phase = next;
                vehicleSelectPanel.SetActive(false);
                weaponSelectPanel.SetActive(true);
                break;
            default:
                phase = 0;
                vehicleSelectPanel.SetActive(true);
                weaponSelectPanel.SetActive(false);
                break;
        }
    }


    public void ChangeSelectVehicle(int index)
    {
        if (index <= 0)
            index = selectedList.Count - 1;
        if (index >= selectedList.Count)
            index = 1;

        GameObject previous = current;
        if (!preLoaded.TryGetValue(selectedList[index], out current))
            current = previous;

        if (previous != null)
            previous.SetActive(false);
        if (current != null)
            current.SetActive(true);

        selected = index;

        if (selected != 0)
        {
            vehicleSelectedHighlight.gameObject.SetActive(true);
            vehicleSelectedHighlight.SetParent(vehicleSelectPod.transform.GetChild(selected - 1).transform);
            vehicleSelectedHighlight.anchoredPosition = new Vector2(vehicleSelectedHighlight.anchoredPosition.x, 0.0f);
        }
    }

    public void ChangeSelectWeapon(int index)
    {
        if (index < 0)
            index = 1;
        if (index > 1)
            index = 0;

        weaponSelected = index;

        weaponSelectedHighlight.gameObject.SetActive(true);
        weaponSelectedHighlight.SetParent(weaponSelectPod.transform.GetChild(weaponSelected).transform);
        weaponSelectedHighlight.anchoredPosition = new Vector2(weaponSelectedHighlight.anchoredPosition.x, 0.0f);
    }

    public void ReserveSpawn()
    {
        PlayerSpawnData newData;
        newData.weaponSelected = weaponSelected;
        newData.selected = selectedList[selected];
        GameMaster.GetInstance().GetFactory().ReservePlayerVehicle(newData);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    private int phase = 0;
    private int selected = 0, weaponSelected = 0;
    private GameObject current;
    private List<VehicleID> selectedList;
    private Dictionary<VehicleID, GameObject> preLoaded = new Dictionary<VehicleID, GameObject>((int)VehicleID.END);
    [SerializeField] private string nextScene;
    [SerializeField] private AircraftData[] data;

    [SerializeField] private GameObject vehicleSelectPanel;
    [SerializeField] private GameObject vehicleSelectPod;
    [SerializeField] private RectTransform vehicleSelectedHighlight;
    [SerializeField] private GameObject weaponSelectPanel;
    [SerializeField] private GameObject weaponSelectPod;
    [SerializeField] private RectTransform weaponSelectedHighlight;
}
