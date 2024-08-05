using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text.RegularExpressions;

namespace OhioRoadInventoryExtractor
{
    public static class Util
    {
        private static Regex rampPattern = new Regex("^Ramp From ([A-Z][a-z]) ([0-9]+[nsew]?)(.+? )To ([A-Z][a-z]) ([0-9]+[nsew]?)(.*)$", RegexOptions.Compiled);

        public static string GetName(string prefix, string name, string type, string suffix)
        {
            string ret = $"{ExpandDirection(prefix)} {ExpandName(name)} {ExpandType(type)} {ExpandDirection(suffix)}";
            ret = Regex.Replace(ret, "\\s{2,}", " ").Trim();
            ret = ToTitleCase(ret);
            ret = ret.Replace("Alley Alley", "Alley");

            return ret.Trim();
        }

        public static string ToTitleCase(string s)
        {
            List<string> toReturn = new List<string>();
            string[] words = s.Split(' ');
            foreach (string word in words)
            {
                if (word.Length <= 1)
                    toReturn.Add(word);
                else
                {
                    if (word == "OF")
                        toReturn.Add("of");
                    else if (word == "AND")
                        toReturn.Add("and");
                    else
                    {
                        string w = word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower();
                        w = Regex.Replace(w, "([-'`\"\\(/\\.])([a-z])", m => m.Groups[1].Value + m.Groups[2].Value.ToUpper());
                        w = Regex.Replace(w, "'S(\\s|$)", "'s$1");
                        w = Regex.Replace(w, "Mc([a-z])", m => "Mc" + m.Groups[1].Value.ToUpper());
                        toReturn.Add(w);
                    }
                }
            }

            string output = string.Join(" ", toReturn.Where(t => !string.IsNullOrWhiteSpace(t)));

            Match ramp = rampPattern.Match(output);
            if(ramp.Success)
            {
                string r = $"Ramp from {ramp.Groups[1].Value.ToUpper().Replace("IR", "I")}-{ramp.Groups[2].Value.ToUpper()}{ramp.Groups[3].Value}to {ramp.Groups[4].Value.ToUpper().Replace("IR", "I")}-{ramp.Groups[5].Value.ToUpper()}{ramp.Groups[6].Value}";
                return Regex.Replace(r, "([0-9][a-z])", (m) => m.Groups[1].Value.ToUpper());
            }

            return output;
        }

        public static string ExpandName(this string s)
        {
            string w = Regex.Replace(s, "^ST\\.", "SAINT");
            if(!w.Contains("ST RT"))
                w = Regex.Replace(w, "^ST\\b", "SAINT");
            w = Regex.Replace(w, "^MT\\.", "MOUNT");
            w = Regex.Replace(w, "^MT\\b", "MOUNT");

            return w;
        }

        public static string ExpandDirection(string s)
        {
            switch (s)
            {
                case "N":
                    return "NORTH";
                case "S":
                    return "SOUTH";
                case "E":
                    return "EAST";
                case "W":
                    return "WEST";
                case "NE":
                    return "NORTHEAST";
                case "NW":
                    return "NORTHWEST";
                case "SE":
                    return "SOUTHEAST";
                case "SW":
                    return "SOUTHWEST";
                default:
                    return s;
            }
        }

        public static string ExpandType(string s)
        {
            switch (s)
            {
                case "ALLY":
                case "ALY":
                    return "ALLEY";
                case "ANX":
                    return "ANNEX";
                case "AVE":
                    return "AVENUE";
                case "BLF":
                    return "BLUFF";
                case "BLVD":
                    return "BOULEVARD";
                case "BND":
                    return "BEND";
                case "BRG":
                    return "BRIDGE";
                case "CIR":
                    return "CIRCLE";
                case "CLB":
                    return "CLUB";
                case "CLF":
                    return "CLIFF";
                case "CLS":
                    return "CLOSE";
                case "CMNS":
                    return "COMMONS";
                case "CORS":
                    return "CORNERS";
                case "COR":
                    return "CORNER";
                case "CRES":
                    return "CRESCENT";
                case "CRK":
                    return "CREEK";
                case "CRST":
                    return "CREST";
                case "CT":
                    return "COURT";
                case "CTR":
                    return "CENTER";
                case "CV":
                    return "COVE";
                case "DRS":
                case "DR":
                    return "DRIVE";
                case "EXPY":
                    return "EXPRESSWAY";
                case "FLD":
                    return "FIELD";
                case "FLDS":
                    return "FIELDS";
                case "FRST":
                    return "FOREST";
                case "GRN":
                    return "GREEN";
                case "GLN":
                    return "GLEN";
                case "GRV":
                    return "GROVE";
                case "GTWY":
                    return "GATEWAY";
                case "HL":
                    return "HILL";
                case "HOLW":
                    return "HOLLOW";
                case "HWY":
                    return "HIGHWAY";
                case "LN":
                    return "LANE";
                case "LNDG":
                    return "LANDING";
                case "LP":
                    return "LOOP";
                case "MNR":
                    return "MANOR";
                case "MDW":
                    return "MEADOW";
                case "ORCH":
                    return "ORCHARD";
                case "PKWY":
                    return "PARKWAY";
                case "PL":
                    return "PLACE";
                case "PLZ":
                    return "PLAZA";
                case "PT":
                    return "POINT";
                case "PNE":
                    return "PINE";
                case "PNES":
                    return "PINES";
                case "PSGE":
                    return "PASSAGE";
                case "RD":
                    return "ROAD";
                case "RDG":
                    return "RIDGE";
                case "RND":
                    return "ROUND";
                case "RTE":
                    return "ROUTE";
                case "SPG":
                    return "SPRING";
                case "SPGS":
                    return "SPRINGS";
                case "SQ":
                    return "SQUARE";
                case "ST":
                    return "STREET";
                case "TER":
                    return "TERRACE";
                case "TRCE":
                    return "TRACE";
                case "TR":
                case "TRL":
                    return "TRAIL";
                case "VIS":
                    return "VISTA";
                case "VLG":
                    return "VILLAGE";
                case "VW":
                    return "VIEW";
                case "XING":
                    return "CROSSING";
                default:
                    return s;
            }
        }

        //https://stackoverflow.com/a/18738281
        public static double Angle(GeoCoordinate g1, GeoCoordinate g2)
        {
            double lat1 = g1.Latitude * Math.PI / 180.0d;
            double lon1 = g1.Longitude * Math.PI / 180.0d;
            double lat2 = g2.Latitude * Math.PI / 180.0d;
            double lon2 = g2.Longitude * Math.PI / 180.0d;

            double deltaLon = lon2 - lon1;

            double y = Math.Sin(deltaLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLon);

            double bearing = Math.Atan2(y, x);

            bearing = bearing * 180.0d / Math.PI;
            bearing = (bearing + 360.0d) % 360.0d;

            return bearing;
        }

        //https://stackoverflow.com/a/20369652
        public static double DistanceFromLineToPoint(GeoCoordinate A, GeoCoordinate B, GeoCoordinate C)
        {
            const double earthRadius = 6371d;
            double bearingAC = Angle(A, C) * Math.PI / 180.0d;
            double bearingAB = Angle(A, B) * Math.PI / 180.0d;

            double Alat = A.Latitude * Math.PI / 180.0d;
            double Clat = C.Latitude * Math.PI / 180.0d;
            double deltaLon = (C.Longitude - A.Longitude) * Math.PI / 180.0d;

            double distAC = Math.Acos(Math.Sin(Alat) * Math.Sin(Clat) + Math.Cos(Alat) * Math.Cos(Clat) * Math.Cos(deltaLon)) * earthRadius;
            double minDist = Math.Abs(Math.Asin(Math.Sin(distAC / earthRadius) * Math.Sin(bearingAC - bearingAB)) * earthRadius);
            return minDist;
        }
    }
}
