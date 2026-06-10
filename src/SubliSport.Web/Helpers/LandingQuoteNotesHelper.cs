namespace SubliSport.Web.Helpers;

public static class LandingQuoteNotesHelper
{
    private const string InternalStart = "<<INTERNAL>>";
    private const string InternalEnd = "<<END_INTERNAL>>";
    private const string ClientStart = "<<CLIENT>>";
    private const string ClientEnd = "<<END_CLIENT>>";

    public static string Pack(string internalSuggestion, string clientProforma) =>
        $"{InternalStart}\n{internalSuggestion.Trim()}\n{InternalEnd}\n{ClientStart}\n{clientProforma.Trim()}\n{ClientEnd}";

    public static (string Internal, string Client) Unpack(string? pricingNotes)
    {
        if (string.IsNullOrWhiteSpace(pricingNotes))
        {
            return (string.Empty, string.Empty);
        }

        if (!pricingNotes.Contains(InternalStart, StringComparison.Ordinal))
        {
            return (pricingNotes.Trim(), string.Empty);
        }

        var internalText = Extract(pricingNotes, InternalStart, InternalEnd);
        var clientText = Extract(pricingNotes, ClientStart, ClientEnd);
        return (internalText, clientText);
    }

    public static string GetClientProforma(string? pricingNotes) => Unpack(pricingNotes).Client;

    public static string GetInternalSuggestion(string? pricingNotes) => Unpack(pricingNotes).Internal;

    public static bool IsLandingQuote(string? pricingNotes) =>
        !string.IsNullOrWhiteSpace(pricingNotes) &&
        pricingNotes.Contains(InternalStart, StringComparison.Ordinal);

    private static string Extract(string text, string start, string end)
    {
        var startIdx = text.IndexOf(start, StringComparison.Ordinal);
        if (startIdx < 0) return string.Empty;
        startIdx += start.Length;
        var endIdx = text.IndexOf(end, startIdx, StringComparison.Ordinal);
        if (endIdx < 0) return text[startIdx..].Trim();
        return text[startIdx..endIdx].Trim();
    }
}
