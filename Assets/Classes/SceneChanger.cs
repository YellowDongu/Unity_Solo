using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    private void Awake()
    {
        if(fadeOutPrefab != null)
        {
            GameObject newInstnace = Instantiate(fadeOutPrefab);
            fadeOut = newInstnace.GetComponent<Image>();
        }

    }


    //===========================================
    // Methods
    //===========================================

    public void ChangeScene(string nextSceneName, bool changeToTrigger)
    {
        if (changing)
            return;
        SceneChangeTrigger = !changeToTrigger;
        StartCoroutine(LoadSceneRoutine(nextSceneName));
    }

    private IEnumerator LoadSceneAsync(string nextSceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
        //float progress = 0.0f;
        while (!operation.isDone)
        {
            //progress = Mathf.Clamp01(operation.progress / 0.9f);
            //Debug.Log($"·Îµù Áß... {progress * 100}%");

            yield return null;
        }
    }

    private IEnumerator LoadSceneRoutine(string nextSceneName)
    {
        changing = true;
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
        operation.allowSceneActivation = false;
        while (!operation.isDone)
        {
            if (operation.progress >= 0.9f)
            {
                if (SceneChangeTrigger)
                    operation.allowSceneActivation = true;
            }
            yield return null;
        }
        SceneChangeTrigger = false;
        changing = false;
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void ActiveTrigger() { SceneChangeTrigger = true; }

    private bool SceneChangeTrigger = false;
    private bool changing = false;

    private Image fadeOut;
    [SerializeField] private GameObject fadeOutPrefab;
}
