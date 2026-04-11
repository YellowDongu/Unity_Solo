using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    //===========================================
    // Methods
    //===========================================

    public void ChangeScene(string nextSceneName, bool changeToTrigger)
    {
        if (changeToTrigger)
        {
            SceneChangeTrigger = true;
            StartCoroutine(LoadSceneRoutine(nextSceneName));
        }
        else
        {
            StartCoroutine(LoadSceneAsync(nextSceneName));
        }
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
    IEnumerator LoadSceneRoutine(string nextSceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            if (operation.progress >= 0.99f)
            {
                if (SceneChangeTrigger)
                {
                    operation.allowSceneActivation = true;
                    SceneChangeTrigger = false;
                    break;
                }
            }
            yield return null;
        }
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void ActiveTrigger() { SceneChangeTrigger = true; }

    private bool SceneChangeTrigger = false;
}
