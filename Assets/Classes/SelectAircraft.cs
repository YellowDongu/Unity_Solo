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
    [System.Serializable]
    public struct AircraftData
    {
        public VehicleID id;
        public Vector3 hangerPosition;
    }

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
    }

    private void Update()
    {
        Selected();
    }

    private void Selected()
    {
        switch (phase)
        {
            case 0:
                if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
                    ChangeSelect(selected + 1);
                else if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
                    ChangeSelect(selected - 1);
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    if (selectedList[selected] != VehicleID.None && selectedList[selected] != VehicleID.END)
                    {
                        ReserveSpawn();
                        GameMaster.GetInstance().GetSceneChanger().ChangeScene(nextScene, false);
                    }
                }
                break;
            case 1:
                if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
                    ChangeSelect(selected + 1);
                else if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
                    ChangeSelect(selected - 1);
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    if (selectedList[selected] != VehicleID.None && selectedList[selected] != VehicleID.END)
                    {
                        ReserveSpawn();
                        GameMaster.GetInstance().GetSceneChanger().ChangeScene(nextScene, false);
                    }
                }
                break;
            default:
                phase = 0;
                break;
        }

    }

    public void ChangeSelect(int index)
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
    }

    public void ReserveSpawn()
    {
        PlayerSpawnData newData;
        newData.weaponSelected = weaponSelected;
        newData.selected = selectedList[selected];
        GameMaster.GetInstance().GetFactory().ReservePlayerVehicle(newData);
    }

    private int phase = 0;
    private int weaponSelected = 0;
    private int selected = 0;
    private GameObject current;
    private List<VehicleID> selectedList;
    private Dictionary<VehicleID, GameObject> preLoaded = new Dictionary<VehicleID, GameObject>((int)VehicleID.END);
    [SerializeField] private string nextScene;
    [SerializeField] private AircraftData[] data;
}
