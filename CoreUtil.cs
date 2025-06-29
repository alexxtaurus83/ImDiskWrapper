using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Serialization; // Required for XML serialization/deserialization

namespace ImDiskWrapper
{
    /// <summary>
    /// Provides core utility functions including file operations, data compression/decompression,
    /// and XML serialization/deserialization.
    /// </summary>
    public class CoreUtil
    {
        //region File Operations

        /// <summary>
        /// Writes a byte array to a new file on the RAM drive, decompressing the data first.
        /// The file name is randomly generated.
        /// </summary>
        /// <param name="bytes">The compressed byte array to write.</param>
        /// <returns>The full path to the newly created file.</returns>
        public string BytesToFile(byte[] bytes)
        {
            // Combines the RAM drive letter (from Data.cs) with a random file name.
            // Assumes Data.RamDriveLetter is correctly set and the RAM drive is mounted.
            var path = Path.Combine(Data.RamDriveLetter + @"\", Path.GetRandomFileName());
            
            // Decompress the bytes before writing them to the file.
            File.WriteAllBytes(path, Decompress(bytes));
            Debug.Print($"Bytes written to file: {path}");
            return path;
        }

        /// <summary>
        /// Reads all bytes from a specified file, compresses them, and returns the compressed byte array.
        /// </summary>
        /// <param name="filePath">The full path to the file to read.</param>
        /// <returns>A compressed byte array containing the file's content.</returns>
        public byte[] FileToBytes(string filePath)
        {
            // Read all bytes from the file and then compress them.
            byte[] fileBytes = File.ReadAllBytes(filePath);
            Debug.Print($"Read {fileBytes.Length} bytes from file: {filePath}");
            return Compress(fileBytes);
        }

        //endregion

        //region Compression Utilities (GZip)

        /// <summary>
        /// Compresses a byte array using GZip compression.
        /// </summary>
        /// <param name="data">The uncompressed byte array to compress.</param>
        /// <returns>A new byte array containing the compressed data.</returns>
        public byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            {
                // Create a GZipStream to write compressed data to the memory stream.
                // CompressionLevel.Optimal provides the best compression ratio, potentially at slower speed.
                using (var zipStream = new GZipStream(compressedStream, CompressionLevel.Optimal))
                {
                    zipStream.Write(data, 0, data.Length); // Write the uncompressed data.
                } // zipStream is automatically closed here, flushing data to compressedStream.

                byte[] compressedData = compressedStream.ToArray(); // Get the compressed data as a byte array.
                Debug.Print($"Compressed {data.Length} bytes to {compressedData.Length} bytes.");
                return compressedData;
            }
        }

        /// <summary>
        /// Decompresses a GZip compressed byte array.
        /// </summary>
        /// <param name="data">The compressed byte array to decompress.</param>
        /// <returns>A new byte array containing the decompressed data.</returns>
        public byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            {
                // Create a GZipStream for decompression, reading from the compressed memory stream.
                using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        zipStream.CopyTo(resultStream); // Copy decompressed data to the result stream.
                        byte[] decompressedData = resultStream.ToArray(); // Get the decompressed data.
                        Debug.Print($"Decompressed {data.Length} bytes to {decompressedData.Length} bytes.");
                        return decompressedData;
                    }
                }
            }
        }

        //endregion

        //region XML Serialization Utilities

        /// <summary>
        /// Serializes an object of type T to an XML string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="data">The object instance to serialize.</param>
        /// <returns>An XML string representing the serialized object.</returns>
        public string SerializeToXml<T>(T data)
        {
            var stringWriter = new StringWriter(); // Used to write XML to a string.
            
            // Configure XML writer settings for UTF-8 encoding and indentation.
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true // Makes the XML output human-readable.
            };

            using (var writer = XmlWriter.Create(stringWriter, settings))
            {
                var xmlSerializer = new XmlSerializer(typeof(T)); // Create a serializer for the given type.
                
                // Serialize the object.
                // XmlQualifiedName.Empty is used to suppress default XML namespaces, often cleaner for simple types.
                xmlSerializer.Serialize(writer, data, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
            }
            Debug.Print($"Object of type {typeof(T).Name} serialized to XML.");
            return stringWriter.ToString(); // Return the accumulated XML string.
        }

        /// <summary>
        /// Deserializes an XML string back into an object of type T.
        /// </summary>
        /// <typeparam name="T">The target type to deserialize the XML into.</typeparam>
        /// <param name="xml">The XML string to deserialize.</param>
        /// <returns>An object instance of type T populated with data from the XML.</returns>
        public T DeserializeFromXml<T>(string xml)
        {
            using (var reader = new StringReader(xml)) // Use a StringReader to read XML from the string.
            {
                var xmlSerializer = new XmlSerializer(typeof(T)); // Create a serializer for the target type.
                
                // Deserialize the XML back into an object of type T.
                T deserializedObject = (T)xmlSerializer.Deserialize(reader);
                Debug.Print($"XML deserialized to object of type {typeof(T).Name}.");
                return deserializedObject;
            }
        }

        //endregion
    }
}