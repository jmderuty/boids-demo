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

        public enum RenderingKind
        {
            AddShip,
            DrawShip,
            HideShipe,
            RemoveShip
        }
    }
}
