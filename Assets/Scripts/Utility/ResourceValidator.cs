using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 资源验证器，检查和报告缺失的美术资源和音效资源
/// </summary>
public class ResourceValidator : MonoBehaviour
{
    [Header("资源检查设置")]
    [SerializeField] private bool autoCheckOnStart = true;
    [SerializeField] private bool showDetailedReport = true;
    [SerializeField] private bool autoFixMissingResources = true;
    
    [Header("必需资源列表")]
    [SerializeField] private string[] requiredSprites = {
        "Card_1", "Card_2", "Card_3", "Card_4", "Card_5",
        "Card_6", "Card_7", "Card_8", "Card_9",
        "Slot_Hand", "Slot_Modifier", "Slot_Battle", "Slot_Destroyed"
    };
    
    [SerializeField] private string[] requiredAudioClips = {
        "CardPlace", "CardDraw", "ButtonClick",
        "GameOver", "Victory", "TimerWarning"
    };
    
    [SerializeField] private string[] requiredMaterials = {
        "CardMaterial", "SlotMaterial"
    };
    
    // 检查结果
    private List<string> missingSprites = new List<string>();
    private List<string> missingAudioClips = new List<string>();
    private List<string> missingMaterials = new List<string>();
    
    private void Start()
    {
        if (autoCheckOnStart)
        {
            ValidateResources();
        }
    }
    
    /// <summary>
    /// 验证所有资源
    /// </summary>
    public void ValidateResources()
    {
        Debug.Log("=== 开始资源验证 ===");
        
        ClearResults();
        
        // 检查各类资源
        CheckSprites();
        CheckAudioClips();
        CheckMaterials();
        
        // 显示报告
        ShowValidationReport();
        
        // 自动修复缺失资源
        if (autoFixMissingResources)
        {
            FixMissingResources();
        }
        
        Debug.Log("=== 资源验证完成 ===");
    }
    
    /// <summary>
    /// 清空检查结果
    /// </summary>
    private void ClearResults()
    {
        missingSprites.Clear();
        missingAudioClips.Clear();
        missingMaterials.Clear();
    }
    
    /// <summary>
    /// 检查精灵资源
    /// </summary>
    private void CheckSprites()
    {
        Debug.Log("检查精灵资源...");
        
        foreach (string spriteName in requiredSprites)
        {
            Sprite sprite = Resources.Load<Sprite>($"Sprites/{spriteName}");
            if (sprite == null)
            {
                missingSprites.Add(spriteName);
                if (showDetailedReport)
                {
                    Debug.LogWarning($"缺失精灵: {spriteName}");
                }
            }
        }
        
        Debug.Log($"精灵检查完成: {requiredSprites.Length - missingSprites.Count}/{requiredSprites.Length} 可用");
    }
    
    /// <summary>
    /// 检查音效资源
    /// </summary>
    private void CheckAudioClips()
    {
        Debug.Log("检查音效资源...");
        
        foreach (string audioName in requiredAudioClips)
        {
            AudioClip clip = Resources.Load<AudioClip>($"Audio/{audioName}");
            if (clip == null)
            {
                missingAudioClips.Add(audioName);
                if (showDetailedReport)
                {
                    Debug.LogWarning($"缺失音效: {audioName}");
                }
            }
        }
        
        Debug.Log($"音效检查完成: {requiredAudioClips.Length - missingAudioClips.Count}/{requiredAudioClips.Length} 可用");
    }
    
    /// <summary>
    /// 检查材质资源
    /// </summary>
    private void CheckMaterials()
    {
        Debug.Log("检查材质资源...");
        
        foreach (string materialName in requiredMaterials)
        {
            Material material = Resources.Load<Material>($"Materials/{materialName}");
            if (material == null)
            {
                missingMaterials.Add(materialName);
                if (showDetailedReport)
                {
                    Debug.LogWarning($"缺失材质: {materialName}");
                }
            }
        }
        
        Debug.Log($"材质检查完成: {requiredMaterials.Length - missingMaterials.Count}/{requiredMaterials.Length} 可用");
    }
    
    /// <summary>
    /// 显示验证报告
    /// </summary>
    private void ShowValidationReport()
    {
        int totalRequired = requiredSprites.Length + requiredAudioClips.Length + requiredMaterials.Length;
        int totalMissing = missingSprites.Count + missingAudioClips.Count + missingMaterials.Count;
        int totalAvailable = totalRequired - totalMissing;
        
        Debug.Log($"=== 资源验证报告 ===");
        Debug.Log($"总计: {totalAvailable}/{totalRequired} 资源可用");
        Debug.Log($"缺失资源数量: {totalMissing}");
        
        if (totalMissing > 0)
        {
            Debug.LogWarning("存在缺失资源，建议使用ResourceManager的程序化生成功能");
        }
        else
        {
            Debug.Log("所有必需资源都已可用！");
        }
        
        // 显示详细缺失列表
        if (showDetailedReport && totalMissing > 0)
        {
            ShowMissingResourcesList();
        }
    }
    
    /// <summary>
    /// 显示缺失资源列表
    /// </summary>
    private void ShowMissingResourcesList()
    {
        if (missingSprites.Count > 0)
        {
            Debug.LogWarning($"缺失精灵 ({missingSprites.Count}): {string.Join(", ", missingSprites)}");
        }
        
        if (missingAudioClips.Count > 0)
        {
            Debug.LogWarning($"缺失音效 ({missingAudioClips.Count}): {string.Join(", ", missingAudioClips)}");
        }
        
        if (missingMaterials.Count > 0)
        {
            Debug.LogWarning($"缺失材质 ({missingMaterials.Count}): {string.Join(", ", missingMaterials)}");
        }
    }
    
    /// <summary>
    /// 修复缺失资源
    /// </summary>
    private void FixMissingResources()
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("ResourceManager未找到，无法自动修复资源");
            return;
        }
        
        int fixedCount = 0;
        
        // 启用程序化生成
        ResourceManager.Instance.SetProceduralGenerationEnabled(true);
        
        // 验证修复结果
        foreach (string spriteName in missingSprites)
        {
            if (spriteName.StartsWith("Card_") && int.TryParse(spriteName.Substring(5), out int cardNumber))
            {
                Sprite generatedSprite = ResourceManager.Instance.GetCardSprite(cardNumber);
                if (generatedSprite != null)
                {
                    fixedCount++;
                    Debug.Log($"已生成缺失的卡牌精灵: {spriteName}");
                }
            }
            else if (spriteName.StartsWith("Slot_"))
            {
                string slotType = spriteName.Substring(5).ToLower();
                Sprite generatedSprite = ResourceManager.Instance.GetSlotSprite(slotType);
                if (generatedSprite != null)
                {
                    fixedCount++;
                    Debug.Log($"已生成缺失的槽位精灵: {spriteName}");
                }
            }
        }
        
        foreach (string audioName in missingAudioClips)
        {
            AudioClip generatedClip = ResourceManager.Instance.GetAudioClip(audioName);
            if (generatedClip != null)
            {
                fixedCount++;
                Debug.Log($"已生成缺失的音效: {audioName}");
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"已通过程序化生成修复 {fixedCount} 个缺失资源");
        }
    }
    
    /// <summary>
    /// 创建Resources目录结构
    /// </summary>
    [ContextMenu("Create Resources Folders")]
    public void CreateResourcesFolders()
    {
        string[] folders = {
            "Assets/Resources",
            "Assets/Resources/Sprites",
            "Assets/Resources/Audio",
            "Assets/Resources/Materials"
        };
        
        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Debug.Log($"已创建文件夹: {folder}");
            }
        }
        
        Debug.Log("Resources目录结构创建完成");
        
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
    
    /// <summary>
    /// 生成资源放置指南
    /// </summary>
    [ContextMenu("Generate Resource Guide")]
    public void GenerateResourceGuide()
    {
        string guide = "=== 资源放置指南 ===\n\n";
        
        guide += "精灵资源 (放置在 Assets/Resources/Sprites/):\n";
        foreach (string sprite in requiredSprites)
        {
            guide += $"  - {sprite}.png\n";
        }
        
        guide += "\n音效资源 (放置在 Assets/Resources/Audio/):\n";
        foreach (string audio in requiredAudioClips)
        {
            guide += $"  - {audio}.wav/.mp3/.ogg\n";
        }
        
        guide += "\n材质资源 (放置在 Assets/Resources/Materials/):\n";
        foreach (string material in requiredMaterials)
        {
            guide += $"  - {material}.mat\n";
        }
        
        guide += "\n注意事项:\n";
        guide += "- 如果没有对应资源，系统会自动使用程序化生成的替代资源\n";
        guide += "- 可以在ResourceValidator中禁用autoFixMissingResources来查看完整的缺失列表\n";
        guide += "- 建议在发布前提供完整的美术资源以获得最佳效果\n";
        
        Debug.Log(guide);
        
        // 可选：保存指南到文件
        #if UNITY_EDITOR
        string guidePath = "Assets/RESOURCE_GUIDE.txt";
        File.WriteAllText(guidePath, guide);
        Debug.Log($"资源指南已保存到: {guidePath}");
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
    
    /// <summary>
    /// 获取验证结果摘要
    /// </summary>
    /// <returns>验证结果字符串</returns>
    public string GetValidationSummary()
    {
        int totalRequired = requiredSprites.Length + requiredAudioClips.Length + requiredMaterials.Length;
        int totalMissing = missingSprites.Count + missingAudioClips.Count + missingMaterials.Count;
        int totalAvailable = totalRequired - totalMissing;
        
        return $"资源状态: {totalAvailable}/{totalRequired} 可用 " +
               $"(缺失: {totalMissing}, 完整度: {(float)totalAvailable / totalRequired * 100:F1}%)";
    }
    
    /// <summary>
    /// 检查单个资源是否存在
    /// </summary>
    /// <param name="resourcePath">资源路径</param>
    /// <returns>是否存在</returns>
    public bool CheckSingleResource(string resourcePath)
    {
        return Resources.Load(resourcePath) != null;
    }
}