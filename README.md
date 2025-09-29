# AGD Converter

A C# console application for converting AGD (Optisurface output) files to various mesh formats.

## Features

- Load AGD files using the existing AGDLoader class
- Convert topology points to PLY mesh format
- Command-line interface with NDesk.Options
- Support for both existing and proposed elevation data
- Coordinate conversion from lat/lon to local coordinates
- Real-time progress bar showing conversion status

## Usage

```bash
# Basic conversion
dotnet run -- -i input.agd -o output.ply

# Specify elevation type
dotnet run -- -i input.agd -o output.ply -e existing

# Show help
dotnet run -- --help
```

## Command Line Options

- `-i, --input=VALUE`: Input AGD file path
- `-o, --output=VALUE`: Output file path  
- `-f, --format=VALUE`: Output format (currently supports 'ply')
- `-e, --elevation=VALUE`: Elevation type: 'existing' or 'proposed' (default: existing)
- `-p, --progress`: Show progress bar during conversion (default: true)
- `-h, --help`: Show help message

## Output Formats

### PLY Format
- Exports topology points as a 3D mesh in PLY format
- Uses EPSG:4326 (WGS84) coordinate system with longitude/latitude
- Generates Delaunay triangulated mesh from topology points
- Supports both existing and proposed elevation data
- X = Longitude (decimal degrees), Y = Latitude (decimal degrees), Z = Elevation (meters)

## Building

```bash
dotnet build
```

## Dependencies

- .NET 6.0
- NDesk.Options (command-line parsing)
