# Room-scale sample

Open `Assets/Scenes/RoomScaleDefense.unity`. The original TabletopRoom remains unchanged.
If needed, outside Play mode use `Tools > MP2a > Create Room Scale Defense` after compiling.

## Test without a headset

1. Press Play. Start in overview, paused for building, with 40 gold and two green tower pads.
2. Use 1/2 to select rapid/long-range tower; left-click each pad to build (20 gold each).
3. Press Tab for the 1.65 m standing viewpoint. Use WASD to walk and hold middle mouse to look.
   You can inspect the room while the battle is still paused.
4. Follow the blue maintenance lane around the board. Inspect tower height and reach the green
   front panel. The tower body is 1.4 m tall; its top cap raises the silhouette slightly.
5. Press Tab to return to overview, then Space to start wave 1. Confirm path movement, tower fire,
   health bars, gold rewards, and base damage still work at the larger scale.
6. When all five enemies are gone, the game pauses for building and adds a 15 gold wave bonus.
   Press Space for wave 2 (7 tougher/faster enemies), then wave 3 (10 tougher/faster enemies).
   The final clear should display ALL WAVES CLEARED. A destroyed base displays BASE LOST.
7. Repeat starting in standing view: clicking a pad must use the currently active camera.
8. Stop Play and confirm normal time resumes. Replay starts in overview/preparation again.

The room is 12 x 11 m, and the floor board is 8 x 6 m. Tower ranges are 2.2/2.7 m; enemy speed
is 0.8 m/s. The front wall is collider-only to preserve the overview camera's view.
The green tower panel is a visual repair placeholder. It has no repair/tool behavior yet.
Desktop walking is a scale-check tool, not an XR rig. No headset or VR rig is included here.
