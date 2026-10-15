# MP2a individual score audit — 3CR / 33 point cap

Source: user-supplied Individual Grading Rubric. This is a code/serialized asset audit, not awarded marks or a substitute for the required video and Individual Contributions statement. No collaboration points. No gameplay changed during this audit.

## Working estimate

24 points of implementation-backed claims, including a 2-point Prefab claim whose third behavior uses sale/recycling as Harvesting. Latest amplifier and collision gameplay still needs explicit Play-mode acceptance. Another 6–7 points are plausible with evidence/interpretation, giving a planning estimate of 30–31, not a guaranteed grade. If sale is not accepted as a qualifying affordance, re-evaluate Prefab Content too.

| Item | Working points | Evidence / recording requirement |
|---|---:|---|
| Passive Stat Gain | 2 | TowerShooting cooldown subtracts deltaTime; DamageAmplifier charge increases over time and has a bar/countdown. |
| Path Following | 2 | EnemyPathFollower advances designer waypoints; show turns in RoomScaleDefense. |
| Death | 2 | EnemyHealth.TakeDamage clamps health to zero and destroys the rat; show health bar and death. |
| Building Content | 4 | Five distinct tower prefabs selectable with keys 1–5; TowerBuildSlot spawns at grid build points and spends scrap. |
| Caretaking Content | 4 | Five tower prefabs upgraded by E; scrap decreases and displayed combat/support stats increase. |
| Harvesting Content | 2 | ScrapCan collected by click, destroyed, and adds SCRAP. Sale claims below may raise this same category to 4, not add another 4. |
| Pop-up Tutorials | 2 | Six first-encounter object-local hints: two projectile towers, soda, fan, amplifier, scrap. Five qualifies for 2; six does not reach the eight threshold. |
| Collision Interaction | 1 | One Projectile.OnTriggerEnter implementation damages EnemyHealth and destroys projectile. Cap/Punch share the script: do not count them as separate collision scripts. |
| Stat Content | 3 | At least eight distinct dynamic stat names below; repeated names across prefabs not counted again. |
| Prefab Content | 2 | Five machines with building, caretaking, and sale/recycling; qualification explanation below. |
| Total | 24 | Requires successful demonstration and assessor acceptance. |

## Dynamic stats to name and demonstrate

1. EnemyHealth.health — decreases on hits; enemy health bar.
2. TowerShooting.fireInterval — decreases on upgrade; tower status interval.
3. TowerShooting.bonusDamage — increases on upgrade; DMG before/after.
4. TowerSpeedUpgrade.upgradeLevel — increases on upgrade; LV display.
5. SodaTower.damagePerSecond — increases on upgrade; DPS display.
6. CoolingFan.speedMultiplier — increases on upgrade; SPEED percentage and affected attack interval.
7. DamageAmplifier.damageMultiplier — increases on upgrade; DAMAGE multiplier.
8. DamageAmplifier.charge — increases during charging, resets on release; charge bar and numeric progress.
9. DamageAmplifier.activeSeconds — decreases during boost; BOOST timer.

Nine supports the 8-stat / 3-point tier, not the 13-stat / 4-point tier. Do not pad with static range, price, duration settings, temporary UI countdowns, duplicate level fields, or player resources. connectedTowers is visible dynamic data but is not needed for this claim.

## Prefab qualification

| Exact prefab | Behavior 1 | Behavior 2 | Behavior 3 |
|---|---|---|---|
| RoomRapidTower | Grid build | Upgrade damage/interval | Sell/recycle instance for SCRAP |
| RoomLongRangeTower | Grid build | Upgrade damage/interval | Sell/recycle instance for SCRAP |
| RoomSodaTower | Grid build | Upgrade DPS | Sell/recycle instance for SCRAP |
| RoomCoolingFan | Grid build | Upgrade speed boost | Sell/recycle instance for SCRAP |
| RoomDamageAmplifier | Grid build | Upgrade damage boost | Sell/recycle instance for SCRAP |

TowerBuildSlot.Sell is player-triggered, deactivates/destroys the built instance and adds 10 SCRAP. This matches the Harvesting wording (destroy an instance in exchange for increasing a resource). Explain it as machine recycling in the contribution statement. Counting a prefab for Content and describing its qualifying affordances is expected by the rubric. Do not assume plain attack animations, UI labels, or particle effects independently qualify as spatial dynamics.

## Additional claims, separate from the 24

- Harvesting +2: five recyclable machines plus ScrapCan means six harvestable prefabs, reaching the five-prefab / 4-point tier. Needs recording and acceptance of machine sale as harvesting. This is a category increase from 2 to 4.
- Neighbor interactions +2: amplifier with RoomRapidTower, RoomLongRangeTower, RoomSodaTower gives three pairs. Show connected tower count changing charge duration, amplifier charge consumed at release, and recipients' damage multiplier rising/falling in recurring cycles. The receiver currently exposes a computed multiplier from the source; the assessor must accept this as changed receiver stats. Do not claim the fan's three one-way buffs automatically satisfy the rule that both instances' stats change through recurring interaction.
- Interface Metaphor: budget 2–3 (rubric allows 0–4). Current fan blades, battery charge bar, soda spray/cone, scrap and colors support it; final models and consistent signifiers affect the score. This is subjective, not code-verified.

Expected planning total with these claims: 30–31; remaining to 33: 2–3. With only the working 24: remaining 9. Do not promise full marks from adding just a foundation without accepting the conditional claims.

## Zero / excluded now

Resource Content: only SCRAP and base lives, below three. Internal field gold is SCRAP, not a second currency. Wild/Intermittent: disabled random scrap spawning and fixed-wave enemies do not qualify; random drop impulse is not random initial spawning. Shatter: automatic rat deaths are not a dedicated player shatter interaction. Crafting: purchasing/upgrading buildings is not resource-to-resource conversion. Foundation not implemented; existing grid cells are not player-built prerequisite prefabs. Built Paths/Path Planning/population resource absent. UV animation deferred; rotating geometry and particles alone do not qualify. Git/PR all excluded by user preference. Disabled melee/obstacle/dismantling code not counted.

## Verification status and short video checklist

Serialized prefabs contain the latest powerLink/damageBoost references, Cap/Punch Rigidbody and SphereCollider, and enemy BoxCollider. RoomScaleDefense contains amplifier references in build slots. Editor.log includes successful amplifier setup validation after earlier transient compilation errors. This does not establish Play-mode collision performance or UI readability.

Record: build all five; show each first tutorial; upgrade each with cost and stat change; collect scrap; recycle each machine and show refund; start wave and show path turns, bullet collision, health loss/death; show amplifier with 1/2/3 attack towers (6/8/10 seconds), boost release, all three receiver types and timer expiry. List exact prefabs/stats and video timestamps in Individual Contributions. Check separate group/VR submission requirements before final submission; this individual rubric alone does not settle them.
