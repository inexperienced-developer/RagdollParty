using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ClientDev
{
    public class TransformUpdate
    {
        public ushort Tick { get; private set; }
        public bool IsTeleport { get; private set; }
        public Vector3 Pos { get; private set; }

        public TransformUpdate(ushort tick, bool isTeleport, Vector3 pos)
        {
            Tick = tick;
            IsTeleport = isTeleport;
            Pos = pos;
        }
    }
}

