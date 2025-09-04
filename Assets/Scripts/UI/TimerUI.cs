using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 计时器UI控制，显示回合剩余时间
/// </summary>
public class TimerUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Text timerText;
    [SerializeField] private Image timerFillImage;
    [SerializeField] private Slider timerSlider;
    
    [Header("视觉效果")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private float warningThreshold = 10f; // 警告阈值（秒）
    [SerializeField] private float dangerThreshold = 5f;   // 危险阈值（秒）
    
    [Header("动画效果")]
    [SerializeField] private bool enablePulseAnimation = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private bool enableShakeEffect = true;
    [SerializeField] private float shakeIntensity = 5f;
    
    // 计时器状态
    private float currentTime = 0f;
    private float maxTime = 0f;
    private bool isRunning = false;
    private bool isPaused = false;
    
    // 动画相关
    private Vector3 originalPosition;
    private Coroutine pulseCoroutine;
    private Coroutine shakeCoroutine;
    
    // 事件
    public System.Action OnTimerExpired;
    public System.Action<float> OnTimerWarning;
    public System.Action<float> OnTimerDanger;
    public System.Action<float> OnTimerTick;
    
    // 属性
    public float CurrentTime => currentTime;
    public float MaxTime => maxTime;
    public float RemainingTime => Mathf.Max(0f, currentTime);
    public float Progress => maxTime > 0 ? (maxTime - currentTime) / maxTime : 0f;
    public bool IsRunning => isRunning && !isPaused;
    public bool IsPaused => isPaused;
    
    private void Start()
    {
        InitializeTimer();
    }
    
    private void Update()
    {
        if (isRunning && !isPaused)
        {
            UpdateTimer();
        }
    }
    
    /// <summary>
    /// 初始化计时器
    /// </summary>
    private void InitializeTimer()
    {
        originalPosition = transform.localPosition;
        
        // 初始化UI组件
        if (timerText == null)
            timerText = GetComponentInChildren<Text>();
        
        if (timerFillImage == null)
            timerFillImage = GetComponentInChildren<Image>();
        
        if (timerSlider == null)
            timerSlider = GetComponentInChildren<Slider>();
        
        // 设置初始状态
        UpdateTimerDisplay();
        SetTimerColor(normalColor);
    }
    
    /// <summary>
    /// 开始计时器
    /// </summary>
    /// <param name="duration">计时时长（秒）</param>
    public void StartTimer(float duration)
    {
        maxTime = duration;
        currentTime = duration;
        isRunning = true;
        isPaused = false;
        
        UpdateTimerDisplay();
        SetTimerColor(normalColor);
        
        // 停止之前的动画
        StopAllAnimations();
        
        Debug.Log($"计时器开始：{duration}秒");
    }
    
    /// <summary>
    /// 停止计时器
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
        isPaused = false;
        currentTime = 0f;
        
        UpdateTimerDisplay();
        StopAllAnimations();
        
        Debug.Log("计时器停止");
    }
    
    /// <summary>
    /// 暂停计时器
    /// </summary>
    public void PauseTimer()
    {
        if (isRunning)
        {
            isPaused = true;
            Debug.Log("计时器暂停");
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
            Debug.Log("计时器恢复");
        }
    }
    
    /// <summary>
    /// 添加时间
    /// </summary>
    /// <param name="seconds">要添加的秒数</param>
    public void AddTime(float seconds)
    {
        currentTime = Mathf.Min(currentTime + seconds, maxTime);
        UpdateTimerDisplay();
        
        Debug.Log($"添加时间：{seconds}秒，当前剩余：{currentTime}秒");
    }
    
    /// <summary>
    /// 减少时间
    /// </summary>
    /// <param name="seconds">要减少的秒数</param>
    public void SubtractTime(float seconds)
    {
        currentTime = Mathf.Max(currentTime - seconds, 0f);
        UpdateTimerDisplay();
        
        if (currentTime <= 0f)
        {
            OnTimerExpired?.Invoke();
        }
        
        Debug.Log($"减少时间：{seconds}秒，当前剩余：{currentTime}秒");
    }
    
    /// <summary>
    /// 更新计时器
    /// </summary>
    private void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        
        // 检查时间状态
        CheckTimeThresholds();
        
        // 更新显示
        UpdateTimerDisplay();
        
        // 触发计时事件
        OnTimerTick?.Invoke(currentTime);
        
        // 检查是否时间到了
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            
            OnTimerExpired?.Invoke();
            
            Debug.Log("时间到！");
            
            // 播放时间到音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTimerWarningSound();
            }
        }
    }
    
    /// <summary>
    /// 检查时间阈值
    /// </summary>
    private void CheckTimeThresholds()
    {
        if (currentTime <= dangerThreshold && currentTime > 0f)
        {
            // 危险状态
            if (timerFillImage != null && timerFillImage.color != dangerColor)
            {
                SetTimerColor(dangerColor);
                OnTimerDanger?.Invoke(currentTime);
                
                if (enablePulseAnimation && pulseCoroutine == null)
                {
                    pulseCoroutine = StartCoroutine(PulseAnimation());
                }
                
                if (enableShakeEffect && shakeCoroutine == null)
                {
                    shakeCoroutine = StartCoroutine(ShakeAnimation());
                }
            }
        }
        else if (currentTime <= warningThreshold && currentTime > dangerThreshold)
        {
            // 警告状态
            if (timerFillImage != null && timerFillImage.color != warningColor)
            {
                SetTimerColor(warningColor);
                OnTimerWarning?.Invoke(currentTime);
                
                if (enablePulseAnimation && pulseCoroutine == null)
                {
                    pulseCoroutine = StartCoroutine(PulseAnimation());
                }
            }
        }
        else if (currentTime > warningThreshold)
        {
            // 正常状态
            if (timerFillImage != null && timerFillImage.color != normalColor)
            {
                SetTimerColor(normalColor);
                StopAllAnimations();
            }
        }
    }
    
    /// <summary>
    /// 更新计时器显示
    /// </summary>
    private void UpdateTimerDisplay()
    {
        // 更新文本
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
        
        // 更新填充图像
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = maxTime > 0 ? currentTime / maxTime : 0f;
        }
        
        // 更新滑动条
        if (timerSlider != null)
        {
            timerSlider.value = maxTime > 0 ? currentTime / maxTime : 0f;
        }
    }
    
    /// <summary>
    /// 设置计时器颜色
    /// </summary>
    /// <param name="color">目标颜色</param>
    private void SetTimerColor(Color color)
    {
        if (timerFillImage != null)
        {
            timerFillImage.color = color;
        }
        
        if (timerText != null)
        {
            timerText.color = color;
        }
    }
    
    /// <summary>
    /// 脉冲动画
    /// </summary>
    private IEnumerator PulseAnimation()
    {
        while (isRunning && currentTime <= warningThreshold)
        {
            float scale = 1f + 0.1f * Mathf.Sin(Time.time * pulseSpeed);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        
        transform.localScale = Vector3.one;
        pulseCoroutine = null;
    }
    
    /// <summary>
    /// 震动动画
    /// </summary>
    private IEnumerator ShakeAnimation()
    {
        while (isRunning && currentTime <= dangerThreshold)
        {
            Vector3 randomOffset = Random.insideUnitCircle * shakeIntensity;
            transform.localPosition = originalPosition + randomOffset;
            yield return new WaitForSeconds(0.05f);
        }
        
        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
    
    /// <summary>
    /// 停止所有动画
    /// </summary>
    private void StopAllAnimations()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        
        transform.localScale = Vector3.one;
        transform.localPosition = originalPosition;
    }
    
    /// <summary>
    /// 设置警告阈值
    /// </summary>
    /// <param name="warning">警告阈值</param>
    /// <param name="danger">危险阈值</param>
    public void SetThresholds(float warning, float danger)
    {
        warningThreshold = warning;
        dangerThreshold = danger;
    }
    
    /// <summary>
    /// 设置动画开关
    /// </summary>
    /// <param name="pulse">脉冲动画</param>
    /// <param name="shake">震动动画</param>
    public void SetAnimationEnabled(bool pulse, bool shake)
    {
        enablePulseAnimation = pulse;
        enableShakeEffect = shake;
        
        if (!pulse && !shake)
        {
            StopAllAnimations();
        }
    }
    
    /// <summary>
    /// 获取格式化的时间字符串
    /// </summary>
    /// <returns>格式化时间</returns>
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    
    private void OnDestroy()
    {
        StopAllAnimations();
    }
}