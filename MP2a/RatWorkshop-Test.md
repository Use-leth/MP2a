# Midnight Cheese Defense — 回收循环

## 最新变更：实体击杀掉落（替代下方旧版格子生成流程）

- Assets > Refresh 后自动配置；手动入口：Tools > MP2a > Setup Physical Scrap Drops。
- ScrapCan 根对象含 Rigidbody 和非 Trigger BoxCollider。开局随机罐头、定时补充已禁用。
- 每只被击败的 RoomScaleEnemy 掉一份实体废料；拾取才加 10 SCRAP，不再同时直接发击杀奖励。逃到奶酪处消失不掉落。
- 废料不登记/预留建造格，可自由下落和滚动。鼠标直接点到废料优先拾取，不会同一次点击又建造。
- 全局 Time.timeScale 在建造阶段为 0，物理也会暂停；最后一个敌人的掉落可能暂时悬空，仍可拾取，下一波恢复物理。
- 测试：开局无罐头；建塔开波；确认击杀掉物、不立即加 10；左键拾取后才加 10；检查波末额外 15 奖励仍正常。
- 首次拾取教程跟随物体位置且面向摄像机，不随刚体翻滚。
- 此版本不再作为 Wild Instances / Intermittent Instances 的得分证据；击杀时间不是随机生成时间。

以下为旧版实现记录，随机格子生成测试已不适用。

## 应用和测试

退出 Play，Assets > Refresh。等待编译；若未自动配置，执行 Tools > MP2a > Setup Rat Workshop。使用 RoomScaleDefense 场景。

1. Play 后应出现 6 个黄色罐头，随机分布在非道路地面格。重新开始，位置应有变化。
2. 指向第一个罐头时，它上方显示 EMPTY CAN 教程；7 秒后隐藏。这个首次教程每次 Play 只触发一次。
3. 记下 SCRAP，左键回收罐头：罐头消失，SCRAP 增加 10。快速重复点击不能重复领取。
4. 按 1 或 2，在刚清空的格子建机器，SCRAP 减少 20；右键出售增加 10。
5. 有罐头的格子不能同时建塔。鼠道、入口与奶酪终点保持禁建。
6. Space 开始波次。战斗时间内随机 12–20 秒补充一个罐头，最多 8 个；不在已有机器、罐头或道路上生成。
7. 暂停建造阶段仍可回收初始罐头，补充计时随战斗暂停。奶酪归零后停止回收交互与补充生成。
8. 波次、射击、出售和范围预览仍正常。近战继续停用。

编译：使用 Unity 自带 Roslyn 和项目引用完成 runtime/editor 编译检查。Play 操作与教程可读性待用户验证。

## 美术替换约定

- ScrapCan.prefab 由首次配置生成：根对象挂 ScrapPickup 和 BoxCollider；Visual 子对象是外观占位；FirstRecycleHint 是教程。
- 更换罐头时替换 Visual 子对象，保留根脚本和引用；按模型尺寸调整根碰撞盒。不要让模型附带的碰撞盒挡住别的格子。
- 罐头约高 0.6 米、宽 0.5 米，底部对齐根对象。整个原型为夸张尺度。
- 老鼠素材用于 RoomScaleEnemy 的外观；保留 EnemyHealth、EnemyPathFollower、血条引用。
- 奶酪素材用于场景 Base 的外观；保留 BaseHealth 和场景中的引用。
- 远程机器素材用于 RoomRapidTower / RoomLongRangeTower；保留 TowerTargeting、TowerShooting、TowerRangeIndicator 和 FirePoint 引用。
- 当前机器还是原来的方块、老鼠还是原来的敌人占位体；改名与黄颜色不代表模型已完成。
- 金币字段仍名为 gold，保留已有序列化数据；当前界面和经济语义统一为 SCRAP。

## 本批评分证据边界

提供 Wild Instances、Intermittent Instances、1 种 Harvesting、1 个空间教程的实现基础；需录像验证。
仅 1 个教程未达到 3 pop-ups 的最低分档。罐头目前也没有三种独立行为，不单独承诺 Prefab Content。
加工台、油、电量和机器维护尚未实现，下一批继续。
