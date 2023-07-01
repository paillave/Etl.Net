using System.Collections.Generic;
using System.Linq;

namespace Paillave.Etl.GraphApi.Provider;

public class ParameterBuilder
{
    public ParameterBuilder()
    {
        OrderByParameter = new List<string>();
    }

    public string FilterParameter { get; set; }

    public IList<string> OrderByParameter { get; private set; }

    public string SelectParameter { get; set; }

    public string SkipParameter { get; set; }

    public string TakeParameter { get; set; }

    public string ExpandParameter { get; set; }

    public bool IncludeCount { get; set; }

    public IEnumerable<KeyValuePair<string, string>> Build()
    {

        var parameters = new List<KeyValuePair<string, string>>();
        if (!string.IsNullOrWhiteSpace(FilterParameter))
        {
            parameters.Add(BuildParameter(StringConstants.FilterParameter, FilterParameter));
        }

        if (!string.IsNullOrWhiteSpace(SelectParameter))
        {
            parameters.Add(BuildParameter(StringConstants.SelectParameter, SelectParameter));
        }

        if (!string.IsNullOrWhiteSpace(SkipParameter))
        {
            parameters.Add(BuildParameter(StringConstants.SkipParameter, SkipParameter));
        }

        if (!string.IsNullOrWhiteSpace(TakeParameter))
        {
            parameters.Add(BuildParameter(StringConstants.TopParameter, TakeParameter));
        }

        if (OrderByParameter.Any())
        {
            parameters.Add(BuildParameter(StringConstants.OrderByParameter, string.Join(",", OrderByParameter)));
        }

        if (!string.IsNullOrWhiteSpace(ExpandParameter))
        {
            parameters.Add(BuildParameter(StringConstants.ExpandParameter, ExpandParameter));
        }

        if (IncludeCount)
        {
            parameters.Add(BuildParameter("$inlinecount", "allpages"));
        }

        return parameters;
    }

    private static KeyValuePair<string, string> BuildParameter(string name, string value)
    {

        return new KeyValuePair<string, string>(name, value);
    }
}
