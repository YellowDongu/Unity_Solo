using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class LockHUD : MonoBehaviour
{
    //===========================================
    // struct/enum
    //===========================================
    private struct LockInfomation
    {
        //===========================================
        // Methods
        //===========================================
        public void GetInfomation(IFFHud hud)
        {
            if (hud == null) return;
            if (iffHUD != null)
                iffHUD.ChangeImageColor(HUDController.greenHUD);
            iffHUD = hud;
            iffHUDTransform = hud.transform as RectTransform;
            initialPosition = GetRandomPosition(iffHUDTransform, 200.0f);
        }
        public void GetInfomation(Image _lockHUD, IFFHud hud)
        {
            lockHUD = _lockHUD;
            transform = _lockHUD.transform as RectTransform;
            GetInfomation(hud);
        }

        public void Update(IFFHud hud, float lockStatus)
        {
            if (iffHUD != hud)
                GetInfomation(hud);

            if (lockStatus == 1.0f)
            {
                lockHUD.gameObject.SetActive(false);
                return;
            }
            else
                lockHUD.gameObject.SetActive(true);

            transform.anchoredPosition = Vector2.Lerp(initialPosition, iffHUDTransform.anchoredPosition, 1.0f - lockStatus);
            lockHUD.color = lockStatus <= 0.0f ? HUDController.redHUD : HUDController.greenHUD;
            iffHUD.ChangeImageColor(lockHUD.color);
        }

        public Vector2 GetRandomPosition(RectTransform transform, float areaSize)
        {
            Vector2 position = transform.anchoredPosition;
            return new Vector2(Random.Range(position.x - areaSize, position.x + areaSize), Random.Range(position.y - areaSize, position.y + areaSize));
        }

        //===========================================
        // Variable & GetSet Methods
        //===========================================
        public Image lockHUD;
        public RectTransform transform;
        public IFFHud iffHUD;
        public RectTransform iffHUDTransform;
        public Vector2 initialPosition;
    }

    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        GameObject newInstance = Instantiate(standardLockPrefab, baseCanvas.transform);
        standardLock.lockHUD = newInstance.GetComponent<Image>();
        standardLock.transform = newInstance.GetComponent<RectTransform>();
        newInstance.SetActive(false);

        specialLockPool = new ObjectPool<Image>(createFunc: () => Instantiate(specialLockPrefab, baseCanvas.transform).GetComponent<Image>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj.gameObject), collectionCheck: false, defaultCapacity: 10, maxSize: 100);
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    void Update()
    {
        if(fcsState)
        {
            int max = targets.Count;

            if (currentAlive.Count != max)
            {
                foreach (var item in currentAlive)
                    specialLockPool.Release(item.lockHUD);
                currentAlive.Clear();

                LockInfomation newStruct = new LockInfomation();
                for (int i = 0; i < max; i++)
                {
                    IFFHud hud = IFFUI.GetIFF(targets[i]);
                    newStruct.GetInfomation(specialLockPool.Get(), hud);
                    currentAlive.Add(newStruct);
                }
            }
            else
            {
                for (int i = 0; i < max; i++)
                    currentAlive[i].Update(IFFUI.GetIFF(targets[i]), lockStatus[i]);
            }


        }
        else if(targets.Count > 0)
            standardLock.Update(IFFUI.GetIFF(targets[0]), lockStatus[0]);
    }

    //===========================================
    // Methods
    //===========================================
    public void BoundPlayer(FireControlSystem fcs)
    {
        fcsState = fcs.GetSelectState();
        targets = fcs.Targets;
        lockStatus = fcs.LockStatus;
        fcs.ChangeState += ChangeState;
    }

    public void ChangeState(bool value)
    {
        fcsState = value;
        if (value)
            standardLock.lockHUD.gameObject.SetActive(false);
        else
        {
            foreach (var item in currentAlive)
                specialLockPool.Release(item.lockHUD);
            currentAlive.Clear();
        }
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    private bool fcsState = false;
    public ReadOnlyCollection<Vehicle> targets;
    public ReadOnlyCollection<float> lockStatus;

    private LockInfomation standardLock = new LockInfomation();
    private List<LockInfomation> currentAlive = new List<LockInfomation>(16);
    private ObjectPool<Image> specialLockPool;

    [SerializeField] private GameObject standardLockPrefab;
    [SerializeField] private GameObject specialLockPrefab;
    [SerializeField] private GameObject baseCanvas;
    [SerializeField] private RectTransform baseCanvasTransform;
    [SerializeField] private IFFUIController IFFUI;
}
