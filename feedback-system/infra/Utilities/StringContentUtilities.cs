using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PulumiInfra.Utilities;

public record CompressedStringContent(string Content, byte[] BrotliContent, byte[] GZipContent);
public record CompressedStringContentBase64(string Content, string ContentBase64, string BrotliBase64, string GZipBase64);

public static class StringContentUtilities
{
    public static CompressedStringContent GenerateCompressedStringContent(JsonNode node)
    {
        return GenerateCompressedStringContent(node.ToJsonString());
    }

    public static CompressedStringContent GenerateCompressedStringContent(string content)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        byte[] brotliContent = OutputBrotliCompressed(bytes);
        byte[] gZipContent = OutputGZipCompressed(bytes);
        return new CompressedStringContent(content, brotliContent, gZipContent);
    }

    public static CompressedStringContentBase64 GenerateCompressedStringContentBase64(JsonNode node)
    {
        return GenerateCompressedStringContentBase64(node.ToJsonString());
    }

    public static CompressedStringContentBase64 GenerateCompressedStringContentBase64(string content)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        string contentBase = OutputStringBase64(bytes);
        string brotliBase = OutputBrotliCompressedBase64(bytes);
        string gZipBase = OutputGZipCompressedBase64(bytes);
        return new CompressedStringContentBase64(content, contentBase, brotliBase, gZipBase);
    }

    public static string OutputStringBase64(byte[] contentBytes)
    {
        return Convert.ToBase64String(contentBytes);
    }

    public static byte[] OutputBrotliCompressed(byte[] contentBytes)
    {
        using MemoryStream memoryStream = new MemoryStream();
        using BrotliStream brotliStream = new BrotliStream(memoryStream, CompressionLevel.Optimal);
        brotliStream.Write(contentBytes, 0, contentBytes.Length);
        brotliStream.Flush();
        return memoryStream.ToArray();
    }

    public static string OutputBrotliCompressedBase64(byte[] contentBytes)
    {
        using MemoryStream memoryStream = new MemoryStream();
        using BrotliStream brotliStream = new BrotliStream(memoryStream, CompressionLevel.Optimal);
        brotliStream.Write(contentBytes, 0, contentBytes.Length);
        brotliStream.Flush();
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public static byte[] OutputGZipCompressed(byte[] contentBytes)
    {
        using MemoryStream memoryStream = new MemoryStream();
        using GZipStream gZipStream = new GZipStream(memoryStream, CompressionLevel.Optimal);
        gZipStream.Write(contentBytes, 0, contentBytes.Length);
        gZipStream.Flush();
        return memoryStream.ToArray();
    }

    public static string OutputGZipCompressedBase64(byte[] contentBytes)
    {
        using MemoryStream memoryStream = new MemoryStream();
        using GZipStream gZipStream = new GZipStream(memoryStream, CompressionLevel.Optimal);
        gZipStream.Write(contentBytes, 0, contentBytes.Length);
        gZipStream.Flush();
        return Convert.ToBase64String(memoryStream.ToArray());
    }
}
