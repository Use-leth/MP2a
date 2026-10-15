# TabletopRoom checkpoint

Open `Assets/Scenes/TabletopRoom.unity` after Unity finishes compiling.
If it has not been created, use `Tools > MP2a > Create Tabletop Room` outside Play mode.

## Desktop test

1. Press Play. The scene starts paused for building, with 60 gold and 5 lives.
2. Focus the Game window. Press 1 for the blue rapid tower, or 2 for the yellow long-range tower.
3. Left-click an empty green pad to build for 20 gold. Clicking an occupied pad must not charge gold.
4. Build three towers: gold should become 40, 20, 0. A fourth purchase must fail without creating a tower.
5. Right-click an occupied pad to sell for 10 gold. A second right-click on the same pad must not refund again.
6. Press Space to start combat. Five red enemies follow the sand path; health bars shrink on hits.
7. Each kill adds 10 gold. Each enemy reaching the base costs 1 life and adds no gold.
8. Buying and selling still work during combat. The wall panel shows the selected tower and transaction feedback.
9. To test leaks, stop and replay, build nothing, and press Space. Expect 60 gold, 0 lives,
   0 remaining enemies, and BASE LOST. Stop and replay to reset the test.

## Future headset test

The preconfigured rig and interaction manager have been removed from TabletopRoom at the user's request.
Add your own rig later, disabling DesktopCamera so only one camera/audio listener setup is active.
The XR packages and separate XRPlacementTest scene remain available as references.

## Inspector tuning

- `Enemies/Enemy1...5`: Enemy Health (health, reward), Enemy Path Follower (speed, path).
- `Assets/RapidTower.prefab` and `Assets/LongRangeTower.prefab`: range and firing interval.
- `BuildSlot1...6`: build cost, sale refund, tower prefab choices, and enemy references.
- `DesktopBuildInput`: desktop camera, instructions, and preparation/start state.
- `BattleTable/Base`: Base Health (lives).
- `PlayerResources`: starting gold.
- `Room/BattleStatus`: Battle Readout references.

The room is an editable blockout with an open front for the desktop camera. Six green pads support
construction; the remaining tiles are visual only. The tool bench is reserved space. BASE LOST displays
a result and blocks further construction; it does not pause the world or open a restart menu.
The previous test scenes remain separate. The room creation menu makes the original blockout;
the saved scene includes the later building checkpoint.
