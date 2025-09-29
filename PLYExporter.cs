using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AGDConverter
{
    public class PLYExporter
    {
        private ConsoleProgressBar? _progressBar;

        /// <summary>
        /// Exports topology points to a PLY mesh file
        /// </summary>
        /// <param name="points">List of topology points to export</param>
        /// <param name="outputPath">Output file path</param>
        /// <param name="useProposedElevation">If true, uses proposed elevation; otherwise uses existing elevation</param>
        /// <param name="showProgress">Whether to show progress bar during conversion</param>
        public void ExportToPLY(List<TopologyPoint> points, string outputPath, bool useProposedElevation = true, bool showProgress = true)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("Points list cannot be null or empty");

            if (showProgress)
            {
                _progressBar = new ConsoleProgressBar();
                Console.WriteLine("Starting PLY conversion...");
            }

            // Find reference point for coordinate conversion
            if (showProgress)
            {
                _progressBar?.Update(0, 4, "Finding reference point");
            }
            var referencePoint = CoordinateConverter.FindReferencePoint(points);
            double refLat = referencePoint.ReferenceLat;
            double refLon = referencePoint.ReferenceLon;
            
            // Convert all points to EPSG:4326 coordinates
            if (showProgress)
            {
                _progressBar?.Update(1, 4, "Converting to EPSG:4326 coordinates");
            }
            var localPoints = new List<(double X, double Y, double Z)>();
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                var coordinates = CoordinateConverter.ConvertToLocalCoordinates(
                    point.Latitude, point.Longitude, refLat, refLon);
                double x = coordinates.X;
                double y = coordinates.Y;
                
                double z = useProposedElevation ? point.ProposedElevation : point.ExistingElevation;
                localPoints.Add((x, y, z));
                
                if (showProgress && (i % 100 == 0 || i == points.Count - 1))
                {
                    _progressBar?.Update(1, 4, $"Converting to EPSG:4326 ({i + 1}/{points.Count})");
                }
            }
            
            // Generate mesh using Delaunay triangulation (simplified approach)
            if (showProgress)
            {
                _progressBar?.Update(2, 4, "Starting triangulation...");
            }
            var triangles = GenerateTriangulation(localPoints);
            
            // Write PLY file
            if (showProgress)
            {
                _progressBar?.Update(3, 4, "Writing PLY file");
            }
            WritePLYFile(localPoints, triangles, outputPath);
            
            if (showProgress)
            {
                _progressBar?.Complete($"PLY file saved: {outputPath}");
            }
        }
        
        /// <summary>
        /// Generates Delaunay triangulation of the points using Bowyer-Watson algorithm
        /// </summary>
        private List<(int, int, int)> GenerateTriangulation(List<(double X, double Y, double Z)> points)
        {
            if (_progressBar != null)
            {
                _progressBar.Update(2, 4, "Starting Delaunay triangulation...");
            }

            var delaunay = new DelaunayTriangulation(_progressBar);
            var triangles = delaunay.Triangulate(points);

            if (_progressBar != null)
            {
                _progressBar.Update(2, 4, $"Delaunay triangulation complete - {triangles.Count} triangles");
            }

            return triangles;
        }
        
        
        /// <summary>
        /// Writes the PLY file with vertices and faces using EPSG:4326 coordinates
        /// </summary>
        private void WritePLYFile(List<(double X, double Y, double Z)> vertices, 
            List<(int, int, int)> triangles, string outputPath)
        {
            using (var writer = new StreamWriter(outputPath))
            {
                // PLY header with EPSG:4326 coordinate system information
                writer.WriteLine("ply");
                writer.WriteLine("format ascii 1.0");
                writer.WriteLine("comment Coordinate system: EPSG:4326 (WGS84)");
                writer.WriteLine("comment X = Longitude (decimal degrees)");
                writer.WriteLine("comment Y = Latitude (decimal degrees)");
                writer.WriteLine("comment Z = Elevation (meters)");
                writer.WriteLine($"element vertex {vertices.Count}");
                writer.WriteLine("property float x");
                writer.WriteLine("property float y");
                writer.WriteLine("property float z");
                writer.WriteLine($"element face {triangles.Count}");
                writer.WriteLine("property list uchar int vertex_indices");
                writer.WriteLine("end_header");
                
                // Write vertices
                for (int i = 0; i < vertices.Count; i++)
                {
                    var vertex = vertices[i];
                    writer.WriteLine($"{vertex.X:F6} {vertex.Y:F6} {vertex.Z:F6}");
                    
                    if (_progressBar != null && (i % 1000 == 0 || i == vertices.Count - 1))
                    {
                        _progressBar.Update(3, 4, $"Writing PLY file - vertices ({i + 1}/{vertices.Count})");
                    }
                }
                
                // Write faces
                for (int i = 0; i < triangles.Count; i++)
                {
                    var triangle = triangles[i];
                    writer.WriteLine($"3 {triangle.Item1} {triangle.Item2} {triangle.Item3}");
                    
                    if (_progressBar != null && (i % 1000 == 0 || i == triangles.Count - 1))
                    {
                        _progressBar.Update(3, 4, $"Writing PLY file - faces ({i + 1}/{triangles.Count})");
                    }
                }
            }
        }
    }
}
