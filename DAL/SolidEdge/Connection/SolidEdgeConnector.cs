using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DAL.SolidEdge.Connection
{
    /// <summary>
    /// Manages connection to the Solid Edge application.
    /// </summary>
    public static class SolidEdgeConnector
    {
        // Fehlercode von Marshal.GetActiveObject(), wenn die App nicht läuft (MK_E_UNAVAILABLE).
        private const int APP_NOT_RUNNING_HRESULT = -2147221021;

        static SolidEdgeFramework.Application _app = null!;


        /// <summary>
        /// Versucht, eine Verbindung zu Solid Edge herzustellen.
        /// - Wenn Solid Edge bereits läuft, wird die bestehende Instanz verwendet.
        /// - Falls Solid Edge nicht läuft und <paramref name="startIfNotRunning"/> <c>true</c> ist, wird es gestartet.
        /// </summary>
        /// <param name="startIfNotRunning">
        /// Gibt an, ob Solid Edge gestartet werden soll, wenn keine laufende Instanz gefunden wird.
        /// </param>
        /// <returns>
        /// Eine <see cref="SolidEdgeFramework.Application"/>-Instanz, wenn die Verbindung erfolgreich ist; andernfalls <c>null</c>.
        /// </returns>
        public static SolidEdgeFramework.Application TryConnect(bool startIfNotRunning)
        {
            try
            {
                OleMessageFilter.Register();

                // 1) Laufende Instanz aus ROT holen
                var running = ComHelpers.GetRunningObjectOrNull("SolidEdge.Application");
                if (running is SolidEdgeFramework.Application seApp)
                {
                    _app = seApp;
                    _app.Visible = true;
                    return _app;
                }

                // 2) Nicht laufend -> optional starten
                if (startIfNotRunning)
                {
                    var type = Type.GetTypeFromProgID("SolidEdge.Application", throwOnError: true);
                    _app = (SolidEdgeFramework.Application)Activator.CreateInstance(type!)!;
                    _app.Visible = true;
                    _app.Activate();
                    return _app;
                }

                // App läuft nicht & startIfNotRunning=false
                throw new InvalidOperationException("Solid Edge läuft nicht und wurde nicht gestartet.");
            }
            catch (COMException comEx) when (comEx.HResult == APP_NOT_RUNNING_HRESULT)
            {
                throw new InvalidOperationException("Solid Edge ist nicht verfügbar.", comEx);
            }
        }


        /// <summary>
        /// Trennt die Verbindung zur aktuell verwendeten Solid Edge Instanz.
        /// </summary>

        /// <returns>
        /// <c>true</c>, wenn die Verbindung erfolgreich getrennt wurde;  
        /// <c>false</c>, wenn keine gültige Instanz vorhanden ist oder ein Fehler beim Trennen auftritt.
        /// </returns>
        public static bool TryDisconnect()
        {
            if (_app == null)
            {
                return false; // Keine App zum Trennen.
            }
            try
            {
                // Versuche, die Verbindung zu Solid Edge zu trennen.
                Marshal.ReleaseComObject(_app);
                OleMessageFilter.Unregister();
                _app = null!;
                return true;
            }
            catch
            {
                // Fehler beim Trennen der Verbindung.
                return false;
            }
        }
    }
}




