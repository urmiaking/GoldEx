namespace GoldEx.Sdk.Server.Application.Extensions;

public static class StreamConverter
{
    public static byte[] ToByteArray(this Stream stream)
    {
        if (stream is MemoryStream memStream)
        {
            return memStream.ToArray();
        }

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public static byte[] oByteArrayWithBinaryReader(Stream stream)
    {
        using var binaryReader = new BinaryReader(stream);
        return binaryReader.ReadBytes((int)stream.Length);
    }
}