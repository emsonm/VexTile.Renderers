using NetTopologySuite.Features;
using VexTile.Common.Enums;

namespace VexTile.Style.Mapbox.Filter;

public class TypeInFilter : Filter
{
    public IList<string> Types { get; }

    public TypeInFilter(IEnumerable<GeometryType> types)
    {
        Types = new List<string>();

        foreach (var type in types)
        {
	        Types.Add(type.ToString());
        }
    }

    public override bool Evaluate(IFeature feature)
    {
        foreach (var type in Types)
        {
	        if (feature.Geometry.GeometryType.Equals(type, StringComparison.Ordinal))
	        {
		        return true;
	        }
        }

        return false;
    }
}
