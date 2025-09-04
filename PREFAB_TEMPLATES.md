# 🎨 预制体模板创建指南

## 📋 **必需的预制体列表**

### **1. 核心游戏预制体**

#### **🃏 Card_Prefab (数字卡牌)**
```
结构:
Card_Prefab (GameObject)
├── 组件:
│   ├── SpriteRenderer (卡牌背景)
│   ├── BoxCollider2D (点击检测)
│   ├── Draggable (拖拽功能)
│   └── Card (卡牌数据脚本)
├── 子对象:
│   ├── CardBackground (SpriteRenderer)
│   ├── CardNumber (TextMesh) - 显示数字
│   ├── CardID (TextMesh) - 显示A-I标识
│   └── TeamIndicator (SpriteRenderer) - 队伍颜色

设置参数:
- SpriteRenderer: 使用ResourceManager生成的精灵
- BoxCollider2D: Size (1, 1.4)
- Draggable: Enable Drag And Drop = true
- Card: 运行时设置卡牌数据
```

#### **🎯 CardSlot_Prefab (卡牌槽位)**
```
结构:
CardSlot_Prefab (GameObject)
├── 组件:
│   ├── SpriteRenderer (槽位背景)
│   ├── BoxCollider2D (放置检测)
│   └── CardSlot (槽位逻辑脚本)
├── 子对象:
│   ├── SlotBackground (SpriteRenderer)
│   ├── SlotID (TextMesh) - 显示A-I标识
│   └── HighlightEffect (SpriteRenderer) - 高亮效果

设置参数:
- SpriteRenderer: 半透明槽位背景
- BoxCollider2D: Size (1, 1.4)
- CardSlot: Slot Type根据用途设置
```

### **2. UI预制体**

#### **🎮 GameUI_Prefab (游戏UI)**
```
结构:
GameUI_Prefab (Canvas)
├── MainMenuPanel
│   ├── Background
│   ├── Logo
│   ├── StartButton
│   └── QuitButton
├── GameplayPanel
│   ├── TopBar
│   │   ├── TimerUI
│   │   ├── PhaseDisplay
│   │   └── ScoreDisplay
│   ├── GameBoardArea
│   ├── HandPanelUI
│   └── ControlPanel
│       ├── EndTurnButton
│       └── PauseButton
└── GameOverPanel
    ├── ResultDisplay
    ├── RestartButton
    └── MenuButton
```

#### **⏰ TimerUI_Prefab (计时器)**
```
结构:
TimerUI_Prefab (GameObject)
├── 组件:
│   ├── RectTransform
│   ├── Image (背景)
│   └── TimerUI (脚本)
├── 子对象:
│   ├── TimerFill (Image) - 填充条
│   ├── TimerText (Text) - 时间显示
│   └── WarningEffect (Image) - 警告效果

设置参数:
- Size: (200, 100)
- Timer Fill: Fill Method = Horizontal
- Timer Text: Font Size = 24, Alignment = Center
```

#### **🃏 HandPanel_Prefab (手牌面板)**
```
结构:
HandPanelUI_Prefab (GameObject)
├── 组件:
│   ├── RectTransform
│   ├── Image (背景)
│   ├── HandPanelUI (脚本)
│   └── Horizontal Layout Group
├── 子对象:
│   ├── CardContainer (Transform)
│   └── ScrollView (可选)

设置参数:
- Size: (900, 160)
- Layout Group: Spacing = 10
- Card Container: 用于动态创建卡牌UI
```

### **3. 特效预制体**

#### **✨ CardPlaceEffect_Prefab (放置特效)**
```
结构:
CardPlaceEffect_Prefab (GameObject)
├── 组件:
│   ├── ParticleSystem
│   └── AudioSource (可选)
├── 设置:
│   ├── Duration: 0.5
│   ├── Start Lifetime: 1.0
│   ├── Start Speed: 2
│   ├── Start Color: Yellow
│   ├── Shape: Circle
│   └── Emission: Burst 20 particles
```

#### **💥 BattleEffect_Prefab (比牌特效)**
```
结构:
BattleEffect_Prefab (GameObject)
├── 组件:
│   ├── ParticleSystem
│   └── AudioSource
├── 设置:
│   ├── Duration: 1.0
│   ├── Start Color: 根据胜负变化
│   ├── Velocity over Lifetime: 向上
│   └── Size over Lifetime: 放大效果
```

---

## 🛠️ **创建步骤详解**

### **步骤1: 创建Card预制体**

1. **创建基础对象**:
   ```
   Hierarchy右键 → Create Empty
   重命名: Card_Prefab
   位置: (0, 0, 0)
   ```

2. **添加核心组件**:
   ```
   Add Component → SpriteRenderer
   Add Component → BoxCollider2D  
   Add Component → Draggable
   ```

3. **配置SpriteRenderer**:
   ```
   Sprite: 留空 (运行时设置)
   Color: White
   Sorting Layer: Default
   Order in Layer: 1
   ```

4. **配置BoxCollider2D**:
   ```
   Size: X=1, Y=1.4
   Is Trigger: false
   ```

5. **配置Draggable**:
   ```
   Is Draggable: true
   Return To Original Position: false
   Scale During Drag: true
   Drag Scale: 1.1
   ```

6. **创建子对象 - CardNumber**:
   ```
   右键Card_Prefab → 3D Object → 3D Text
   重命名: CardNumber
   位置: (0, 0, -0.1)
   
   TextMesh设置:
   - Text: "1"
   - Font Size: 24
   - Color: White
   - Anchor: Middle Center
   ```

7. **创建子对象 - CardID**:
   ```
   右键Card_Prefab → 3D Object → 3D Text
   重命名: CardID  
   位置: (0, 0.5, -0.1)
   
   TextMesh设置:
   - Text: "A"
   - Font Size: 16
   - Color: White
   - Anchor: Middle Center
   ```

8. **保存预制体**:
   ```
   拖拽Card_Prefab到Assets/Art/Prefabs/文件夹
   ```

### **步骤2: 创建CardSlot预制体**

1. **创建基础对象**:
   ```
   Hierarchy右键 → Create Empty
   重命名: CardSlot_Prefab
   ```

2. **添加组件**:
   ```
   Add Component → SpriteRenderer
   Add Component → BoxCollider2D
   Add Component → CardSlot
   ```

3. **配置组件**:
   ```
   SpriteRenderer:
   - Sprite: 使用ResourceManager生成的槽位精灵
   - Color: (1, 1, 1, 0.5)
   
   BoxCollider2D:
   - Size: (1, 1.4)
   
   CardSlot:
   - Slot Type: 根据用途设置
   - Owner Team: 根据位置设置
   ```

4. **创建子对象 - SlotLabel**:
   ```
   3D Object → 3D Text
   重命名: SlotLabel
   位置: (0, -0.6, -0.1)
   
   TextMesh设置:
   - Text: "A"
   - Font Size: 14
   - Color: Gray
   ```

5. **保存预制体**

### **步骤3: 创建UI预制体**

#### **TimerUI预制体**:
1. **创建UI对象**:
   ```
   Canvas右键 → UI → Image
   重命名: TimerUI_Prefab
   ```

2. **配置主体**:
   ```
   Image:
   - Color: (0, 0, 0, 0.7)
   
   RectTransform:
   - Width: 200, Height: 100
   
   Add Component → TimerUI
   ```

3. **创建子对象**:
   ```
   ├── TimerFill (Image)
   │   - Image Type: Filled
   │   - Fill Method: Horizontal
   │   - Color: Green
   ├── TimerText (Text)
   │   - Text: "01:00"
   │   - Font Size: 24
   │   - Alignment: Center
   └── WarningBorder (Image)
       - Color: Red (初始透明)
       - 用于警告效果
   ```

4. **配置TimerUI脚本引用**:
   ```
   Timer Text: 拖拽TimerText
   Timer Fill Image: 拖拽TimerFill
   Normal Color: Green
   Warning Color: Yellow  
   Danger Color: Red
   ```

---

## 🎯 **最终场景层次结构**

完成后的场景应该是这样的：
```
Scene: MainScene
├── Main Camera
├── Directional Light
├── MainCanvas (UI)
│   ├── MainMenuPanel
│   ├── GameplayPanel
│   └── GameOverPanel
├── GameManager (核心管理器)
│   ├── GameManager (脚本)
│   ├── GamePhaseManager (脚本)
│   ├── ResourceManager (脚本)
│   └── PerformanceMonitor (脚本)
├── UIManager (UI管理器)
│   └── UIManager (脚本)
├── AudioManager (音频管理器)
│   ├── AudioManager (脚本)
│   ├── MusicSource (AudioSource)
│   └── SFXSource (AudioSource)
├── GameBoard (游戏棋盘)
│   ├── GameBoardLayout (脚本)
│   ├── HumanHandArea (9个卡牌槽)
│   ├── HumanModifierArea (9个加减牌槽)
│   ├── BattleArea (9个比牌槽)
│   ├── AIModifierArea (9个加减牌槽)
│   ├── AIHandArea (9个卡牌槽)
│   └── DestroyedArea (销毁区域)
├── HumanPlayer (人类玩家)
│   └── NumberPlayer (脚本)
├── AIPlayer (AI玩家)
│   └── NumberPlayer (脚本)
└── EventSystem (UI事件系统)
```

---

## 🚀 **快速验证命令**

在Unity中，您可以使用以下方法快速验证设置：

### **Console命令验证**:
```csharp
// 检查管理器
Debug.Log("GameManager: " + (GameManager.Instance != null));
Debug.Log("UIManager: " + (UIManager.Instance != null));  
Debug.Log("AudioManager: " + (AudioManager.Instance != null));
Debug.Log("ResourceManager: " + (ResourceManager.Instance != null));

// 检查资源
ResourceValidator validator = FindObjectOfType<ResourceValidator>();
if (validator != null) validator.ValidateResources();

// 检查性能
PerformanceMonitor monitor = FindObjectOfType<PerformanceMonitor>();
if (monitor != null) Debug.Log(monitor.GetPerformanceReport());
```

### **Inspector快速检查**:
1. 选择GameManager → 确认所有引用都不是None
2. 选择UIManager → 确认所有UI引用都已设置
3. 选择AudioManager → 确认音频源已配置
4. 运行游戏 → 观察Console输出无错误

---

## 🎯 **成功标志**

当您看到以下内容时，说明设置成功：

1. **Console输出**:
   ```
   资源管理器初始化完成
   已生成9张程序化卡牌精灵
   已生成4种程序化槽位精灵
   已生成6种程序化音效
   游戏阶段管理器初始化完成
   Human Player 初始化完成
   AI Player 初始化完成
   ```

2. **游戏界面**:
   - 主菜单正常显示
   - 点击开始游戏能正常切换
   - 看到1x9的卡牌布局
   - 计时器正常工作
   - AI自动进行操作

3. **功能正常**:
   - 卡牌可以拖拽
   - 音效正常播放
   - 游戏阶段正常切换
   - 比牌逻辑正常工作

**如果都正常，恭喜您！游戏已经可以完整运行了！** 🎉

---

## 📞 **需要帮助？**

如果在设置过程中遇到问题：

1. **检查Console**: 查看错误信息
2. **参考指南**: SETUP_GUIDE.md中的常见问题部分
3. **使用调试工具**: PerformanceMonitor和ResourceValidator
4. **逐步验证**: 按照检查清单逐项确认

**记住：即使没有任何美术资源，游戏也能完美运行！** 🚀