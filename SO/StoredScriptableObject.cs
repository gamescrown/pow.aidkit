using System;
using System.IO;
using UnityEngine;

namespace pow.aidkit
{
    public abstract class StoredScriptableObject : ScriptableObject
    {
        private string _filePath;
        private string _tempFilePath;

        private const string Password = "^*!1BYe8%ThKk@~sE2DP5tzNq/GF?g#nrdCv$wyAXjfVp3=c&u";

        private void Init(string s)
        {
            string encryptedName = TextEncryption.Encrypt(s, Password);
            _filePath = Path.Combine(Application.persistentDataPath, encryptedName);
            _tempFilePath = Path.Combine(Application.persistentDataPath, $"temp{encryptedName}");
        }

        private void Load()
        {
            if (string.IsNullOrEmpty(_filePath) || string.IsNullOrEmpty(_tempFilePath)) return;

            if (!File.Exists(_filePath)) return;

            try
            {
                Encryption.DecryptFile(_filePath, _tempFilePath, Password);
            }
            catch (Exception e)
            {
                File.Delete(_filePath);
                Debug.LogWarning($"An error occured while decrypting the saved file for: {name}. File is deleted." +
                                 $"\nError: {e}");
            }

            using (var reader = new BinaryReader(File.Open(_tempFilePath, FileMode.Open)))
            {
                Read(reader);
            }

            File.Delete(_tempFilePath);
        }

        protected void Save()
        {
            if (string.IsNullOrEmpty(_filePath) || string.IsNullOrEmpty(_tempFilePath)) return;

            using (var writer = new BinaryWriter(File.Open(_tempFilePath, FileMode.Create)))
            {
                Write(writer);
            }

            Encryption.EncryptFile(_tempFilePath, _filePath, Password);

            File.Delete(_tempFilePath);
        }

        public virtual void OnEnable()
        {
            Init(name);
            Load();
        }

        protected abstract void Write(BinaryWriter writer);
        protected abstract void Read(BinaryReader reader);

        /// <summary>
        /// If you want to reset the data from ResetGameProgress, override this method and call base.Reset()
        /// </summary>
        public virtual void Reset()
        {
            Debug.Log($"Resetting stored file values: {name}");
        }

        public void RemoveStoredFile()
        {
            if (string.IsNullOrEmpty(_filePath)) return;
            if (!File.Exists(_filePath)) return;
            File.Delete(_filePath);
        }
    }
}