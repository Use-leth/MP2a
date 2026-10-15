# VR bottle-cap shelf test

Save your rig/Simulator scene changes and exit Play. Refresh Assets; if needed use Tools > MP2a > Setup VR Bottle Cap Shelf. The setup targets RoomScaleDefense and does not create or modify an XR rig or interaction manager.

Shelf: VRBottleCapShelf, at (-7.5, 0, -2). You can reposition the entire root in Edit mode. It references the existing PlayerResources. Grab controls follow your current Simulator bindings.

1. With at least 20 SCRAP, grab the displayed tower. Exactly 20 is deducted, and a new display sample appears.
2. Move the held tower over a ground cell near its build height: green if free, red if road/reserved/occupied. Release: it snaps and becomes active. No second charge.
3. Start a wave with SPACE. Pick the tower up during combat: it stops firing. Release on another free cell: it resumes, and the old cell is free. Upgrade state remains on the same object.
4. Drop on an invalid location: no activation, no refund or extra charge; the purchased object falls under physics and can be picked up again.
5. Set PlayerResources.gold below 20 in Play: display shows insufficient funds and the sample cannot be selected. Purchased towers remain movable.
6. Two held towers must not occupy the same slot; only the first valid release claims it. Holding/releasing should never repeatedly deduct currency.

Desktop mouse building is disabled via DesktopBuildInput.enableMouseBuilding to prevent Simulator mouse controls also constructing/selling towers. SPACE remains available for waves. Enable Mouse Building can be restored for desktop tests; desktop pickup/upgrade mouse targeting is not active during this first VR test.

Only shelf-created bottle-cap towers are grabbable in this milestone. Other machines, VR scrap collection, hammer upgrades, and recycling-bin interactions are subsequent steps. The remaining UI and end-to-end XR camera configuration have not been migrated. Runtime/editor compilation passed; Simulator interaction testing is pending.
