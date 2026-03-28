Assets/
├── 01_Scenes/               # 按游戏流程划分场景
│   ├── MainMenu/
│   └── GameScene/
├── 02_Scripts/              # 核心代码，按系统模块划分
│   ├── Managers/            # 全局管理器（通常挂载在空物体上，贯穿全局）
│   ├── Player/              # 玩家角色控制
│   ├── Characters/          # NPC 与敌对实体
│   ├── Interactables/       # 环境与可交互遗物
│   ├── UI/                  # 对话与界面
│   └── Utils/               # 通用工具与特效
├── 03_Prefabs/              # 预制体，必须是组装完毕可直接拖入场景的资产
│   ├── Characters/          # 如 花火 等 NPC 或敌对实体的预制体
│   ├── Interactables/       # 包含触发器和检查脚本的遗物/线索预制体
│   └── UI/                  # 动态字幕 Canvas、主菜单 UI 等
├── 04_Art/                  # 美术资产
│   ├── Models/              # 3D 角色模型（PMX导入文件等）与旧校舍环境
│   ├── Materials/           # 渲染管线配置相关的材质
│   └── Textures/            # 贴图与纹理
├── 05_Animations/           # 动画资产
│   ├── Animators/           # 状态机控制器
│   ├── Timelines/           # 特定物品专属剧情触发的 Timeline 轨道
│   └── Clips/               # 基础动画片段
├── 06_Audio/                # 音频资源
│   ├── BGM/                 # 梦境环境音
│   ├── SFX/                 # 物品交互音效、恶灵追逐警告音
│   └── Voice/               # 角色语音
├── 07_Settings/             # 配置文件
│   ├── Cinemachine/         # 虚拟相机配置文件
│   └── Input/               # 输入系统配置
└── Plugins/                 # 第三方插件与依赖库