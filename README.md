# SortingVR

An educational VR game about sorting algorithms. Each algorithm gets its own room, and the rules of the room teach you how the algorithm works. The first room, **Bubble Sort**, is in development.

## Bubble Sort Room

You sort 8 test tubes in a bright lab. The height of the liquid in each tube shows its value (1–8).

- The tubes stand in a rack with 8 slots.
- You can only compare **two neighboring tubes**. Put them on the left and right holders of the analytical balance. It lights up under the heavier tube and shows `>`, `<` or `=`.
- If the left tube is heavier, swap the pair when you put it back in the rack. If not, return the tubes as they were.
- Keep making passes until a full pass has no swaps. When the tubes are sorted, the rack reads as a color gradient.

Things in the room that help you:

- **Guide board**: the current pass, comparison and swap counts, the active pair, and the next step. Mistakes are flagged with an orange OOPS.
- **Slot lights**: green when a pair goes back correctly, red under the slot that has a mistake.
- **Code board**: a bubble sort listing in Python or C. The line that matches your last action is highlighted.
- **Variables board**: live values of `n`, `p`, `i`, `a[i]`, `a[i + 1]`, `swapped` and the array.
- **Hover labels**: show a tube's value while you point at it or hold it.

## Tech Stack

- Unity **6000.2.7f2** (Unity 6.2), Universal Render Pipeline 17.2
- XR Interaction Toolkit 3.4, OpenXR (Meta Quest), XR Hands
- TextMeshPro (Ubuntu Mono font)
- Target: **Meta Quest** standalone, 72 FPS (baked lighting, no realtime shadows or post-processing)

## Getting Started

1. Clone the repository and open it in Unity Hub with Unity **6000.2.7f2**.
2. Open `Assets/Scenes/BubbleSort.unity`.
3. **Without a headset:** press Play. The XR Device Simulator is already in the scene. Grab tubes with the controller grip (simulator: select a controller, then `Shift+G`).
4. **On Quest:** switch the build target to Android, check that OpenXR is enabled with the Meta Quest feature group, then use Build And Run.

## Project Structure

```
Assets/
  Scenes/            BubbleSort scene
  Scripts/
    Core/            Shared, room-independent code (SortingVR.Core)
    BubbleSort/      Bubble sort room logic (SortingVR.BubbleSort)
    UI/              Boards, labels, lights (SortingVR.UI)
  Shared/            Shaders (fake liquid), fonts, assets used by every room
  Materials/Lab/     Lab materials and generated textures
  Prefabs/           TestTube prefab
```

## Architecture

The algorithm logic is **plain C# with no UnityEngine dependency**. It raises events, and the visuals, sound and XR layers subscribe to them, so the same logic can be reused in other rooms.

- `SortingVR.Core`: `SortableItem`, `ValueComparer` (counts comparisons), `CodeListing` / `ICodeTrace` / `IVariableWatch` (code and variable boards), `LiquidWobble` (fake liquid physics).
- `SortingVR.BubbleSort`: `BubbleSortGuide` (step state machine: steps, mistakes, pass and completion events), `ComparisonScale`, `BubbleSortCodeTrace`, `BubbleSortVariables`, plus MonoBehaviour drivers that connect the rack sockets to the logic.
- `SortingVR.UI`: `GuideBoardView`, `CodeBoardView`, `VariablesBoardView`, `SlotLights`, `TubeValueLabel`, `FaceHeadset`, `LanguageToggle`.

Hot code is written to be zero-GC. It is event-driven, caches components, and updates text only when a value changes.

## Roadmap

- [x] Bubble Sort room: environment, interaction, guide, code and variables boards
- [ ] Exit door and "sorted" lamp sequence
- [ ] Main menu / room selection
- [ ] Insertion Sort room
- [ ] Merge Sort room

## Third-Party

- [Ubuntu Mono](https://design.ubuntu.com/font) font under the Ubuntu Font Licence.
- Unity packages under their respective licenses.
