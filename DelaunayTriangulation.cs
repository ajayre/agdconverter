using System;
using System.Collections.Generic;
using System.Linq;

namespace AGDConverter
{
    public class DelaunayTriangulation
    {
        private ConsoleProgressBar? _progressBar;

        public DelaunayTriangulation(ConsoleProgressBar? progressBar = null)
        {
            _progressBar = progressBar;
        }

        /// <summary>
        /// Performs Delaunay triangulation using the Bowyer-Watson algorithm
        /// </summary>
        /// <param name="points">List of 3D points to triangulate</param>
        /// <returns>List of triangles as vertex indices</returns>
        public List<(int, int, int)> Triangulate(List<(double X, double Y, double Z)> points)
        {
            if (points == null || points.Count < 3)
                return new List<(int, int, int)>();

            _progressBar?.Update(0, 100, "Initializing Delaunay triangulation...");

            // Create a super triangle that contains all points
            var superTriangle = CreateSuperTriangle(points);
            var triangles = new List<Triangle> { superTriangle };
            var validTriangles = new List<Triangle> { superTriangle };

            _progressBar?.Update(5, 100, "Processing points...");

            // Process each point
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                var badTriangles = new List<Triangle>();

                // Find all triangles whose circumcircle contains the current point
                foreach (var triangle in validTriangles)
                {
                    if (IsPointInCircumcircle(point, triangle))
                    {
                        badTriangles.Add(triangle);
                    }
                }

                // Remove bad triangles from the list
                foreach (var badTriangle in badTriangles)
                {
                    validTriangles.Remove(badTriangle);
                }

                // Find the boundary of the polygonal hole
                var polygon = new List<Edge>();
                foreach (var badTriangle in badTriangles)
                {
                    foreach (var edge in badTriangle.Edges)
                    {
                        bool isShared = false;
                        foreach (var otherBadTriangle in badTriangles)
                        {
                            if (otherBadTriangle != badTriangle && otherBadTriangle.Edges.Any(e => e.HasEdge(edge)))
                            {
                                isShared = true;
                                break;
                            }
                        }
                        if (!isShared)
                        {
                            polygon.Add(edge);
                        }
                    }
                }

                // Create new triangles with the current point
                foreach (var edge in polygon)
                {
                    var newTriangle = new Triangle(edge.Point1, edge.Point2, point);
                    validTriangles.Add(newTriangle);
                }

                // Update progress
                if (i % 10 == 0 || i == points.Count - 1)
                {
                    _progressBar?.Update(5 + (i * 85 / points.Count), 100, 
                        $"Processing point {i + 1}/{points.Count} - {validTriangles.Count} triangles");
                }
            }

            _progressBar?.Update(90, 100, "Removing super triangle...");

            // Remove triangles that contain vertices from the super triangle
            var resultTriangles = new List<Triangle>();
            foreach (var triangle in validTriangles)
            {
                if (!triangle.ContainsAnyVertex(superTriangle.Point1, superTriangle.Point2, superTriangle.Point3))
                {
                    resultTriangles.Add(triangle);
                }
            }

            _progressBar?.Update(95, 100, "Converting to indices...");

            // Convert triangles to vertex indices
            var result = new List<(int, int, int)>();
            foreach (var triangle in resultTriangles)
            {
                int index1 = FindPointIndex(triangle.Point1, points);
                int index2 = FindPointIndex(triangle.Point2, points);
                int index3 = FindPointIndex(triangle.Point3, points);
                
                if (index1 >= 0 && index2 >= 0 && index3 >= 0)
                {
                    result.Add((index1, index2, index3));
                }
            }

            _progressBar?.Update(100, 100, $"Delaunay triangulation complete - {result.Count} triangles");

            return result;
        }

        private Triangle CreateSuperTriangle(List<(double X, double Y, double Z)> points)
        {
            // Find bounding box
            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            // Create a large triangle that contains all points
            double width = maxX - minX;
            double height = maxY - minY;
            double margin = Math.Max(width, height) * 2;

            var p1 = (minX - margin, minY - margin, 0.0);
            var p2 = (maxX + margin, minY - margin, 0.0);
            var p3 = ((minX + maxX) / 2, maxY + margin, 0.0);

            return new Triangle(p1, p2, p3);
        }

        private bool IsPointInCircumcircle((double X, double Y, double Z) point, Triangle triangle)
        {
            var (cx, cy, radius) = CalculateCircumcircle(triangle);
            double dx = point.X - cx;
            double dy = point.Y - cy;
            return (dx * dx + dy * dy) <= radius * radius;
        }

        private (double cx, double cy, double radius) CalculateCircumcircle(Triangle triangle)
        {
            var p1 = triangle.Point1;
            var p2 = triangle.Point2;
            var p3 = triangle.Point3;

            double ax = p1.X;
            double ay = p1.Y;
            double bx = p2.X;
            double by = p2.Y;
            double cx = p3.X;
            double cy = p3.Y;

            double d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
            if (Math.Abs(d) < 1e-10) // Degenerate triangle
            {
                return (0, 0, 0);
            }

            double ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
            double uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;

            double radius = Math.Sqrt((ax - ux) * (ax - ux) + (ay - uy) * (ay - uy));

            return (ux, uy, radius);
        }

        private int FindPointIndex((double X, double Y, double Z) point, List<(double X, double Y, double Z)> points)
        {
            const double tolerance = 1e-10;
            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                if (Math.Abs(p.X - point.X) < tolerance && 
                    Math.Abs(p.Y - point.Y) < tolerance && 
                    Math.Abs(p.Z - point.Z) < tolerance)
                {
                    return i;
                }
            }
            return -1;
        }
    }

    public class Triangle
    {
        public (double X, double Y, double Z) Point1 { get; }
        public (double X, double Y, double Z) Point2 { get; }
        public (double X, double Y, double Z) Point3 { get; }

        public Triangle((double X, double Y, double Z) p1, (double X, double Y, double Z) p2, (double X, double Y, double Z) p3)
        {
            Point1 = p1;
            Point2 = p2;
            Point3 = p3;
        }

        public List<Edge> Edges => new List<Edge>
        {
            new Edge(Point1, Point2),
            new Edge(Point2, Point3),
            new Edge(Point3, Point1)
        };

        public bool ContainsAnyVertex((double X, double Y, double Z) v1, (double X, double Y, double Z) v2, (double X, double Y, double Z) v3)
        {
            return IsSamePoint(Point1, v1) || IsSamePoint(Point1, v2) || IsSamePoint(Point1, v3) ||
                   IsSamePoint(Point2, v1) || IsSamePoint(Point2, v2) || IsSamePoint(Point2, v3) ||
                   IsSamePoint(Point3, v1) || IsSamePoint(Point3, v2) || IsSamePoint(Point3, v3);
        }

        private bool IsSamePoint((double X, double Y, double Z) p1, (double X, double Y, double Z) p2)
        {
            const double tolerance = 1e-10;
            return Math.Abs(p1.X - p2.X) < tolerance && 
                   Math.Abs(p1.Y - p2.Y) < tolerance && 
                   Math.Abs(p1.Z - p2.Z) < tolerance;
        }
    }

    public class Edge
    {
        public (double X, double Y, double Z) Point1 { get; }
        public (double X, double Y, double Z) Point2 { get; }

        public Edge((double X, double Y, double Z) p1, (double X, double Y, double Z) p2)
        {
            Point1 = p1;
            Point2 = p2;
        }

        public bool HasEdge(Edge other)
        {
            return (IsSamePoint(Point1, other.Point1) && IsSamePoint(Point2, other.Point2)) ||
                   (IsSamePoint(Point1, other.Point2) && IsSamePoint(Point2, other.Point1));
        }

        private bool IsSamePoint((double X, double Y, double Z) p1, (double X, double Y, double Z) p2)
        {
            const double tolerance = 1e-10;
            return Math.Abs(p1.X - p2.X) < tolerance && 
                   Math.Abs(p1.Y - p2.Y) < tolerance && 
                   Math.Abs(p1.Z - p2.Z) < tolerance;
        }
    }
}
