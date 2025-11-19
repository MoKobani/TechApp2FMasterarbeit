namespace DAL.Network
{
    /// <summary>
    /// Sammlung häufig benötigter ASCII-Steuerzeichen für das Socket-Protokoll.
    /// </summary>
    public static class AsciiControlCharacters
    {
        /// <summary>Start of Heading (SOH, 0x01) – markiert den Beginn der Nachricht.</summary>
        public const char StartOfHeading =  (char)1;

        /// <summary>Start of Text (STX, 0x02) – trennt Header- von Nutzdaten.</summary>
        public const char StartOfText = (char)2;

        /// <summary>End of Transmission (EOT, 0x04) – markiert das Ende der Nachricht.</summary>
        public const char EndOfTransmission = (char)4;

        /// <summary>Record Separator (RS, 0x1E) – trennt Datensätze voneinander.</summary>
        public const char RecordSeparator = (char)30;

        /// <summary>Unit Separator (US, 0x1F) – trennt Schlüssel und Werte innerhalb eines Datensatzes.</summary>
        //public const char UnitSeparator = (char)31;
    }
}
