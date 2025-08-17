using Shouldly;

namespace BenMakesGames.AesJson.Test;

public class AesJsonSerializerTests
{
    [Fact]
    public void ReadFileGetsSameObjectUsedToWriteFile()
    {
        const string password = "any password";
        const string salt = "any salt";

        var testObject = new TestClass
        {
            A = "Hello",
            B = -1 / 12f
        };

        File.Delete("test.file");

        AesJsonSerializer.WriteFile(testObject, "test.file", password, salt);

        var readObject = AesJsonSerializer.ReadFile<TestClass>("test.file", password, salt)
            ?? throw new Exception("Failed to read object from file");

        File.Delete("test.file");

        readObject.A.ShouldBe(testObject.A);
        readObject.B.ShouldBe(testObject.B);
    }
}

public sealed class TestClass
{
    public required string A { get; init; }
    public required float B { get; init; }
}
