using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using LTR.IO.ImDisk; // Core library for interacting with ImDisk Virtual Disk Driver

namespace ImDiskWrapper
{
    /// <summary>
    /// Provides a wrapper around the LTR.IO.ImDisk library for common virtual disk operations.
    /// This class simplifies the creation, management, and removal of RAM disks and image-based virtual drives.
    /// </summary>
    public class ImDiskWrapper
    {
        /// <summary>
        /// Loads a virtual drive from an existing image file and assigns it a specific drive letter.
        /// </summary>
        /// <param name="driveLetter">The desired drive letter (e.g., "Z:") for the virtual drive.</param>
        /// <param name="driveImage">The full path to the drive image file (e.g., "C:\\path\\to\\myimage.iso").</param>
        /// <returns>The device number of the created virtual drive, or UInt32.MinValue if creation fails.</returns>
        public uint LoadVirtualDrive(string driveLetter, string driveImage)
        {
            try
            {
                uint deviceNumber = uint.MaxValue; // Initialize with a max value, indicating no device created yet.

                // Create the virtual device using ImDiskAPI.CreateDevice.
                // Parameters:
                // Size: 0 (size determined by image file)
                // Offset: 0 (start at beginning of image)
                // Flags (e.g., DeviceTypeHD for hard disk, FileTypeAwe for AWE memory backing if needed,
                // TypeVM for volatile memory backing, Removable for removable drive).
                // ImageFileName: Path to the image file.
                // ReadOnly: false (read-write access).
                // DriveLetter: The desired drive letter.
                // DeviceNumber: Output parameter for the created device number.
                // PartitionInfo: IntPtr.Zero (no specific partition info).
                ImDiskAPI.CreateDevice(0, 0, 0, 0, 0,
                                       ImDiskFlags.DeviceTypeHD | ImDiskFlags.FileTypeAwe | ImDiskFlags.TypeVM | ImDiskFlags.Removable,
                                       driveImage, false, driveLetter, ref deviceNumber, IntPtr.Zero);
                
                Debug.Print($"Virtual drive '{driveImage}' mounted as '{driveLetter}' with device number {deviceNumber}.");
                return deviceNumber;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes.
                Debug.Print($"Error loading virtual drive '{driveImage}': {ex.Message}");
                Console.WriteLine($"Error loading virtual drive '{driveImage}': {ex.Message}"); // Also output to console
                return uint.MinValue; // Indicate failure.
            }
        }

        /// <summary>
        /// Finds the first available free drive letter on the system.
        /// </summary>
        /// <returns>A string representing the first free drive letter followed by a colon (e.g., "E:"), or an empty string if none found.</returns>
        public string FindFreeDrive()
        {
            string freeDriveLetter = ImDiskAPI.FindFreeDriveLetter() + ":";
            Debug.Print($"Found free drive letter: {freeDriveLetter}");
            return freeDriveLetter;
        }

        /// <summary>
        /// Creates a new RAM disk (virtual hard drive backed by memory) and assigns it a drive letter.
        /// </summary>
        /// <param name="drive">The desired drive letter (e.g., "R:") for the RAM disk.</param>
        /// <param name="length">The size of the RAM disk in megabytes (MB).</param>
        /// <returns>The device number of the created RAM disk, or UInt32.MinValue if creation fails.</returns>
        public uint CreateRamDrive(string drive, long length)
        {
            try
            {
                uint deviceNumber = uint.MaxValue; // Initialize with a max value.
                long sizeInBytes = length * 1048576; // Convert MB to bytes (1 MB = 1024 * 1024 bytes)

                // Create the RAM disk using ImDiskAPI.CreateDevice.
                // Parameters:
                // Size: The size of the RAM disk in bytes.
                // Offset, Flags, ImageFileName, ReadOnly: Set appropriately for a RAM disk (null image, etc.).
                // DriveLetter: The desired drive letter.
                // DeviceNumber: Output parameter.
                ImDiskAPI.CreateDevice(sizeInBytes, 0, 0, 0, 0,
                                       ImDiskFlags.DeviceTypeHD | ImDiskFlags.FileTypeAwe | ImDiskFlags.TypeVM | ImDiskFlags.Removable,
                                       null, false, drive, ref deviceNumber, IntPtr.Zero);
                
                Thread.Sleep(2000); // Give the system a moment to recognize the new drive.
                Debug.Print($"RAM drive '{drive}' created with size {length}MB and device number {deviceNumber}.");
                return deviceNumber;
            }
            catch (Exception ex)
            {
                // Log and output the exception.
                Debug.Print($"Error creating RAM drive '{drive}' with length {length}MB: {ex.Message}");
                Console.WriteLine($"Error creating RAM drive '{drive}' with length {length}MB: {ex.Message}");
                return uint.MinValue; // Indicate failure.
            }
        }

        /// <summary>
        /// Forcibly removes a virtual drive given its device number.
        /// </summary>
        /// <param name="deviceNumber">The device number of the virtual drive to remove.</param>
        public void RemoveVirtualDrive(uint deviceNumber)
        {
            try
            {
                // Attempt to forcibly remove the specified virtual device.
                ImDiskAPI.ForceRemoveDevice(deviceNumber);
                Debug.Print($"Virtual drive with device number {deviceNumber} removed.");
            }
            catch (Exception ex)
            {
                Debug.Print($"Error removing virtual drive with device number {deviceNumber}: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes all currently active virtual drives managed by ImDisk.
        /// </summary>
        public void RemoveAllVirtualDrives()
        {
            try
            {
                // Iterate through all active ImDisk devices and forcibly remove them.
                foreach (var dev in ImDiskAPI.GetDeviceList())
                {
                    uint deviceNumber = Convert.ToUInt32(dev);
                    ImDiskAPI.ForceRemoveDevice(deviceNumber);
                    Debug.Print($"Removed virtual drive with device number {deviceNumber}.");
                }
                Debug.Print("All ImDisk virtual drives removed.");
            }
            catch (Exception ex)
            {
                Debug.Print($"Error removing all virtual drives: {ex.Message}");
            }
        }

        /// <summary>
        /// Formats a specified drive letter with the NTFS file system using the system's `format.com` utility.
        /// This is a blocking operation, waiting for the format process to complete.
        /// </summary>
        /// <param name="driveLetter">The drive letter to format (e.g., "R:").</param>
        public void FormatDrive(string driveLetter)
        {
            Debug.Print($"Attempting to format drive: {driveLetter} as NTFS.");
            // Launch `format.com` with /FS:NTFS for file system, /Q for quick format, and /Y for auto-confirm.
            LaunchProcess($"format.com {driveLetter} /FS:NTFS /Q /Y", true);
            Debug.Print($"Format process for drive {driveLetter} completed.");
        }

        // --- P/Invoke Declarations for CreateProcess and related functions ---
        // These are used internally to launch system processes (like format.com)
        // and wait for their completion.

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CreateProcess(
            String imageName,           // lpApplicationName
            String cmdLine,             // lpCommandLine
            IntPtr lpProcessAttributes, // lpProcessAttributes
            IntPtr lpThreadAttributes,  // lpThreadAttributes
            Int32 boolInheritHandles,   // bInheritHandles
            Int32 dwCreationFlags,      // dwCreationFlags
            IntPtr lpEnvironment,       // lpEnvironment
            IntPtr lpszCurrentDir,      // lpCurrentDirectory
            byte[] si,                  // lpStartupInfo (STARTUPINFOA struct)
            ProcessInfo pi              // lpProcessInformation (PROCESS_INFORMATION struct)
        );

        [DllImport("kernel32.dll")]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetExitCodeProcess(IntPtr hProcess, ref int lpExitCode);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// Internal class to hold process information obtained from CreateProcess.
        /// Corresponds to the PROCESS_INFORMATION structure in Windows API.
        /// </summary>
        private class ProcessInfo
        {
            public IntPtr HProcess;   // Handle to the process
            public IntPtr HThread;    // Handle to the primary thread of the process
            public IntPtr ProcessId;  // Process ID
            public IntPtr ThreadId;   // Thread ID
        }

        /// <summary>
        /// Launches an external process and optionally waits for its completion.
        /// </summary>
        /// <param name="commandToLaunch">The command line to execute (e.g., "cmd.exe /c dir").</param>
        /// <param name="waitforexit">If true, the method will block until the launched process exits.</param>
        private void LaunchProcess(string commandToLaunch, bool waitforexit)
        {
            var pi = new ProcessInfo(); // Holds information about the new process.
            var si = new byte[128];     // Represents STARTUPINFO structure, typically 128 bytes.

            Debug.Print($"Launching process: {commandToLaunch}");
            // Call the Windows API CreateProcess function.
            // First parameter (imageName) is null as cmdLine contains the full command.
            CreateProcess(null, commandToLaunch, IntPtr.Zero, IntPtr.Zero, 0, 0, IntPtr.Zero, IntPtr.Zero, si, pi);
            
            Thread.Sleep(2000); // Give the process a moment to start up.

            if (waitforexit)
            {
                // Wait indefinitely for the process to exit (0xFFFFFFFF = INFINITE).
                WaitForSingleObject(pi.HProcess, 0xFFFFFFFF);
            }

            var exitCode = 0;
            // Retrieve the exit code of the process.
            GetExitCodeProcess(pi.HProcess, ref exitCode);
            Debug.Print($"Process '{commandToLaunch}' exited with code: {exitCode}");

            // Close the process and thread handles to release resources.
            CloseHandle(pi.HProcess);
            CloseHandle(pi.HThread);
            
            Thread.Sleep(1000); // Small delay before returning.
        }
    }
}