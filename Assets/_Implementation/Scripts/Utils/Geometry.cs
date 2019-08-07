using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Geometry
{

    // Getting centroid of polygon
    public static Vector3 CalculateCentroid(List<Vector3> points)
    {
        // Enumerate points.
        float sumX = 0.0f;
        float sumY = 0.0f;
        float sumZ = 0.0f;
        foreach (Vector3 point in points)
        {
            sumX += point.x;
            sumY += point.y;
            sumZ += point.z;
        }

        // Average.
        float x = sumX / points.Count;
        float y = sumY / points.Count;
        float z = sumZ / points.Count;

        // Assign.
        return new Vector3(x, y, z);
    }

    // Getting convex polygon
    public static List<Vector3> GetConvexHull(List<Vector3> points)
    {
        //If we have just 3 points, then they are the convex hull, so return those
        if (points.Count == 3)
        {
            //These might not be ccw, and they may also be colinear
            return points;
        }

        //If fewer points, then we cant create a convex hull
        if (points.Count < 3)
        {
            return null;
        }



        //The list with points on the convex hull
        List<Vector3> convexHull = new List<Vector3>();

        //Step 1. Find the vertex with the smallest x coordinate
        //If several have the same x coordinate, find the one with the smallest z
        Vector3 startVertex = points[0];

        Vector3 startPos = startVertex;

        for (int i = 1; i < points.Count; i++)
        {
            Vector3 testPos = points[i];

            //Because of precision issues, we use Mathf.Approximately to test if the x positions are the same
            if (testPos.x < startPos.x || (Mathf.Approximately(testPos.x, startPos.x) && testPos.z < startPos.z))
            {
                startVertex = points[i];

                startPos = startVertex;
            }
        }

        //This vertex is always on the convex hull
        convexHull.Add(startVertex);

        points.Remove(startVertex);



        //Step 2. Loop to generate the convex hull
        Vector3 currentPoint = convexHull[0];

        //Store colinear points here - better to create this list once than each loop
        List<Vector3> colinearPoints = new List<Vector3>();

        int counter = 0;

        while (true)
        {
            //After 2 iterations we have to add the start position again so we can terminate the algorithm
            //Cant use convexhull.count because of colinear points, so we need a counter
            if (counter == 2)
            {
                points.Add(convexHull[0]);
            }

            //Pick next point randomly
            Vector3 nextPoint = points[UnityEngine.Random.Range(0, points.Count)];

            //To 2d space so we can see if a point is to the left is the vector ab
            Vector2 a = new Vector2(currentPoint.x, currentPoint.z);

            Vector2 b = new Vector2(nextPoint.x, nextPoint.z);

            //Test if there's a point to the right of ab, if so then it's the new b
            for (int i = 0; i < points.Count; i++)
            {
                //Dont test the point we picked randomly
                if (points[i].Equals(nextPoint))
                {
                    continue;
                }

                Vector2 c = new Vector2(points[i].x, points[i].z);

                //Where is c in relation to a-b
                // < 0 -> to the right
                // = 0 -> on the line
                // > 0 -> to the left
                float relation = (a.x - c.x) * (b.y - c.y) - (a.y - c.y) * (b.x - c.x);

                //Colinear points
                //Cant use exactly 0 because of floating point precision issues
                //This accuracy is smallest possible, if smaller points will be missed if we are testing with a plane
                float accuracy = 0.00001f;

                if (relation < accuracy && relation > -accuracy)
                {
                    colinearPoints.Add(points[i]);
                }
                //To the right = better point, so pick it as next point on the convex hull
                else if (relation < 0f)
                {
                    nextPoint = points[i];

                    b = new Vector2(nextPoint.x, nextPoint.z);

                    //Clear colinear points
                    colinearPoints.Clear();
                }
                //To the left = worse point so do nothing
            }



            //If we have colinear points
            if (colinearPoints.Count > 0)
            {
                colinearPoints.Add(nextPoint);

                //Sort this list, so we can add the colinear points in correct order
                colinearPoints = colinearPoints.OrderBy(n => Vector3.SqrMagnitude(n - currentPoint)).ToList();

                convexHull.AddRange(colinearPoints);

                currentPoint = colinearPoints[colinearPoints.Count - 1];

                //Remove the points that are now on the convex hull
                for (int i = 0; i < colinearPoints.Count; i++)
                {
                    points.Remove(colinearPoints[i]);
                }

                colinearPoints.Clear();
            }
            else
            {
                convexHull.Add(nextPoint);

                points.Remove(nextPoint);

                currentPoint = nextPoint;
            }

            //Have we found the first point on the hull? If so we have completed the hull
            if (currentPoint.Equals(convexHull[0]))
            {
                //Then remove it because it is the same as the first point, and we want a convex hull with no duplicates
                convexHull.RemoveAt(convexHull.Count - 1);

                break;
            }

            counter += 1;
        }

        return convexHull;
    }

    // Checks if polygon contains point
    public static bool PolygonContainsPoint(List<Vector2> polygon, Vector2 point)
    {
        int j = polygon.Count - 1;
        bool inside = false;
        for (int i = 0; i < polygon.Count; j = i++)
        {
            if (((polygon[i].y <= point.y && point.y < polygon[j].y) || (polygon[j].y <= point.y && point.y < polygon[i].y)) && (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                inside = !inside;
        }
        return inside;
    }

    // Returns distance from line to point
    public static double DistanceFromPointToLine(Vector2 point, Vector2 l1, Vector2 l2)
    {
        return Math.Abs((l2.x - l1.x) * (l1.y - point.y) - (l1.x - point.x) * (l2.y - l1.y)) / Math.Sqrt(Math.Pow(l2.x - l1.x, 2) + Math.Pow(l2.y - l1.y, 2));
    }

    // Numeric equality
    private static bool IsEqual(double d1, double d2)
    {
        return Math.Abs(d1 - d2) <= 0.000000001d;
    }

    // Intersection point od two lines
    public static Vector2 GetIntersectionPoint(Vector2 l1p1, Vector2 l1p2, Vector2 l2p1, Vector2 l2p2)
    {
        double A1 = l1p2.y - l1p1.y;
        double B1 = l1p1.x - l1p2.x;
        double C1 = A1 * l1p1.x + B1 * l1p1.y;

        double A2 = l2p2.y - l2p1.y;
        double B2 = l2p1.x - l2p2.x;
        double C2 = A2 * l2p1.x + B2 * l2p1.y;

        double det = A1 * B2 - A2 * B1;
        if (IsEqual(det, 0d))
        {
            return Vector2.zero;
        }
        else
        {
            double x = (B2 * C1 - B1 * C2) / det;
            double y = (A1 * C2 - A2 * C1) / det;
            bool online1 = ((Math.Min(l1p1.x, l1p2.x) < x || IsEqual(Math.Min(l1p1.x, l1p2.x), x))
                && (Math.Max(l1p1.x, l1p2.x) > x || IsEqual(Math.Max(l1p1.x, l1p2.x), x))
                && (Math.Min(l1p1.y, l1p2.y) < y || IsEqual(Math.Min(l1p1.y, l1p2.y), y))
                && (Math.Max(l1p1.y, l1p2.y) > y || IsEqual(Math.Max(l1p1.y, l1p2.y), y))
                );
            bool online2 = ((Math.Min(l2p1.x, l2p2.x) < x || IsEqual(Math.Min(l2p1.x, l2p2.x), x))
                && (Math.Max(l2p1.x, l2p2.x) > x || IsEqual(Math.Max(l2p1.x, l2p2.x), x))
                && (Math.Min(l2p1.y, l2p2.y) < y || IsEqual(Math.Min(l2p1.y, l2p2.y), y))
                && (Math.Max(l2p1.y, l2p2.y) > y || IsEqual(Math.Max(l2p1.y, l2p2.y), y))
                );

            if (online1 && online2)
                return new Vector2((float)x, (float)y);
        }
        return Vector2.zero;
    }

    // Intersection points of line and polygon
    public static List<Vector2> GetIntersectionPoints(Vector2 l1p1, Vector2 l1p2, List<Vector2> poly)
    {
        List<Vector2> intersectionPoints = new List<Vector2>();
        for (int i = 0; i < poly.Count; i++)
        {

            int next = (i + 1 == poly.Count) ? 0 : i + 1;

            Vector2 ip = GetIntersectionPoint(l1p1, l1p2, poly[i], poly[next]);

            if (ip != Vector2.zero) intersectionPoints.Add(ip);

        }

        return intersectionPoints;
    }

    // Adding non repeating points to pool
    private static void AddPoints(List<Vector2> pool, List<Vector2> newpoints)
    {
        foreach (Vector2 np in newpoints)
        {
            bool found = false;
            foreach (Vector2 p in pool)
            {
                if (IsEqual(p.x, np.x) && IsEqual(p.y, np.y))
                {
                    found = true;
                    break;
                }
            }
            if (!found) pool.Add(np);
        }
    }

    // Ordering list of point clockwise
    private static List<Vector2> OrderClockwise(List<Vector2> points)
    {
        double mX = 0;
        double my = 0;
        foreach (Vector2 p in points)
        {
            mX += p.x;
            my += p.y;
        }
        mX /= points.Count;
        my /= points.Count;

        return points.OrderBy(v => Math.Atan2(v.y - my, v.x - mX)).ToList<Vector2>();
    }

    // Getting intersection of polygons
    public static List<Vector2> GetIntersectionOfPolygons(List<Vector2> poly1, List<Vector2> poly2)
    {
        List<Vector2> clippedCorners = new List<Vector2>();

        // Add  the corners of poly1 which are inside poly2       
        for (int i = 0; i < poly1.Count; i++)
        {
            if (PolygonContainsPoint(poly2, poly1[i]))
                AddPoints(clippedCorners, new List<Vector2> { poly1[i] });
        }

        // Add the corners of poly2 which are inside poly1
        for (int i = 0; i < poly2.Count; i++)
        {
            if (PolygonContainsPoint(poly1, poly2[i]))
                AddPoints(clippedCorners, new List<Vector2> { poly2[i] });
        }

        // Add  the intersection points
        for (int i = 0, next = 1; i < poly1.Count; i++, next = (i + 1 == poly1.Count) ? 0 : i + 1)
        {
            AddPoints(clippedCorners, GetIntersectionPoints(poly1[i], poly1[next], poly2));
        }

        return OrderClockwise(clippedCorners);
    }

    // Returns true if point is on line
    public static bool IsPointOnLine(Vector2 linePointA, Vector2 linePointB, Vector2 point)
    {
        float valX = (point.x - linePointA.x) / (linePointB.x - linePointA.x);
        float valY = (point.y - linePointA.y) / (linePointB.y - linePointA.y);
        return (Mathf.Abs(valX - valY) < 0.00001d);
    }

    // Shrinks given intersection to minimum (considering other polygons). Key - polygon, value - number of initial polygons
    private static KeyValuePair<List<Vector2>, int> GetMinimalIntersection(List<Vector2> minimalIntersection, List<List<Vector2>> polygons)
    {
        int polygonsJoined = 0;
        foreach (List<Vector2> polygon in polygons)
        {
            List<Vector2> intersection = GetIntersectionOfPolygons(minimalIntersection, polygon);
            if (intersection.Count > 0)
            {
                minimalIntersection = intersection;
                polygonsJoined++;
            }
        }
        return new KeyValuePair<List<Vector2>, int>(minimalIntersection, polygonsJoined);
    }

    // Getting list hashcode order independent
    static int GetOrderIndependentHashCode<T>(IEnumerable<T> source)
    {
        int hash = 0;
        foreach (T element in source)
        {
            hash = hash ^ EqualityComparer<T>.Default.GetHashCode(element);
        }
        return hash;
    }

    // Returns all intersections between polygons. Key - polygon, value - number of initial polygons
    public static List<KeyValuePair<List<Vector2>, int>> GetAllPolygonsMinimalIntersections(List<List<Vector2>> polygons)
    {
        List<KeyValuePair<List<Vector2>, int>> intersections = new List<KeyValuePair<List<Vector2>, int>>();
        for (int i = 0; i < polygons.Count; i++)
        {
            for (int j = i + 1; j < polygons.Count; j++)
            {
                List<Vector2> polygonsIntersection = GetIntersectionOfPolygons(polygons[i], polygons[j]);
                if (polygonsIntersection.Count > 0)
                {
                    List<List<Vector2>> remainingPolygons = new List<List<Vector2>>(polygons);
                    remainingPolygons.Remove(polygons[i]);
                    remainingPolygons.Remove(polygons[j]);
                    KeyValuePair<List<Vector2>, int> minimalIntersection = GetMinimalIntersection(polygonsIntersection, remainingPolygons);
                    intersections.Add(new KeyValuePair<List<Vector2>, int>(minimalIntersection.Key, minimalIntersection.Value + 2));
                }
            }
        }
        for (int i = 0; i < intersections.Count; i++)
        {
            for (int j = i + 1; j < intersections.Count; j++)
            {
                if (GetOrderIndependentHashCode(intersections[i].Key) == GetOrderIndependentHashCode(intersections[j].Key))
                {
                    intersections.RemoveAt(j);
                    j--;
                }
            }
        }
        return intersections;
    }

}
