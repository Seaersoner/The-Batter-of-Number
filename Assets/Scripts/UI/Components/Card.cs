using UnityEngine;

/// <summary>
/// 卡牌视觉组件，挂载在卡牌预制体上
/// </summary>
public class Card : MonoBehaviour
{
    [Header("卡牌组件")]
    [SerializeField] private SpriteRenderer cardBackground;
    [SerializeField] private TextMesh cardNumber;
    [SerializeField] private TextMesh cardID;
    [SerializeField] private SpriteRenderer teamIndicator;
    
    [Header("视觉效果")]
    [SerializeField] private Color redTeamColor = Color.red;
    [SerializeField] private Color blueTeamColor = Color.blue;
    [SerializeField] private Color hiddenCardColor = new Color(0.2f, 0.2f, 0.2f);
    
    // 卡牌数据
    private NumberCard cardData;
    private NumberPlayer owner;
    private bool isRevealed = true;
    
    // 属性
    public NumberCard CardData => cardData;
    public NumberPlayer Owner => owner;
    public bool IsRevealed => isRevealed;
    
    private void Start()
    {
        InitializeCard();
    }
    
    /// <summary>
    /// 初始化卡牌
    /// </summary>
    private void InitializeCard()
    {
        // 自动获取子组件
        if (cardBackground == null)
            cardBackground = GetComponent<SpriteRenderer>();
        
        if (cardNumber == null)
            cardNumber = transform.Find("CardNumber")?.GetComponent<TextMesh>();
        
        if (cardID == null)
            cardID = transform.Find("CardID")?.GetComponent<TextMesh>();
        
        if (teamIndicator == null)
            teamIndicator = transform.Find("TeamIndicator")?.GetComponent<SpriteRenderer>();
        
        // 如果没有找到子对象，自动创建
        if (cardNumber == null)
        {
            CreateCardNumber();
        }
        
        if (cardID == null)
        {
            CreateCardID();
        }
        
        if (teamIndicator == null)
        {
            CreateTeamIndicator();
        }
    }
    
    /// <summary>
    /// 设置卡牌数据
    /// </summary>
    /// <param name="data">卡牌数据</param>
    public void SetCardData(NumberCard data)
    {
        cardData = data;
        UpdateCardVisual();
    }
    
    /// <summary>
    /// 设置卡牌拥有者
    /// </summary>
    /// <param name="player">拥有者</param>
    public void SetOwner(NumberPlayer player)
    {
        owner = player;
        UpdateCardVisual();
    }
    
    /// <summary>
    /// 设置卡牌是否揭开
    /// </summary>
    /// <param name="revealed">是否揭开</param>
    public void SetRevealed(bool revealed)
    {
        isRevealed = revealed;
        UpdateCardVisual();
    }
    
    /// <summary>
    /// 更新卡牌视觉效果
    /// </summary>
    public void UpdateCardVisual()
    {
        if (cardData == null) return;
        
        // 设置卡牌背景
        if (cardBackground != null)
        {
            if (ResourceManager.Instance != null)
            {
                Sprite cardSprite = ResourceManager.Instance.GetCardSprite(cardData.OriginalValue);
                if (cardSprite != null)
                {
                    cardBackground.sprite = cardSprite;
                }
            }
            
            // 设置颜色
            if (isRevealed)
            {
                cardBackground.color = GetTeamColor();
            }
            else
            {
                cardBackground.color = hiddenCardColor;
            }
        }
        
        // 设置数字显示
        if (cardNumber != null)
        {
            if (isRevealed)
            {
                cardNumber.text = cardData.CurrentValue.ToString();
                
                // 如果数值被修改，用不同颜色显示
                if (cardData.CurrentValue != cardData.OriginalValue)
                {
                    cardNumber.color = cardData.CurrentValue > cardData.OriginalValue ? Color.green : Color.red;
                }
                else
                {
                    cardNumber.color = Color.white;
                }
            }
            else
            {
                cardNumber.text = "?";
                cardNumber.color = Color.gray;
            }
        }
        
        // 设置ID显示
        if (cardID != null)
        {
            cardID.text = cardData.CardId;
            cardID.color = isRevealed ? Color.white : Color.gray;
        }
        
        // 设置队伍指示器
        if (teamIndicator != null)
        {
            teamIndicator.color = GetTeamColor();
        }
    }
    
    /// <summary>
    /// 获取队伍颜色
    /// </summary>
    /// <returns>队伍对应的颜色</returns>
    private Color GetTeamColor()
    {
        if (cardData == null) return Color.white;
        
        switch (cardData.Owner)
        {
            case NumberCard.PlayerTeam.Red:
                return redTeamColor;
            case NumberCard.PlayerTeam.Blue:
                return blueTeamColor;
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// 创建卡牌数字显示
    /// </summary>
    private void CreateCardNumber()
    {
        GameObject numberObj = new GameObject("CardNumber");
        numberObj.transform.SetParent(transform);
        numberObj.transform.localPosition = new Vector3(0, 0, -0.1f);
        
        cardNumber = numberObj.AddComponent<TextMesh>();
        cardNumber.text = "1";
        cardNumber.fontSize = 24;
        cardNumber.color = Color.white;
        cardNumber.anchor = TextAnchor.MiddleCenter;
        
        MeshRenderer renderer = numberObj.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 2;
    }
    
    /// <summary>
    /// 创建卡牌ID显示
    /// </summary>
    private void CreateCardID()
    {
        GameObject idObj = new GameObject("CardID");
        idObj.transform.SetParent(transform);
        idObj.transform.localPosition = new Vector3(0, 0.5f, -0.1f);
        
        cardID = idObj.AddComponent<TextMesh>();
        cardID.text = "A";
        cardID.fontSize = 16;
        cardID.color = Color.white;
        cardID.anchor = TextAnchor.MiddleCenter;
        
        MeshRenderer renderer = idObj.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 2;
    }
    
    /// <summary>
    /// 创建队伍指示器
    /// </summary>
    private void CreateTeamIndicator()
    {
        GameObject indicatorObj = new GameObject("TeamIndicator");
        indicatorObj.transform.SetParent(transform);
        indicatorObj.transform.localPosition = new Vector3(0.3f, 0.5f, -0.05f);
        indicatorObj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        
        teamIndicator = indicatorObj.AddComponent<SpriteRenderer>();
        teamIndicator.color = Color.white;
        teamIndicator.sortingOrder = 3;
        
        // 创建简单的圆形指示器
        teamIndicator.sprite = CreateCircleSprite();
    }
    
    /// <summary>
    /// 创建圆形精灵
    /// </summary>
    /// <returns>圆形精灵</returns>
    private Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
                    pixels[y * size + x] = Color.white;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    /// <summary>
    /// 播放卡牌动画
    /// </summary>
    /// <param name="animationType">动画类型</param>
    public void PlayCardAnimation(string animationType)
    {
        switch (animationType)
        {
            case "Reveal":
                // 揭开动画
                StartCoroutine(RevealAnimation());
                break;
            case "Play":
                // 出牌动画
                StartCoroutine(PlayAnimation());
                break;
            case "Destroy":
                // 销毁动画
                StartCoroutine(DestroyAnimation());
                break;
        }
    }
    
    /// <summary>
    /// 揭开动画
    /// </summary>
    private System.Collections.IEnumerator RevealAnimation()
    {
        Vector3 originalScale = transform.localScale;
        
        // 缩小
        float duration = 0.2f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 1.2f, elapsedTime / duration);
            transform.localScale = originalScale * scale;
            yield return null;
        }
        
        // 恢复
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(1.2f, 1f, elapsedTime / duration);
            transform.localScale = originalScale * scale;
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// 出牌动画
    /// </summary>
    private System.Collections.IEnumerator PlayAnimation()
    {
        Vector3 originalScale = transform.localScale;
        
        // 放大效果
        float duration = 0.3f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 1.3f, elapsedTime / duration);
            transform.localScale = originalScale * scale;
            yield return null;
        }
        
        transform.localScale = originalScale * 1.3f;
    }
    
    /// <summary>
    /// 销毁动画
    /// </summary>
    private System.Collections.IEnumerator DestroyAnimation()
    {
        // 淡出效果
        float duration = 0.5f;
        float elapsedTime = 0f;
        
        Color originalColor = cardBackground.color;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            
            if (cardBackground != null)
                cardBackground.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            if (cardNumber != null)
                cardNumber.color = new Color(cardNumber.color.r, cardNumber.color.g, cardNumber.color.b, alpha);
            
            if (cardID != null)
                cardID.color = new Color(cardID.color.r, cardID.color.g, cardID.color.b, alpha);
            
            yield return null;
        }
        
        // 移动到销毁区域
        // 这里可以添加移动到销毁区域的逻辑
    }
    
    /// <summary>
    /// 获取卡牌信息字符串
    /// </summary>
    /// <returns>卡牌信息</returns>
    public override string ToString()
    {
        if (cardData != null)
        {
            return $"Card {cardData.CardId}({cardData.CurrentValue}) - {cardData.Owner}";
        }
        return "Empty Card";
    }
}