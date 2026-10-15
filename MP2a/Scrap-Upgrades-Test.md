# 远程机器废品升级

## 应用

退出 Play，Assets > Refresh。配置会自动执行；手动入口是 Tools > MP2a > Setup Tower Speed Upgrades。使用 RoomScaleDefense。

## 测试及录屏

1. 按 1 建瓶盖机（20 SCRAP），指向塔身或所在格子，显示等级、伤害、射击间隔、下一次价格和效果。
2. E 升一级：扣 15，伤害从 1 到 2，间隔从 0.60 到 0.48 秒，面板变黄。
3. 再按 E：扣 30，伤害到 3，间隔到 0.384 秒（界面四舍五入显示 0.38），面板变青。
4. 满级再次按 E，不扣钱、不改变数值。
5. 按 2 建拳套炮：间隔按 1.00 -> 0.80 -> 0.64 秒升级；伤害同样 1 -> 2 -> 3。
6. 余额不足时按 E，提示所需金额，余额与数值不变。
7. 开波确认实际子弹伤害和射击频率提高，其他未升级塔保持原值。
8. 右键塔身或格子都能出售，只退原来的 10 废品；重建后从 0 级开始。
9. 每种机器首次悬停显示 7 秒操作提示，位置在机器上方。之后悬停仍显示数值，移开隐藏。

编译已用 Unity 自带 Roslyn 与项目引用检查通过。Play 效果仍待测试。

## 替换 asset

- 保留 prefab 根对象的 TowerTargeting、TowerShooting、TowerSpeedUpgrade、TowerRangeIndicator 和 BoxCollider。
- 将新模型作为根对象的子对象；只隐藏/替换旧 MeshRenderer、MeshFilter 及装饰子对象，不删除根对象。
- FirePoint 指向新模型炮口位置；TowerShooting.projectilePrefab 指向要发射的子弹 prefab。
- TowerSpeedUpgrade.upgradePanel 指向一个独立可变色面板，可换为新模型上的灯。没有外观引用也不影响数值升级。
- 保留 UpgradeStatus 或重新指定 statusLabel。若更改根缩放，需要调整标签缩放和位置。
- 新模型的碰撞体应在同一根对象下，以便 GetComponentInParent 找到升级脚本；根 BoxCollider 按新模型尺寸调整。
- 三角面材质最好支持 _BaseColor 或 _Color，否则变色提示需另设简单灯面板。

## 得分边界

升级用玩家资源增加实例动态 Stat，且有数值、颜色及实际攻击反馈，适合作为 Caretaking 候选证据。
两种 prefab 的维护数量仍落在 1 Affordance = 2 分档，第三种合格 prefab 才到 3 分。
伤害和射击间隔在贡献声明中逐项列名，不把每种塔上的同名字段重复当成不同名称。
两种机器的相同 E 升级教程不提前视为两个独立新机制；独立教程数量仍需审计。
