using System;
using System.IO;
using UnityEngine;

namespace pow.aidkit
{
    public class StoredScriptableObject : ScriptableObject
    {
        protected const string Password = "^*!1BYe8%ThKk@~sE2DP5tzNq/GF?g#nrdCv$wyAXjfVp3=c&u";
        protected string FilePath;
        protected string TempFilePath;

        protected void Load(Action<BinaryReader> readAction)
        {
            if (string.IsNullOrEmpty(FilePath)) return;
            if (string.IsNullOrEmpty(TempFilePath)) return;
            if (!File.Exists(FilePath)) return;

            try
            {
                Encryption.DecryptFile(FilePath, TempFilePath, Password);
            }
            catch (Exception e)
            {
                File.Delete(FilePath);
                Debug.LogWarning($"An error occured while decrypting the saved file for: {name}. File is deleted." +
                                 $"\nError: {e}");
            }

            using (var reader = new BinaryReader(File.Open(TempFilePath, FileMode.Open)))
            {
                readAction?.Invoke(reader);
            }

            File.Delete(TempFilePath);
        }

        protected void Save(Action<BinaryWriter> writeAction)
        {
            if (string.IsNullOrEmpty(FilePath)) return;
            if (string.IsNullOrEmpty(TempFilePath)) return;

            if (File.Exists(Path.Combine(Application.persistentDataPath, Application.productName)))
            {
                File.Create(Path.Combine(Application.persistentDataPath, Application.productName));
            }

            using (var writer = new BinaryWriter(File.Open(TempFilePath, FileMode.Create)))
            {
                writeAction?.Invoke(writer);
            }

            Encryption.EncryptFile(TempFilePath, FilePath, Password);

            File.Delete(TempFilePath);
        }

        public void RemoveStoredFile()
        {
            if (string.IsNullOrEmpty(FilePath)) return;
            if (!File.Exists(FilePath)) return;
            File.Delete(FilePath);
        }
    }
}