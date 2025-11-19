using System;
using System.Runtime.InteropServices;

namespace DAL.SolidEdge.Connection
{
    /// <summary>
    /// Helper für COM: findet laufende Objekte über die ROT (Ersatz für Marshal.GetActiveObject).
    /// </summary>
    internal static class ComHelpers
    {
        private const int S_OK = 0;
        private const int MK_E_UNAVAILABLE = unchecked((int)0x800401E3);

        /// <summary>Win32: ProgID → CLSID.</summary>
        [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
        private static extern int CLSIDFromProgID(string lpszProgID, out Guid pclsid);

        /// <summary>Win32: liefert laufendes Objekt aus der ROT (oleaut32).</summary>
        [DllImport("oleaut32.dll")]
        private static extern int GetActiveObject(ref Guid rclsid, nint reserved, out nint ppunk);

        /// <summary>
        /// Holt ein laufendes COM-Objekt per ProgID oder gibt null zurück.
        /// </summary>
        /// <param name="progId">z. B. "SolidEdge.Application".</param>
        /// <returns>Das Objekt oder null, wenn nichts läuft.</returns>
        /// <exception cref="COMException">Bei HRESULT-Fehlern außer MK_E_UNAVAILABLE.</exception>
        public static object? GetRunningObjectOrNull(string progId)
        {
            // ProgID → CLSID auflösen
            int hr = CLSIDFromProgID(progId, out Guid clsid);
            if (hr != S_OK)
                Marshal.ThrowExceptionForHR(hr);

            // Laufendes Objekt aus der ROT holen (oleaut32!)
            hr = GetActiveObject(ref clsid, nint.Zero, out nint punk);
            if (hr == MK_E_UNAVAILABLE)
                return null; // nichts läuft

            if (hr != S_OK)
                Marshal.ThrowExceptionForHR(hr);

            try
            {
                // IUnknown → RCW
                return Marshal.GetObjectForIUnknown(punk);
            }
            finally
            {
                if (punk != nint.Zero)
                    Marshal.Release(punk);
            }
        }
    }
}
