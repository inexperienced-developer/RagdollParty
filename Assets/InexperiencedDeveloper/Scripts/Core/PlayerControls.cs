using InexperiencedDeveloper.Utils;
using InexperiencedDeveloper.ActiveRagdoll;
using System.Collections.Generic;
using UnityEngine;

namespace InexperiencedDeveloper.Core.Controls
{
    public class PlayerControls : MonoBehaviour
    {
        public PlayerInputActions PlayerActions;
        public ActiveRagdoll.Player Player { get; private set; }
        public Vector3 Movement { get; private set; }
        public Vector2 Look { get; private set; }
        public float CameraPitchAngle { get; private set; }
        public float CameraYawAngle { get; private set; }
        public float TargetPitchAngle { get; private set; }
        public float TargetYawAngle { get; private set; }
        public float LeftExtend { get; private set; }
        public float RightExtend { get; private set; }
        public bool LeftGrab { get; private set; }
        public bool RightGrab { get; private set; }
        public bool Jump { get; private set; }

        public Vector3 WalkDir { get; private set; }
        public float WalkSpeed { get; private set; }
        private Vector3 walkLocalDir;
        private Vector3 lastWalkDir;
        public float UnsmoothedWalkSpeed;

        private List<float> mouseInputsX = new List<float>();
        private List<float> mouseInputsY = new List<float>();

        private void Awake()
        {
            PlayerActions = new PlayerInputActions();
            Player = GetComponent<ActiveRagdoll.Player>();
        }

        private void OnEnable()
        {
            PlayerActions.Enable();
        }

        private void OnDisable()
        {
            PlayerActions.Disable();
        }

        private void Update()
        {
            ReadInput();
            HandleInput();
        }

        public void ReceiveInputs(Vector3 movement, Vector2 lookDir, bool jump)
        {
            Movement = movement;
            CameraPitchAngle = lookDir.x;
            CameraYawAngle = lookDir.y;
            Jump = jump;
        }

        private void ReadInput()
        {
            Movement = CalcKeyWalk;
            //Add conditionals for mouse/controller
            CameraYawAngle += Smoothing.SmoothValue(mouseInputsX, CalcKeyLook.x);
            CameraPitchAngle -= Smoothing.SmoothValue(mouseInputsY, CalcKeyLook.y);
            CameraPitchAngle = Mathf.Clamp(CameraPitchAngle, -80f, 80f);
            Look = CalcKeyLook;
            Jump = GetJump;
            LeftExtend = GetLeftExtend();
            RightExtend = GetRightExtend();
        }

        private void HandleInput()
        {
            walkLocalDir = Movement;
            TargetPitchAngle = Mathf.MoveTowards(TargetPitchAngle, CameraPitchAngle, 180f * Time.fixedDeltaTime / 0.1f);
            TargetYawAngle = CameraYawAngle;
            Quaternion rot = Quaternion.Euler(0, CameraYawAngle, 0);
            Vector3 dir = rot * walkLocalDir;
            UnsmoothedWalkSpeed = dir.magnitude;
            dir = new Vector3(FilterAxisAcceleration(lastWalkDir.x, dir.x), 0f, FilterAxisAcceleration(lastWalkDir.z, dir.z));
            WalkSpeed = dir.magnitude;
            if(WalkSpeed > 0f)
            {
                WalkDir = dir;
            }
            lastWalkDir = dir;
            LeftGrab = LeftExtend > 0;
            RightGrab = RightExtend > 0;
        }

        private float FilterAxisAcceleration(float currVal, float desiredVal)
        {
            float timeStep = Time.fixedDeltaTime;
            float min = 0.2f;
            if (currVal * desiredVal <= 0)
                currVal = 0;
            if (Mathf.Abs(currVal) > Mathf.Abs(desiredVal))
                currVal = desiredVal;
            if (Mathf.Abs(currVal) < min)
                timeStep = Mathf.Max(timeStep, min - Mathf.Abs(currVal));
            if (Mathf.Abs(currVal) > 0.8f)
                timeStep /= 3f;
            return Mathf.MoveTowards(currVal, desiredVal, timeStep);
        }

        public Vector3 CalcKeyWalk
        {
            get
            {
                Vector2 movement = PlayerActions.Player.Movement.ReadValue<Vector2>();
                return new Vector3(movement.x, 0, movement.y);
            }
        }

        public Vector2 CalcKeyLook => PlayerActions.Player.Look.ReadValue<Vector2>();
        public bool GetJump => PlayerActions.Player.Jump.IsPressed();
        public float GetLeftExtend() => PlayerActions.Player.LeftHand.ReadValue<float>();
        public float GetRightExtend() => PlayerActions.Player.RightHand.ReadValue<float>();

    }
}