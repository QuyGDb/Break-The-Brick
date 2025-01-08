using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Watermelon
{
    public static class Serializer
    {
        private static string persistentDataPath;

        public static void Initialise()
        {
            persistentDataPath = Application.persistentDataPath;
        }

        /// <summary>
        /// Deserializes file located at Persistent Data Path.
        /// </summary>
        /// <param name="fileName">Name of input file.</param>
        /// <returns>Deserialized object if file exists or new instance if doesn't.</returns>
        public static T Deserialize<T>(string fileName, bool logIfFileNotExists = false) where T : new()
        {
            string absolutePath = persistentDataPath + "/" + fileName;

            if (FileExistsAtPath(absolutePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(absolutePath, FileMode.Open);

                try
                {
                    T deserializedObject = (T)bf.Deserialize(file);

                    return deserializedObject;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    return new T();
                }
                finally
                {
                    file.Close();
                }
            }
            else
            {
                if (logIfFileNotExists)
                {
                    Debug.LogWarning("File at path : \"" + absolutePath + "\" does not exist.");
                }
                return new T();
            }
        }

        /// <summary>
        /// Serializes file to Persistent Data Path.
        /// </summary>
        /// <param name="fileName">Name of output file.</param>
        /// <param name="objectToSerialize">Reference to object that should be serialized.</param>
        public static void Serialize<T>(T objectToSerialize, string fileName)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.Open(persistentDataPath + "/" + fileName, FileMode.Create))
            {
                bf.Serialize(file, objectToSerialize);
            }
        }

        /// <summary>
        /// Checks if file exists at Persistent Data Path.
        /// </summary>
        /// <param name="fileName">Name of file to check.</param>
        /// <returns>True if file exists ans false otherwise.</returns>
        public static bool FileExistsAtPDP(string fileName)
        {
            return File.Exists(persistentDataPath + "/" + fileName);
        }

        /// <summary>
        /// Checks if file exists at Persistent Data Path.
        /// </summary>
        /// <param name="absolutePath">Absolute path to file(including file name and extention.</param>
        /// <returns>True if file exists ans false otherwise.</returns>
        public static bool FileExistsAtPath(string absolutePath)
        {
            return File.Exists(absolutePath);
        }

        /// <summary>
        /// Checks if file exists add specified directory.
        /// </summary>
        /// <param name="directoryPath">Full path to directory. Ends with directory name (without "/").</param>
        /// <param name="fileName">Name of file to check.</param>
        /// <returns>True if file exists and false otherwise.</returns>
        public static bool FileExistsAtPath(string directoryPath, string fileName)
        {
            return File.Exists(directoryPath + "/" + fileName);
        }

        /// <summary>
        /// Delete file at Persistent Data Path.
        /// </summary>
        /// <param name="fileName">Name of file to check.</param>
        public static void DeleteFileAtPDP(string fileName)
        {
            File.Delete(persistentDataPath + "/" + fileName);
        }

        /// <summary>
        /// Delete file at specified path.
        /// </summary>
        /// <param name="absolutePath">Absolute path to file(including file name and extention.</param>
        public static void DeleteFileAtPath(string absolutePath)
        {
            File.Delete(absolutePath);
        }

        /// <summary>
        /// Delete file at specified path.
        /// </summary>
        /// <param name="fileName">Name of file to check.</param>
        /// <param name="directoryPath">Full path to directory. Ends with directory name (without "/").</param>
        public static void DeleteFileAtPath(string directoryPath, string fileName)
        {
            File.Delete(directoryPath + "/" + fileName);
        }
    }
}