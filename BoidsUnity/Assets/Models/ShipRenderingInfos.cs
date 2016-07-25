using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Models
{
    public class ShipRenderingInfos
    {
        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }

        public ushort Team { get; set; }

        public RenderingKind Kind { get; set; }

        public List<UsedSkillMsg> Skills { get; set; }

        [Flags]
        public enum RenderingKind
        {
            AddShip = 1,
            DrawShip = 2,
            HideShipe = 4,
            RemoveShip = 8,
            Explode = 16
        }
    }
}
