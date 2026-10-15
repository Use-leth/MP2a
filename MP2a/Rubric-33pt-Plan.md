# MP2a：3CR / 33 分计划

依据用户提供的 Individual Grading Rubric。以下是候选得分路线，不是已获得成绩；最终以教师判断、个人实际贡献及视频证据为准。每批完成后先在 Unity 测试。

## 当前审计

- 最新资源决定：废品 SCRAP 是唯一建造/升级货币，不做油、电量或维护度。下方旧候选路线的三资源、三配方不能直接沿用，总分需重新核算；不能宣称已达到 33 分。
- 已实现待演示：两种远程塔花 15/30 废品升级，提升伤害和射击速度，有物体旁数值与首次教程、面板颜色反馈。见 Scrap-Upgrades-Test.md。

- 最新决定：废料改为击杀后的 Rigidbody + Collider 掉落，停用开局随机格子生成和定时补充。下面计划的 Wild / Intermittent 共 4 分仍是未完成候选项，需要后续独立实现符合规则的随机生成，或改选其他评分项。不得把击杀掉落直接计入这两项。

- 已有：两种远程塔的付费网格建造、出售、金币与基地生命显示、敌人血条、路径跟随、击杀销毁、子弹运动和射击冷却。
- 暂停：近战建造、道路建造、障碍拦截及维修入口。保留源码和预制体；波次生成明确禁用敌人障碍攻击。
- Path Following（2）、Death（2）、Passive Stat Gain（2）、两种 Building Content（2）有实现基础，仍需录像证明。
- 当前只有金币、基地生命两项明显可见玩家资源，未达 Resource Content 的 3 项门槛。
- 不按文件数量认领 Prefab Content：每个申报 prefab 必须有至少三种有效 affordance / spatial dynamic。
- 不把所有 Inspector 参数算作 Stat：必须真实随交互或时间改变，并有可见反馈；distinct names 的认定按评分规则保守列举。
- 固定间隔、固定位置的波次不是 Wild Instances 或 Intermittent Instances。
- Projectile 当前是距离阈值命中，没有物理碰撞回调；暂不申报 Collision Interaction。
- 常驻说明牌不是首次遇到机制时、出现在物体旁的 Pop-up Tutorial。
- 未核验 Git 仓库、PR、审批与合并证据。不得预先记为已完成。

## 候选路线：约 35 分，目标最终 33 分

| 评分项 | 目标分 | 实现和验收证据 |
|---|---:|---|
| Prefab Content | 2 | 至少 5 个合格 prefab；逐个列出三种实际行为，不能仅换颜色 |
| Stat Content | 3 | 至少 8 个不同名称的动态 Stat，逐个展示变化 |
| Passive Stat Gain | 2 | 冷却或加工进度使用 deltaTime 积分，显示进度 |
| Resource Content | 1 | 三项：Gold、Scrap、Base Lives，始终可见且真实消耗/获得 |
| Wild Instances | 2 | 开局在合法空地随机生成可回收物 |
| Interface Metaphor | 3 | 防御工坊：炮塔、回收物、加工台；统一形状、工具及颜色提示。此项主观评分 |
| Building Content | 3 | 快速塔、远程塔、加工台，共三种可付费网格建造的 prefab |
| Pop-up Tutorials | 2 | 五种首次触发的空间教程，贴近对应对象 |
| Git Merged | 2 | 真实批准并合并到 main 的 PR，截图带紫色 merged 标记 |
| Harvesting Content | 3 | 三种可回收 prefab，玩家交互后获得资源且对象/Stat 可见变化 |
| Caretaking Content | 2 | 至少一种远程塔可维护，改变可见 Stat；重做并验证已暂停的升级交互 |
| Crafting Content | 2 | 三种明确不同、可操作的资源交换配方 |
| Death | 2 | 敌人血量到零销毁 |
| Path Following | 2 | 敌人沿多段路径转弯 |
| Intermittent Instances | 2 | 已有 Wild Instances 后，在随机时间追加可回收物 |
| Shatter Content | 2 | 玩家拆解一件残骸，替换成可拾取回收物 |
| 合计（全部验收后） | 35 | 当前未完成；若内容资格或主观评分不足，补真实功能或协作证据 |

## 实施顺序

1. 远程版本回归：道路禁建、1/2 建塔、出售、范围提示、三波敌人正常通过。先测试。
2. 回收批次：新增 Scrap 和一种随机废料，玩家回收增加资源；提供物体旁首次教程和资源显示。先测试。
3. 加工批次：可建造加工台、三种资源交换、加工进度提示；为每个配方明确成本和产出，避免无成本套利。
4. 内容批次：三种回收物形成不同交互；可拆解残骸生成可回收物；随机时间补充生成，避免占路、占塔、重叠。
5. 维护批次：远程塔维护或补给、可见动态 Stat、完整五个空间教程；逐个补齐 prefab 的三项有效行为。
6. 证据批次：检查 Stat 和 prefab 明细、录屏演示脚本、Individual Contributions；真实完成 PR 审批和合并。

## 提交门槛

- 不以“存在脚本”替代“可实际演示”。建立 prefab -> 三项行为 -> Stat -> 视频时间点的表。
- 不把同一种交互重复认领 Collision 和 Neighbor。
- 以上评分文本没有单独列出 VR 评分，但不能据此推断课程允许仅交桌面交互；还需核对完整作业与小组要求，并验证计划提交的 XR 操作。
- 敌人排队/避让、NavMesh、复杂近战目前不在优先范围。
