using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class DoorRevealAfterPass3D : MonoBehaviour
{
    [Header("Door")]
    [Tooltip("Door object to hide at start and reveal after the player passes this trigger.")]
    [SerializeField] private Transform doorRoot;

    [Tooltip("Hide the door renderers when the scene starts.")]
    [SerializeField] private bool hideDoorOnStart = true;

    [Tooltip("Disable the door colliders until the player passes this trigger.")]
    [SerializeField] private bool disableDoorCollidersOnStart = true;

    [Header("Trigger")]
    [Tooltip("Axis used to decide whether the player crossed this trigger. Local Right usually works for side-scroller gates.")]
    [SerializeField] private Vector3 localPassAxis = Vector3.right;

    [Tooltip("Reveal as soon as the player crosses the trigger center. If disabled, reveal when the player exits from the opposite side.")]
    [SerializeField] private bool revealAtCenter = true;

    [Tooltip("Only reveal once. Keep this enabled for doors that block the return path.")]
    [SerializeField] private bool triggerOnce = true;

    [Tooltip("If enabled, the trigger object is disabled after the door is revealed.")]
    [SerializeField] private bool disableTriggerAfterReveal = true;

    private readonly Dictionary<PlayerMotor3D, float> enteredSides = new Dictionary<PlayerMotor3D, float>();
    private Collider triggerCollider;
    private Renderer[] doorRenderers;
    private Collider[] doorColliders;
    private bool revealed;

    private void Reset()
    {
        doorRoot = transform.parent != null ? transform.parent : transform;
        EnsureTriggerCollider();
    }

    private void Awake()
    {
        EnsureTriggerCollider();
        CacheDoorParts();

        if (hideDoorOnStart || disableDoorCollidersOnStart)
        {
            SetDoorVisible(false);
        }
    }

    private void OnValidate()
    {
        EnsureTriggerCollider();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (revealed && triggerOnce)
        {
            return;
        }

        PlayerMotor3D player = other.GetComponentInParent<PlayerMotor3D>();
        if (player == null)
        {
            return;
        }

        enteredSides[player] = GetPlayerSide(player.transform.position);

        if (revealAtCenter)
        {
            TryRevealAtCenter(player);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (revealed && triggerOnce)
        {
            return;
        }

        PlayerMotor3D player = other.GetComponentInParent<PlayerMotor3D>();
        if (player == null)
        {
            return;
        }

        if (!enteredSides.ContainsKey(player))
        {
            enteredSides[player] = GetPlayerSide(player.transform.position);
        }

        if (revealAtCenter)
        {
            TryRevealAtCenter(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMotor3D player = other.GetComponentInParent<PlayerMotor3D>();
        if (player == null)
        {
            return;
        }

        if (!revealAtCenter && enteredSides.TryGetValue(player, out float enteredSide))
        {
            float exitSide = GetPlayerSide(player.transform.position);
            if (HasCrossed(enteredSide, exitSide))
            {
                RevealDoor();
            }
        }

        enteredSides.Remove(player);
    }

    private void TryRevealAtCenter(PlayerMotor3D player)
    {
        if (!enteredSides.TryGetValue(player, out float enteredSide))
        {
            return;
        }

        float currentSide = GetPlayerSide(player.transform.position);
        if (HasCrossed(enteredSide, currentSide))
        {
            RevealDoor();
        }
    }

    private void RevealDoor()
    {
        if (revealed && triggerOnce)
        {
            return;
        }

        revealed = true;
        SetDoorVisible(true);

        if (disableTriggerAfterReveal && triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }

    private void SetDoorVisible(bool visible)
    {
        CacheDoorParts();

        if (doorRenderers != null)
        {
            for (int i = 0; i < doorRenderers.Length; i++)
            {
                if (doorRenderers[i] != null)
                {
                    doorRenderers[i].enabled = visible || !hideDoorOnStart;
                }
            }
        }

        if (doorColliders != null)
        {
            for (int i = 0; i < doorColliders.Length; i++)
            {
                Collider doorCollider = doorColliders[i];
                if (doorCollider != null && doorCollider != triggerCollider)
                {
                    doorCollider.enabled = visible || !disableDoorCollidersOnStart;
                }
            }
        }
    }

    private void CacheDoorParts()
    {
        Transform root = doorRoot != null ? doorRoot : transform;
        doorRenderers = root.GetComponentsInChildren<Renderer>(true);
        doorColliders = root.GetComponentsInChildren<Collider>(true);
    }

    private void EnsureTriggerCollider()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private float GetPlayerSide(Vector3 playerPosition)
    {
        Vector3 axis = transform.TransformDirection(localPassAxis);
        if (axis.sqrMagnitude < 0.0001f)
        {
            axis = transform.right;
        }

        return Vector3.Dot(playerPosition - transform.position, axis.normalized);
    }

    private static bool HasCrossed(float enteredSide, float currentSide)
    {
        if (Mathf.Abs(enteredSide) < 0.01f)
        {
            return Mathf.Abs(currentSide) > 0.01f;
        }

        return Mathf.Sign(enteredSide) != Mathf.Sign(currentSide) || Mathf.Abs(currentSide) < 0.01f;
    }
}
