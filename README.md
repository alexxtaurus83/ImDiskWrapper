LTRData.ImDiskNet Usage with ImDiskWrapper
This document outlines the core functionalities provided by the ImDiskWrapper C# class, which simplifies interactions with the LTRData.ImDiskNet library for managing virtual disks.

The LTRData.ImDiskNet NuGet package provides .NET bindings for the ImDisk Virtual Disk Driver, allowing programmatic creation, mounting, and management of RAM disks and image-backed virtual drives on Windows systems.

Key Concepts
Virtual Drive: A drive that appears as a physical disk to the operating system but is backed by a file, network, or system memory (RAM disk).

Device Number: A unique identifier assigned by the ImDisk driver to each mounted virtual device.

RAM Disk: A type of virtual drive where the disk space is allocated from the system's RAM, offering extremely fast read/write performance.

Image-based Virtual Drive: A virtual drive mounted from an existing disk image file (e.g., .iso, .vhd, .img).

ImDiskWrapper Class Functions
The ImDiskWrapper class encapsulates common operations, making it easier to integrate virtual disk functionality into your .NET applications.


LoadVirtualDrive
Mounts a virtual drive from a specified image file (.iso, .vhd, etc.) to a given drive letter.

FindFreeDrive
Locates and returns the first available unused drive letter on the system.

CreateRamDrive
Creates a new RAM disk (memory-backed virtual drive) of a specified size in megabytes and assigns it to a given drive letter.

RemoveVirtualDrive
Forcibly unmounts and removes a virtual drive from the system using its unique device number.

RemoveAllVirtualDrives
Iterates through all currently active virtual drives managed by ImDisk and forcibly removes them.

FormatDrive
Formats a specified virtual (or physical) drive letter with the NTFS file system using the system's format.com utility. This operation waits for completion.

CoreUtil Class Functions
The CoreUtil class provides general utility methods, some of which interact with the ImDiskWrapper context (e.g., using Data.RamDriveLetter).


BytesToFile
Decompresses a byte array and writes it to a new randomly named file on the configured RAM drive.

FileToBytes
Reads a file from a specified path, compresses its content, and returns it as a byte array.

Compress
Compresses a byte array using GZip compression.

Decompress
Decompresses a GZip compressed byte array.

SerializeToXml<T>
Serializes an object of a given type T into an XML string.

DeserializeFromXml<T>
Deserializes an XML string back into an object of a given type T.



RamDriveLetter
Stores the drive letter of the mounted RAM disk.

RamDriveId
Stores the device number of the mounted RAM disk.

Port
Stores a configured TCP port, typically for inter-process communication (e.g., gRPC).

Cert
Placeholder for certificate string (currently not loaded from config).

Logger, MaiLogger
Interfaces for application logging (requires an external ILogger implementation).

LoadConfiguration
Loads application settings (like Port) from App.config and initializes RAM drive properties.

CheckPortIsUsed
Checks if the configured Port is currently in use on localhost.

Cleanup
Resets the tracked RamDriveLetter and RamDriveId values to their defaults (does not unmount drives).

Usage Notes
LTRData.ImDiskNet Dependency: These classes rely on the LTRData.ImDiskNet NuGet package. Ensure it is installed in your project.

Administrator Privileges: Interacting with virtual disk drivers (especially creating/removing drives) often requires administrator privileges for your application.


format.com: The FormatDrive method uses format.com, a command-line utility. Ensure it's available in the system's PATH.