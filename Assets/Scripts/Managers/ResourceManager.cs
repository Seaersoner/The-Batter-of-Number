using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 资源管理器，处理美术资源和音效资源的加载与补足
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    [Header("默认资源")]
    [SerializeField] private Sprite defaultCardSprite;
    [SerializeField] private Sprite defaultSlotSprite;
    [SerializeField] private AudioClip defaultSoundEffect;
    [SerializeField] private Material defaultCardMaterial;
    
    [Header("程序化生成设置")]
    [SerializeField] private bool enableProceduralGeneration = true;
    [SerializeField] private Color[] defaultCardColors = {
        Color.red, Color.blue, Color.green, Color.yellow, 
        Color.magenta, Color.cyan, Color.white, Color.gray, Color.black
    };
    
    // 资源缓存
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    
    // 程序化生成的资源
    private Sprite[] generatedCardSprites;
    private AudioClip[] generatedSoundEffects;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeResourceManager();
    }
    
    /// <summary>
    /// 初始化资源管理器
    /// </summary>
    private void InitializeResourceManager()
    {
        // 生成默认资源
        GenerateDefaultResources();
        
        // 预加载常用资源
        PreloadCommonResources();
        
        Debug.Log("资源管理器初始化完成");
    }
    
    /// <summary>
    /// 生成默认资源
    /// </summary>
    private void GenerateDefaultResources()
    {
        if (enableProceduralGeneration)
        {
            GenerateCardSprites();
            GenerateSlotSprites();
            GenerateSoundEffects();
        }
    }
    
    /// <summary>
    /// 程序化生成卡牌精灵
    /// </summary>
    private void GenerateCardSprites()
    {
        generatedCardSprites = new Sprite[9];
        
        for (int i = 0; i < 9; i++)
        {
            // 创建卡牌纹理
            Texture2D cardTexture = CreateCardTexture(100, 140, defaultCardColors[i], i + 1);
            
            // 创建精灵
            Sprite cardSprite = Sprite.Create(
                cardTexture,
                new Rect(0, 0, cardTexture.width, cardTexture.height),
                new Vector2(0.5f, 0.5f)
            );
            
            cardSprite.name = $"Generated_Card_{i + 1}";
            generatedCardSprites[i] = cardSprite;
            
            // 缓存精灵
            spriteCache[$"card_{i + 1}"] = cardSprite;
        }
        
        Debug.Log("已生成9张程序化卡牌精灵");
    }
    
    /// <summary>
    /// 创建卡牌纹理
    /// </summary>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="baseColor">基础颜色</param>
    /// <param name="number">卡牌数字</param>
    /// <returns>生成的纹理</returns>
    private Texture2D CreateCardTexture(int width, int height, Color baseColor, int number)
    {
        Texture2D texture = new Texture2D(width, height);
        
        // 填充背景
        Color[] pixels = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                
                // 创建渐变效果
                float gradientFactor = 1f - (float)y / height * 0.3f;
                Color pixelColor = baseColor * gradientFactor;
                
                // 添加边框
                if (x < 3 || x >= width - 3 || y < 3 || y >= height - 3)
                {
                    pixelColor = Color.black;
                }
                // 添加内边框
                else if (x < 6 || x >= width - 6 || y < 6 || y >= height - 6)
                {
                    pixelColor = Color.white;
                }
                
                pixels[index] = pixelColor;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply(false, true); // 修复：标记为不可读以节省内存
        
        // 缓存纹理
        textureCache[$"card_texture_{number}"] = texture;
        
        return texture;
    }
    
    /// <summary>
    /// 生成槽位精灵
    /// </summary>
    private void GenerateSlotSprites()
    {
        // 生成不同类型的槽位
        string[] slotTypes = { "hand", "modifier", "battle", "destroyed" };
        Color[] slotColors = { 
            new Color(0.8f, 0.8f, 0.8f, 0.5f), // 手牌槽
            new Color(0.6f, 1f, 0.6f, 0.5f),   // 加减牌槽
            new Color(1f, 0.6f, 0.6f, 0.5f),   // 比牌槽
            new Color(0.5f, 0.5f, 0.5f, 0.5f)  // 销毁槽
        };
        
        for (int i = 0; i < slotTypes.Length; i++)
        {
            Texture2D slotTexture = CreateSlotTexture(100, 140, slotColors[i]);
            Sprite slotSprite = Sprite.Create(
                slotTexture,
                new Rect(0, 0, slotTexture.width, slotTexture.height),
                new Vector2(0.5f, 0.5f)
            );
            
            slotSprite.name = $"Generated_Slot_{slotTypes[i]}";
            spriteCache[$"slot_{slotTypes[i]}"] = slotSprite;
        }
        
        Debug.Log("已生成4种程序化槽位精灵");
    }
    
    /// <summary>
    /// 创建槽位纹理
    /// </summary>
    private Texture2D CreateSlotTexture(int width, int height, Color baseColor)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                
                // 创建虚线边框效果
                bool isBorder = (x % 10 < 5 && (y < 2 || y >= height - 2)) || 
                               (y % 10 < 5 && (x < 2 || x >= width - 2));
                
                pixels[index] = isBorder ? Color.white : baseColor;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply(false, true); // 修复：标记为不可读以节省内存
        
        return texture;
    }
    
    /// <summary>
    /// 程序化生成音效
    /// </summary>
    private void GenerateSoundEffects()
    {
        // 为不同音效生成简单的程序化声音
        string[] soundNames = { 
            "CardPlace", "CardDraw", "ButtonClick", 
            "GameOver", "Victory", "TimerWarning" 
        };
        
        foreach (string soundName in soundNames)
        {
            AudioClip generatedClip = GenerateSimpleBeep(soundName);
            if (generatedClip != null)
            {
                audioCache[soundName] = generatedClip;
            }
        }
        
        Debug.Log("已生成6种程序化音效");
    }
    
    /// <summary>
    /// 生成简单的提示音
    /// </summary>
    private AudioClip GenerateSimpleBeep(string soundType)
    {
        int sampleRate = 44100;
        float duration = 0.2f;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] audioData = new float[samples];
        
        // 根据音效类型设置不同的频率
        float frequency = soundType switch
        {
            "CardPlace" => 440f,    // A4音
            "CardDraw" => 523f,     // C5音
            "ButtonClick" => 880f,  // A5音
            "GameOver" => 220f,     // A3音
            "Victory" => 659f,      // E5音
            "TimerWarning" => 330f, // E4音
            _ => 440f
        };
        
        for (int i = 0; i < samples; i++)
        {
            float time = (float)i / sampleRate;
            float wave = Mathf.Sin(2 * Mathf.PI * frequency * time);
            
            // 添加淡入淡出效果
            float envelope = 1f;
            if (time < 0.05f) envelope = time / 0.05f;
            else if (time > duration - 0.05f) envelope = (duration - time) / 0.05f;
            
            audioData[i] = wave * envelope * 0.3f; // 降低音量
        }
        
        AudioClip clip = AudioClip.Create($"Generated_{soundType}", samples, 1, sampleRate, false);
        clip.SetData(audioData, 0);
        
        return clip;
    }
    
    /// <summary>
    /// 预加载常用资源
    /// </summary>
    private void PreloadCommonResources()
    {
        // 尝试从Resources文件夹加载资源
        LoadResourcesFromFolder();
    }
    
    /// <summary>
    /// 从Resources文件夹加载资源
    /// </summary>
    private void LoadResourcesFromFolder()
    {
        // 加载精灵资源
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites");
        foreach (Sprite sprite in sprites)
        {
            if (!spriteCache.ContainsKey(sprite.name))
            {
                spriteCache[sprite.name] = sprite;
            }
        }
        
        // 加载音效资源
        AudioClip[] audioClips = Resources.LoadAll<AudioClip>("Audio");
        foreach (AudioClip clip in audioClips)
        {
            if (!audioCache.ContainsKey(clip.name))
            {
                audioCache[clip.name] = clip;
            }
        }
        
        Debug.Log($"从Resources加载了 {sprites.Length} 个精灵和 {audioClips.Length} 个音效");
    }
    
    /// <summary>
    /// 获取卡牌精灵
    /// </summary>
    /// <param name="cardNumber">卡牌数字(1-9)</param>
    /// <returns>卡牌精灵</returns>
    public Sprite GetCardSprite(int cardNumber)
    {
        string key = $"card_{cardNumber}";
        
        // 优先使用缓存的资源
        if (spriteCache.ContainsKey(key))
        {
            return spriteCache[key];
        }
        
        // 尝试从Resources加载
        Sprite loadedSprite = Resources.Load<Sprite>($"Sprites/Card_{cardNumber}");
        if (loadedSprite != null)
        {
            spriteCache[key] = loadedSprite;
            return loadedSprite;
        }
        
        // 使用程序化生成的精灵
        if (generatedCardSprites != null && cardNumber >= 1 && cardNumber <= 9)
        {
            return generatedCardSprites[cardNumber - 1];
        }
        
        // 最后使用默认精灵
        return defaultCardSprite;
    }
    
    /// <summary>
    /// 获取槽位精灵
    /// </summary>
    /// <param name="slotType">槽位类型</param>
    /// <returns>槽位精灵</returns>
    public Sprite GetSlotSprite(string slotType)
    {
        string key = $"slot_{slotType}";
        
        if (spriteCache.ContainsKey(key))
        {
            return spriteCache[key];
        }
        
        // 尝试从Resources加载
        Sprite loadedSprite = Resources.Load<Sprite>($"Sprites/Slot_{slotType}");
        if (loadedSprite != null)
        {
            spriteCache[key] = loadedSprite;
            return loadedSprite;
        }
        
        // 使用默认精灵
        return defaultSlotSprite;
    }
    
    /// <summary>
    /// 获取音效
    /// </summary>
    /// <param name="soundName">音效名称</param>
    /// <returns>音效片段</returns>
    public AudioClip GetAudioClip(string soundName)
    {
        // 优先使用缓存的资源
        if (audioCache.ContainsKey(soundName))
        {
            return audioCache[soundName];
        }
        
        // 尝试从Resources加载
        AudioClip loadedClip = Resources.Load<AudioClip>($"Audio/{soundName}");
        if (loadedClip != null)
        {
            audioCache[soundName] = loadedClip;
            return loadedClip;
        }
        
        // 使用默认音效
        return defaultSoundEffect;
    }
    
    /// <summary>
    /// 获取材质
    /// </summary>
    /// <param name="materialName">材质名称</param>
    /// <returns>材质</returns>
    public Material GetMaterial(string materialName)
    {
        Material loadedMaterial = Resources.Load<Material>($"Materials/{materialName}");
        return loadedMaterial != null ? loadedMaterial : defaultCardMaterial;
    }
    
    /// <summary>
    /// 检查资源是否存在
    /// </summary>
    /// <param name="resourcePath">资源路径</param>
    /// <returns>是否存在</returns>
    public bool ResourceExists(string resourcePath)
    {
        return Resources.Load(resourcePath) != null;
    }
    
    /// <summary>
    /// 清空资源缓存
    /// </summary>
    public void ClearCache()
    {
        spriteCache.Clear();
        audioCache.Clear();
        textureCache.Clear();
        
        Debug.Log("资源缓存已清空");
    }
    
    /// <summary>
    /// 获取资源统计信息
    /// </summary>
    /// <returns>资源统计字符串</returns>
    public string GetResourceStats()
    {
        return $"资源统计:\n" +
               $"- 精灵缓存: {spriteCache.Count}\n" +
               $"- 音效缓存: {audioCache.Count}\n" +
               $"- 纹理缓存: {textureCache.Count}\n" +
               $"- 程序化生成: {(enableProceduralGeneration ? "启用" : "禁用")}";
    }
    
    /// <summary>
    /// 设置程序化生成开关
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public void SetProceduralGenerationEnabled(bool enabled)
    {
        enableProceduralGeneration = enabled;
        
        if (enabled)
        {
            GenerateDefaultResources();
        }
        
        Debug.Log($"程序化生成已{(enabled ? "启用" : "禁用")}");
    }
    
    private void OnDestroy()
    {
        ClearCache();
    }
}