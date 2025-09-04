using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 性能监控系统
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    [Header("监控设置")]
    [SerializeField] private bool enableMonitoring = true;
    [SerializeField] private bool showOnScreenDisplay = false;
    [SerializeField] private float updateInterval = 1f;
    [SerializeField] private int maxLogEntries = 100;
    
    [Header("性能阈值")]
    [SerializeField] private float fpsWarningThreshold = 45f;
    [SerializeField] private float fpsErrorThreshold = 30f;
    [SerializeField] private int memoryWarningThreshold = 150; // MB
    [SerializeField] private int memoryErrorThreshold = 200;   // MB
    
    // 性能数据
    private float currentFPS;
    private float averageFPS;
    private int currentMemoryMB;
    private int peakMemoryMB;
    private float frameTime;
    
    // 统计数据
    private Queue<float> fpsHistory = new Queue<float>();
    private Queue<int> memoryHistory = new Queue<int>();
    private int frameCount;
    private float lastUpdateTime;
    
    // 警告系统
    private bool hasShownFPSWarning = false;
    private bool hasShownMemoryWarning = false;
    
    // 属性
    public float CurrentFPS => currentFPS;
    public float AverageFPS => averageFPS;
    public int CurrentMemoryMB => currentMemoryMB;
    public int PeakMemoryMB => peakMemoryMB;
    public bool IsPerformanceGood => currentFPS >= fpsWarningThreshold && currentMemoryMB <= memoryWarningThreshold;
    
    private void Start()
    {
        if (enableMonitoring)
        {
            InvokeRepeating(nameof(UpdatePerformanceData), 0f, updateInterval);
            Debug.Log("性能监控系统已启动");
        }
    }
    
    private void Update()
    {
        if (!enableMonitoring) return;
        
        // 更新帧时间
        frameTime = Time.deltaTime;
        frameCount++;
        
        // 计算实时FPS
        if (frameTime > 0)
        {
            currentFPS = 1f / frameTime;
        }
    }
    
    /// <summary>
    /// 更新性能数据
    /// </summary>
    private void UpdatePerformanceData()
    {
        // 计算平均FPS
        float timeSinceLastUpdate = Time.time - lastUpdateTime;
        if (timeSinceLastUpdate > 0)
        {
            float avgFPS = frameCount / timeSinceLastUpdate;
            
            // 更新FPS历史
            fpsHistory.Enqueue(avgFPS);
            if (fpsHistory.Count > maxLogEntries)
            {
                fpsHistory.Dequeue();
            }
            
            // 计算平均FPS
            float totalFPS = 0;
            foreach (float fps in fpsHistory)
            {
                totalFPS += fps;
            }
            averageFPS = totalFPS / fpsHistory.Count;
        }
        
        // 更新内存使用
        currentMemoryMB = (int)(System.GC.GetTotalMemory(false) / 1024 / 1024);
        if (currentMemoryMB > peakMemoryMB)
        {
            peakMemoryMB = currentMemoryMB;
        }
        
        // 更新内存历史
        memoryHistory.Enqueue(currentMemoryMB);
        if (memoryHistory.Count > maxLogEntries)
        {
            memoryHistory.Dequeue();
        }
        
        // 检查性能警告
        CheckPerformanceWarnings();
        
        // 重置计数器
        frameCount = 0;
        lastUpdateTime = Time.time;
    }
    
    /// <summary>
    /// 检查性能警告
    /// </summary>
    private void CheckPerformanceWarnings()
    {
        // FPS警告
        if (currentFPS < fpsErrorThreshold)
        {
            if (!hasShownFPSWarning)
            {
                Debug.LogError($"严重性能问题：FPS过低 ({currentFPS:F1}) - 低于阈值 {fpsErrorThreshold}");
                hasShownFPSWarning = true;
            }
        }
        else if (currentFPS < fpsWarningThreshold)
        {
            if (!hasShownFPSWarning)
            {
                Debug.LogWarning($"性能警告：FPS较低 ({currentFPS:F1}) - 低于阈值 {fpsWarningThreshold}");
                hasShownFPSWarning = true;
            }
        }
        else
        {
            hasShownFPSWarning = false;
        }
        
        // 内存警告
        if (currentMemoryMB > memoryErrorThreshold)
        {
            if (!hasShownMemoryWarning)
            {
                Debug.LogError($"严重内存问题：内存使用过高 ({currentMemoryMB}MB) - 超过阈值 {memoryErrorThreshold}MB");
                hasShownMemoryWarning = true;
            }
        }
        else if (currentMemoryMB > memoryWarningThreshold)
        {
            if (!hasShownMemoryWarning)
            {
                Debug.LogWarning($"内存警告：内存使用较高 ({currentMemoryMB}MB) - 超过阈值 {memoryWarningThreshold}MB");
                hasShownMemoryWarning = true;
            }
        }
        else
        {
            hasShownMemoryWarning = false;
        }
    }
    
    /// <summary>
    /// 强制垃圾回收
    /// </summary>
    public void ForceGarbageCollection()
    {
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();
        
        Debug.Log("已执行强制垃圾回收");
    }
    
    /// <summary>
    /// 获取性能报告
    /// </summary>
    /// <returns>性能报告字符串</returns>
    public string GetPerformanceReport()
    {
        return $"=== 性能报告 ===\n" +
               $"当前FPS: {currentFPS:F1}\n" +
               $"平均FPS: {averageFPS:F1}\n" +
               $"当前内存: {currentMemoryMB}MB\n" +
               $"峰值内存: {peakMemoryMB}MB\n" +
               $"帧时间: {frameTime * 1000:F1}ms\n" +
               $"性能状态: {(IsPerformanceGood ? "良好" : "需要优化")}";
    }
    
    /// <summary>
    /// 重置统计数据
    /// </summary>
    public void ResetStatistics()
    {
        fpsHistory.Clear();
        memoryHistory.Clear();
        peakMemoryMB = currentMemoryMB;
        frameCount = 0;
        lastUpdateTime = Time.time;
        
        Debug.Log("性能统计数据已重置");
    }
    
    /// <summary>
    /// 设置监控开关
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public void SetMonitoringEnabled(bool enabled)
    {
        enableMonitoring = enabled;
        
        if (enabled)
        {
            InvokeRepeating(nameof(UpdatePerformanceData), 0f, updateInterval);
            Debug.Log("性能监控已启用");
        }
        else
        {
            CancelInvoke(nameof(UpdatePerformanceData));
            Debug.Log("性能监控已禁用");
        }
    }
    
    // GUI显示
    private void OnGUI()
    {
        if (!enableMonitoring || !showOnScreenDisplay) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("性能监控", GUI.skin.GetStyle("label"));
        GUILayout.Label($"FPS: {currentFPS:F1} (平均: {averageFPS:F1})");
        GUILayout.Label($"内存: {currentMemoryMB}MB (峰值: {peakMemoryMB}MB)");
        GUILayout.Label($"帧时间: {frameTime * 1000:F1}ms");
        
        GUI.color = IsPerformanceGood ? Color.green : Color.red;
        GUILayout.Label($"状态: {(IsPerformanceGood ? "良好" : "需要优化")}");
        GUI.color = Color.white;
        
        if (GUILayout.Button("强制GC"))
        {
            ForceGarbageCollection();
        }
        
        if (GUILayout.Button("重置统计"))
        {
            ResetStatistics();
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    // 调试命令
    [ContextMenu("Print Performance Report")]
    private void PrintPerformanceReport()
    {
        Debug.Log(GetPerformanceReport());
    }
    
    [ContextMenu("Force Garbage Collection")]
    private void ContextMenuForceGC()
    {
        ForceGarbageCollection();
    }
    
    [ContextMenu("Reset Statistics")]
    private void ContextMenuResetStats()
    {
        ResetStatistics();
    }
}