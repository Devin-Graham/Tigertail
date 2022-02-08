using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TigerTail.FPSController
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [DisallowMultipleComponent]
    public class FPSMovement : MonoBehaviour
    {
        /// <summary>Rigidbody attached to this player object.</summary>
        private Rigidbody rb;

        /// <summary>Capsule Collider attached to this player object.</summary>
        private CapsuleCollider cc;

        [Flags]
        public enum State
        {
            Moving = 1,
            Jumping = 1 << 1,
            Falling = 1 << 2,
            Immobilized = 1 << 3,
            Knockback = 1 << 4
        }
        private State state;

        [Tooltip("Player movement speed.")]
        [Range(0.1f, 20f)]
        [SerializeField] private float moveSpeed = 10f;

        [Tooltip("Force of the player's jump.")]
        [Range(4f, 10f)]
        [SerializeField] private float jumpForce = 6f;

        [Tooltip("Player's ability to influence their movement mid-air.")]
        [Range(0.001f, 0.005f)]
        [SerializeField] private float airStrafeModifier = 0.003f;

        [Tooltip("Distance below player required for them to be considered falling.\nIncrease this value if you find you can't jump while moving downhill slightly.")]
        [Range(0.01f, 0.2f)]
        [SerializeField] private float fallDistanceBuffer = 0.1f;

        /// <summary>Time the player last jumped at.</summary>
        private float lastJumpTime;

        private void Awake()
        {
            cc = GetComponent<CapsuleCollider>();
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            var moveVelocity = HandleMovement();
            var jumpVelocity = HandleJumping();

            HandleMovementByState(moveVelocity, jumpVelocity);
        }

        // Fixed Update was used for this as it is Physics code and all Unity physics runs on this loop.
        // The update rate of FixedUpdate is set in the Unity Player settings under Time. (Set to 200Hz for this project)
        private void FixedUpdate()
        {
            CheckIfTouchingGround(); 
        }

        /// <summary>Checks if the player is currently touching the ground and sets their state accordingly.</summary>
        private void CheckIfTouchingGround()
        {
            const float JUMP_GRACE_TIME = 0.35f;
            if (state.HasFlag(State.Jumping) && Time.time - JUMP_GRACE_TIME < lastJumpTime) // We just started a jump, don't immediately ground us.
                return;

            var fallingRayDistance = cc.height / 2 + fallDistanceBuffer;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, fallingRayDistance))
            {
                ToggleState(State.Jumping | State.Falling, false);
                var adjustedPosition = transform.position;
                adjustedPosition.y = hit.point.y + cc.height / 2;
                transform.position = adjustedPosition;
            }
            else
            {
                ToggleState(State.Falling, true);
            }
        }

        /// <summary>Handles movement on a per-state basis.</summary>
        /// <remarks>If the user is airborne they will need force-based control, while normal movement should be instantaneous.</remarks>
        private void HandleMovementByState(Vector3 moveVelocity, Vector3 jumpVelocity)
        {
            if (HasAnyState(State.Jumping | State.Falling | State.Knockback))
                rb.AddForce(moveVelocity * airStrafeModifier + jumpVelocity, ForceMode.VelocityChange);
            else if (state.HasFlag(State.Moving))
                rb.velocity = moveVelocity;
            else
                rb.velocity = Vector3.zero;
        }

        /// <summary>Returns the velocity vector for a jump.</summary>
        private Vector3 HandleJumping()
        {
            if (HasAnyState(State.Jumping | State.Falling | State.Knockback | State.Immobilized))
                return Vector3.zero;

            if (Input.GetKeyUp(KeyCode.Space))
            {
                ToggleState(State.Jumping, true);
                lastJumpTime = Time.time;
                return Vector3.up * jumpForce;
            }

            return Vector3.zero;
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 500, 20), state.ToString());
        }

        /// <summary>Returns the velocity vector for regular movement.</summary>
        private Vector3 HandleMovement()
        {
            var moveVelocity = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
            {
                moveVelocity += transform.forward;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                moveVelocity -= transform.forward;
            }

            if (Input.GetKey(KeyCode.A))
            {
                moveVelocity -= transform.right;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                moveVelocity += transform.right;
            }

            if (HasAnyState(State.Immobilized))
                moveVelocity = Vector3.zero;

            ToggleState(State.Moving, moveVelocity != Vector3.zero); // We're moving if we have a non-zero velocity.

            // Going forward/back and left/right at the same time creates a right triangle with magnitude sqrt(2).
            // Normalizing this makes you move at the same speed regardless of input combination.
            moveVelocity = moveVelocity.normalized;

            return moveVelocity * moveSpeed;
        }

        /// <summary>Adds knockback to the player.</summary>
        public void AddKnockback(Vector3 knockbackVelocity)
        {
            state |= State.Knockback;
            rb.AddForce(knockbackVelocity);
        }

        /// <summary>Sets a state flag based on whether or not it should be <paramref name="active"/>.</summary>
        /// <param name="state">Which state to modify.</param>
        /// <param name="active">Whether or not the state should be active.</param>
        private void ToggleState(State state, bool active)
        {
            if (active)
            {
                this.state |= state;
                return;
            }

            this.state &= ~state;
        }

        /// <summary>Checks to see if any of the states passed in <paramref name="state"/> are active.</summary>
        private bool HasAnyState(State state)
        {
            return (this.state & state) != 0;
        }
    }
}
