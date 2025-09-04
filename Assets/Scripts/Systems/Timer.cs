using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 通用计时器组件，可用于各种计时需求
/// </summary>
public class Timer : MonoBehaviour
{
    [Header("计时器设置")]
    [SerializeField] private float duration = 10f;
    [SerializeField] private bool startOnAwake = false;
    [SerializeField] private bool loop = false;
    [SerializeField] private bool unscaledTime = false;
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    
    // 计时器状态
    private float currentTime;
    private bool isRunning;
    private bool isPaused;
    private Coroutine timerCoroutine;
    
    // 事件
    public Action OnTimerStart;
    public Action OnTimerComplete;
    public Action OnTimerPause;
    public Action OnTimerResume;
    public Action OnTimerStop;
    public Action<float> OnTimerTick; // 参数：剩余时间
    public Action<float> OnTimerProgress; // 参数：进度百分比 (0-1)
    
    // 属性
    public float Duration
    {
        get => duration;
        set => duration = Mathf.Max(0f, value);
    }
    
    public float CurrentTime => currentTime;
    public float RemainingTime => Mathf.Max(0f, currentTime);
    public float ElapsedTime => duration - currentTime;
    public float Progress => duration > 0 ? (duration - currentTime) / duration : 0f;
    public bool IsRunning => isRunning && !isPaused;
    public bool IsPaused => isPaused;
    public bool IsComplete => currentTime <= 0f && !isRunning;
    public bool Loop
    {
        get => loop;
        set => loop = value;
    }
    
    private void Awake()
    {
        currentTime = duration;
        
        if (startOnAwake)
        {
            StartTimer();
        }
    }
    
    private void OnDestroy()
    {
        StopTimer();
    }
    
    /// <summary>
    /// 开始计时器
    /// </summary>
    public void StartTimer()
    {
        StartTimer(duration);
    }
    
    /// <summary>
    /// 开始计时器并设置持续时间
    /// </summary>
    /// <param name="newDuration">新的持续时间</param>
    public void StartTimer(float newDuration)
    {
        duration = Mathf.Max(0f, newDuration);
        currentTime = duration;
        isRunning = true;
        isPaused = false;
        
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        
        timerCoroutine = StartCoroutine(TimerCoroutine());
        
        OnTimerStart?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log($"Timer started: {duration}s");
        }
    }
    
    /// <summary>
    /// 暂停计时器
    /// </summary>
    public void PauseTimer()
    {
        if (isRunning && !isPaused)
        {
            isPaused = true;
            OnTimerPause?.Invoke();
            
            if (showDebugInfo)
            {
                Debug.Log($"Timer paused at: {currentTime}s");
            }
        }
    }
    
    /// <summary>
    /// 恢复计时器
    /// </summary>
    public void ResumeTimer()
    {
        if (isRunning && isPaused)
        {
            isPaused = false;
            OnTimerResume?.Invoke();
            
            if (showDebugInfo)
            {
                Debug.Log($"Timer resumed at: {currentTime}s");
            }
        }
    }
    
    /// <summary>
    /// 停止计时器
    /// </summary>
    public void StopTimer()
    {
        if (isRunning)
        {
            isRunning = false;
            isPaused = false;
            
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
            
            OnTimerStop?.Invoke();
            
            if (showDebugInfo)
            {
                Debug.Log("Timer stopped");
            }
        }
    }
    
    /// <summary>
    /// 重置计时器
    /// </summary>
    public void ResetTimer()
    {
        StopTimer();
        currentTime = duration;
        
        if (showDebugInfo)
        {
            Debug.Log("Timer reset");
        }
    }
    
    /// <summary>
    /// 重启计时器
    /// </summary>
    public void RestartTimer()
    {
        ResetTimer();
        StartTimer();
    }
    
    /// <summary>
    /// 添加时间
    /// </summary>
    /// <param name="timeToAdd">要添加的时间</param>
    public void AddTime(float timeToAdd)
    {
        currentTime += timeToAdd;
        
        if (showDebugInfo)
        {
            Debug.Log($"Added {timeToAdd}s to timer. Current: {currentTime}s");
        }
    }
    
    /// <summary>
    /// 减少时间
    /// </summary>
    /// <param name="timeToSubtract">要减少的时间</param>
    public void SubtractTime(float timeToSubtract)
    {
        currentTime = Mathf.Max(0f, currentTime - timeToSubtract);
        
        if (showDebugInfo)
        {
            Debug.Log($"Subtracted {timeToSubtract}s from timer. Current: {currentTime}s");
        }
    }
    
    /// <summary>
    /// 设置剩余时间
    /// </summary>
    /// <param name="time">剩余时间</param>
    public void SetRemainingTime(float time)
    {
        currentTime = Mathf.Max(0f, time);
        
        if (showDebugInfo)
        {
            Debug.Log($"Set remaining time to: {currentTime}s");
        }
    }
    
    /// <summary>
    /// 计时器协程
    /// </summary>
    private IEnumerator TimerCoroutine()
    {
        while (currentTime > 0f)
        {
            if (!isPaused)
            {
                float deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                currentTime -= deltaTime;
                
                // 确保时间不会小于0
                currentTime = Mathf.Max(0f, currentTime);
                
                // 触发事件
                OnTimerTick?.Invoke(currentTime);
                OnTimerProgress?.Invoke(Progress);
            }
            
            yield return null;
        }
        
        // 计时器完成
        isRunning = false;
        OnTimerComplete?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log("Timer completed");
        }
        
        // 如果是循环计时器，重新开始
        if (loop)
        {
            StartTimer();
        }
    }
    
    /// <summary>
    /// 获取格式化的时间字符串（分:秒）
    /// </summary>
    public string GetFormattedTime()
    {
        return GetFormattedTime(currentTime);
    }
    
    /// <summary>
    /// 获取格式化的时间字符串（分:秒）
    /// </summary>
    /// <param name="time">时间（秒）</param>
    public static string GetFormattedTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    
    /// <summary>
    /// 获取格式化的时间字符串（时:分:秒）
    /// </summary>
    public string GetFormattedTimeHMS()
    {
        return GetFormattedTimeHMS(currentTime);
    }
    
    /// <summary>
    /// 获取格式化的时间字符串（时:分:秒）
    /// </summary>
    /// <param name="time">时间（秒）</param>
    public static string GetFormattedTimeHMS(float time)
    {
        int hours = Mathf.FloorToInt(time / 3600f);
        int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }
    
    /// <summary>
    /// 创建一个一次性计时器
    /// </summary>
    /// <param name="duration">持续时间</param>
    /// <param name="onComplete">完成回调</param>
    /// <param name="parent">父对象（可选）</param>
    /// <returns>计时器组件</returns>
    public static Timer CreateTimer(float duration, Action onComplete = null, Transform parent = null)
    {
        GameObject timerObj = new GameObject("Timer");
        
        if (parent != null)
        {
            timerObj.transform.SetParent(parent);
        }
        
        Timer timer = timerObj.AddComponent<Timer>();
        timer.Duration = duration;
        
        if (onComplete != null)
        {
            timer.OnTimerComplete += onComplete;
            timer.OnTimerComplete += () => Destroy(timerObj); // 自动销毁
        }
        
        timer.StartTimer();
        return timer;
    }
    
    /// <summary>
    /// 创建一个延迟执行器
    /// </summary>
    /// <param name="delay">延迟时间</param>
    /// <param name="action">要执行的动作</param>
    /// <param name="parent">父对象（可选）</param>
    /// <returns>计时器组件</returns>
    public static Timer Delay(float delay, Action action, Transform parent = null)
    {
        return CreateTimer(delay, action, parent);
    }
    
    /// <summary>
    /// 创建一个循环计时器
    /// </summary>
    /// <param name="interval">间隔时间</param>
    /// <param name="action">要执行的动作</param>
    /// <param name="parent">父对象（可选）</param>
    /// <returns>计时器组件</returns>
    public static Timer Repeat(float interval, Action action, Transform parent = null)
    {
        GameObject timerObj = new GameObject("RepeatTimer");
        
        if (parent != null)
        {
            timerObj.transform.SetParent(parent);
        }
        
        Timer timer = timerObj.AddComponent<Timer>();
        timer.Duration = interval;
        timer.Loop = true;
        
        if (action != null)
        {
            timer.OnTimerComplete += action;
        }
        
        timer.StartTimer();
        return timer;
    }
    
    /// <summary>
    /// 停止并销毁计时器
    /// </summary>
    public void DestroyTimer()
    {
        StopTimer();
        Destroy(gameObject);
    }
    
    // Unity Inspector 调试按钮
    [ContextMenu("Start Timer")]
    private void StartTimerDebug()
    {
        StartTimer();
    }
    
    [ContextMenu("Pause Timer")]
    private void PauseTimerDebug()
    {
        PauseTimer();
    }
    
    [ContextMenu("Resume Timer")]
    private void ResumeTimerDebug()
    {
        ResumeTimer();
    }
    
    [ContextMenu("Stop Timer")]
    private void StopTimerDebug()
    {
        StopTimer();
    }
    
    [ContextMenu("Reset Timer")]
    private void ResetTimerDebug()
    {
        ResetTimer();
    }
}