using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Esri.FileGDB;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using RBush;

namespace OhioRoadInventoryExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            //Road Inventory must be downloaded and extracted as a geodatabase
            //https://gis.dot.state.oh.us/tims_classic/Data/Download
            //See BUILDING.txt for information on dependencies
            const string gdbDir = @"path/where/geodatabase/extracted";
            const string outputDir = @".\OH";

            Directory.CreateDirectory(outputDir);

            const double delta = 0.00005d; //used as an offset, in degrees lat or lon, when searching for nodes around a particular one
            const double epsilon = 0.0000001d; //used for deciding if two coordinate pairs are close enough to be equivalent

            if (!Directory.Exists(gdbDir))
                throw new Exception("Input geodatabase location must be configured first!");

            //Ohio county abbreviations and those of the other counties bordering them
            Dictionary<string, string[]> counties = File.ReadAllLines("ohioCounties.txt").Where(l => !string.IsNullOrEmpty(l)).ToDictionary(l => l.Split('\t')[0], l => l.Split('\t')[1].Split(','));

            //for finding linestrings that end near a county border, since we only load one county at a time and need to find connections accross those borders
            Borders borders = new Borders(@".\ohio-counties.geojson");

            //this program outputs osm xml with arbitrary ids that are not actually valid but have to be positive to work with Mapnik
            long nextNodeId = 1;
            long nextWayId = 1;

            //the maximum number of named ways dead ending at the same node, just for information
            int maxAtOneNode = 0;

            List<Node> nearBorderNodes = new List<Node>();
            RBush<Node> nearBorderNodeTree = new RBush<Node>();

            using (Geodatabase geodatabase = Geodatabase.Open(gdbDir))
            using (Table t = geodatabase.OpenTable("RoadInventory"))
            {
                Dictionary<string, int> columns = new Dictionary<string, int>();
                Dictionary<int, FieldType> columnTypes = new Dictionary<int, FieldType>();
                for (int i = 0; i < t.FieldInformation.Count; i++)
                {
                    columns.Add(t.FieldInformation.GetFieldName(i), i);
                    columnTypes.Add(i, t.FieldInformation.GetFieldType(i));
                }

                CoordinateSystemFactory cFac = new CoordinateSystemFactory();
                CoordinateTransformationFactory tFac = new CoordinateTransformationFactory();
                var transform = tFac.CreateFromCoordinateSystems(ProjectedCoordinateSystem.WebMercator, cFac.CreateFromWkt(@"GEOGCS[""NAD83"",
    DATUM[""North_American_Datum_1983"",
        SPHEROID[""GRS 1980"",6378137,298.257222101],
        TOWGS84[0,0,0,0,0,0,0]],
    PRIMEM[""Greenwich"",0,
        AUTHORITY[""EPSG"",""8901""]],
    UNIT[""degree"",0.0174532925199433,
        AUTHORITY[""EPSG"",""9122""]],
    AUTHORITY[""EPSG"",""4269""]]"));

                Console.WriteLine("==first pass==");
                //first of two passes:
                //for each county:
                //get all roads
                //transform their tags as needed
                //merge co-located nodes
                //output as osm xml
                foreach (string county in counties.Keys)
                {
                    Console.WriteLine(county);

                    Dictionary<string, Way> ways = new Dictionary<string, Way>();
                    //if a road has multiple route numbers, it is represented as multiple linestrings at the same coordinates, 
                    //with most properties stored just on one main linestring and the rest just containing IDs and the additional route numbers
                    List<Way> overlappingWays = new List<Way>();

                    foreach (Row r in t.Search("*", $"COUNTY_CD='{county}'", RowInstance.Recycle))
                    {
                        Dictionary<string, string> tags = new Dictionary<string, string>();
                        foreach (var pair in columns)
                        {
                            if (pair.Key == "SHAPE")
                                continue;

                            if (r.IsNull(pair.Value))
                                continue;

                            switch (columnTypes[pair.Value])
                            {
                                case FieldType.Date:
                                    tags.Add(pair.Key, r.GetDate(pair.Value).ToString("yyyy-MM-dd"));
                                    break;
                                case FieldType.Double:
                                    tags.Add(pair.Key, r.GetDouble(pair.Value).ToString());
                                    break;
                                case FieldType.Integer:
                                    tags.Add(pair.Key, r.GetInteger(pair.Value).ToString());
                                    break;
                                case FieldType.String:
                                    tags.Add(pair.Key, r.GetString(pair.Value));
                                    break;
                                case FieldType.OID:
                                    tags.Add(pair.Key, r.GetOID().ToString());
                                    break;
                                case FieldType.SmallInteger:
                                    tags.Add(pair.Key, r.GetShort(pair.Value).ToString());
                                    break;
                                default:
                                    throw new Exception(columnTypes[pair.Value].ToString());
                            }

                            //change all zero widths to null
                            if (pair.Key.Contains("WIDTH") && tags[pair.Key] == "0")
                                tags.Remove(pair.Key);
                        }

                        MultiPartShapeBuffer g = r.GetGeometry();
                        int numlines = g.NumParts;

                        List<GeoCoordinate[]> lines = new List<GeoCoordinate[]>();
                        for (int i = 0; i < numlines; i++)
                        {
                            List<GeoCoordinate> points = new List<GeoCoordinate>();
                            int end = i < numlines - 1 ? g.Parts[i + 1] : g.NumPoints;
                            for (int j = g.Parts[i]; j < end; j++)
                            {
                                Point pt = g.Points[j];
                                double[] after = transform.MathTransform.Transform(new double[] { pt.x, pt.y });
                                points.Add(new GeoCoordinate(after[1], after[0]));
                            }
                            lines.Add(points.ToArray());
                        }

                        if (lines.Count > 1)
                        {
                            throw new Exception("MultiLineString");//there aren't any in the road inventory at least
                        }
                        else if (lines.Count == 1)
                        {
                            //LineString
                            Way w = new Way(nextWayId++, lines.First(), tags);

                            if (!tags.ContainsKey("ROADWAY_INVENTORY_ID"))
                                throw new Exception("No primary key: ROADWAY_INVENTORY_ID");
                            string key = tags["ROADWAY_INVENTORY_ID"];

                            if (tags.ContainsKey("ROUTE_TYPE") && tags["ROUTE_TYPE"] != "*" && tags.ContainsKey("ROUTE_NBR") && tags["ROUTE_NBR"] != "*")
                            {
                                string rte = tags["ROUTE_TYPE"] + " " + Regex.Replace(tags["ROUTE_NBR"], "^0+", "");//trim leading zeros, all route numbers are 5 digits
                                if (tags.ContainsKey("ROUTE_SUFFIX") && tags["ROUTE_SUFFIX"] != "*")//ODOT uses asterisks for nulls in certain fields
                                    rte += tags["ROUTE_SUFFIX"];
                                if (tags.ContainsKey("ROUTE_EXTENSION_CD") && tags["ROUTE_EXTENSION_CD"] != "*")
                                    rte += tags["ROUTE_EXTENSION_CD"];
                                w.Routes.Add(rte);
                            }

                            if (tags.ContainsKey("PRIMARY_OVERLAP_ID"))
                                overlappingWays.Add(w);
                            else if (tags.ContainsKey("SPLIT_JUR_OVERLAP_ID"))
                                continue;//discard
                            else
                                ways.Add(key, w);
                        }
                        else
                            continue;
                    }

                    foreach (Way w in overlappingWays)
                    {
                        if (ways.ContainsKey(w.Tags["PRIMARY_OVERLAP_ID"]))
                        {
                            Way primary = ways[w.Tags["PRIMARY_OVERLAP_ID"]];
                            if (w.Routes.Count > 0)
                                primary.Routes.AddRange(w.Routes);
                            else
                                throw new Exception("Why overlap?");
                        }
                        else //this overlapping linestring references one belonging to another county so it isn't loaded right now - there are very few of these, just discard it
                            Console.WriteLine("Discarding other county overlap ref: " + w.Tags["PRIMARY_OVERLAP_ID"] + " " + string.Join(";", w.Routes));
                    }

                    Console.WriteLine($"{ways.Count} ways found");

                    Dictionary<long, Node> nodes = new Dictionary<long, Node>();
                    RBush<Node> nodeTree = new RBush<Node>();

                    foreach (Way w in ways.Values)
                    {
                        //generate and connect nodes
                        w.NodeIds = new long[w.Coords.Length];
                        for (int i = 0; i < w.Coords.Length; i++)
                        {
                            bool foundNode = false;
                            Node[] nearNodes = nodeTree.Search(new RBush.Envelope(w.Coords[i].Longitude - delta, w.Coords[i].Latitude - delta, w.Coords[i].Longitude + delta, w.Coords[i].Latitude + delta)).ToArray();
                            foreach (Node n in nearNodes)
                            {
                                if (Math.Abs(n.Coords.Latitude - w.Coords[i].Latitude) < epsilon && Math.Abs(n.Coords.Longitude - w.Coords[i].Longitude) < epsilon)
                                {
                                    //node at coords already exists, reuse it
                                    w.NodeIds[i] = n.Id;
                                    n.Parents.Add(w);
                                    foundNode = true;
                                    break;
                                }
                            }

                            if (!foundNode)
                            {
                                //new node
                                Node n = new Node(nextNodeId++, w.Coords[i]);
                                w.NodeIds[i] = n.Id;
                                n.Parents.Add(w);
                                nodes.Add(n.Id, n);
                                nodeTree.Insert(n);
                            }
                        }

                        //simplify way tags
                        Dictionary<string, string> newTags = new Dictionary<string, string>();

                        //see also: https://gis.dot.state.oh.us/tims_classic/Glossary
                        string prefix = w.Tags.ContainsKey("STREET_PREFIX_DIR_CD") ? w.Tags["STREET_PREFIX_DIR_CD"] : "";
                        string street_name = w.Tags.ContainsKey("STREET_NAME") ? w.Tags["STREET_NAME"] : "";
                        string type = w.Tags.ContainsKey("STREET_SUFFIX_CD") ? w.Tags["STREET_SUFFIX_CD"] : "";
                        string suffix = w.Tags.ContainsKey("STREET_DIR_SUFFIX_CD") ? w.Tags["STREET_DIR_SUFFIX_CD"] : "";
                        string name = Util.GetName(prefix, street_name, type, suffix);
                        if (!string.IsNullOrEmpty(name))
                        {
                            newTags.Add("name", name);
                            w.Name = name;
                        }

                        //storing speed limit as just a number without the " mph" suffix since this isn't actually going into osm and its easier to deal with as an integer
                        if (w.Tags.ContainsKey("SPEED_LIMIT_NBR"))
                            newTags.Add("maxspeed", w.Tags["SPEED_LIMIT_NBR"]);

                        if (w.Tags.ContainsKey("LANES_NBR"))
                            newTags.Add("lanes", w.Tags["LANES_NBR"]);

                        string surfaceLeft = w.Tags.ContainsKey("SURFACE_TYPE_LEFT_CD") ? w.Tags["SURFACE_TYPE_LEFT_CD"] : "";
                        string surfaceRight = w.Tags.ContainsKey("SURFACE_TYPE_RIGHT_CD") ? w.Tags["SURFACE_TYPE_RIGHT_CD"] : "";
                        string surfaceCode;
                        if (surfaceLeft == surfaceRight)
                            surfaceCode = surfaceLeft;
                        else if (string.IsNullOrEmpty(surfaceLeft))
                            surfaceCode = surfaceRight;
                        else if (string.IsNullOrEmpty(surfaceRight))
                            surfaceCode = surfaceLeft;
                        else
                            surfaceCode = $"{surfaceLeft};{surfaceRight}";

                        string surface;
                        switch (surfaceCode)
                        {
                            case "B":
                                surface = "bricks";
                                break;
                            case "C":
                            case "D":
                            case "E":
                                surface = "concrete";
                                break;
                            case "G":
                            case "K":
                                surface = "asphalt";
                                break;
                            case "I":
                            case "L":
                                surface = "chipseal";
                                break;
                            case "M":
                                surface = "gravel";
                                break;
                            case "U":
                                surface = "unpaved";
                                break;
                            case "X":
                                surface = "impassible";
                                break;
                            default:
                                surface = "";
                                break;
                        }

                        if (!string.IsNullOrEmpty(surfaceCode) && surfaceCode != ";")
                            newTags.Add("SURFACE_TYPE_CD", surfaceCode);
                        if (!string.IsNullOrEmpty(surface))
                            newTags.Add("surface", surface);

                        if (w.Routes.Count > 0)
                            newTags.Add("ref", string.Join(";", w.Routes.Select(r => r.Replace("IR", "I"))));

                        string roadWidth = w.Tags.ContainsKey("ROADWAY_WIDTH") ? w.Tags["ROADWAY_WIDTH"] : "";
                        string widthLeft = w.Tags.ContainsKey("SURFACE_WIDTH_LEFT") ? w.Tags["SURFACE_WIDTH_LEFT"] : "";
                        string widthRight = w.Tags.ContainsKey("SURFACE_WIDTH_RIGHT") ? w.Tags["SURFACE_WIDTH_RIGHT"] : "";

                        string shoulderWidthLeftIn = w.Tags.ContainsKey("SHOULDER_PVD_WIDTH_IN_LT") ? w.Tags["SHOULDER_PVD_WIDTH_IN_LT"] : "";
                        string shoulderWidthRightIn = w.Tags.ContainsKey("SHOULDER_PVD_WIDTH_IN_RT") ? w.Tags["SHOULDER_PVD_WIDTH_IN_RT"] : "";
                        string shoulderWidthLeftOut = w.Tags.ContainsKey("SHOULDER_PVD_WIDTH_OUT_LT") ? w.Tags["SHOULDER_PVD_WIDTH_OUT_LT"] : "";
                        string shoulderWidthRightOut = w.Tags.ContainsKey("SHOULDER_PVD_WIDTH_OUT_RT") ? w.Tags["SHOULDER_PVD_WIDTH_OUT_RT"] : "";
                        string shoulderWidthLeftTotal = w.Tags.ContainsKey("SHOULDER_TOTAL_WIDTH_LT") ? w.Tags["SHOULDER_TOTAL_WIDTH_LT"] : "";
                        string shoulderWidthRightTotal = w.Tags.ContainsKey("SHOULDER_TOTAL_WIDTH_RT") ? w.Tags["SHOULDER_TOTAL_WIDTH_RT"] : "";

                        string width = null, shoulderLeftWidth = null, shoulderRightWidth = null;

                        if (!string.IsNullOrEmpty(roadWidth) && string.IsNullOrEmpty(widthLeft) && string.IsNullOrEmpty(widthRight))
                            width = roadWidth;
                        else if (!string.IsNullOrEmpty(widthLeft) && !string.IsNullOrEmpty(widthRight))
                            width = (int.Parse(widthLeft) + int.Parse(widthRight)).ToString();

                        if (!string.IsNullOrEmpty(shoulderWidthLeftTotal))
                            shoulderLeftWidth = shoulderWidthLeftTotal;
                        else if (!string.IsNullOrEmpty(shoulderWidthLeftIn))
                            shoulderLeftWidth = shoulderWidthLeftIn;
                        else if (!string.IsNullOrEmpty(shoulderWidthLeftOut))
                            shoulderLeftWidth = shoulderWidthLeftOut;

                        if (!string.IsNullOrEmpty(shoulderWidthRightTotal))
                            shoulderRightWidth = shoulderWidthRightTotal;
                        else if (!string.IsNullOrEmpty(shoulderWidthRightIn))
                            shoulderRightWidth = shoulderWidthRightIn;
                        else if (!string.IsNullOrEmpty(shoulderWidthRightOut))
                            shoulderRightWidth = shoulderWidthRightOut;

                        if (string.IsNullOrEmpty(shoulderLeftWidth) && string.IsNullOrEmpty(shoulderRightWidth) && !string.IsNullOrEmpty(roadWidth) && !string.IsNullOrEmpty(widthLeft) && !string.IsNullOrEmpty(widthRight))
                        {
                            //they stored different widths for the whole road and for the left and right sides separately added together, the differnce may be the paved shoulder
                            int diff = int.Parse(roadWidth) - int.Parse(widthLeft) - int.Parse(widthRight);
                            //if this calculated shoulder width is too large, sepecially for smaller, slower roads, then it's almost definitely wrong.
                            //like, a 25mph residential street probably doesn't have 8 foot shoulders but a highway might.
                            //highways seem to typically have their shoulder widths defined explicitly however, and won't hit this point.
                            if (diff >= 2 && (diff <= 16 || (diff <= 26 && newTags.ContainsKey("maxspeed") && int.Parse(newTags["maxspeed"]) > 40)))
                            {
                                shoulderLeftWidth = shoulderRightWidth = ((int)(diff / 2)).ToString();
                            }
                        }

                        if (!string.IsNullOrEmpty(width))
                            newTags.Add("width", width);
                        if (!string.IsNullOrEmpty(shoulderLeftWidth))
                            newTags.Add("shoulder:left:width", shoulderLeftWidth);
                        if (!string.IsNullOrEmpty(shoulderRightWidth))
                            newTags.Add("shoulder:right:width", shoulderRightWidth);


                        if (w.Tags.ContainsKey("TRUCK_ROUTE_IND") && w.Tags["TRUCK_ROUTE_IND"] == "Y")
                            newTags.Add("hgv", "designated");

                        if (w.Tags.ContainsKey("DIVIDED_HWY_IND") && w.Tags["DIVIDED_HWY_IND"] == "Y")
                            newTags.Add("DIVIDED_HWY_IND", "Y");

                        if (w.Tags.ContainsKey("NHS_CD"))
                            switch (w.Tags["NHS_CD"])
                            {
                                case "N":
                                    newTags.Add("NHS", "yes");
                                    break;
                                case "S":
                                    newTags.Add("NHS", "STRAHNET");
                                    break;
                                case "C":
                                    newTags.Add("NHS", "STRAHNET (connector)");
                                    break;
                            }
                        if (w.Tags.ContainsKey("STRAHNET_CD"))
                            switch (w.Tags["STRAHNET_CD"])
                            {
                                case "S":
                                    newTags["NHS"] = "STRAHNET";
                                    break;
                                case "C":
                                    newTags["NHS"] = "STRAHNET (connector)";
                                    break;
                            }

                        if (w.Tags.ContainsKey("DIRECTION_OF_TRAVEL_CD"))
                        {
                            if (w.Tags["DIRECTION_OF_TRAVEL_CD"] == "F")
                                newTags.Add("oneway", "yes");
                            else if (w.Tags["DIRECTION_OF_TRAVEL_CD"] == "T")
                                newTags.Add("oneway", "-1");
                        }

                        if (w.Tags.ContainsKey("SCENIC_BYWAY_CD") && w.Tags.ContainsKey("SCENIC_ROUTE_NUMBER"))
                            newTags.Add("SCENIC_BYWAY", w.Tags["SCENIC_BYWAY_CD"] + " " + w.Tags["SCENIC_ROUTE_NUMBER"]);

                        foreach (string s in new string[] { "ROADWAY_INVENTORY_ID", "NLF_ID", "FUNCTION_CLASS_CD" })
                        {
                            if (w.Tags.ContainsKey(s))
                                newTags.Add(s, w.Tags[s]);
                        }

                        if (newTags.Count > 0)
                            w.Tags = newTags;
                    }

                    //find and store nodes near county borders along with the names of their connected ways
                    double searchDist = 0.002d;//arbitrary, in degrees lat/lon, about 150-250 meters
                    foreach (Node n in nodes.Values)
                    {
                        BorderSegment[] segments = borders.SegmentTree.Search(new RBush.Envelope(n.Coords.Longitude - searchDist, n.Coords.Latitude - searchDist, n.Coords.Longitude + searchDist, n.Coords.Latitude + searchDist)).ToArray();
                        foreach (BorderSegment segment in segments)
                        {
                            double minDist = Util.DistanceFromLineToPoint(segment.P1, segment.P2, n.Coords);
                            if (minDist <= 0.02d)//km
                            {
                                Node borderNode = null;
                                bool foundNode = false;
                                Node[] nearExistingBorderNodes = nearBorderNodeTree.Search(new RBush.Envelope(n.Coords.Longitude - delta, n.Coords.Latitude - delta, n.Coords.Longitude + delta, n.Coords.Latitude + delta)).ToArray();
                                foreach (Node ne in nearExistingBorderNodes)
                                {
                                    if (Math.Abs(ne.Coords.Latitude - n.Coords.Latitude) < epsilon && Math.Abs(ne.Coords.Longitude - n.Coords.Longitude) < epsilon)
                                    {
                                        foundNode = true;
                                        borderNode = ne;
                                        break;
                                    }
                                }

                                if (!foundNode)
                                {
                                    borderNode = new Node(n.Id, n.Coords);
                                    borderNode.Tags["county"] = county;
                                    nearBorderNodes.Add(borderNode);
                                    nearBorderNodeTree.Insert(borderNode);
                                }
                                else
                                    borderNode.Tags["county"] = borderNode.Tags["county"] + ";" + county;

                                borderNode.Tags[county + "id"] = n.Id.ToString();
                                borderNode.Tags[county + "ParentEnds"] = string.Join(";", n.Parents.Where(p => p.NodeIds[0] == n.Id || p.NodeIds[p.NodeIds.Length - 1] == n.Id).Select(p => string.IsNullOrEmpty(p.Name) ? "--blank--" : p.Name));
                                borderNode.Tags[county + "ParentMiddles"] = string.Join(";", n.Parents.Where(p => p.NodeIds[0] != n.Id && p.NodeIds[p.NodeIds.Length - 1] != n.Id).Select(p => string.IsNullOrEmpty(p.Name) ? "--blank--" : p.Name));

                                Way w = n.Parents.First();
                                int index = w.IndexOf(n.Id);
                                GeoCoordinate secondPoint = index == w.NodeIds.Length - 1 ? nodes[w.NodeIds[w.NodeIds.Length - 2]].Coords : nodes[w.NodeIds[index + 1]].Coords;
                                double angle = Util.Angle(n.Coords, secondPoint);
                                borderNode.Tags[county + "FirstWayAngle"] = Math.Round(angle, 5).ToString();

                                break;
                            }
                        }
                    }

                    using (StreamWriter swOsmXml = new StreamWriter(Path.Combine(outputDir, $"{county}.osm")))
                    {
                        swOsmXml.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
                        swOsmXml.WriteLine("<osm version='0.6' upload='false'>");

                        foreach (Node n in nodes.Values)
                            swOsmXml.Write(n.ToXml());
                        foreach (Way w in ways.Values)
                            swOsmXml.Write(w.ToXml());
                        swOsmXml.WriteLine("</osm>");
                    }
                }
            }

            //filter out nodes that don't form connections
            nearBorderNodes = nearBorderNodes.Where(n => n.Tags.ContainsKey("county") && n.Tags["county"].Contains(";")).ToList();

            //writing this out for debug purposes only
            using (StreamWriter swOsmXml = new StreamWriter(Path.Combine(outputDir, "nearBorderNodes.osm")))
            {
                swOsmXml.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
                swOsmXml.WriteLine("<osm version='0.6' upload='false'>");

                foreach (Node n in nearBorderNodes)
                    swOsmXml.Write(n.ToXml());
                swOsmXml.WriteLine("</osm>");
            }

            Console.WriteLine("==second pass==");
            //second of two passes:
            foreach (string county in counties.Keys)
            {
                Console.WriteLine(county);
                Dictionary<long, Way> ways = new Dictionary<long, Way>();
                Dictionary<long, Node> nodes = new Dictionary<long, Node>();
                //separte scope so the xml doesn't need to stay loaded after we've read it
                {
                    XDocument doc = XDocument.Load(Path.Combine(outputDir, $"{county}.osm"));
                    foreach (XElement elem in doc.Descendants("node"))
                    {
                        Node n = new Node(elem);
                        nodes.Add(n.Id, n);
                    }
                    foreach (XElement elem in doc.Descendants("way"))
                    {
                        Way w = new Way(elem);
                        ways.Add(w.Id, w);
                        foreach (long nId in w.NodeIds)
                        {
                            Node n = nodes[nId];
                            if (!n.Parents.Contains(w))
                                n.Parents.Add(w);
                        }
                    }
                }

                var intersections = nodes.Values.Where(n => n.Parents.Count > 1).ToArray();
                Console.WriteLine($"Found {intersections.Length} intersections");

                //below, county codes are all 3 characters, so Length > 3 means a node connected to roads leading into 2 or more different counties
                Dictionary<long, Node> externalConnections = nearBorderNodes.Where(n => n.Tags["county"].Contains(county) && n.Tags["county"].Length > 3).ToDictionary(n => long.Parse(n.Tags[county + "id"]), n => n);
                HashSet<long> externalConnIds = externalConnections.Keys.ToHashSet();

                Console.WriteLine($"Found {externalConnections.Count} intersections with roads outside this county");

                //find intersections within the county where a road name dead-ends there and doesn't continue through the point along the same or any other way
                foreach (Node n in intersections)
                {
                    List<double> angles = new List<double>();

                    Node externNode = null;
                    List<string> externNames = null;
                    if (externalConnections.ContainsKey(n.Id))
                    {
                        externNode = externalConnections[n.Id];
                        externNames = externNode.Tags.Where(pair => pair.Key.Contains("Parent") && !pair.Key.StartsWith(county) && !string.IsNullOrEmpty(pair.Value)).SelectMany(pair => pair.Value.Split(';')).Select(s => s == "--blank--" ? "" : s).ToList();
                        externalConnIds.Remove(n.Id);
                    }

                    foreach (Way w in n.Parents)
                    {
                        int index = w.IndexOf(n.Id);
                        if (index > 0 && index < w.NodeIds.Length - 1)
                            continue;//this way leaves this node in two directions, not a dead-end

                        if (w.NodeIds.Count(id => id == n.Id) > 1)
                            continue;//this way uses this node more than once, not a dead-end

                        string[] names = n.Parents.Where(p => p != w).Select(p => p.Name).Distinct().ToArray();
                        if (externNames != null && externNames.Count > 0)
                            names = names.Concat(externNames).ToArray();
                        if (names.Contains(w.Name))
                            continue;//another way connecting here has the same name, not a dead-end

                        GeoCoordinate secondPoint = index == 0 ? nodes[w.NodeIds[1]].Coords : nodes[w.NodeIds[w.NodeIds.Length - 2]].Coords;

                        double angle = Util.Angle(n.Coords, secondPoint);
                        angles.Add(angle);
                    }

                    for (int i = 0; i < angles.Count; i++)
                    {
                        n.Tags.Add($"nameEndAngle{i}", Math.Round(angles[i], 2).ToString());
                    }

                    if (angles.Count == 2 && n.Parents.Count == 2 && externNames == null)
                    {
                        n.Tags.Add("connections", n.Parents.Count.ToString());//connections=2 is basically just a flag for mapnik to display the straight separator symbol instead of the angled one
                        n.Tags.Add("separatorAngle", Math.Round((angles[0] + angles[1]) / 2.0d, 2).ToString());
                    }

                    maxAtOneNode = Math.Max(maxAtOneNode, angles.Count);
                }

                //do it again for roads ending at the county line
                Console.WriteLine($"There are {externalConnIds.Count} external connections not part of intersections within the county");
                foreach (long exId in externalConnIds)
                {
                    Node n = nodes[exId];
                    Node externNode = externalConnections[exId];
                    List<string> externNames = externNode.Tags.Where(pair => pair.Key.Contains("Parent") && !pair.Key.StartsWith(county) && !string.IsNullOrEmpty(pair.Value)).SelectMany(pair => pair.Value.Split(';')).Select(s => s == "--blank--" ? "" : s).ToList();
                    if (n.Parents.Count != 1)
                        throw new Exception("Something is wrong");

                    Way w = n.Parents[0];
                    int index = w.IndexOf(n.Id);
                    if (index > 0 && index < w.NodeIds.Length - 1)
                        continue;//this way leaves this node in two directions, not a dead-end

                    if (w.NodeIds.Count(id => id == n.Id) > 1)
                        continue;//this way uses this node more than once, not a dead-end

                    if (externNames.Contains(w.Name))
                        continue;//this way's name continues into another county

                    GeoCoordinate secondPoint = index == 0 ? nodes[w.NodeIds[1]].Coords : nodes[w.NodeIds[w.NodeIds.Length - 2]].Coords;

                    double angle = Util.Angle(n.Coords, secondPoint);

                    if (n.Tags.Count > 0)
                        throw new Exception("Something else is wrong");
                    n.Tags.Add($"nameEndAngle0", Math.Round(angle, 2).ToString());
                    if (externNames.Count == 1 && externNode.Tags.Count(pair => !pair.Key.StartsWith(county) && pair.Key.Contains("End") && !string.IsNullOrEmpty(pair.Value)) == 1)
                    {
                        n.Tags.Add("connections", "2");
                        double newAngle = (angle + double.Parse(externNode.Tags.Where(p => p.Key.Contains("FirstWayAngle") && !p.Key.StartsWith(county)).First().Value)) / 2.0d;
                        n.Tags.Add("separatorAngle", Math.Round(newAngle, 2).ToString());
                    }
                }

                //write this county's data back out
                using (StreamWriter swOsmXml = new StreamWriter(Path.Combine(outputDir, $"{county}.osm")))
                {
                    swOsmXml.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
                    swOsmXml.WriteLine("<osm version='0.6' upload='false'>");

                    foreach (Node n in nodes.Values)
                        swOsmXml.Write(n.ToXml());
                    foreach (Way w in ways.Values)
                        swOsmXml.Write(w.ToXml());
                    swOsmXml.WriteLine("</osm>");
                }

            }

            Console.WriteLine($"Next node id: {nextNodeId}");
            Console.WriteLine($"Next way id:  {nextWayId}");
            Console.WriteLine($"Max angle tags on one node: {maxAtOneNode}");

        }
    }
}