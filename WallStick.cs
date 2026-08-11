// ============================================
//  Wall Stick Script (Networked - Photon PUN 2)
//  Made by DecoyVR
// ============================================

using UnityEngine;
using Photon.Pun;

/// <summary>
/// Allows the local player to "stick" to any surface tagged or layered "Stick"
/// while holding a controller trigger button, and release on trigger up.
/// Networked with Photon PUN 2 so other clients in the room can see the
/// player stick and unstick in real time.
///
/// Attach this to your networked Player prefab (needs Rigidbody, Collider,
/// and a PhotonView).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PhotonView))]
public class WallStick : MonoBehaviourPun
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
        // Only the client that owns this player object should read input
        // and drive the physics. Remote copies of this player on other
        // clients just react to RPCs below.
        if (!photonView.IsMine) return;

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

    // ---------------------------------------------------
    //  Local (owner) side: applies physics + tells everyone
    // ---------------------------------------------------

    private void StickToSurface()
    {
        isStuck = true;
        ApplyStickPhysics();

        // Tell every other client this player is now stuck
        photonView.RPC(nameof(RPC_SetStuck), RpcTarget.Others, true);
    }

    private void Unstick()
    {
        isStuck = false;
        ApplyUnstickPhysics();

        // Tell every other client this player let go
        photonView.RPC(nameof(RPC_SetStuck), RpcTarget.Others, false);
    }

    private void ApplyStickPhysics()
    {
        // Cache current physics settings so we can restore them later
        cachedUseGravity = rb.useGravity;
        cachedConstraints = rb.constraints;

        // Freeze the player in place
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void ApplyUnstickPhysics()
    {
        // Restore physics settings
        rb.useGravity = cachedUseGravity;
        rb.constraints = cachedConstraints;
    }

    // ---------------------------------------------------
    //  Remote (non-owner) side: mirrors the stuck state
    // ---------------------------------------------------

    [PunRPC]
    private void RPC_SetStuck(bool stuck)
    {
        // This runs on every OTHER client, for the non-owned copy of this
        // player. It won't drive physics (remote rigidbodies are typically
        // kinematic and moved by a PhotonTransformView instead), but this
        // is where you'd trigger animations, VFX, sound, etc. so it's
        // visually obvious to everyone else that this player is stuck.
        isStuck = stuck;

        // Example hooks (uncomment / wire up to your own systems):
        // animator.SetBool("IsStuck", stuck);
        // stickVFX.SetActive(stuck);
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
        if (!photonView.IsMine) return;

        if (IsStickSurface(collision.collider))
            isTouchingStickSurface = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!photonView.IsMine) return;

        if (IsStickSurface(collision.collider))
        {
            isTouchingStickSurface = false;
            if (isStuck) Unstick(); // safety: unstick if we physically leave the surface
        }
    }

    // --- Use these instead if your player collider IS a trigger ---
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if (IsStickSurface(other))
            isTouchingStickSurface = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine) return;

        if (IsStickSurface(other))
        {
            isTouchingStickSurface = false;
            if (isStuck) Unstick();
        }
    }
}