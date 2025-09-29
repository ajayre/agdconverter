using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace AGDConverter
{
	public class AGDLoader
	{
		public List<TopologyPoint> LoadTopologyData(string filePath)
		{
			var topologyPoints = new List<TopologyPoint>();

			string[] lines = File.ReadAllLines(filePath);

			for (int i = 1; i < lines.Length; i++) // Skip header
			{
				string[] fields = lines[i].Split(',');
				if (fields.Length >= 7)
				{
					TopologyPoint point = new TopologyPoint
					{
						Latitude = double.Parse(fields[0].Trim(), CultureInfo.InvariantCulture),
						Longitude = double.Parse(fields[1].Trim(), CultureInfo.InvariantCulture),
						Code = fields[5].Trim(),
						Comments = fields[6].Trim()
					};

					// Parse existing elevation if available
					if (!string.IsNullOrEmpty(fields[2].Trim()))
					{
						point.ExistingElevation = double.Parse(fields[2].Trim(), CultureInfo.InvariantCulture);
					}

					// Parse proposed elevation if available
					if (!string.IsNullOrEmpty(fields[3].Trim()))
					{
						point.ProposedElevation = double.Parse(fields[3].Trim(), CultureInfo.InvariantCulture);
					}

					// Only process points that have both existing and proposed elevations
					if (point.ExistingElevation != 0 && point.ProposedElevation != 0)
					{
						// Parse cut/fill if available
						if (!string.IsNullOrEmpty(fields[4].Trim()) && fields[4].Trim() != "0.000")
						{
							// agd file uses cut = negative but we use cut = positive
							point.CutFill = -double.Parse(fields[4].Trim(), CultureInfo.InvariantCulture);
						}
						else
						{
							// Calculate cut/fill from existing and proposed elevations
							// Positive = cut (existing > proposed), Negative = fill (existing < proposed)
							point.CutFill = point.ExistingElevation - point.ProposedElevation;
						}

						topologyPoints.Add(point);
					}
				}
			}

			return topologyPoints;
		}
	}
}

