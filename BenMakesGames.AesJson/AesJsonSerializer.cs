using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;

namespace BenMakesGames.AesJson;

public static class AesJsonSerializer
{
    /// <summary>
    /// Serializes, gzips, and encrypts (using Argon2i) an object to a file.
    /// </summary>
    /// <remarks>
    /// The same <c>password</c>, <c>salt</c>, and <c>profile</c> will be needed to read the file.
    /// </remarks>
    /// <param name="dto">Object to serialize</param>
    /// <param name="fileName">File to (over)write</param>
    /// <param name="password">Should be 16-32 bytes of hard-to-guess characters</param>
    /// <param name="salt">Should be 16 bytes</param>
    /// <param name="profile">Defaults to DesktopGame. (Controls settings for Argon2i hashing.)</param>
    /// <typeparam name="T"></typeparam>
    public static void WriteFile<T>(T dto, string fileName, string password, string salt, Profile profile = Profile.DesktopGame)
    {
        using var aes = CreateArgon2iAes(password, salt, profile);

        WriteFile(dto, fileName, aes);
    }

    /// <summary>
    /// Decrypts (using Argon2i), ungzips, and deserializes an object from a file.
    /// </summary>
    /// <remarks>
    /// The <c>password</c>, <c>salt</c>, and <c>profile</c> must match the values used to write the file, or an exception will be thrown.
    /// </remarks>
    /// <param name="fileName">File to read</param>
    /// <param name="password">Should be 16-32 bytes of hard-to-guess characters</param>
    /// <param name="salt">Should be 16 bytes</param>
    /// <param name="profile">Defaults to DesktopGame. (Controls degree of parallelism for Argon2i hashing.)</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Object of type T, or null.</returns>
    public static T? ReadFile<T>(string fileName, string password, string salt, Profile profile = Profile.DesktopGame)
    {
        using var aes = CreateArgon2iAes(password, salt, profile);

        return ReadFile<T>(fileName, aes);
    }

    /// <summary>
    /// Serializes, gzips, and encrypts an object to a file, using an encryption scheme of your choice.
    /// </summary>
    /// <param name="dto">Object to serialize</param>
    /// <param name="fileName">File to (over)write</param>
    /// <param name="aes"><c>Aes</c> instance to use for encryption</param>
    /// <typeparam name="T"></typeparam>
    public static void WriteFile<T>(T dto, string fileName, Aes aes)
    {
        var file = File.CreateText(fileName);

        using (var cryptoStream = new CryptoStream(file.BaseStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
        using (var gzip = new GZipStream(cryptoStream, CompressionLevel.Optimal))
        using (var gzipStream = new StreamWriter(gzip))
        using (var json = new Utf8JsonWriter(gzipStream.BaseStream))
        {
            JsonSerializer.Serialize(json, dto);
            gzipStream.Flush();
        }
    }

    /// <summary>
    /// Decrypts, ungzips, and deserializes an object from a file, using an encryption scheme of your choice.
    /// </summary>
    /// <remarks>
    /// The <c>password</c>, <c>salt</c>, and <c>cores</c> must match the values used to write the file, or an exception will be thrown.
    /// </remarks>
    /// <param name="fileName">File to read</param>
    /// <param name="aes"><c>Aes</c> instance to use for decryption</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Object of type T, or null.</returns>
    public static T? ReadFile<T>(string fileName, Aes aes)
    {
        using (var file = File.OpenText(fileName))
        using (var cryptoStream = new CryptoStream(file.BaseStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
        using (var gzip = new GZipStream(cryptoStream, CompressionMode.Decompress))
        using (var gzipStream = new StreamReader(gzip))
        {
            return JsonSerializer.Deserialize<T>(gzipStream.ReadToEnd());
        }
    }

    /// <summary>
    /// Asynchronously serializes, gzips, and encrypts (using Argon2i) an object to a file.
    /// </summary>
    /// <remarks>
    /// The same <c>password</c>, <c>salt</c>, and <c>profile</c> will be needed to read the file.
    /// </remarks>
    /// <param name="dto">Object to serialize</param>
    /// <param name="fileName">File to (over)write</param>
    /// <param name="password">Should be 16-32 bytes of hard-to-guess characters</param>
    /// <param name="salt">Should be 16 bytes</param>
    /// <param name="profile">Defaults to DesktopGame. (Controls settings for Argon2i hashing.)</param>
    /// <typeparam name="T"></typeparam>
    public static async Task WriteFileAsync<T>(T dto, string fileName, string password, string salt, Profile profile = Profile.DesktopGame)
    {
        var aes = CreateArgon2iAes(password, salt, profile);

        await WriteFileAsync(dto, fileName, aes);
    }

    /// <summary>
    /// Asynchronously decrypts (using Argon2i), ungzips, and deserializes an object from a file.
    /// </summary>
    /// <remarks>
    /// The <c>password</c>, <c>salt</c>, and <c>profile</c> must match the values used to write the file, or an exception will be thrown.
    /// </remarks>
    /// <param name="fileName">File to read</param>
    /// <param name="password">Should be 16-32 bytes of hard-to-guess characters</param>
    /// <param name="salt">Should be 16 bytes</param>
    /// <param name="profile">Defaults to DesktopGame. (Controls degree of parallelism for Argon2i hashing.)</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Object of type T, or null.</returns>
    public static async Task<T?> ReadFileAsync<T>(string fileName, string password, string salt, Profile profile = Profile.DesktopGame)
    {
        var aes = CreateArgon2iAes(password, salt, profile);

        return await ReadFileAsync<T>(fileName, aes);
    }

    /// <summary>
    /// Asynchronously serializes, gzips, and encrypts an object to a file, using an encryption scheme of your choice.
    /// </summary>
    /// <param name="dto">Object to serialize</param>
    /// <param name="fileName">File to (over)write</param>
    /// <param name="aes"><c>Aes</c> instance to use for encryption</param>
    /// <typeparam name="T"></typeparam>
    public static async Task WriteFileAsync<T>(T dto, string fileName, Aes aes)
    {
        var file = File.CreateText(fileName);

        await using (var cryptoStream = new CryptoStream(file.BaseStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
        await using (var gzip = new GZipStream(cryptoStream, CompressionLevel.Optimal))
        await using (var gzipStream = new StreamWriter(gzip))
        {
            await JsonSerializer.SerializeAsync(gzipStream.BaseStream, dto);
            await gzipStream.FlushAsync();
        }
    }

    /// <summary>
    /// Asynchronously decrypts, ungzips, and deserializes an object from a file, using an encryption scheme of your choice.
    /// </summary>
    /// <remarks>
    /// The <c>password</c>, <c>salt</c>, and <c>cores</c> must match the values used to write the file, or an exception will be thrown.
    /// </remarks>
    /// <param name="fileName">File to read</param>
    /// <param name="aes"><c>Aes</c> instance to use for decryption</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Object of type T, or null.</returns>
    public static async Task<T?> ReadFileAsync<T>(string fileName, Aes aes)
    {
        using (var file = File.OpenText(fileName))
        await using (var cryptoStream = new CryptoStream(file.BaseStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
        await using (var gzip = new GZipStream(cryptoStream, CompressionMode.Decompress))
        using (var gzipStream = new StreamReader(gzip))
        {
            return await JsonSerializer.DeserializeAsync<T>(gzipStream.BaseStream);
        }
    }

    private static Aes CreateArgon2iAes(string password, string salt, Profile profile)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

        argon2.Salt = Encoding.UTF8.GetBytes(salt);

        (argon2.DegreeOfParallelism, argon2.Iterations, argon2.MemorySize) = profile switch
        {
            Profile.WebServer => (8, 3, 64 * 1024),
            Profile.DesktopGame => (2, 1, 16 * 1024),
            Profile.MobileGame => (1, 1, 8 * 1024),
            _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, "Unknown device type")
        };

        var aes = Aes.Create();

        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 256;
        aes.Key = argon2.GetBytes(32);
        aes.IV = argon2.GetBytes(16);

        return aes;
    }
}

/// <summary>
/// Controls the parameters used for Argon2i hashing.
/// </summary>
public enum Profile
{
    /// <summary>
    /// Good defaults for security-conscious applications in the year 2025 where the password is never available to end-users (e.g. in an online key vault).
    /// </summary>
    WebServer,

    /// <summary>
    /// Super-fast defaults for PC/console games where the password and salt are ultimately available to dedicated players.
    /// </summary>
    DesktopGame,

    /// <summary>
    /// Super-fast defaults for mobile games where the password and salt are ultimately available to dedicated players.
    /// </summary>
    MobileGame,
}
