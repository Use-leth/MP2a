# MP2a 最终录制核分（3CR，上限 33）

## 最新清理及逐项申报总览

用户已确认桶盖修复成功。清理移出 Assets：25 个过时 Editor 设置脚本、RoomViewControls.cs、BreakableAppliance.cs，以及无引用的 VRBottleCapTower.prefab、RoomMeleeTower.prefab。含 meta 的可恢复副本在 CleanupBackup/2026-10-15；该目录位于 Assets 之外，不参与 Unity 导入。其余模型保留，因为旧测试场景或现用资源仍有引用。保留三个 Editor 工具：展示柜设置、模型同步、自动瞄准与桶盖配置。

完整目标：Prefab 2 + Stats 3 + Passive 2 + Buildings 4 + Caretaking 4 + Harvesting 4 + Collisions 1 + Neighbors 2 + Death 2 + Path 2 + UV 3 + 首次教程 2 = 31；Interface Metaphor 预计 2–3，加总 33–34，个人封顶 33。首次教程及 VR 全流程仍需录屏证明，不能把此估算当作已获成绩。只确认桶盖成功并不等于五塔所有 affordance 都验收完成。

清理后运行时、Editor 编译通过。ProjectSettings/EditorBuildSettings.asset 目前仍只包含 SampleScene；直接录当前 RoomScaleDefense 不受此项影响，若需交付可执行程序必须另设正确的启动场景。

## 本轮补强（待 Play 验收）

- PurchasedTowerGrab 已添加五种机器首次 XR hover/select 的 10 秒物体旁教程，每个展示柜每局仅触发一次；同时显示伤害、间隔、bonusDamage、DPS、风扇倍率、蓄电倍率/连接数/充放电计时。下文所述教程和 VR 属性缺口已完成代码修复，但尚未完成实际交互验收。
- 五个教程若演示成功，可申报 Pop-up Tutorials 2 分：预估从 31–32 提升至 33–34（3CR 封顶 33）。这不是保证分数；原有建筑/升级/回收、邻居规则及 Interface Metaphor 仍需充分证据。
- 已添加 Tools → MP2a → Sync Tower Models To VR Displays，更新原始 Room prefab 到五个 VR 副本并重新绑定展示柜。退出 Play 刷新后自动执行一次；以后改模型可再次手动运行。保留垃圾桶、按钮和展示柜的位置、旋转及缩放。
- 垃圾桶根物体旋转约 -180.9°、缩放 1.5，模型、桶壁、触发区共同继承变换。按钮和 BoxCollider 同物体；提示与按钮一起移动到新区域。两个文字提示已改为面向主相机。碰撞/可达性仍须 Play 验收。
- 本轮运行时与 Editor 编译通过；未运行 Simulator/头显交互测试。

依据用户提供的 Individual Grading Rubric。2026-10-15 静态代码及序列化场景检查；不是实机验收或最终授分。不计 Git/PR。按当前 VR 流程重新核分，取代旧审核中桌面教程和 UV 尚未实现的结论。

## 可申报分数

| 条目 | 分数 | 视频证据与条件 |
|---|---:|---|
| Prefab Content | 2 | 五种 VR 机器，每种展示建造、升级、回收三种 affordance；需认可回收作为 harvesting |
| Stat Content | 3 | 至少八个不同名称的动态 Stat，名单见下；需可视化证据 |
| Passive Stat Gain | 2 | 蓄电充能随时间增加、增益剩余时间递减 |
| Building Content | 4 | 五种机器从展示柜购买，释放到格子并吸附 |
| Caretaking Content | 4 | 五种机器分别用锤子升级，资源减少、等级和效果增加 |
| Harvesting Content | 4 | 五种机器分别入桶回收，加上 ScrapCan；六个 prefab 仍是五档的 4 分 |
| Collision Interaction | 1 | Projectile.OnTriggerEnter 消耗弹体并扣敌人血；两种弹体共用脚本不重复计算 |
| Neighbor Interaction | 2 | 蓄电器分别与三种攻击塔作用，充能消耗且受益塔伤害倍率变化；需接受 computed multiplier 为 stat 变化 |
| Death | 2 | 老鼠血量归零，实例销毁 |
| Path Following | 2 | 老鼠沿有转弯的设计路径移动 |
| Texture Animation Signifiers | 3 | VR_RoomDamageAmplifier 确认包含 AmplifierUVFlow 和 panel 引用；显示 UV 箭头滚动随充能/释放改变 |
| 小计 | 29 | 全部以实际演示成功和评阅认可为条件 |
| Interface Metaphor | 预计 2–3 / 最多 4 | 工坊柜购买、锤子升级、垃圾桶回收、风扇增速、电池充能；写明 signifier 和映射 |

当前规划约 31–32 / 33，不能承诺满分。若关键 VR 流程失败，建筑、照料、收获和 Prefab 分数都会受影响；29 也不是已经到手的分数。

最直接的补强是恢复 VR 的五个首次遇见物体旁教程，争取 Pop-up Tutorials 2 分，给出 33–34 的申报空间。当前 PurchasedTowerGrab 只有通用 hover 提示，不调用旧 SetHovered 首次教程；DesktopBuildInput.enableMouseBuilding 为 0，所以本次暂计 0。不要把常驻说明当成五个已验证的首次教程。

Solar Coaster 的条带 UV 滚动可另外讨论，但不放入保底；需要展示 mesh strip、方向性位移并确认评阅解释。风扇旋转和粒子效果本身不是 UV 动画。

## 精确名称（贡献说明必须列出）

Prefab：VR_RoomRapidTower、VR_RoomLongRangeTower、VR_RoomSodaTower、VR_RoomCoolingFan、VR_RoomDamageAmplifier。不要把它们的旧桌面版本再重复计数。Harvesting 额外列 ScrapCan；随机外观不是独立机制。

动态 Stat：EnemyHealth.health；TowerShooting.fireInterval、bonusDamage；TowerSpeedUpgrade.upgradeLevel；SodaTower.damagePerSecond；CoolingFan.speedMultiplier；DamageAmplifier.damageMultiplier、charge、activeSeconds。九个满足八档 3 分，不能算十三档。等级重名不跨塔重复凑数。

不计：Resource Content（只有 SCRAP 和基地生命，不足三种）、Wild Instances、Intermittent Instances、Shatter、Crafting、Foundation、Built Paths、Path Planning、Population Pathing、Git/PR。固定波次、死亡掉落和随机外观不代替这些要求。

## 录制前阻断项

1. 相机：磁盘场景已无 OverviewCamera；Play 的 Game 窗口须实际跟随 XR 头部。
2. 控制器：本次读到 XRDeviceSimulatorSettings 自动启动为 0、Prefab 为空。虽然 Simulator 样例目录已导入，不能视为自动运行成功。用 Simulator 时先验证左右手追踪；真头显则验证实际设备。不应为显示控制器而关闭追踪模式管理。
3. 五塔的购买、放置、再次搬动、锤击升级、入桶回收必须走通。锤子和桶口碰撞需要实际检查。
4. VR hover 目前只展示通用等级/成本，旧详细 DPS、充能、倍率 UI 依赖桌面 SetHovered。录制前应将这些信息接入 XR hover，并恢复首次教程；否则 Stats 和 Neighbor 的证据不够清楚。
5. 初始 SCRAP 为 120：买五塔花 100，五塔各升一级另需 75，总共至少 175。通过实际波次收集废品赚取差额，或分段从新局录制；不要假装一局 120 足够全部演示。

## 建议分段录制顺序（按课程实际视频时长限制压缩）

1. 工坊与操作：展示第一人称、左右手、scoreboard 的 SCRAP/BASE；简述购买/升级/回收隐喻。
2. 五种建筑：各拿一台，余额扣除，落格吸附；顺便展示局部首次教程（补齐并验证后）。搬动一台证明免费移动且升级保留。
3. 战斗：按墙边按钮开波；展示路径转弯、实体弹体命中、血条下降、死亡和废品掉落。收集足够废品支持升级。
4. 五种照料：锤子逐台升级；镜头同时包含升级提示/属性变化和资源扣除，不只拍敲击火花。
5. 邻居与 UV 特写：三种攻击塔靠近蓄电器，展示接入数、6/8/10 秒充能周期、释放后倍率提升与到期恢复。拍清箭头纹理确实滚动，以及释放时颜色/速度变化。风扇作为额外反馈展示。
6. 收获收尾：废品握着进桶不结算，松手才结算；五种已购机器各回收一次，显示 +10。放在战斗展示之后，避免提前拆掉阵容。

不必为此评分表录完八波；胜败结果可作额外短镜头，另查课程整组提交要求。

## 提交配套

- Individual Contributions：逐条列 rubric 名称、Prefab/Stat 精确名、机制描述、视频时间戳；写清个人实际贡献。
- Interface Metaphor：每种 prefab 列可操作行为、标识及其含义。例如价签→购买成本；绿格→合法放置；锤子升级文字/火花→升级成功；桶口/返还提示→回收。
- 最终先检查原始录像：文字可读、操作成功、没有 Console 错误遮挡、音轨可听。不要只依赖剪辑预览。
- 此表没有涵盖尚未提供的整组提交规格、视频时限或截止时间；提交前对照课程原始要求。
