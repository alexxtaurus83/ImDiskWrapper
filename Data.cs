using System;
using System.IO;
using System.Configuration; // Required for ConfigurationManager.AppSettings
using System.Net.Sockets;   // Required for TcpClient

namespace ImDiskWrapper
{
    /// <summary>
    /// A static class to hold application-wide data and configuration settings.
    /// This includes settings for the RAM drive, network port, certificates, and logging interfaces.
    /// It also provides utility methods related to configuration and cleanup.
    /// </summary>
    public static class Data
    {
        //region Configuration Settings

        /// <summary>
        /// Gets or sets the drive letter assigned to the RAM disk (e.g., "R:").
        /// This is populated after a RAM drive is successfully created.
        /// </summary>
        public static string RamDriveLetter { get; set; }

        /// <summary>
        /// Gets or sets the device number of the created RAM disk.
        /// This is a unique identifier used by ImDisk.
        /// </summary>
        public static uint RamDriveId { get; set; }

        /// <summary>
        /// Gets or sets the TCP port number used by the application, likely for gRPC communication.
        /// Defaulted from AppSettings or to 50051.
        /// </summary>
        public static int Port { get; set; }

        /// <summary>
        /// Gets or sets the certificate string, likely for secure communication.
        /// (Currently not populated from configuration in LoadConfiguration).
        /// </summary>
        public static string Cert { get; set; }

        //endregion

        //region Logging Interfaces

        /// <summary>
        /// Gets or sets the primary logging interface for general application logs.
        /// (Assumes an ILogger interface is defined elsewhere in the project).
        /// </summary>
        public static ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets a secondary logging interface, possibly for main application activities.
        /// (Assumes an ILogger interface is defined elsewhere in the project).
        /// </summary>
        public static ILogger MaiLogger { get; set; }

        //endregion

        //region Utility Methods for Configuration and State Management

        /// <summary>
        /// Loads application configuration settings, specifically the gRPC port, from App.config.
        /// Initializes RamDriveLetter and RamDriveId to default empty/min values.
        /// </summary>
        public static void LoadConfiguration()
        {
            RamDriveLetter = string.Empty; // Reset RAM drive letter.
            RamDriveId = uint.MinValue;    // Reset RAM drive ID.

            // Attempt to parse "GRPCPort" from application settings in App.config.
            // If not found or invalid, defaults to 50051.
            int parsedPort;
            if (int.TryParse(ConfigurationManager.AppSettings["GRPCPort"], out parsedPort))
            {
                Port = parsedPort;
                Debug.Print($"Configuration: GRPCPort loaded as {Port}.");
            }
            else
            {
                Port = 50051; // Default port if not specified or invalid.
                Debug.Print($"Configuration: GRPCPort not found or invalid, defaulting to {Port}.");
            }
            // Note: Cert is not loaded from AppSettings in this method as per the original code.
        }

        /// <summary>
        /// Checks if the configured 'Port' is currently in use by an application listening on localhost.
        /// </summary>
        /// <returns>True if the port is in use, false otherwise.</returns>
        public static bool CheckPortIsUsed()
        {
            try
            {
                // Attempt to connect to localhost on the configured port.
                // A successful connection indicates the port is in use.
                using (var client = new TcpClient())
                {
                    client.Connect("localhost", Port);
                    Debug.Print($"Port {Port} is in use.");
                    return true;
                }
            }
            catch (SocketException ex)
            {
                // Catch specific SocketException (e.g., connection refused) indicating port is not in use.
                Debug.Print($"Port {Port} is not in use or connection failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions.
                Debug.Print($"An unexpected error occurred while checking port {Port}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cleans up static data by resetting RAM drive letter and ID to their default empty/min values.
        /// This does NOT remove the actual virtual drive, only resets the tracked state.
        /// </summary>
        public static void Cleanup()
        {
            RamDriveLetter = string.Empty; // Reset RAM drive letter.
            RamDriveId = uint.MinValue;    // Reset RAM drive ID.
            Debug.Print("Static Data (RamDriveLetter, RamDriveId) cleaned up.");
        }

        //endregion
    }

    /// <summary>
    /// Placeholder interface for logging.
    /// This needs to be defined elsewhere in the project if actual logging is to be implemented.
    /// Example: public interface ILogger { void LogInfo(string message); void LogError(string message, Exception ex = null); }
    /// </summary>
    public interface ILogger
    {
        // Example methods; actual implementation depends on your logging framework
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
        void LogDebug(string message);
    }
}