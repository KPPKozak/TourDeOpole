﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TourDeOpole.Models
{
    public class Place
    {
        public int PlaceID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public bool IsCustom { get; set; }
    }
}