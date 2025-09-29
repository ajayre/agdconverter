using System;
using System.Collections.Generic;
using System.IO;
using NDesk.Options;

namespace AGDConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            bool showHelp = false;
            string? inputFile = null;
            string? outputFile = null;
            string outputFormat = "ply";
            bool useProposedElevation = false;
            bool showProgress = true;
            
            var options = new OptionSet()
            {
                { "i|input=", "Input AGD file path", v => inputFile = v },
                { "o|output=", "Output file path", v => outputFile = v },
                { "f|format=", "Output format (default: ply)", v => outputFormat = v },
                { "e|elevation=", "Elevation type: existing or proposed (default: existing)", v => useProposedElevation = v?.ToLower() == "proposed" },
                { "p|progress", "Show progress bar (default: true)", v => showProgress = v != null },
                { "h|help", "Show this help message", v => showHelp = v != null }
            };
            
            try
            {
                options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine("Try '--help' for more information.");
                return;
            }
            
            if (showHelp || string.IsNullOrEmpty(inputFile) || string.IsNullOrEmpty(outputFile))
            {
                ShowHelp(options);
                return;
            }
            
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Error: Input file '{inputFile}' does not exist.");
                return;
            }
            
            try
            {
                if (showProgress)
                {
                    Console.WriteLine($"Converting AGD file: {inputFile}");
                    Console.WriteLine($"Output file: {outputFile}");
                    Console.WriteLine($"Output format: {outputFormat}");
                    Console.WriteLine($"Using elevation: {(useProposedElevation ? "proposed" : "existing")}");
                    Console.WriteLine();
                }
                
                // Load AGD file
                if (showProgress)
                {
                    Console.WriteLine("Loading AGD file...");
                }
                var loader = new AGDLoader();
                var points = loader.LoadTopologyData(inputFile);
                
                if (showProgress)
                {
                    Console.WriteLine($"Loaded {points.Count} topology points");
                    Console.WriteLine();
                }
                
                if (points.Count == 0)
                {
                    Console.WriteLine("Warning: No valid topology points found in the AGD file.");
                    return;
                }
                
                // Convert based on output format
                switch (outputFormat.ToLower())
                {
                    case "ply":
                        var plyExporter = new PLYExporter();
                        plyExporter.ExportToPLY(points, outputFile, useProposedElevation, showProgress);
                        if (!showProgress)
                        {
                            Console.WriteLine($"Successfully exported PLY file: {outputFile}");
                        }
                        break;
                        
                    default:
                        Console.WriteLine($"Error: Unsupported output format '{outputFormat}'");
                        Console.WriteLine("Supported formats: ply");
                        return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        
        static void ShowHelp(OptionSet options)
        {
            Console.WriteLine("AGD Converter - Convert AGD files to various mesh formats");
            Console.WriteLine();
            Console.WriteLine("Usage: AGDConverter [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  AGDConverter -i input.agd -o output.ply");
            Console.WriteLine("  AGDConverter -i input.agd -o output.ply -f ply -e existing");
            Console.WriteLine("  AGDConverter -i input.agd -o output.ply --progress");
        }
    }
}
