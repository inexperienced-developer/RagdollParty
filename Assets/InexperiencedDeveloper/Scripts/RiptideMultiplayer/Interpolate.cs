using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ClientDev
{
    public class Interpolate : MonoBehaviour
    {
        [SerializeField] private float timeElapsed = 0;
        [SerializeField] private float timeToReachTarget = 0.05f;
        [SerializeField] private float movementThreshold = 1f;

        private readonly List<TransformUpdate> futureTransformUpdates = new();
        private float squareMovementThreshold;
        private TransformUpdate to;
        private TransformUpdate from;
        private TransformUpdate prev;

        private void Start()
        {
            squareMovementThreshold = movementThreshold * movementThreshold;
            to = new TransformUpdate(NetworkManager.Instance.ServerTick, false, transform.position);
            from = new TransformUpdate(NetworkManager.Instance.LerpTick, false, transform.position);
            prev = new TransformUpdate(NetworkManager.Instance.LerpTick, false, transform.position);
        }

        private void Update()
        {
            for(int i = 0; i < futureTransformUpdates.Count; i++)
            {
                if(NetworkManager.Instance.ServerTick >= futureTransformUpdates[i].Tick)
                {
                    if (futureTransformUpdates[i].IsTeleport)
                    {
                        to = futureTransformUpdates[i];
                        from = to;
                        prev = to;
                        transform.position = to.Pos;
                    }
                }
                else
                {
                    prev = to;
                    to = futureTransformUpdates[i];
                    from = new TransformUpdate(NetworkManager.Instance.LerpTick, false, transform.position);
                }

                futureTransformUpdates.RemoveAt(i);
                i--;
                timeElapsed = 0;
                timeToReachTarget = (to.Tick - from.Tick) * Time.fixedDeltaTime;
            }

            timeElapsed += Time.deltaTime;
            LerpPos(timeElapsed / timeToReachTarget);
        }

        private void LerpPos(float lerpAmount)
        {
            if((to.Pos - prev.Pos).sqrMagnitude < squareMovementThreshold)
            {
                if(to.Pos != from.Pos)
                {
                    //transform.position = Vector3.Lerp(from.Pos, to.Pos, lerpAmount);
                    transform.position = to.Pos;
                }
                return;
            }

            //transform.position = Vector3.LerpUnclamped(from.Pos, to.Pos, lerpAmount);
            transform.position = from.Pos;
        }
        
        public void NewUpdate(ushort tick, bool isTeleport, Vector3 pos)
        {
            if (tick <= NetworkManager.Instance.LerpTick && !isTeleport) return;

            for(int i = 0; i < futureTransformUpdates.Count; i++)
            {
                if(tick < futureTransformUpdates[i].Tick)
                {
                    futureTransformUpdates.Insert(i, new TransformUpdate(tick, isTeleport, pos));
                    return;
                }
            }

            futureTransformUpdates.Add(new TransformUpdate(tick, isTeleport, pos));
        }
    }
}

