using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 场景管理器，处理场景切换和加载
/// </summary>
public class SceneManager : Singleton<SceneManager>
{
    [Header("场景名称")]
    [SerializeField] private string mainMenuSceneName = "MenuScene";
    [SerializeField] private string gameSceneName = "MainScene";
    
    [Header("加载设置")]
    [SerializeField] private bool showLoadingScreen = true;
    [SerializeField] private float minimumLoadingTime = 1f;
    
    // 事件
    public System.Action<string> OnSceneLoadStarted;
    public System.Action<string> OnSceneLoadCompleted;
    public System.Action<float> OnSceneLoadProgress;
    
    // 当前场景信息
    public string CurrentSceneName => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    
    private void Start()
    {
        // 确保场景管理器在场景切换时不被销毁
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// 加载主菜单场景
    /// </summary>
    public void LoadMainMenuScene()
    {
        LoadScene(mainMenuSceneName);
    }
    
    /// <summary>
    /// 加载游戏场景
    /// </summary>
    public void LoadGameScene()
    {
        LoadScene(gameSceneName);
    }
    
    /// <summary>
    /// 加载指定场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("场景名称不能为空！");
            return;
        }
        
        if (showLoadingScreen)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
    
    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        OnSceneLoadStarted?.Invoke(sceneName);
        
        // 显示加载界面
        if (UIManager.Instance != null)
        {
            // 这里可以显示加载界面
            // UIManager.Instance.ShowLoadingScreen();
        }
        
        float startTime = Time.time;
        
        // 开始异步加载
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        // 等待加载完成
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            OnSceneLoadProgress?.Invoke(progress);
            
            // 当加载进度达到90%时，检查是否满足最小加载时间
            if (asyncLoad.progress >= 0.9f)
            {
                float elapsedTime = Time.time - startTime;
                if (elapsedTime >= minimumLoadingTime)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }
            
            yield return null;
        }
        
        // 隐藏加载界面
        if (UIManager.Instance != null)
        {
            // UIManager.Instance.HideLoadingScreen();
        }
        
        OnSceneLoadCompleted?.Invoke(sceneName);
        
        Debug.Log($"场景 '{sceneName}' 加载完成");
    }
    
    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentScene = CurrentSceneName;
        LoadScene(currentScene);
    }
    
    /// <summary>
    /// 检查场景是否存在
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    /// <returns>场景是否存在</returns>
    public bool SceneExists(string sceneName)
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameFromPath == sceneName)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取所有场景名称
    /// </summary>
    /// <returns>场景名称数组</returns>
    public string[] GetAllSceneNames()
    {
        string[] sceneNames = new string[UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings];
        
        for (int i = 0; i < sceneNames.Length; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
        
        return sceneNames;
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Unity事件
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"场景 '{scene.name}' 已加载");
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"场景 '{scene.name}' 已卸载");
    }
}