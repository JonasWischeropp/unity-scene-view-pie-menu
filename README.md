# Scene View Pie Menu
This small unity package makes it easy to create pie menus in the scene view to trigger custom actions.
The pie menu works similar to the [blender](blender.org) pie menu.

<p align="center">
<img src="demo.gif", alt="Demonstration">
</p>

## Usage
Create a `PieMenu` object with `PieMenuEntry`s and trigger its `Perform` method with a [`ClutchShortcut`](https://docs.unity3d.com/ScriptReference/ShortcutManagement.ClutchShortcutAttribute.html) (see [example](#exmaple)).  
Overloads of the `PieMenuEntry` constructor allow setting the icon with a custom texture or by a string value, to use unity build in icons. A non-exhaustive icon list can be found [here](https://github.com/halak/unity-editor-icons/blob/master/README.md).

## Example
The following example shows how this package can be used to create the pie menu in the demo.
```c#
using JonasWischeropp.Unity.EditorTools.SceneView;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public static class DrawModePieMenu {
    static PieMenu overlay = new PieMenu(new PieMenuEntry[]{
        new PieMenuEntry("Shaded", "TreeEditor.Material", () => SetDrawMode(DrawCameraMode.Normal)),
        new PieMenuEntry("Wireframe", "TreeEditor.Geometry On", () => SetDrawMode(DrawCameraMode.Wireframe)),
        new PieMenuEntry("Shaded Wireframe", "d_PreMatSphere", () => SetDrawMode(DrawCameraMode.TexturedWire)),
    });

    [ClutchShortcut("Draw Mode Pie Menu", typeof(SceneView), KeyCode.Z)]
    static void PerformPieMenu(ShortcutArguments arguments) {
        overlay.Perform(arguments);
    }

    static void SetDrawMode(DrawCameraMode mode) {
        SceneView.lastActiveSceneView.cameraMode = SceneView.GetBuiltinCameraMode(mode);
    }
}
```
> I would recommend to disable `Project Settings > Input Manager > Use Physical Keys`. Otherwise, triggering a shortcut like `KeyCode.Z` would for example require pressing `Y` on a `QWERTZ` keyboard because of the swapped key locations.

## Setup
Installation using the Package Manager:
1. Click on the `+` in the `Package Manager` window
2. Chose `Add package from git URL...`
3. Insert the following URL `https://github.com/JonasWischeropp/unity-scene-view-pie-menu.git`  
A specific [release](https://github.com/JonasWischeropp/unity-scene-view-pie-menu/releases) version can be specified by appending `#<version>` (e.g. `...e-menu.git#1.0.1`).
4. Press the `Add`-Button
