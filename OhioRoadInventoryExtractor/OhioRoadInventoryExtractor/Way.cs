using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace OhioRoadInventoryExtractor
{
    public class Way
    {
        public long[] NodeIds;
        public long Id;
        public GeoCoordinate[] Coords;
        public Dictionary<string, string> Tags;
        public string Name;
        public List<string> Routes = new List<string>();

        public Way(long id, GeoCoordinate[] coords, Dictionary<string, string> tags = null)
        {
            this.Id = id;
            this.Coords = coords;
            this.Tags = tags;

            if (Tags == null)
                Tags = new Dictionary<string, string>();

            if (Tags.ContainsKey("name"))
                Name = Tags["name"];
            else
                Name = "";
        }

        public Way(XElement elem)
        {
            if (elem.Name != "way")
                throw new Exception("Can't parse a way from this xml");
            Id = long.Parse(elem.Attribute("id").Value);

            List<long> nIds = new List<long>();
            foreach(var nd in elem.Descendants("nd"))
                nIds.Add(long.Parse(nd.Attribute("ref").Value));
            NodeIds = nIds.ToArray();

            Tags = new Dictionary<string, string>();
            foreach (var pair in elem.Descendants("tag"))
                Tags.Add(pair.Attribute("k").Value, pair.Attribute("v").Value);

            if (Tags.ContainsKey("name"))
                Name = Tags["name"];
            else
                Name = "";
        }

        public int IndexOf(long nodeId)
        {
            for (int i = 0; i < NodeIds.Length; i++)
                if (NodeIds[i] == nodeId)
                    return i;
            return -1;
        }

        public override string ToString()
        {
            return $"way/{Id} {Name}".Trim();
        }

        public string ToXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<way id='{Id}' version='1' visible='true'>");
            foreach (long id in NodeIds)
            {
                sb.AppendLine($"<nd ref='{id}' />");
            }
            foreach (var pair in Tags)
            {
                string val = pair.Value.Replace("&", "&amp;").Replace("'", "&apos;").Replace("<", "&lt;").Replace(">", "&gt;");
                if (!string.IsNullOrEmpty(val))
                    sb.AppendLine($"<tag k='{pair.Key}' v='{val}' />");
            }
            sb.AppendLine("</way>");
            return sb.ToString();
        }

    }
}
