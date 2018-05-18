using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/**
 * Securely save player preferences in Unity3d projects.
 * This uses DES Encryption to encrypt your player prefs.
 *
 * @author Rawand Fatih (rawandnf@gmail.com)
 * Copyright © 2014 Rawand Fatih
 * 
 * @contributor Luciano Donati (me@lucianodonati.com)
 */
public class SecurePlayerPrefs
{

    /*
   * The key is splited to three parts:
   *     - The prefix, first part.
   *     - Three randomly generated characters for each device.
   *     - The suffix, the end part of the key.
   * These constants are used in storing the random part for each device,
   * And for the prefs it uses the random number instead of PRIVATE_KEY_RAND.
   */
    private static readonly string PRIVATE_KEY_PREFIX = "1uc";
    private static readonly string PRIVATE_KEY_SUFFIX = "d0n";

    // The private key of DES encryption.
    private static string privateKey;

    // Is the secure prefs initialized.
    private static bool isInit = false;

    // Your private key to store the randomly generated key for each device.
    private static readonly string RAND_KEY = "a9Gjj5R2eM9LkOIU45o6";

    // Should errors/exceptions be logged.
    private static bool logErrorsEnabled = false;


    /**
	 * Initializes the encryptor. If its the frist time, it will generate
	 * a random 3 digit number and puts it between private key and its appendix.
	 * If this was initialized on this device before, it will load it and create
	 * the private key.
	 */
    public static void Init()
    {
        if (HasKey(RAND_KEY))
            privateKey = PRIVATE_KEY_PREFIX + GetString(RAND_KEY) + PRIVATE_KEY_SUFFIX;
        else
        {
            int rand = UnityEngine.Random.Range(100,999);
            privateKey = PRIVATE_KEY_PREFIX + rand + PRIVATE_KEY_SUFFIX;
            SetInt(RAND_KEY, rand);
        }
        isInit = true;
    }


    /**
	 * Is the decryptor initialized.
	 */
    public static bool isInitialized()
    {
        return isInit;
    }

    /**
	 * Change the state of error log enabled.
	 * 
	 * @param state The new state of error log enabled.
	 */
    public static void setLogErrorsEnabled(bool state)
    {
        logErrorsEnabled = state;
    }

    /**
	 * Saves a string in player preferences but securly encrypted.
	 * @param key The preference id.
	 * @param val The value of the preference, it will be encrypted.
	 */
    public static void SetString(string key, string val)
    {
        PlayerPrefs.SetString(key, Encrypt(val));
    }

    /**
	 * Saves a float in the player prefs securely encrypted.
	 * Note that everything is saved as strings, but can use methods provided
	 * in this class to get the float, or just get your string decrypt and parse
	 * it back to float.
	 * 
	 * @param key The preference id.
	 * @param val The value of preference, it will be encrypted.
	 * @see {@code #SetString(key: string, val: string)}
	 */
    public static void SetFloat(string key, float val)
    {
        SetString(key, val + String.Empty);
    }

    /**
	 * Saves a float in the player prefs securely encrypted.
	 * Note that everything is saved as strings, but can use methods provided
	 * in this class to get the int, or just get your string decrypt and parse
	 * it back to int.
	 * 
	 * @param key The preference id.
	 * @param val The value of preference, it will be encrypted.
	 * @see {@code #SetString(key: string, val: string)}
	 */
    public static void SetInt(string key, int val)
    {
        SetString(key, val + String.Empty);
    }

    /**
	 * Save a boolean in the player prefs.
	 * 
	 * @param key The preference id.
	 * @param val The value of preference, it will be encrypted.
	 * @see {@code #SetString(key: string, val: bool)}
	 */
    public static void SetBool(string key, bool val)
    {
        SetInt(key, ((val) ? 1 : 0));
    }

    /**
	 * Get a securly encrypted text from the player preferences.
	 *
	 * @param key The id of the player preferences.
	 * @param defaultValue The default to return.
	 * @return The decrypted value or default in case of not found.
	 */
    public static string GetString(String key, String defaultValue)
    {
        string s = PlayerPrefs.GetString(key, defaultValue);
        if (s != defaultValue && s != String.Empty && s != null)
        {
            try
            {
                return Decrypt(s);
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

    /**
	 * Get a securly encrypted int from the player preferences.
	 *
	 * @param key The id of the player preferences.
	 * @param defaultValue The default to return.
	 * @return The decrypted value or default in case of not found
	 */
    public static int GetInt(String key, int defaultValue)
    {
        string s = PlayerPrefs.GetString(key);
        if (s != String.Empty && s != null)
        {
            try
            {
                string d = Decrypt(s);
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

    /**
	 * Get a securly encrypted float from the player preferences.
	 *
	 * @param key The id of the player preferences.
	 * @param defaultValue The default to return.
	 * @return The decrypted value or default in case of not found
	 */
    public static float GetFloat(String key, float defaultValue)
    {
        string s = PlayerPrefs.GetString(key);
        if (s != String.Empty && s != null)
        {
            try
            {
                string d = Decrypt(s);
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

    /**
	 * Get a securly encrypted bool from the player preferences.
	 *
	 * @param key The id of the player preferences.
	 * @param defaultValue the default value to return.
	 * @return The decrypted value or default in case of not found.
	 */
    public static bool GetBool(string key, bool defaultValue)
    {
        int i = GetInt(key);
        if (i == 1)
        {
            return true;
        }
        return defaultValue;
    }

    /**
	 * Removes key and its corresponding value from the preferences.
	 *
	 * @param key The id of the player preferences.
	 */
    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    /**
	 * Removes all keys and values from the preferences.
	 * Use with caution.
	 */
    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    /**
	 * Returns true if key exists in the preferences.
	 *
	 * @param key The id of the preference.
	 */
    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    /**
	 * Writes all modified preferences to disk.
	 * 
   * By default Unity writes preferences to disk on Application Quit.
   * In case when the game crashes or otherwise prematuraly exits,
   * you might want to write the PlayerPrefs at sensible 'checkpoints'
   * in your game. This function will write to disk potentially causing
   * a small hiccup, therefore it is not recommended to call during actual gameplay.
	 */
    public static void Save()
    {
        PlayerPrefs.Save();
    }

    /**
   * Decrypts a cipher with DES.
   *
   * @param encryptedString The cipher to decrypt.
   */
    private static string Decrypt(string encryptedString)
    {
        DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
        desProvider.Mode = CipherMode.ECB;
        desProvider.Padding = PaddingMode.PKCS7;
        desProvider.Key = Encoding.ASCII.GetBytes(privateKey);
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

    /**
   * Encrypt a plain text with DES.
   *
   * @param plainText The text to encrypt.
   */
    private static string Encrypt(string plainText)
    {
        DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
        desProvider.Mode = CipherMode.ECB;
        desProvider.Padding = PaddingMode.PKCS7;
        desProvider.Key = Encoding.ASCII.GetBytes(privateKey);
        using (MemoryStream stream = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(stream, desProvider.CreateEncryptor(), CryptoStreamMode.Write))
            {
                byte[] data = Encoding.Default.GetBytes(plainText);
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock(); // <-- Add this
                return Convert.ToBase64String(stream.ToArray());
            }
        }
    }
}
