using NetTopologySuite.Features;
using VexTile.Common.Enums;

namespace VexTile.Style.Mapbox.Filter;

public class TypeNotEqualsFilter(GeometryType type) : Filter
{
    public string Type { get; } = type.ToString();

    public override bool Evaluate(IFeature feature) => !feature.Geometry.GeometryType.Equals(Type, StringComparison.Ordinal);
}
