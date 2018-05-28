// —————————————————————–
// File: SecurePlayerPrefs.cs
// Original @author Rawand Fatih (rawandnf@gmail.com) | Copyright © 2014 Rawand Fatih
//
// @Contributor: Luciano Donati
// 
// me@lucianodonati.com <a href="http://www.lucianodonati.com">www.lucianodonati.com</a>
// 
// Description: Extension of Unity's PlayerPrefs system that uses DES encryption to secure the data stored.
// —————————————————————–

namespace LucianoDonati.SecurePlayerPrefs
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Extension of Unity's <see cref="PlayerPrefs"/> system that uses DES encryption to secure the data stored.
    /// </summary>
    public class SecurePlayerPrefs
    {
        #region Fields

        /*
         * The key is splited to three parts:
         *     - The prefix, first part.
         *     - Three randomly generated characters for each device.
         *     - The suffix, the end part of the key.
         * These constants are used in storing the random part for each device,
         * And for the prefs it uses the random number instead of PRIVATE_KEY_RAND.
         */

        /// <summary>
        /// Defines the prefix of the key. You should hardcode this to something different.
        /// </summary>
        private static readonly string PRIVATE_KEY_PREFIX = "1uc";

        /// <summary>
        /// Defines the suffix of the key. You should hardcode this to something different.
        /// </summary>
        private static readonly string PRIVATE_KEY_SUFFIX = "d0n";

        /// <summary>
        /// Defines the randomly generated key for each device
        /// </summary>
        private static readonly string RAND_KEY = "a9Gjj5R2eM9LkOIU45o6";

        /// <summary>
        /// Has the random key been generated?
        /// </summary>
        private static bool isInitialized = false;

        /// <summary>
        /// Defines the private key of DES encryption
        /// </summary>
        private static string privateKey;

        /// <summary>
        /// Activates output of Debug.Log() if excepions are caught.
        /// </summary>
        private static bool logErrorsEnabled = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether we should Debug.Log()
        /// </summary>
        public bool LogErrorsEnabled
        {
            get { return logErrorsEnabled; }
            set { logErrorsEnabled = value; }
        }


        /// <summary>
        /// Gets whether the class has been Initialized or not.
        /// </summary>
        public bool IsInitialized
        {
            get { return isInitialized; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the encryptor. 
        /// 
        /// <para>If its the frist time, it will generate a random 3 digit number and
        /// put it between <see cref="PRIVATE_KEY_PREFIX"/> and <see cref="PRIVATE_KEY_SUFFIX"/>.</para> 
        /// <para>If this was initialized on this device before, it will load it and create
        /// the private key.</para> 
        /// </summary>
        public static void Init()
        {
            if (HasKey(RAND_KEY))
                privateKey = PRIVATE_KEY_PREFIX + GetString(RAND_KEY) + PRIVATE_KEY_SUFFIX;
            else
            {
                int rand = UnityEngine.Random.Range(100, 999);
                privateKey = PRIVATE_KEY_PREFIX + rand + PRIVATE_KEY_SUFFIX;
                SetInt(RAND_KEY, rand);
            }
            isInitialized = true;
        }

        /// <summary>
        /// Get a securly encrypted float from the player preferences
        /// </summary>
        /// <param name="key">The key of the preference</param>
        /// <param name="defaultValue">The default <see cref="float"/> to use in case the key doesn't exist.</param>
        /// <returns>The decrypted <see cref="float"/></returns>
        public static float GetFloat(String key, float defaultValue = 0)
        {
            string persistedString = PlayerPrefs.GetString(key);
            if (persistedString != null && persistedString != String.Empty)
            {
                try
                {
                    string d = Decrypt(persistedString);
                    float f = float.Parse(d);
                    return f;
                }
                catch (Exception ex)
                {
                    if (logErrorsEnabled)
                    {
                        Debug.Log(ex.StackTrace);
                    }
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Saves a float in the player prefs securely encrypted.
        /// <para>Note that everything is saved as strings and parsed back to its original data type.</para>
        /// </summary>
        /// <param name="key">The key of the preference</param>
        /// <param name="val">The <see cref="float"/> of the preference, it will be encrypted. </param>
        public static void SetFloat(string key, float val)
        {
            SetString(key, val + String.Empty);
        }

        /// <summary>
        /// Get a securely encrypted bool from the player preferences.
        /// </summary>
        /// <param name="key">The key of the preference</param>
        /// <param name="defaultValue">The default <see cref="bool"/> to use in case the key doesn't exist.</param>
        /// <returns>The decrypted <see cref="bool"/></returns>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            int i = GetInt(key);
            if (i == 1)
            {
                return true;
            }
            return defaultValue;
        }

        /// <summary>
        /// Saves a bool in the player prefs securely encrypted.
        /// <para>Note that everything is saved as strings and parsed back to its original data type.</para>
        /// </summary>
        /// <param name="key">The key of the preference</param>
        /// <param name="val">The <see cref="bool"/> of the preference, it will be encrypted. </param>
        public static void SetBool(string key, bool val)
        {
            SetInt(key, ((val) ? 1 : 0));
        }

        /// <summary>
        /// Get a securely encrypted int from the player preferences.
        /// </summary>
        /// <param name="key">The key of the preference</param>
        /// <param name="defaultValue">The default <see cref="int"/> to use in case the key doesn't exist.</param>
        /// <returns>The decrypted <see cref="int"/></returns>
        public static int GetInt(String key, int defaultValue = 0)
        {
            string persistedString = PlayerPrefs.GetString(key);
            if (persistedString != String.Empty && persistedString != null)
            {
                try
                {
                    string d = Decrypt(persistedString);
                    int i = int.Parse(d);
                    return i;
                }
                catch (Exception ex)
                {
                    if (logErrorsEnabled)
                    {
                        Debug.Log(ex.StackTrace);
                    }
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Saves an int in the player prefs securely encrypted.
        /// <para>Note that everything is saved as strings and parsed back to its original data type.</para>
        /// </summary>
        /// <param name="key">The key of the preference</param>
        /// <param name="val">The <see cref="int"/> of the preference, it will be encrypted. </param>
        public static void SetInt(string key, int val)
        {
            SetString(key, val + String.Empty);
        }

        /// <summary>
        /// Get a securely encrypted string from the player preferences.
        /// </summary>
        /// <param name="key">The key of the preference</param>
        /// <returns>The decrypted <see cref="string"/></returns>
        public static string GetString(String key)
        {
            string persistedString = PlayerPrefs.GetString(key);
            if (persistedString != null && persistedString != String.Empty)
            {
                try
                {
                    return Decrypt(persistedString);
                }
                catch (Exception ex)
                {
                    if (logErrorsEnabled)
                    {
                        Debug.Log(ex.StackTrace);
                    }
                }
            }
            return persistedString;
        }

        /// <summary>
        /// Get a securely encrypted string from the player preferences.
        /// </summary>
        /// <param name="key">The key of the preference</param>
        /// <param name="defaultValue">The default <see cref="String"/> to use in case the key doesn't exist.</param>
        /// <returns>The decrypted <see cref="string"/></returns>
        public static string GetString(String key, String defaultValue)
        {
            string persistedString = PlayerPrefs.GetString(key, defaultValue);
            if (persistedString != null && persistedString != defaultValue && persistedString != String.Empty)
            {
                try
                {
                    return Decrypt(persistedString);
                }
                catch (Exception ex)
                {
                    if (logErrorsEnabled)
                    {
                        Debug.Log(ex.StackTrace);
                    }
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Saves a string in the player prefs securely encrypted.
        /// </summary>
        /// <param name="key">The key of the preference</param>
        /// <param name="val">The <see cref="string"/> of the preference, it will be encrypted. </param>
        public static void SetString(string key, string val)
        {
            PlayerPrefs.SetString(key, Encrypt(val));
        }

        /// <summary>
        /// Removes key and its corresponding value from the preferences.
        /// </summary>
        /// <param name="key">The key of the preference to delete.</param>
        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        /// <summary>
        /// Removes all keys and values from the preferences.
        /// <para>Use with caution!</para>
        /// </summary>
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        /// <summary>
        /// Returns true if key exists in the preferences.
        /// </summary>
        /// <param name="key">The key of the preference</param>
        /// <returns><see cref="bool"/> wether the key exists or not.</returns>
        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// Writes all modified preferences to disk.
        /// <para>This function will write to disk potentially causing a small hiccup,<br />
        /// be mindful of when to use it.</para>
        /// </summary>
        public static void Save()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Decrypts a cipher with DES.
        /// </summary>
        /// <param name="encryptedString">The cipher to decrypt</param>
        /// <returns>The decripted cipher <see cref="string"/></returns>
        private static string Decrypt(string encryptedString)
        {
            DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
                Key = Encoding.ASCII.GetBytes(privateKey)
            };
            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(encryptedString)))
            {
                using (CryptoStream cs = new CryptoStream(stream, desProvider.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs, Encoding.ASCII))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// Encrypts a plain text with DES
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>The encrypted text</returns>
        private static string Encrypt(string plainText)
        {
            DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
                Key = Encoding.ASCII.GetBytes(privateKey)
            };

            using (MemoryStream stream = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(stream, desProvider.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] data = Encoding.Default.GetBytes(plainText);
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
        }

        #endregion
    }
}
