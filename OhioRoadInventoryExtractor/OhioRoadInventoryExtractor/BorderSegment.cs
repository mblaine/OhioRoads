using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;
using RBush;

namespace OhioRoadInventoryExtractor
{
    public class BorderSegment : ISpatialData
    {
        public string Name;
        public GeoCoordinate P1;
        public GeoCoordinate P2;

        private Envelope env;
        public ref readonly Envelope Envelope
        {
            get { return ref env; }
        }


        public BorderSegment(string name, GeoCoordinate g1, GeoCoordinate g2)
        {
            Name = name;
            P1 = g1;
            P2 = g2;

            env = new Envelope(Math.Min(P1.Longitude, P2.Longitude), Math.Min(P1.Latitude, P2.Latitude), Math.Max(P1.Longitude, P2.Longitude), Math.Max(P1.Latitude, P2.Latitude));
        }

    }
}
