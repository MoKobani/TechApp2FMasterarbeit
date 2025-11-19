namespace DAL.Network
{
    /// <summary>
    /// Ergebnis eines TCP-Sendevorgangs inklusive optionaler Fehlermeldung.
    /// </summary>
    public readonly record struct TransmissionResult(bool Success, string? Message)
    {
        public static TransmissionResult Ok(string? message ) => new(true,  string.IsNullOrWhiteSpace(message) ? null : message);

        public static TransmissionResult Fail(string? error) => new(false, string.IsNullOrWhiteSpace(error) ? null : error);
    }
}
