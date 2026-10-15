# 汽水塔：固定方向扇形群伤

退出 Play，Assets > Refresh。自动配置后，Console 应有 Soda checks passed 和首次的 Soda tower ready。若未自动应用：Tools > MP2a > Setup Soda Tower。

## 操作验收

1. 按 3，指向普通空地：出现 3 米 / 60 度扇形。指向道路无法建造。
2. 保持指向空地按 R，扇形每次转 90 度。左键建造，喷口朝向与预览一致。
3. 指向塔身或所在格子按 R，喷口、扇形判定和喷流同步旋转。升级/出售仍使用 E / 右键。
4. 在入口附近、道路旁放塔，扇形沿鼠道方向覆盖一段路。Space 开波，多只老鼠处于扇形内时应同时掉血和闪白。
5. 转到背向道路：伤害停止、喷流消失。再转回道路恢复。射程外或扇形侧面老鼠不受伤。
6. 默认 DPS 3，每 0.2 秒判定一次、每次 0.6 伤害。每次 E 升级提高 1.5 DPS；费用 15/30，满级 6 DPS。
7. 检查已有两种塔、废品回收、卖塔及波次正常。

## 资产与 VR 接口

- RoomSodaTower 根对象保留 SodaTower、TowerTargeting（只作射程数据，组件禁用）、TowerRangeIndicator、BoxCollider。
- Visual / NozzleVisual 是外观占位，可换素材；根对象保持单位缩放，Unity 本地 +Z 为喷射方向。
- SprayOrigin 引用粒子系统，SodaStatus 是状态文字。可替换粒子效果，伤害不依赖粒子碰撞。
- 检测以塔根对象水平位置为扇形原点；预览同源。若修改喷口位置，需检查喷流与地面扇形仍有对应关系。
- RotateQuarterTurn() 用于桌面 90 度调整；SetFacing(float yaw) 可供后续 VR 调用任意角度。VR 输入尚未接入。
- 已撤下拆电器的自动配置、点击入口；本次场景设置会移除 DismantlingCorner（若曾生成）。

## 验证状态和评分

- runtime/editor 编译已通过，旧索敌 API 的弃用警告仍存在。
- 配置脚本在隔离预览场景检查扇形内外、距离、旋转、余额不足、升级上限；Console 日志才代表 Unity 内检查已运行。
- 喷流的视觉同步、UI 可读性、多目标实际攻击仍需 Play 验收。
- 第三种建造 prefab 和第三种可升级 prefab 可分别推进 Building / Caretaking 到 3 分档（各比此前 +1）。扇形伤害是距离/角度计算，不计物理碰撞分。
- 不申报未采用的拆电器、Shatter 或协作分。
