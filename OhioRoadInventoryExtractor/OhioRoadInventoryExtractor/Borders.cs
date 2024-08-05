using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;
using Newtonsoft.Json.Linq;
using RBush;

namespace OhioRoadInventoryExtractor
{
    public class Borders
    {
        public List<BorderSegment> Segments = new List<BorderSegment>();
        public RBush<BorderSegment> SegmentTree = new RBush<BorderSegment>();

        public Borders(string geojsonPath)
        {
            JObject featureCollection = JObject.Parse(File.ReadAllText(geojsonPath));

            foreach(JObject feature in featureCollection["features"].Cast<JObject>())
            {
                string name = feature["properties"]["ref"].ToString();
                if (feature["geometry"]["type"].ToString() != "LineString")
                    throw new Exception("Border not a linestring");

                GeoCoordinate[] points = feature["geometry"]["coordinates"].Select(c => new GeoCoordinate((double)c[1], (double)c[0])).ToArray();
                for(int i = 0; i < points.Length - 1; i++)
                {
                    Segments.Add(new BorderSegment(name, points[i], points[i + 1]));
                }

                SegmentTree.BulkLoad(Segments);
            }
        }
    }
}
