using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RBush;

namespace OhioRoadInventoryExtractor
{
    public class Node : ISpatialData
    {
        public long Id;
        public GeoCoordinate Coords;
        public Dictionary<string, string> Tags;
        public string Name;

        public List<Way> Parents = new List<Way>();

        private Envelope env;
        public ref readonly Envelope Envelope
        {
            get { return ref env; }
        }

        public Node(long id, GeoCoordinate coords, Dictionary<string, string> tags = null)
        {
            this.Id = id;
            this.Coords = coords;
            this.Tags = tags;
            if (Tags == null)
                Tags = new Dictionary<string, string>();

            env = new Envelope(Coords.Longitude, Coords.Latitude, Coords.Longitude, Coords.Latitude);
        }

        public Node(XElement elem)
        {
            if (elem.Name != "node")
                throw new Exception("Can't parse a node from this xml");
            Id = long.Parse(elem.Attribute("id").Value);
            Coords = new GeoCoordinate(double.Parse(elem.Attribute("lat").Value), double.Parse(elem.Attribute("lon").Value));

            Tags = new Dictionary<string, string>();
            foreach (var pair in elem.Descendants("tag"))
                Tags.Add(pair.Attribute("k").Value, pair.Attribute("v").Value);

            env = new Envelope(Coords.Longitude, Coords.Latitude, Coords.Longitude, Coords.Latitude);
        }

        public override string ToString()
        {
            return $"node/{Id}";
        }

        public string ToXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"<node id='{Id}' version='1' visible='true' lat='{Coords.Latitude}' lon='{Coords.Longitude}'");
            if (Tags == null || Tags.Count == 0)
                sb.AppendLine(" />");
            else
            {
                sb.AppendLine(">");
                foreach (var pair in Tags)
                {
                    string val = pair.Value.Replace("&", "&amp;").Replace("'", "&apos;").Replace("<", "&lt;").Replace(">", "&gt;");
                    if(!string.IsNullOrEmpty(val))
                        sb.AppendLine($"<tag k='{pair.Key}' v='{val}' />");
                }
                sb.AppendLine("</node>");
            }
            return sb.ToString();
        }
    }
}
