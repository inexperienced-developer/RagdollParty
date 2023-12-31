using InexperiencedDeveloper.Core.Controls;
using InexperiencedDeveloper.Multiplayer.Riptide.ClientDev;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InexperiencedDeveloper.ActiveRagdoll
{
    [RequireComponent(typeof(Interpolate))]
    public class Player : MonoBehaviour
    {
        public Ragdoll Ragdoll { get; private set; }
        public PlayerControls Controls { get; private set; }
        public RagdollMovement Movement { get; private set; }
        public Rigidbody[] Rigidbodies { get; private set; }
        public GroundManager GroundManager { get; private set; }
        public PlayerState State { get; private set; }
        public bool Grounded { get; private set; }
        public float Mass { get; private set; }
        public float Weight { get; private set; }
        public bool IsClimbing { get; private set; }

        public Player GrabbedByPlayer;

        public Vector3 TargetDir { get; private set; }

        public bool SkipLimiting;
        public bool Jump;
        private float jumpDelay;
        private float groundDelay;
        public Vector3 Momentum
        {
            get
            {
                Vector3 total = Vector3.zero;
                foreach(Rigidbody rb in Rigidbodies)
                {
                    total += rb.velocity * rb.mass;
                }
                return total;
            }
        }


        //DELETE FOR HACKING
        public float Speed;

        private Vector3[] velocities;

        public static List<Player> all = new List<Player>();

        private void Awake()
        {
            //REMOVE ME
            Grounded = true;
        }

        private void OnEnable()
        {
            all.Add(this);

        }

        private void OnDisable()
        {
            all.Remove(this);
        }

        public void Init()
        {
            Movement = GetComponent<RagdollMovement>();
            GroundManager = GetComponent<GroundManager>();
            Controls = GetComponent<PlayerControls>();
            Ragdoll = GetComponentInChildren<Ragdoll>();
            Ragdoll.BindBall(transform);
            Movement.Init();
            Speed = 400;
            //REMOVEME
            InitBodies();
        }

        private void InitBodies()
        {
            Rigidbodies = GetComponentsInChildren<Rigidbody>();
            velocities = new Vector3[Rigidbodies.Length];
            Mass = 0f;
            for(int i = 0; i < Rigidbodies.Length; i++)
            {
                Rigidbody rb = Rigidbodies[i];
                if(rb != null)
                {
                    rb.maxAngularVelocity = 10;
                    Mass += rb.mass;
                }
            }
            Weight = Mass * -Physics.gravity.y;
        }

        private void FixedUpdate()
        {
            //A LOT TO ADD
            jumpDelay -= Time.fixedDeltaTime;
            ProcessInput();
            Quaternion rot = Quaternion.Euler(Controls.CameraPitchAngle, Controls.CameraYawAngle, 0);
            TargetDir = rot * Vector3.forward;
            //ADD DEAD/SPAWNING CONTROL LOCKS
            //CHECK GROUND ANGLE
            //CHECK GRAB STATUS
            //SET BALL GROUND ANGLE
            //CHECK CLIMB STATUS
            //CHECK GRAB STATUS AGAIN
            //SEE IF SPAWNING
            if(State != PlayerState.Dead)
            {
                //ProcessFall();
                if (Grounded)
                {
                    if (Controls.Jump && jumpDelay <= 0)
                    {
                        State = PlayerState.Jump;
                        Jump = true;
                        jumpDelay = 0.5f;
                        groundDelay = 0.2f;
                    }
                    else if (Controls.WalkSpeed > 0f)
                    {
                        State = PlayerState.Run;
                    }
                    else
                    {
                        State = PlayerState.Idle;
                    }
                }
                //INTEGRATE CLIMBING
            }
            if (SkipLimiting)
            {
                SkipLimiting = false;
                return;
            }
            for(int i = 0; i < Rigidbodies.Length; i++)
            {
                Vector3 vel = velocities[i];
                Vector3 rbVel = Rigidbodies[i].velocity;
                Vector3 diff = rbVel - vel;
                if(Vector3.Dot(vel, diff) < 0)
                {
                    Vector3 normalized = vel.normalized;
                    float magnitude = vel.magnitude;
                    float val = -Vector3.Dot(normalized, diff);
                    float power = Mathf.Clamp(val, 0f, magnitude);
                    diff += normalized * power;
                }
                float magnitudeLimit = 1000f * Time.deltaTime;
                if(diff.magnitude > magnitudeLimit)
                {
                    Vector3 clamp = Vector3.ClampMagnitude(diff, magnitudeLimit);
                    rbVel -= diff - clamp;
                    Rigidbodies[i].velocity = rbVel;
                }
                velocities[i] = rbVel;
            }
        }

        private void ProcessInput()
        {
            if (Movement.enabled)
                Movement.OnFixedUpdate();
        }
    }
}

