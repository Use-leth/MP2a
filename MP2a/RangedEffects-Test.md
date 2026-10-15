# 远程塔特效

退出 Play，Assets > Refresh，自动配置；手动入口 Tools > MP2a > Setup Ranged Tower Effects。

- 瓶盖机：黄色短促枪口碎屑、细短弹道、命中碎屑。
- 拳套炮：橙色较大发射脉冲、粗长弹道、较大的命中爆点。
- 特效仅随实际射击和有效命中触发；目标提前死亡导致子弹消失时不产生假命中。
- 伤害、速度、射击间隔和升级逻辑未改。旧 Projectile 预制体不改，新建 CapProjectile / PunchProjectile 给房间版塔使用。
- 命中特效实例 1.5 秒后清理；使用游戏时间，因此暂停时会一同暂停。

## 测试

1. 各建一座塔，未开波时不应发出闪光或爆点。
2. Space 开波：两种颜色、枪口、尾迹和命中粒子应可辨认。
3. E 升级后射速提高，枪口特效应随实际发射频率同步变化。
4. 目标离开范围/死亡时停止开火；右键出售停止发射，已射出子弹保留原有行为。
5. 本轮已通过 runtime/editor 编译；材质颜色、粒子尺寸和可读性等待 Play 验收。

## 替换素材

- TowerShooting.muzzleFlash 是炮口 ParticleSystem 引用，flashParticleCount 控制每枪粒子数。
- FirePoint 子对象下的 MuzzleFlash 可替换为 asset 特效。预制体关闭 Play On Awake，保持与实际开火 Emit 同步。
- Projectile.impactEffect 是命中特效引用；保持非循环，持续时间小于 1.5 秒，或同步调整清理时长。
- 子弹的 TrailRenderer 可调宽度、长度和材质，也可换为 asset 自带外观。
- 这些是视觉反馈，不自动计为 UV 动画或物理碰撞评分。
