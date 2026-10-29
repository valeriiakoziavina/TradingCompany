namespace TradingCompany.BL;

public class BusinessRuleException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public BusinessRuleException(string error) : this([error])
    {
    }

    public BusinessRuleException(IReadOnlyList<string> errors) : base(string.Join(Environment.NewLine, errors))
    {
        Errors = errors;
    }
}
