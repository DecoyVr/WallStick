# Wall Stick Script

**Made by DecoyVR**

A simple Unity script that lets a player "stick" to any surface tagged or layered `Stick` while holding a controller trigger button, and release when the trigger is let go. Designed for VR projects but works for any Rigidbody-based player.

---

## What It Does

- Detects when the player is touching a surface tagged/layered **"Stick"**.
- While touching that surface **and** holding the trigger, the player freezes in place (no gravity, no drifting).
- Releasing the trigger un-freezes the player and restores normal physics.

---

## Setup Instructions

### 1. Add the script to your player
- Drag `WallStick.cs` onto your player GameObject (the one with your VR rig / character controller).
- The script requires a **Rigidbody** — Unity will add one automatically if it's missing.
- Make sure the player also has a **Collider** (e.g. Capsule Collider or Sphere Collider).

### 2. Create the "Stick" tag and/or layer
You can use either a Tag, a Layer, or both — the script checks for a match on either.

**Tag:**
1. Select any stickable object → Inspector → Tag dropdown → **Add Tag...**
2. Create a new tag named `Stick`.
3. Assign it to your wall/ceiling objects.

**Layer:**
1. Select any stickable object → Inspector → Layer dropdown → **Add Layer...**
2. Create a new layer named `Stick`.
3. Assign it to your wall/ceiling objects.
4. On the `WallStick` component in the Inspector, set the **Stick Layer** field to that layer.

You don't need both — just pick whichever fits your project.

### 3. Set up trigger input
By default, the script reads input using Unity's **legacy Input Manager** via `Input.GetAxis("TriggerButton")`.

- Go to **Edit → Project Settings → Input Manager**.
- Make sure an axis named `TriggerButton` exists and is mapped to your VR controller's trigger.
- Or, change the `Trigger Button Axis` field in the Inspector to match whatever axis name you're already using.

> **Using the new Input System or XR Interaction Toolkit instead?**
> Open `WallStick.cs` and look at the `IsTriggerHeld()` method — there's a commented-out example showing how to swap in `UnityEngine.XR.InputDevice` trigger reading. Replace the `Input.GetAxis(...)` line with that approach.

### 4. Choose Collision vs Trigger detection
The script includes **both** detection methods so it works either way:

- If your player's Collider is a **normal collider** (not "Is Trigger"), it uses `OnCollisionEnter` / `OnCollisionExit`.
- If your player's Collider **is** a trigger ("Is Trigger" checked), it uses `OnTriggerEnter` / `OnTriggerExit`.

You can leave both in — only the relevant pair will actually fire, so there's no conflict. If you want a cleaner script, delete whichever pair doesn't apply to your setup.

---

## Inspector Fields

| Field | Description |
|---|---|
| **Stick Tag** | The tag checked for stickable surfaces. Default: `Stick` |
| **Stick Layer** | The layer mask checked for stickable surfaces |
| **Trigger Button Axis** | Name of the input axis mapped to your trigger button |
| **Trigger Threshold** | How far the trigger must be pressed (0–1) to count as "held". Default: `0.5` |

---

## Known Limitations / Things to Test

- While stuck, the Rigidbody's constraints are fully frozen (`FreezeAll`), which locks rotation as well as position. This is usually fine for VR since head/hand movement isn't physics-driven, but test it with your specific rig.
- If the player physically leaves the stick surface's collider while still holding the trigger, they'll automatically unstick (safety check) rather than staying frozen in mid-air.
- This script doesn't include a "peel off" push or visual feedback — it's a bare-bones foundation you can build on.

---

## Credit

This script was made by **DecoyVR**. Please don't take credit for it if you share or repost it.
