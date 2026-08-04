using System;
using UnityEngine;

namespace Olallieberry
{
    /// <summary>
    /// Moves an <see cref="OWRigidbody"/> between a series of waypoints.
    /// Supports looping, ping-pong movement, waiting, and configurable starting behavior.
    /// The platform always keeps the rotation of the first waypoint.
    /// </summary>
    [RequireComponent(typeof(OWRigidbody))]
    public class WaypointMovingPlatform : MonoBehaviour
    {
        /// <summary>
        /// The waypoints the platform moves between.
        /// The first waypoint also determines the platform's fixed rotation.
        /// </summary>
        [Header("Waypoints")]
        [Tooltip("The waypoints the platform moves between.\nThe first waypoint also determines the platform's fixed rotation.")]
        public Transform[] waypoints = Array.Empty<Transform>();

        /// <summary>
        /// Time to wait after reaching a waypoint.
        /// </summary>
        [Header("Movement")]
        [Tooltip("Time to wait after reaching a waypoint.")]
        public float waitTime = 0f;

        /// <summary>
        /// Movement speed in units per second.
        /// </summary>
        [Tooltip("Movement speed in units per second.")]
        public float moveSpeed = 2f;

        /// <summary>
        /// Index of the waypoint used when the platform initializes.
        /// </summary>
        [Header("Startup")]
        [Tooltip("Index of the waypoint used when the platform initializes.")]
        public int startingWaypoint = 0;

        /// <summary>
        /// Whether the platform begins moving when initialized.
        /// </summary>
        [Tooltip("Whether the platform begins moving when initialized.")]
        public bool startMoving = true;

        /// <summary>
        /// Whether to restart from the first waypoint after reaching the last.
        /// Ignored when <see cref="pingPong"/> is enabled.
        /// </summary>
        [Header("Looping")]
        [Tooltip("Whether to restart from the first waypoint after reaching the last.\nIgnored when Ping Pong is enabled.")]
        public bool loop = true;

        /// <summary>
        /// Whether to reverse direction after reaching either end of the waypoint list.
        /// </summary>
        [Header("Ping Pong")]
        [Tooltip("Whether to reverse direction after reaching either end of the waypoint list.")]
        public bool pingPong = false;

        /// <summary>
        /// Whether the platform initially moves backward through the waypoint list.
        /// Only used when <see cref="pingPong"/> is enabled.
        /// </summary>
        [Tooltip("Whether the platform initially moves backward through the waypoint list.\nOnly used when Ping Pong is enabled.")]
        public bool startReversing = false;

        private OWRigidbody _owBody;
        private int _index;
        private bool _reversing;
        private bool _isMoving;
        private bool _stopAtTarget;
        private float _waitTimer;
        private Vector3 _holdPosition;
        private Quaternion _fixedRotation;

        public void OnValidate()
        {
            if (waypoints == null || waypoints.Length == 0)
                return;

            startingWaypoint = Mathf.Clamp(startingWaypoint, 0, waypoints.Length - 1);
        }

        /// <summary>
        /// Initializes the platform and creates a default waypoint if none exist.
        /// </summary>
        public void Awake()
        {
            _owBody = this.GetAttachedOWRigidbody(false);

            if (waypoints == null || waypoints.Length == 0)
            {
                GameObject waypointObject = new GameObject(name + "_Waypoint");
                waypointObject.transform.SetParent(_owBody.GetOrigParent());
                waypointObject.transform.SetPositionAndRotation(
                    transform.position,
                    transform.rotation);

                waypoints = new[] { waypointObject.transform };
            }

            _index = Mathf.Clamp(startingWaypoint, 0, waypoints.Length - 1);
            _reversing = startReversing;
            _fixedRotation = waypoints[0] != null ? waypoints[0].rotation : transform.rotation;
            _holdPosition = _owBody.GetPosition();

            MoveToWaypoint(_index);
            _holdPosition = _owBody.GetPosition();

            _waitTimer = 0f;
            _isMoving = startMoving;
        }

        /// <summary>
        /// Updates the platform's movement toward the current waypoint.
        /// </summary>
        public void UpdateMovement(float deltaTime)
        {
            KeepFixedRotation();

            if (!_isMoving)
            {
                HoldPosition();
                return;
            }

            if (waypoints == null || waypoints.Length == 0)
                return;

            if (_index < 0 || _index >= waypoints.Length)
                return;

            if (_waitTimer > 0f)
            {
                HoldPosition();
                _waitTimer = Mathf.Max(0f, _waitTimer - deltaTime);
                return;
            }

            Transform target = waypoints[_index];

            if (target == null)
            {
                AdvanceIndex();
                return;
            }

            Vector3 currentPosition = _owBody.GetPosition();
            Vector3 toTarget = target.position - currentPosition;

            if (toTarget.magnitude <= 0.001f) // position tolerance. distance from a waypoint at which it is considered reached.
            {
                ReachWaypoint(target);
                return;
            }

            float desiredSpeed = 5f * Mathf.Sqrt(toTarget.magnitude);
            float distanceCap = deltaTime > 0f
                ? Mathf.Min(moveSpeed, toTarget.magnitude / deltaTime)
                : 0f;

            desiredSpeed = Mathf.Min(desiredSpeed, distanceCap);

            Vector3 velocity = toTarget.normalized * desiredSpeed;
            _owBody.SetVelocity(GetParentPointVelocity(currentPosition) + velocity);
        }

        /// <summary>
        /// Updates the platform each physics frame.
        /// The component remains enabled even while the platform is stopped.
        /// </summary>
        public void FixedUpdate() => UpdateMovement(Time.fixedDeltaTime);

        /// <summary>
        /// Handles arriving at the current waypoint.
        /// </summary>
        private void ReachWaypoint(Transform waypoint)
        {
            _holdPosition = waypoint.position;

            HoldPosition();

            bool reachedFinalWaypoint =
                !pingPong &&
                !loop &&
                _index >= waypoints.Length - 1;

            if (_stopAtTarget || reachedFinalWaypoint)
            {
                FinishStopping();
                return;
            }

            _waitTimer = GetWaitTime(_index);
            AdvanceIndex();
        }

        /// <summary>
        /// Keeps the platform at its last stopped or reached position.
        /// </summary>
        private void HoldPosition()
        {
            _owBody.MoveToPosition(_holdPosition);
            _owBody.SetVelocity(GetParentPointVelocity(_holdPosition));
        }

        /// <summary>
        /// Keeps the platform at the rotation of the first waypoint.
        /// </summary>
        private void KeepFixedRotation()
        {
            _owBody.MoveToRotation(_fixedRotation);
            _owBody.SetAngularVelocity(Vector3.zero);
        }

        /// <summary>
        /// Returns the parent body's velocity at the supplied position.
        /// </summary>
        private Vector3 GetParentPointVelocity(Vector3 position)
        {
            OWRigidbody parentBody = _owBody.GetOrigParentBody();
            return parentBody != null ? parentBody.GetPointVelocity(position) : Vector3.zero;
        }

        /// <summary>
        /// Returns the configured wait duration for a waypoint.
        /// </summary>
        private float GetWaitTime(int waypointIndex)
        {
            return Mathf.Max(0f, waitTime);
        }

        /// <summary>
        /// Advances to the next waypoint based on the current movement mode.
        /// </summary>
        private void AdvanceIndex()
        {
            if (waypoints.Length <= 1)
                return;

            if (pingPong)
            {
                _index += _reversing ? -1 : 1;

                if (_index >= waypoints.Length)
                {
                    _index = waypoints.Length - 2;
                    _reversing = true;
                }
                else if (_index < 0)
                {
                    _index = 1;
                    _reversing = false;
                }

                return;
            }

            _index++;

            if (_index >= waypoints.Length)
                _index = loop ? 0 : waypoints.Length - 1;
        }

        /// <summary>
        /// Immediately moves the platform to the specified waypoint.
        /// </summary>
        private void MoveToWaypoint(int waypointIndex)
        {
            Transform waypoint = waypoints[waypointIndex];

            if (waypoint == null)
                return;

            _owBody.MoveToPosition(waypoint.position);
            _owBody.MoveToRotation(_fixedRotation);
            _holdPosition = waypoint.position;
        }

        /// <summary>
        /// Starts or resumes movement toward the current target waypoint.
        /// </summary>
        public void StartMoving()
        {
            _stopAtTarget = false;
            _isMoving = true;
        }

        /// <summary>
        /// Finishes moving to the current target waypoint, then stops there.
        /// If the platform is already waiting at a waypoint, it stops immediately.
        /// </summary>
        public void StopMoving()
        {
            if (!_isMoving)
                return;

            if (_waitTimer > 0f)
            {
                FinishStopping();
                return;
            }

            _stopAtTarget = true;
        }

        /// <summary>
        /// Completes a stop without disabling or suspending the platform.
        /// </summary>
        private void FinishStopping()
        {
            _isMoving = false;
            _stopAtTarget = false;
            _waitTimer = 0f;
            HoldPosition();
        }

        /// <summary>
        /// Toggles platform movement.
        /// </summary>
        public void ToggleMoving()
        {
            if (_isMoving)
                StopMoving();
            else
                StartMoving();
        }

        /// <summary>
        /// Changes the current target waypoint.
        /// </summary>
        public void SetWaypoint(int waypointIndex, bool snapToPoint = false)
        {
            if (waypoints == null || waypoints.Length == 0)
                return;

            _index = Mathf.Clamp(waypointIndex, 0, waypoints.Length - 1);
            _waitTimer = 0f;
            _stopAtTarget = false;

            if (snapToPoint)
                MoveToWaypoint(_index);
        }

        /// <summary>
        /// Resets the platform to its configured starting waypoint.
        /// </summary>
        public void ResetToStart()
        {
            if (waypoints == null || waypoints.Length == 0)
                return;

            _index = Mathf.Clamp(startingWaypoint, 0, waypoints.Length - 1);
            _reversing = startReversing;
            _waitTimer = 0f;
            _stopAtTarget = false;

            MoveToWaypoint(_index);

            if (!_isMoving)
                HoldPosition();
        }
    }
}