using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 游戏棋盘布局管理器，处理1x9横向布局
/// </summary>
public class GameBoardLayout : MonoBehaviour
{
    [Header("布局设置")]
    [SerializeField] private float cardSpacing = 120f;          // 卡牌间距
    [SerializeField] private float rowSpacing = 150f;           // 行间距
    [SerializeField] private Vector2 cardSize = new Vector2(100f, 140f); // 卡牌尺寸
    
    [Header("区域设置")]
    [SerializeField] private Transform humanHandArea;           // 人类手牌区域
    [SerializeField] private Transform humanModifierArea;       // 人类加减牌区域
    [SerializeField] private Transform battleArea;              // 比牌区域
    [SerializeField] private Transform aiModifierArea;          // AI加减牌区域
    [SerializeField] private Transform aiHandArea;              // AI手牌区域
    [SerializeField] private Transform destroyedArea;           // 销毁区域
    
    [Header("卡牌预制体")]
    [SerializeField] private GameObject numberCardPrefab;       // 数字卡牌预制体
    [SerializeField] private GameObject modifierCardPrefab;     // 加减牌预制体
    [SerializeField] private GameObject cardSlotPrefab;         // 卡牌槽预制体
    
    // 卡牌槽数组（A-I，共9个位置）
    private CardSlot[] humanHandSlots = new CardSlot[9];
    private CardSlot[] humanModifierSlots = new CardSlot[9];
    private CardSlot[] battleSlots = new CardSlot[9];
    private CardSlot[] aiModifierSlots = new CardSlot[9];
    private CardSlot[] aiHandSlots = new CardSlot[9];
    
    // 卡牌ID标识
    private readonly string[] cardIds = { "A", "B", "C", "D", "E", "F", "G", "H", "I" };
    
    // 属性
    public CardSlot[] HumanHandSlots => humanHandSlots;
    public CardSlot[] HumanModifierSlots => humanModifierSlots;
    public CardSlot[] BattleSlots => battleSlots;
    public CardSlot[] AIModifierSlots => aiModifierSlots;
    public CardSlot[] AIHandSlots => aiHandSlots;
    
    private void Start()
    {
        InitializeGameBoard();
    }
    
    /// <summary>
    /// 初始化游戏棋盘
    /// </summary>
    private void InitializeGameBoard()
    {
        CreateBoardAreas();
        CreateCardSlots();
        SetupLayout();
        
        Debug.Log("游戏棋盘布局初始化完成 - 1x9横向布局");
    }
    
    /// <summary>
    /// 创建棋盘区域
    /// </summary>
    private void CreateBoardAreas()
    {
        // 如果没有指定区域，自动创建
        if (humanHandArea == null)
        {
            humanHandArea = CreateArea("HumanHandArea", Vector3.zero);
        }
        
        if (humanModifierArea == null)
        {
            humanModifierArea = CreateArea("HumanModifierArea", new Vector3(0, rowSpacing, 0));
        }
        
        if (battleArea == null)
        {
            battleArea = CreateArea("BattleArea", new Vector3(0, rowSpacing * 2, 0));
        }
        
        if (aiModifierArea == null)
        {
            aiModifierArea = CreateArea("AIModifierArea", new Vector3(0, rowSpacing * 3, 0));
        }
        
        if (aiHandArea == null)
        {
            aiHandArea = CreateArea("AIHandArea", new Vector3(0, rowSpacing * 4, 0));
        }
        
        if (destroyedArea == null)
        {
            destroyedArea = CreateArea("DestroyedArea", new Vector3(cardSpacing * 10, rowSpacing * 2, 0));
        }
    }
    
    /// <summary>
    /// 创建区域GameObject
    /// </summary>
    /// <param name="areaName">区域名称</param>
    /// <param name="position">位置</param>
    /// <returns>创建的Transform</returns>
    private Transform CreateArea(string areaName, Vector3 position)
    {
        GameObject areaObj = new GameObject(areaName);
        areaObj.transform.SetParent(transform);
        areaObj.transform.localPosition = position;
        return areaObj.transform;
    }
    
    /// <summary>
    /// 创建所有卡牌槽
    /// </summary>
    private void CreateCardSlots()
    {
        // 创建人类手牌槽
        CreateSlotRow(humanHandSlots, humanHandArea, "HumanHand", NumberCard.PlayerTeam.Red);
        
        // 创建人类加减牌槽
        CreateSlotRow(humanModifierSlots, humanModifierArea, "HumanModifier", NumberCard.PlayerTeam.Red);
        
        // 创建比牌槽
        CreateSlotRow(battleSlots, battleArea, "Battle", NumberCard.PlayerTeam.Red); // 中性区域
        
        // 创建AI加减牌槽
        CreateSlotRow(aiModifierSlots, aiModifierArea, "AIModifier", NumberCard.PlayerTeam.Blue);
        
        // 创建AI手牌槽
        CreateSlotRow(aiHandSlots, aiHandArea, "AIHand", NumberCard.PlayerTeam.Blue);
    }
    
    /// <summary>
    /// 创建一行卡牌槽
    /// </summary>
    /// <param name="slots">槽数组</param>
    /// <param name="parent">父对象</param>
    /// <param name="prefix">名称前缀</param>
    /// <param name="team">队伍</param>
    private void CreateSlotRow(CardSlot[] slots, Transform parent, string prefix, NumberCard.PlayerTeam team)
    {
        for (int i = 0; i < 9; i++)
        {
            Vector3 position = new Vector3((i - 4) * cardSpacing, 0, 0); // 居中排列
            string slotName = $"{prefix}_{cardIds[i]}";
            
            GameObject slotObj = CreateCardSlot(slotName, position, parent);
            CardSlot slot = slotObj.GetComponent<CardSlot>();
            
            if (slot == null)
            {
                slot = slotObj.AddComponent<CardSlot>();
            }
            
            // 设置槽属性
            slot.SetSlotInfo(cardIds[i], i, team);
            slots[i] = slot;
        }
    }
    
    /// <summary>
    /// 创建单个卡牌槽
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="position">位置</param>
    /// <param name="parent">父对象</param>
    /// <returns>创建的GameObject</returns>
    private GameObject CreateCardSlot(string name, Vector3 position, Transform parent)
    {
        GameObject slotObj;
        
        if (cardSlotPrefab != null)
        {
            slotObj = Instantiate(cardSlotPrefab, parent);
        }
        else
        {
            // 创建简单的卡牌槽
            slotObj = new GameObject(name);
            slotObj.transform.SetParent(parent);
            
            // 添加视觉组件
            SpriteRenderer renderer = slotObj.AddComponent<SpriteRenderer>();
            renderer.color = new Color(1f, 1f, 1f, 0.3f); // 半透明白色
            
            // 添加碰撞器
            BoxCollider2D collider = slotObj.AddComponent<BoxCollider2D>();
            collider.size = cardSize;
        }
        
        slotObj.name = name;
        slotObj.transform.localPosition = position;
        
        return slotObj;
    }
    
    /// <summary>
    /// 设置布局
    /// </summary>
    private void SetupLayout()
    {
        // 设置相机位置以适应布局
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0, rowSpacing * 2, -10);
            
            // 调整相机大小以适应整个棋盘
            float boardWidth = cardSpacing * 9;
            float boardHeight = rowSpacing * 5;
            
            Camera.main.orthographicSize = Mathf.Max(boardWidth / (2 * Camera.main.aspect), boardHeight / 2) + 50f;
        }
    }
    
    /// <summary>
    /// 根据ID获取人类手牌槽
    /// </summary>
    /// <param name="cardId">卡牌ID</param>
    /// <returns>对应的槽</returns>
    public CardSlot GetHumanHandSlot(string cardId)
    {
        int index = System.Array.IndexOf(cardIds, cardId);
        return index >= 0 && index < humanHandSlots.Length ? humanHandSlots[index] : null;
    }
    
    /// <summary>
    /// 根据ID获取AI手牌槽
    /// </summary>
    /// <param name="cardId">卡牌ID</param>
    /// <returns>对应的槽</returns>
    public CardSlot GetAIHandSlot(string cardId)
    {
        int index = System.Array.IndexOf(cardIds, cardId);
        return index >= 0 && index < aiHandSlots.Length ? aiHandSlots[index] : null;
    }
    
    /// <summary>
    /// 根据ID获取比牌槽
    /// </summary>
    /// <param name="cardId">卡牌ID</param>
    /// <returns>对应的槽</returns>
    public CardSlot GetBattleSlot(string cardId)
    {
        int index = System.Array.IndexOf(cardIds, cardId);
        return index >= 0 && index < battleSlots.Length ? battleSlots[index] : null;
    }
    
    /// <summary>
    /// 放置卡牌到指定槽
    /// </summary>
    /// <param name="card">卡牌</param>
    /// <param name="slot">目标槽</param>
    /// <returns>是否成功放置</returns>
    public bool PlaceCardToSlot(NumberCard card, CardSlot slot)
    {
        if (card == null || slot == null)
        {
            return false;
        }
        
        return slot.PlaceCard(card);
    }
    
    /// <summary>
    /// 将卡牌移动到销毁区域
    /// </summary>
    /// <param name="card">要销毁的卡牌</param>
    public void MoveCardToDestroyedArea(NumberCard card)
    {
        if (card == null || destroyedArea == null)
        {
            return;
        }
        
        // 在销毁区域创建卡牌的视觉表示
        GameObject destroyedCardObj = CreateDestroyedCardVisual(card);
        
        // 计算销毁区域的位置
        int destroyedCount = destroyedArea.childCount;
        Vector3 position = new Vector3(
            (destroyedCount % 5) * (cardSize.x + 10f), // 每行5张
            -(destroyedCount / 5) * (cardSize.y + 10f), // 向下排列
            0
        );
        
        destroyedCardObj.transform.localPosition = position;
        
        Debug.Log($"卡牌 {card.CardId}({card.CurrentValue}) 移动到销毁区域");
    }
    
    /// <summary>
    /// 创建销毁卡牌的视觉表示
    /// </summary>
    /// <param name="card">卡牌数据</param>
    /// <returns>视觉对象</returns>
    private GameObject CreateDestroyedCardVisual(NumberCard card)
    {
        GameObject cardObj = new GameObject($"Destroyed_{card.CardId}");
        cardObj.transform.SetParent(destroyedArea);
        
        // 添加视觉组件
        SpriteRenderer renderer = cardObj.AddComponent<SpriteRenderer>();
        renderer.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // 灰色半透明
        
        // 添加文本显示卡牌信息
        GameObject textObj = new GameObject("CardText");
        textObj.transform.SetParent(cardObj.transform);
        textObj.transform.localPosition = Vector3.zero;
        
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = $"{card.CardId}\n{card.CurrentValue}";
        textMesh.fontSize = 20;
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;
        
        return cardObj;
    }
    
    /// <summary>
    /// 清空所有卡牌槽
    /// </summary>
    public void ClearAllSlots()
    {
        ClearSlotArray(humanHandSlots);
        ClearSlotArray(humanModifierSlots);
        ClearSlotArray(battleSlots);
        ClearSlotArray(aiModifierSlots);
        ClearSlotArray(aiHandSlots);
        
        // 清空销毁区域
        if (destroyedArea != null)
        {
            for (int i = destroyedArea.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(destroyedArea.GetChild(i).gameObject);
            }
        }
        
        Debug.Log("所有卡牌槽已清空");
    }
    
    /// <summary>
    /// 清空槽数组
    /// </summary>
    /// <param name="slots">槽数组</param>
    private void ClearSlotArray(CardSlot[] slots)
    {
        foreach (CardSlot slot in slots)
        {
            if (slot != null)
            {
                slot.ClearSlot();
            }
        }
    }
    
    /// <summary>
    /// 高亮可用的槽
    /// </summary>
    /// <param name="slots">要高亮的槽数组</param>
    /// <param name="highlight">是否高亮</param>
    public void HighlightSlots(CardSlot[] slots, bool highlight)
    {
        foreach (CardSlot slot in slots)
        {
            if (slot != null)
            {
                if (highlight)
                {
                    slot.Highlight();
                }
                else
                {
                    slot.Unhighlight();
                }
            }
        }
    }
    
    /// <summary>
    /// 获取布局信息描述
    /// </summary>
    /// <returns>布局描述</returns>
    public string GetLayoutDescription()
    {
        return "游戏布局 (从下到上):\n" +
               "1. 人类手牌区域 (A-I)\n" +
               "2. 人类加减牌区域\n" +
               "3. 比牌区域\n" +
               "4. AI加减牌区域\n" +
               "5. AI手牌区域 (A-I)\n" +
               "销毁区域位于右侧";
    }
}