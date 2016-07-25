﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models
{
    public class Weapon
    {
        public string id { get; set; }

        public int damage { get; set; }

        public float precision { get; set; }

        public long fireTimestamp { get; set; }

        public int coolDown { get; set; }

        public int range { get; set; }
    }
}