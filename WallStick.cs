// ============================================
//  Wall Stick Script
//  Made by DecoyVR
// ============================================

using UnityEngine;

/// <summary>
/// Allows the player to "stick" to any surface tagged or layered "Stick"
/// while holding a controller trigger button, and release on trigger up.
/// Attach this to the player object (needs a Rigidbody and a Collider).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WallStick : MonoBehaviour
{
    [Header("Made by DecoyVR")]
    [Space(5)]

    [Header("Stick Settings")]
    [Tooltip("Tag that counts as a stickable surface.")]
    [SerializeField] private string stickTag = "Stick";

    [Tooltip("Layer(s) that count as a stickable surface.")]
    [SerializeField] private LayerMask stickLayer;

    [Header("Input")]
    [Tooltip("Name of the input axis/button for the trigger. " +
             "If using the new XR Input system, see the alternate method below.")]
    [SerializeField] private string triggerButtonAxis = "TriggerButton";
    [SerializeField] private float triggerThreshold = 0.5f;

    private Rigidbody rb;
    private bool isTouchingStickSurface = false;
    private bool isStuck = false;

    // Cached physics state to restore on release
    private bool cachedUseGravity;
    private RigidbodyConstraints cachedConstraints;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        bool triggerHeld = IsTriggerHeld();

        if (triggerHeld && isTouchingStickSurface && !isStuck)
        {
            StickToSurface();
        }
        else if (!triggerHeld && isStuck)
        {
            Unstick();
        }
    }

    private bool IsTriggerHeld()
    {
        // Legacy Input Manager approach (works with axis mapped to trigger)
        return Input.GetAxis(triggerButtonAxis) >= triggerThreshold;

        // --- If using Unity's new Input System / XR Interaction Toolkit instead, ---
        // --- replace the line above with something like: ---
        // UnityEngine.XR.InputDevice device = ...;
        // device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue);
        // return triggerValue >= triggerThreshold;
    }

    private void StickToSurface()
    {
        isStuck = true;

        // Cache current physics settings so we can restore them later
        cachedUseGravity = rb.useGravity;
        cachedConstraints = rb.constraints;

        // Freeze the player in place
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void Unstick()
    {
        isStuck = false;

        // Restore physics settings
        rb.useGravity = cachedUseGravity;
        rb.constraints = cachedConstraints;
    }

    private bool IsStickSurface(Collider other)
    {
        bool tagMatch = !string.IsNullOrEmpty(stickTag) && other.CompareTag(stickTag);
        bool layerMatch = (stickLayer.value & (1 << other.gameObject.layer)) != 0;
        return tagMatch || layerMatch;
    }

    // --- Use these if your player collider is NOT a trigger ---
    private void OnCollisionEnter(Collision collision)
    {
        if (IsStickSurface(collision.collider))
            isTouchingStickSurface = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (IsStickSurface(collision.collider))
        {
            isTouchingStickSurface = false;
            if (isStuck) Unstick(); // safety: unstick if we physically leave the surface
        }
    }

    // --- Use these instead if your player collider IS a trigger ---
    private void OnTriggerEnter(Collider other)
    {
        if (IsStickSurface(other))
            isTouchingStickSurface = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsStickSurface(other))
        {
            isTouchingStickSurface = false;
            if (isStuck) Unstick();
        }
    }
}
