# VR Game About Sorting Algorithms

An educational VR game. Each sorting algorithm lives in its own room, and the room's constraints make the player grasp the idea of the algorithm. For now we are building only the first scene: Bubble Sort.

## Technologies

- Unity 6.2, VR template (check that the render pipeline is URP, and do not change it).
- XR Interaction Toolkit 3.x, OpenXR.
- Target: Meta Quest (standalone headset, performance matters). Testing without a headset is done through the XR Device Simulator.
- Code language: C#. Comments and object names in English, discussion with me in English.

## Project Structure (planned)

Scenes (all in Assets/Scenes):
- MainMenu: main menu, room selection. Do NOT create yet.
- BubbleSort: the first room. This is the only one we are working on.
- InsertionSort, MergeSort: future rooms. Do NOT create yet.

Folders (create as needed, do not litter with empty ones):
- Assets/Scenes
- Assets/Scripts (subfolders: Core, BubbleSort, UI)
- Assets/Materials/Lab
- Assets/Prefabs
- Assets/Shared: anything that will be useful across all scenes (XR rig, shared scripts, shared materials)

Code that will be needed in several rooms (the sortable item, the step log, counting comparisons and swaps) should be written as shared code, not inside the BubbleSort folder.

## Code Architecture and C#

- Namespaces strictly follow folders: Assets/Scripts/Core -> `SortingVR.Core`, BubbleSort -> `SortingVR.BubbleSort`, UI -> `SortingVR.UI`, Editor scripts -> `SortingVR.Editor`. One class per file, the file name equals the class name.
- XR Interaction Toolkit 3.x: use only the current API. Do not use obsolete members from 2.x. If you are unsure about a class or event name, check the package sources in Packages/com.unity.xr.interaction.toolkit instead of guessing. Input only through the Input System, not UnityEngine.Input.
- UI: all text through TextMeshPro (including World Space). Update text only when the value changes. The font must support Cyrillic.
- Performance (Zero-GC in hot code): in Update, LateUpdate, FixedUpdate and frequent callbacks, do not create class objects, arrays or lists, and do not use GetComponent, Find*, LINQ, string concatenation or Debug.Log. Cache components in Awake/Start, cache objects like WaitForSeconds. Allocations are acceptable during scene initialization.
- Algorithm logic (steps, comparisons, swaps) is written as pure C# without UnityEngine and reports events. Visuals, sound and the XR layer subscribe to these events. Design the logic so it can be reused in other rooms.
- Inspector fields: [SerializeField] private, not public.

## Room Theme and Style

A bright, pleasant laboratory. Calm, clean, friendly, with nothing gloomy or scary.

Palette (URP Lit, matte materials):

| Purpose | Color |
|---|---|
| Walls | #F4F7F9 (warm white) |
| Floor | #DDE7EC (light gray-blue) |
| Accent 1 (mint) | #8FD3C1 |
| Accent 2 (soft blue) | #A9CCE3 |
| Metal (scale, handles) | #B8C2CC |
| "Done" signal | #6FCF97 (green) |
| "Error" signal | #F2994A (orange, not red) |

Glowing materials use URP Unlit (always full brightness): Lab_Mint_Glow (#8FD3C1; glow strips), Lab_Yellow_Glow (#FFD93D; scale lamp), Lab_Liquid_1 ... Lab_Liquid_8.

Test tube liquids (one URP Unlit material per value, Lab_Liquid_1 ... Lab_Liquid_8, so they read clearly through the glass): 1 #FFD93D, 2 #3DDBB5, 3 #22C7D9, 4 #3FA9F5, 5 #4C6EF5, 6 #8B5CF6, 7 #D65DE0, 8 #FF5C9A. In sorted order the rack reads as a gradient. Glass: Lab_Glass, URP Lit Transparent, white at 15% alpha.

Lighting: soft diffused light, glowing ceiling panels (emissive), fake windows (emissive panels, no real sun). Bake the lighting (Baked), do not use realtime shadows.
Current bright setup: 8 ceiling panels with area lights (Panel_Ceiling_0..7 / Light_Area_0..7, Baked, intensity 10; 6 and 7 are the middle row at (±1.25, 3.17, 0)); 4 window area lights (Light_Window_0..3, Baked, #EAF4FF, intensity 2, 1.1 x 1.2 m, facing into the room); ambient = Trilight (sky #F4F7F9 x1.5, equator #E6ECEF x1.4, ground #D5DEE3 x1.3), not the skybox; LightingSettings: indirect intensity 1.4, albedo boost 1.4. Higher values (ceiling 14, indirect 1.6) blow out the floor and walls to pure white. Re-bake after adding or moving static objects (a bake takes ~20 s).

## Dimensions and Layout

Units: 1 = 1 meter. The origin is at the center of the floor. The player spawns at (0, 0, 0), facing +Z (set by me on the XR Origin Hands (XR Rig) Transform). Dimensions may be changed only at my request.

- Room: 8 m along X, 7 m along Z, height 3.2 m. Walls at x = ±4 and z = ±3.5.
- Scale table (Room/Furniture/Table_Scales, placed by me): at (-1.984, 0, 0.317), rotated 90 degrees, long side along Z, tabletop at 0.9 m.
- Comparison scale (Room/Interactive/Scale_Comparison): on the scale table at (-1.85, 0.90, 0.317), rotated -90 degrees so the player faces it looking along -X. Pan_Left is at the lower Z, Pan_Right at the higher Z (left/right from the player's view). Not parented to the table (the table has a non-uniform scale).
- Table with the test tube rack (Room/Furniture/Table_Rack): at (0, 0, 1.83), tabletop at 0.9 m, about 2.62 m wide (X) and 0.76 m deep (Z).
- Test tube rack (Room/Interactive/Rack_TestTubes): on that table at (0, 0.915, 1.826), uniformly scaled 1.679. In world units it is about 1.68 m long, slots are about 20 cm apart, the rail gap is about 10 cm. Keep this scale.
- Left wall (x = -4): pseudocode panel.
- Right wall (x = +4): exit door with an indicator lamp above it. Door opening z = -0.5..0.5, height 2.1 m, metal frame (Room/Structure/DoorFrame). Room/Interactive/Door_Exit: root on the hinge at (4.03, 0, -0.45) so it can swing later; Door_Slab (mint, 0.9 x 2.06 m, BoxCollider, closes the doorway), small window, kick plate, lever handle. Room/Interactive/Lamp_Sorted at (3.99, 2.38, 0): metal housing + Lamp_Lens (white = off; to be switched to the "done" green when sorting is finished).
- Above the rack on the far wall: pass counter display.
- On the sides: cabinets, shelves with flasks, a plant, decor. Simple shapes only.
- Tabletops use Lab_TableTop (#C5D3DC light gray-blue resin, slightly glossy), not the wall material.
- Floor: Lab_Floor uses a generated tile texture (Assets/Materials/Lab/Textures/Floor_Tile.png, 512 px, one 60 x 60 cm tile with grout on its edges and a faint speckle), tiling 13.667 x 12, offset X 0.167 so a grout line runs through the room center.
- Wall trim (Room/Structure/WallTrim) on every wall: lower panels (wainscot) 0.10-1.0 m in Lab_WallLower (#CFE9E1 soft mint), metal baseboard (0-0.10 m) and metal bumper rail at 1.0 m. Accent stripe (Room/Structure/AccentStripe): soft blue (Lab_SoftBlue) band at 2.68 m on all walls.
- Ceiling: tile grid lines every 0.6 m (Room/Structure/CeilingGrid); Room/Props/Ceiling holds 2 smoke detectors, 6 sprinklers and a cable tray along the far wall at ~3.05 m.
- Color approach: walls stay light, color comes from furniture and fixtures (mint and soft-blue doors, mint stool seats, colorful flasks, soft-blue stripe).
- Base cabinets (Room/Furniture/Cabinets): white carcass (with a BoxCollider), Lab_TableTop countertop at 0.9 m, 0.62 m deep, doors ~0.5 m with metal handles. Far wall x = -3.98..-1.7 and 1.7..3.98 (mint doors); west wall z = 1.6..2.86, east wall z = 0.9..2.86 and -2.9..-1.3 (soft-blue doors).
- Wall shelves (Room/Furniture/Shelves) on the far wall at 1.5 and 1.9 m, x = -3.0..-1.8 and 1.8..3.0, with 24 opaque flasks/bottles/beakers in the liquid colors (Room/Props/Flasks). Two lab stools (Room/Furniture/Stools) at (±2.5, 0, 2.45), mint seats.
- Decor (Room/Props): potted plants in the near corners (±3.55, 0, -3.1) (Lab_Pot #E8B98F, Lab_Leaf #8BC48A); near (south) wall: clock at x = -0.9, y = 2.3; whiteboard at x = 2.0 (1.6 x 1.0 m) with 8 bars short-to-tall in the liquid colors; coat rack with two lab coats at x = -2.75..-2.25. East wall by the door: fire extinguisher at z = -0.95 (Lab_Extinguisher #E86A5A) and a first-aid box at z = -1.0, y = 1.45 (green cross in Lab_Leaf).
- Windows (Room/Structure/Windows): fake windows, 1.1 x 1.2 m, center at 1.95 m, URP Unlit Lab_Window (#E3F3FB) pane with metal frame, cross bars and a white sill. West wall at z = ±2.35, east wall at z = ±2.25.
- Service rail (Room/Props/ServiceRail): metal rail at 1.15 m (just above the cabinet countertops) with outlets (white plate, dark slots) and valves (metal base, mint lever). Far wall x = -3.6..3.6; west wall z = ±(1.4..3.2); east wall z = ±(1.0..3.2).
- Vents (Room/Props/Vents) at 2.9 m: far wall x = ±2.6, near wall x = ±1.5. Signs (Room/Props/Signs): blank plates (mint squares, soft-blue Lab_SoftBlue circles) at ~1.6 m: far wall x = ±3.3, near wall x = 0, east wall next to the door at z = 0.85.
- Kept free on purpose: west wall center (pseudocode panel), far wall above the rack (pass counter display), above the door (Lamp_Sorted).

## Room Game Idea (so the environment serves it)

We sort 8 test tubes. A tube's value is visible from the height of its liquid level.

The room reflects the meaning of the bubble sort algorithm:
- The tubes stand in a row in the rack (8 slots), and the player can see all of them.
- Comparison happens only between two adjacent tubes. The player takes a pair and places them on the scale pans: left and right. The scale tilts toward the larger value.
- If the left one is larger, the tubes must be swapped and returned to the rack. If not, they go back as they were.

## Scene Working Rules

1. Before any changes, get the scene hierarchy and look at what already exists.
2. Work in small steps: one step, then I check it in Play with the simulator. Do not do everything in one huge action.
3. Do not delete or move without my request: XR Origin Hands (XR Rig), EventSystem, XR Device Simulator
4. All room objects live under the root object Room and are organized into groups:
   - Room/Structure: floor, walls, ceiling, doorway
   - Room/Furniture: tables, cabinets, shelves
   - Room/Interactive: scale, rack, test tubes, door, lamp
   - Room/Props: small decor
   - Room/Lighting: light sources, panels, light probes
   - Room/UI: pseudocode panel, counter display
5. Object names are clear and without spaces, in the style: Table_Scale, Rack_TestTubes, Slot_0 ... Slot_7, Pan_Left, Pan_Right, Door_Exit, Lamp_Sorted, Board_Pseudocode, Display_PassCounter. Slots are numbered left to right from the player's point of view.
6. For now only simple shapes (Cube, Cylinder, Sphere, Capsule, Plane). No external assets and no downloading packages without my permission.
7. Create materials in Assets/Materials/Lab, reuse the same materials, do not create duplicates.
8. The floor must have a collider so the player does not fall through.
9. Save the scene after every completed step.
10. If something is ambiguous (size, color, position), ask a question or propose an option instead of silently making something up.

## Performance (Quest)

- Target: 72 FPS. Keep the triangle and material counts small, mark environment objects as Static for batching and light baking.
- Transparency only where it is needed (the glass of the test tubes). Everything else is opaque.
- No realtime shadows, post-processing or heavy shaders.
- Glowing indicators (lamps, LEDs) use URP Unlit materials. The `_EMISSION` keyword on URP Lit materials set from scripts gets switched off again on load in this project, so Lit "emission" is not reliable here.
- Colliders only where interaction is needed, plus the floor and walls.

## Interaction (XR)

- Test tube: prefab Assets/Prefabs/TestTube.prefab, 5 cm in diameter and 25 cm tall, pivot at the bottom center. Children: Glass, Liquid (no colliders). Liquid height = value x 2.5 cm (values 1-8). Root has CapsuleCollider, Rigidbody (0.1 kg, interpolated) and XRGrabInteractable (Use Dynamic Attach on, Throw On Detach off).
- Instances: Room/Interactive/TestTubes/Tube_0 ... Tube_7, starting values left to right 5, 2, 7, 1, 8, 3, 6, 4. Grabbed with the controller grip (simulator: select controller, Shift+G).
- Rack slots Slot_0 ... Slot_7 are XRSocketInteractors with a trigger BoxCollider and an Attach child on top of the mint marker. Each socket starts holding its tube. Socketed tubes are kinematic, so they cannot be knocked over. A tube can be returned to any empty slot; swapping = hold one tube in each hand and put each into the other's slot.
- Comparison scale = analytical balance (no beam, nothing tilts): white Balance_Body, sloped Display_Panel in front (dark screen with TextMeshPro Display_Text, 4 buttons, mint glow strip), glass Chamber (draft shield, Lab_Glass, metal frame, front open, door parked on top, no colliders on glass/frame). Inside: two tube holders Pan_Left / Pan_Right, 20 cm apart (XRSocketInteractors with an Attach child; plate, glowing rim, 3 prongs). Lamp_Left / Lamp_Right are glowing rings (12 cm) at the bottom of each holder, under the holder plate.
- Script SortingVR.BubbleSort.ComparisonScale (event driven, no Update): when both holders have a tube, it compares their SortableItem values through ValueComparer, lights the ring under the heavier tube yellow (Lab_Yellow_Glow: URP Unlit #FFD93D, always full brightness; off = Lab_Wall white) and shows ">" (left heavier), "<" (right heavier) or "="; otherwise both lamps are off and the display shows "--". It shows only which tube is heavier, not by how much.
- Display font: TextMesh Pro "LiberationSans SDF" (its dynamic fallback "LiberationSans SDF - Fallback" covers Cyrillic).
- Shared code: SortingVR.Core.SortableItem (the item's int value, on the TestTube prefab; per-instance values are set on Tube_0 ... Tube_7) and SortingVR.Core.ValueComparer (pure C#, Compare(left, right) returns 1 / -1 / 0, counts comparisons, raises Compared).
- At runtime XRI moves a held/socketed tube to the scene root; this is expected.
- All interactive spots must be within reach: height 0.9-1.4 m, no farther than about 0.7 m from the table.
- Buttons and panels must be readable from a distance of 1-2 m.

## What Not to Do

- Do not change the Unity version, the render pipeline or package versions.
- Do not create other scenes (MainMenu, InsertionSort, MergeSort) and do not delete SampleScene without my request.
- Do not write game logic until I ask for it.
- Do not commit to Git or change .gitignore without being asked.

## How We Work

I describe the goal, you propose a short plan and wait for confirmation, then you do one step and report what changed (which objects were created, and where). If you need a scene screenshot for self-checking, take it through the Unity CLI (the screenshot command).