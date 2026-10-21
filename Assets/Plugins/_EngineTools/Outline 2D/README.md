# Simple Outline
A pixel perfect outline for 2D sprites in Unity URP. One component, no material to set up.

Add the component to a GameObject with a SpriteRenderer and the outline appears. It reads the sprite alpha around every pixel, so it follows the exact shape of the art, including holes and thin parts.
The shader is a Shader Graph asset built from plain nodes. They sit in labelled groups, so it is easy to read and change. There are no textures and no HLSL.

## Requirements
- Unity 6000.0 or newer with the Universal Render Pipeline and Shader Graph.
- SpriteRenderer only. Built-in and HDRP are not supported.

## Quick start
1. Select a GameObject that has a SpriteRenderer.
2. Add Component > Roam Moon > Simple Outline.
3. Pick the Outline Color and set Thickness.

## Demo scene

| Item | What it is |
|---|---|
| `DemoScene.unity` | Several ducks side by side, each with different outline settings. |
| `Prefabs` | Two ready ducks. One uses the component, the other uses a material from the Materials folder. |

The demo scene uses the pack's own URP asset and 2D Renderer (`Settings/Simple Outline Pipeline`). Opening it switches the project to them, and opening any other scene switches back. You can also do it by hand from Tools > Roam Moon > Simple Outline.

## Settings

| Setting | What it does |
|---|---|
| Outline Color | HDR color. The alpha channel sets the opacity. Values above 1 glow when your project has Bloom. |
| Thickness | 0 to 16. Width in texture pixels, not screen pixels. |
| Alpha Threshold | 0 to 1. The alpha above which a pixel counts as solid. Lower it for sprites with soft edges. |
| Edge Softness | 0 to 1. 0 is a hard edge, good for pixel art. Higher values fade the outline. |
| Diagonal Samples | Looks in 8 directions instead of 4. Corners come out rounder. Turn it off for a blocky look. |
| Inner Outline | Draws the outline inside the silhouette instead of outside. |
| Outline Only | Hides the sprite and keeps only the outline. |
| Pulse, Pulse Speed | Breathes the outline brightness between 1x and 5x. Speed 3 is about two seconds per cycle. |

The Advanced foldout holds the Outline Shader field. The component fills it in for you, so leave it alone.

## Sprite import
- Outlines grow outward, so the sprite needs empty space around it inside its rectangle. Set Mesh Type to Full Rect. With Tight, the mesh hugs the sprite and cuts the outline off.
- If the sprite is in a Sprite Atlas, set the Padding higher than your largest Thickness. Without padding the outline picks up neighbouring sprites.

## Good to know

**Bloom.** Outline Color is HDR. A color around 4 with a Bloom threshold near 1 gives a hot glow. Without Bloom everything still works, it just does not glow.

**Sprite color.** The SpriteRenderer color tints the sprite and the outline together, the same way Unity tints a normal sprite.

**Lights.** The shader is unlit. Light 2D objects do not change the sprite or the outline.

**Swapping sprites.** The component tells the shader the size of the sprite texture and updates it when the sprite changes, so frame animations keep a steady thickness.

## Scripting
```csharp
var outline = enemy.GetComponent<SpriteOutlineEffect>();
outline.OutlineColor = Color.red;
outline.Thickness = 3f;
outline.Pulse = true;

enemy.AddComponent<SpriteOutlineEffect>();
```
Every setting is a property with the same name as the Inspector field, and it applies right away, so it is safe to change per frame.
To use a material instead of the component, create one from `Roam Moon/Simple Outline` and set Texture Size to the size of your texture in pixels, for example 512 by 512. A material has no component to fill it in. The `Duck Outline` material in the Materials folder shows it.

## Performance
- Nine texture reads per pixel: the sprite and eight neighbours. That is cheap for normal 2D scenes. Avoid very large Thickness on many full screen sprites.
- Each sprite has its own small material, and settings go through a MaterialPropertyBlock, so these sprites are not SRP batched.
- Nothing allocates per frame.

## Problems

| You see | Try |
|---|---|
| Shader not found | URP or Shader Graph is missing. Install both from the Package Manager. |
| The outline is cut off at the sprite edge | Mesh Type is Tight, or there is not enough empty space. Use Full Rect. |
| The outline is the wrong width on a material | Texture Size on the material does not match the texture. Set it in pixels. |
| The outline does not glow | There is no Bloom. Add a Global Volume with a Bloom override. |
| The outline picks up other sprites | The sprite is in an atlas without enough Padding. |
| My original material came back | That is intended. The component restores the material the renderer had before. |

## Folder
Shaders (the Shader Graph), Scripts (the component), Editor (the Inspector and the pipeline switch for the demo), Settings (URP asset and 2D Renderer for the demo), Materials and Prefabs (the demo ducks), Demo Scene and Sprites.

## More
Advanced Outline is the larger package from the same author: 24 animated outline styles including Fire, Ice and Electric, plus lit variants.

## Support
Roam Moon. When you write, include your Unity and URP version and a screenshot if you can.
