using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

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

            if(!hud.gameObject.activeInHierarchy)
            {
                lockHUD.gameObject.SetActive(false);
                return;
            }
            else
                lockHUD.gameObject.SetActive(true);

            if (iffHUD != null)
                iffHUD.ChangeImageColor(HUDController.greenHUD);
            iffHUD = hud;
            iffHUDTransform = hud.transform as RectTransform;
            initialPosition = GetRandomPosition(iffHUDTransform, 200.0f);
        }
        public void GetInfomation(Image _lockHUD, IFFHud hud)
        {
            lockHUD = _lockHUD;
            lockHUD.gameObject.SetActive(true);
            transform = _lockHUD.transform as RectTransform;
            GetInfomation(hud);
        }

        public bool Update(IFFHud hud, float lockStatus)
        {
            if (hud == null)
            {
                lockHUD.gameObject.SetActive(false);
                return true;
            }

            if (iffHUD != hud)
                GetInfomation(hud);

            if (!hud.gameObject.activeInHierarchy || lockStatus == 1.0f)
            {
                lockHUD.gameObject.SetActive(false);
                return true;
            }
            else
                lockHUD.gameObject.SetActive(true);

            if(lockStatus <= 0.0f)
            {
                transform.anchoredPosition = iffHUDTransform.anchoredPosition;
                iffHUD.ChangeImageColor(lockHUD.color = HUDController.redHUD);
            }
            else
            {
                transform.anchoredPosition = Vector2.Lerp(initialPosition, iffHUDTransform.anchoredPosition, 1.0f - lockStatus);
                iffHUD.ChangeImageColor(lockHUD.color = HUDController.greenHUD);
            }

            return false;
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

        standardLocksound = GameMaster.GetInstance().Sound().GetSound("StandardLock");
        //specialLocksound = GameMaster.GetInstance().Sound().GetSound("");
        source.clip = standardLocksound;
        specialLockPool = new ObjectPool<Image>(createFunc: () => Instantiate(specialLockPrefab, baseCanvas.transform).GetComponent<Image>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj.gameObject), collectionCheck: false, defaultCapacity: 10, maxSize: 100);
    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    void Update()
    {
        float time = 1.0f;
        if(fcsState)
        {
            int max = targets.Count;
            for (int i = 0; i < max; i++)
            {
                time = Mathf.Min(time, lockStatus[i]);
                currentAlive[i].Update(IFFUI.GetIFF(targets[i]), lockStatus[i]);
            }
        }
        else
        {
            if (targets.Count > 0)
            {
                time = lockStatus[0];
                standardLock.Update(IFFUI.GetIFF(targets[0]), lockStatus[0]);
            }
            else
                standardLock.lockHUD.gameObject.SetActive(false);
        }
        LockSound(time);
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
        {
            standardLock.lockHUD.gameObject.SetActive(false);
            int max = targets.Count;
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
            foreach (var item in currentAlive)
                specialLockPool.Release(item.lockHUD);
            currentAlive.Clear();
            standardLock.lockHUD.gameObject.SetActive(true);
        }
    }

    public void BeforeTargetChanged()
    {
        foreach (var item in currentAlive)
            specialLockPool.Release(item.lockHUD);
        currentAlive.Clear();
    }

    public void TargetChanged()
    {
        if (!fcsState)
        {
            standardLock.lockHUD.gameObject.SetActive(targets.Count > 0);
        }
        else
        {
            int max = targets.Count;
            LockInfomation newStruct = new LockInfomation();
            for (int i = 0; i < max; i++)
            {
                IFFHud hud = IFFUI.GetIFF(targets[i]);
                newStruct.GetInfomation(specialLockPool.Get(), hud);
                currentAlive.Add(newStruct);
            }
        }
    }

    public void LockSound(float minimum)
    {
        if (minimum >= 1.0f)
        {
            if (source.isPlaying)
                source.Stop();
            return;
        }

        if (minimum > 0.0f)
        {
            soundTime -= Time.deltaTime;

            if (soundTime <= 0.0f)
            {
                source.loop = false;
                soundTime = minimum * 0.5f;
                sound = !sound;
                if (sound)
                    source.Play();
                else
                    source.Stop();
            }
        }
        else
        {
            if (!source.loop)
            {
                soundTime = 0.0f;
                sound = false;

                source.loop = true;
                source.Play();
            }
        }
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    private bool fcsState = false;
    private bool sound = false;
    private float soundTime = 0.0f;
    private AudioClip standardLocksound;
    private AudioClip specialLocksound;

    private LockInfomation standardLock = new LockInfomation();
    private List<LockInfomation> currentAlive = new List<LockInfomation>(16);
    private ObjectPool<Image> specialLockPool;

    [SerializeField] private GameObject standardLockPrefab;
    [SerializeField] private GameObject specialLockPrefab;
    [SerializeField] private GameObject baseCanvas;
    [SerializeField] private RectTransform baseCanvasTransform;
    [SerializeField] private IFFUIController IFFUI;
    [SerializeField] private AudioSource source;

    private ReadOnlyCollection<Vehicle> targets;
    private ReadOnlyCollection<float> lockStatus;
}
