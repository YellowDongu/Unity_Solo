using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class RaderUI : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Initialize()
    {
        uiPool[0] = airPool = new ObjectPool<Image>(createFunc: () => Instantiate(airPrefab).GetComponent<Image>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj.gameObject), collectionCheck: false, defaultCapacity: 10, maxSize: 100);
        uiPool[1] = airTGTPool = new ObjectPool<Image>(createFunc: () => Instantiate(airTGTPrefab).GetComponent<Image>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj.gameObject), collectionCheck: false, defaultCapacity: 10, maxSize: 100);
        uiPool[2] = groundPool = new ObjectPool<Image>(createFunc: () => Instantiate(groundPrefab).GetComponent<Image>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj.gameObject), collectionCheck: false, defaultCapacity: 10, maxSize: 100);
        uiPool[3] = groundTGTPool = new ObjectPool<Image>(createFunc: () => Instantiate(groundTGTPrefab).GetComponent<Image>(), actionOnGet: obj => obj.gameObject.SetActive(true), actionOnRelease: obj => obj.gameObject.SetActive(false), actionOnDestroy: obj => Destroy(obj.gameObject), collectionCheck: false, defaultCapacity: 10, maxSize: 100);

        raderTransform = raderUI.gameObject.GetComponent<RectTransform>();
        expendedRaderTransform = expendedRaderUI.gameObject.GetComponent<RectTransform>();

        Vector3 leftBottom = leftBottomBoarder.transform.position;
        Vector3 rightTop = rightTopBoarder.transform.position;
        worldMinX = Mathf.Min(rightTop.x, leftBottom.x);
        worldMinZ = Mathf.Min(rightTop.z, leftBottom.z);
        worldSizeX = Mathf.Abs(rightTop.x - leftBottom.x);
        worldSizeZ = Mathf.Abs(rightTop.z - leftBottom.z);

        whole = new ReadOnlyCollection<Vehicle>[teamMax];
        for (int i = 0; i < teamMax; i++)
            whole[i] = GameMaster.GetInstance().GetFactory().GetAll(i);

        for (int i = 0; i < 4; i++)
            aliveUI[i] = new List<(Image item, int flag)>(50);

        raderState = true;
        ChangeState();
    }

    public void BoundPlayer(Aircraft _player)
    {
        player = _player;
        rader = _player.Rader();
        inRader = rader.InRangeTarget;
        running = true;

        Initialize();
        StartCoroutine(DistributedHUDUpdate());
    }

    //===========================================
    // Methods
    //===========================================
    private void OnDisable()
    {
        running = false;
    }

    private void OnDestroy()
    {
        running = false;
    }

    public IEnumerator DistributedHUDUpdate()
    {
        while (running)
        {
            Color baseColor = HUDController.greenHUD;
            Color color = HUDController.greenHUD;
            Image dispatched = null;

            if (raderState)
            {
                for (int i = 0; i < teamMax; i++)
                {
                    if (!running)
                        break;

                    foreach ((Image item, int flag) in aliveUI[i])
                    {
                        item.rectTransform.SetParent(null);
                        uiPool[flag].Release(item);
                    }
                    aliveUI[i].Clear();

                    switch (i)
                    {
                        case 0: color = baseColor = HUDController.unknown; break;
                        case 1: color = baseColor = HUDController.ally; break;
                        case 2: color = baseColor = HUDController.normal; break;
                        default: color = baseColor = HUDController.greenHUD; break;
                    }

                    foreach (Vehicle target in whole[i])
                    {
                        int flag = 0;
                        if (target.IsLand)
                            flag = 2;
                        if (target.IsTGT)
                        {
                            flag += 1;
                            color = HUDController.redHUD;
                        }

                        dispatched = uiPool[flag].Get();
                        dispatched.rectTransform.SetParent(expendedRaderTransform);
                        dispatched.rectTransform.anchoredPosition = GetRaderPosition(target.gameObject.transform.position);
                        aliveUI[i].Add((dispatched, flag));
                        color = baseColor;
                    }
                    yield return new WaitForFixedUpdate();
                }
            }
            else
            {
                Quaternion inversed = Quaternion.Euler(0.0f, -player.gameObject.transform.eulerAngles.y, 0.0f);

                foreach ((Image item, int flag) in aliveUI[0])
                {
                    item.rectTransform.SetParent(null);
                    uiPool[flag].Release(item);
                }
                aliveUI[0].Clear();

                foreach (Vehicle target in inRader)
                {
                    int flag = 0;

                    Vector2 position = GetRaderPosition(inversed * (target.gameObject.transform.position - player.gameObject.transform.position)) + localRaderOffset;
                    if (Mathf.Abs(position.x) > 200.0f || Mathf.Abs(position.y) > 200.0f)
                        continue;

                    switch (target.Team)
                    {
                        case 0: color = HUDController.unknown; break;
                        case 1: color = HUDController.ally; break;
                        case 2: color = HUDController.normal; break;
                        default: color = HUDController.greenHUD; break;
                    }

                    if (target.IsLand)
                        flag = 2;
                    if (target.IsTGT)
                    {
                        flag += 1;
                        color = HUDController.redHUD;
                    }


                    dispatched = uiPool[flag].Get();
                    dispatched.rectTransform.SetParent(raderTransform);
                    dispatched.color = color;
                    dispatched.rectTransform.anchoredPosition = position;

                    aliveUI[0].Add((dispatched, flag));
                }
                yield return new WaitForFixedUpdate();
            }
        }
    }



    public void ChangeState()
    {
        raderState = !raderState;

        for (int i = 0; i < teamMax; i++)
        {
            foreach ((Image item, int flag) in aliveUI[i])
            {
                item.rectTransform.SetParent(null);
                uiPool[flag].Release(item);
            }
            aliveUI[i].Clear();
        }

        if (raderState)
        {
            minX = worldMinX;
            minZ = worldMinZ;
            sizeX = worldSizeX;
            sizeZ = worldSizeZ;

            raderUI.SetActive(false);
            expendedRaderUI.SetActive(true);
            size = raderTransform.rect.size;
        }
        else
        {
            sizeX = localRaderSize;
            sizeZ = localRaderSize;

            minX = 0;
            minZ = 0;

            raderUI.SetActive(true);
            expendedRaderUI.SetActive(false);
            size = expendedRaderTransform.rect.size;
        }
    }

    private Vector2 GetRaderPosition(Vector3 worldPosition)
    {
        return new Vector2(size.x * (worldPosition.x - minX) / sizeX, size.y * (worldPosition.z - minZ) / sizeZ);
    }
    private Vector2 GetRaderPosition(Vector3 worldPosition, Vector2 size)
    {
        return new Vector2(size.x * (worldPosition.x - minX) / sizeX, size.y * (worldPosition.z - minZ) / sizeZ);

        //Vector2 size = raderTransform.rect.size;
        //Vector2 size = raderTransform.rect.size;

        //float minX = Mathf.Min(rightTop.x, leftBottom.x);
        //float minZ = Mathf.Min(rightTop.z, leftBottom.z);
        //worldPosition.x -= minX;
        //worldPosition.z -= minZ;

        //float x = Mathf.Abs(rightTop.x - leftBottom.x);
        //float z = Mathf.Abs(rightTop.z - leftBottom.z);

        //worldPosition.x /= x;
        //worldPosition.z /= z;

        //size.x *= worldPosition.x;
        //size.y *= worldPosition.z;

        //return size;
    }


    //===========================================
    // Variable & GetSet Methods
    //===========================================

    const int teamMax = 3;

    private float worldMinX = 0.0f;
    private float worldMinZ = 0.0f;
    private float worldSizeX = 0.0f;
    private float worldSizeZ = 0.0f;

    private float minX = 0.0f;
    private float minZ = 0.0f;
    private float sizeX = 0.0f;
    private float sizeZ = 0.0f;

    private bool raderState = false;
    private bool running = true;

    private Vector2 size;

    private RectTransform raderTransform;
    private RectTransform expendedRaderTransform;
    private Vehicle player;
    private Rader rader;
    private ReadOnlyCollection<Vehicle> inRader;


    [SerializeField] private GameObject raderUI;
    [SerializeField] private GameObject expendedRaderUI;

    [SerializeField] private float localRaderSize;
    [SerializeField] private Vector2 localRaderOffset;
    [SerializeField] private GameObject rightTopBoarder;
    [SerializeField] private GameObject leftBottomBoarder;


    public ReadOnlyCollection<Vehicle>[] whole;
    public List<(Image hud, int flag)> aliving = new List<(Image, int)>(50);
    public List<(Image item, int flag)>[] aliveUI = new List<(Image, int)>[4];

    [SerializeField] private GameObject airPrefab;
    [SerializeField] private GameObject groundPrefab;
    [SerializeField] private GameObject airTGTPrefab;
    [SerializeField] private GameObject groundTGTPrefab;

    [SerializeField] private ObjectPool<Image>[] uiPool = new ObjectPool<Image>[4];
    [SerializeField] private ObjectPool<Image> airPool;
    [SerializeField] private ObjectPool<Image> groundPool;
    [SerializeField] private ObjectPool<Image> airTGTPool;
    [SerializeField] private ObjectPool<Image> groundTGTPool;
}
