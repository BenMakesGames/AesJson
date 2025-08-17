# What Is It?

`AesJson` provides utility functions for serializing and deserializing JSON data to/from disk, using gzip and AES encryption. Both synchronous and asynchronous methods are available.

To make this library easy to use, it includes methods that use Argon2i encryption (via [Konscious.Security.Cryptography.Argon2](https://github.com/kmaragon/Konscious.Security.Cryptography)), but you can use any hashing algorithm you like by using MS's `Aes` class.

I personally use this library for the save/load system in my desktop games, to _discourage_ players from cheating. (Remember: for an application that runs fully on the player's machine, the encryption password and salt must be baked into your application, and so dedicated players CAN find & extract them. Encryption in that scenario is a deterrent, only.)

> [🧚 **Hey, listen!** You can support my development of open-source software on Patreon](https://www.patreon.com/BenMakesGames)

# Example Usage (using Argon2i)

```csharp
const string Password = "some super-secret password";
const string Salt = "some salt";

...

var data = new MyDataClass { Name = "Example", Value = 42 };

AesJsonSerializer.WriteFile(data, "path/to/file.save", Password, Salt);

var loadedData = AesJsonSerializer.ReadFile<MyDataClass>("path/to/file.save", Password, Salt);
```

# Example Usage (using any algorithm)

```csharp
var aes = Aes.Create();

// Set up the AES instance with your desired key size, mode, padding, etc.

var data = new MyDataClass { Name = "Example", Value = 42 };

AesJsonSerializer.WriteFile(data, "path/to/file.save", aes);

var loadedData = AesJsonSerializer.ReadFile<MyDataClass>("path/to/file.save", aes);
```
