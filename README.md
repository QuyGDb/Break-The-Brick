# Introduction

This game is inspired by the core gameplay mechanics of **Break the Sun** *(available on Google Play)*, extended with two camera perspectives — **First Person and Third Person** — alongside an **Idle progression system** that lets players upgrade attributes to overcome increasingly difficult levels.

---

## Purpose

The project serves as a practical stepping stone from 2D to 3D game development. On the technical side, it covers working with the OXYZ coordinate system, 3D geometry, asset management (models, materials, Humanoid animations), physics handling, and performance optimization in a 3D environment. On the design side, it explores Idle, Casual, and Arcade game structures — with a focus on touch-friendly UI/UX and progression mechanics suited for mobile.

---

## Shatter System — Improvements Over Break the Sun

**Assumed approach in Break the Sun:**
Objects are either fully destroyed on trigger, or simulate partial destruction by stacking pre-cut child objects on top of one another.

**The limitation:** Partial destruction based on damage requires child objects whose top and bottom surfaces align precisely — which significantly restricts the range of shapes that can be used as destructible objects.

**The solution implemented here** removes that constraint entirely. Using **Rayfire for Unity**, objects can be shattered into irregular fragments without any manual stacking, supporting far more varied geometry.

---

## Implementation

**Step 1 — Fragment preparation**

Add a **Rayfire Shatter** component to the target object. Configure the fragment count and type, then bake the results into a `gameobject_root` containing all generated pieces.

**Step 2 — DestructibleBrick prefab structure**

The prefab consists of three parts:

- **gameobject_root** — holds all fragments, with a **Rayfire Rigid** component attached. Optionally paired with Rayfire Bust and Rayfire Debris for visual effects on destruction. Full settings are available in the project prefab.

- **Activator** — an empty GameObject with a **Rayfire Activator**, initially positioned above `gameobject_root`. As it moves downward, any fragments it passes through are switched to *dynamic* physics type.

- **Bomb** — an empty GameObject with a **Rayfire Bomb**, with a radius large enough to encompass the entire `gameobject_root`. It detonates after the Activator activates the target fragments.

---

## How It Works

Two Y-axis reference points are recorded when the Activator traverses `gameobject_root`: `topPosition` (the top of the object) and `bottomPosition` (the base). A third value, `offsetPosition`, accounts for a small contact buffer.

When the brick takes damage, the Activator moves downward along the Y-axis by:

```
HP loss (%) × (topPosition − offsetPosition)
```

Fragments within the Activator's new range are set to dynamic, then destroyed by the Bomb detonation.

**Why not use `topPosition − bottomPosition` as the full range?**
With a small damage percentage and large fragments, this would cause the entire object to be destroyed prematurely. Instead, `offsetPosition` acts as a safety margin. When HP reaches zero, the Activator is moved directly to `bottomPosition` and the Bomb clears all remaining fragments.

---

**Unity Version:** `2021.3.43f1`
