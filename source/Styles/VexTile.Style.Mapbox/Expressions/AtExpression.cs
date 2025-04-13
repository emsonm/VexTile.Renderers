using Newtonsoft.Json.Linq;
using VexTile.Common.Primitives;

namespace VexTile.Style.Mapbox.Expressions;

public class AtExpression(IExpression index, IExpression input) : Expression
{
    public static IExpression? Parse(JArray array, ExpressionParser parser)
    {
        if (array == null)
            throw new ArgumentException("");

        var length = array.Count;

        if (length != 3)
        {
            parser.Error($"Expected 2 arguments, but found {length - 1} instead.");

            return null;
        }

        var index = parser.Parse(array[1], 1, typeof(MGLNumberType));
        var inputType = parser.Expected;
        var input = parser.Parse(array[2], 2, inputType);

        return new AtExpression(index, input);
    }

    public override object Evaluate(EvaluationContext ctx)
    {
        throw new System.NotImplementedException();
    }

    public override object PossibleOutputs()
    {
        throw new System.NotImplementedException();
    }
}
