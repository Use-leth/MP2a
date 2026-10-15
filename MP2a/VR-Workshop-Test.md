# Wall workshop batch

Save the scene with your rig, exit Play and refresh. Tools > MP2a > Setup VR Wall Workshop applies the setup if it has not run automatically. The group VRWallWorkshop is placed relative to ScoreboardPanel on the scoreboard side; its children are five displays, the hammer station on the left, recycling bin on the right, and a next-wave button in front. Move this group as a whole if decoration requires more clearance.

## Combined Simulator test

1. Grab each of the five display types. Each first purchase deducts 20 SCRAP exactly once. Display replenishes. Below 20, new samples cannot be selected, but purchased machines remain movable.
2. Release onto a green cell: snap and activate. Road/reserved/occupied cells stay invalid. Invalid drops remain paid and can be grabbed again; they do not shoot or supply boosts.
3. Pick up and relocate each type: no new charge and upgrades remain. Held/off-grid attack towers stop attacking; held fan/amplifier stop supplying buffs. Rotate soda in hand, then place: its yaw sets the cone direction.
4. Grab loose dropped scrap: currency must not change yet. Hold it inside the bin: still no change. Release inside: scrap disappears and its value is credited once.
5. Grab a purchased machine and release it inside the bin: it disappears and refunds 10 SCRAP once. Never-owned display samples cannot be recycled. No refund for the hammer or scenery.
6. Grab the hammer by its handle. Swing the upper hit head into a placed machine: first/second upgrades cost 15/30. Holding the head against the tower must not repeatedly spend. Move out and strike again after 0.8s. Maximum level/insufficient money/held or unplaced machines must not spend resources. Check the status message and level change.
7. Select the green NextWaveButton with the rig's select action. It starts the next available wave, cannot start duplicate waves during combat, and stops after defeat/final wave. SPACE remains a fallback.
8. Verify scoreboard visibility, reachable shelf heights, hammer model orientation and hit-head alignment. Those require Simulator or headset inspection; compiler validation alone cannot establish them.

## Implementation notes

Each VR machine is a separate VR_Room*.prefab created from the existing machine prefab. Future source model edits do not automatically update these copies. The original room models, rig and input bindings are untouched. ScrapCan gains XRI grab capability while retaining its random appearances and physical drop behavior. Desktop mouse building remains disabled to avoid Simulator conflicts. The first single-tower VRBottleCapTower prefab is retained but the wall shelf uses the new five-machine set.

Runtime and editor compilation passed. Actual interaction and imported hammer geometry alignment remain unverified. No claim that the full XR loop has been play-tested.
