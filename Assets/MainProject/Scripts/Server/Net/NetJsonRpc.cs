using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class NetJsonRpc
{
    public static async Task WriteAsync(NetworkStream stream, string json, CancellationToken ct)
    {
        byte[] payload = Encoding.UTF8.GetBytes(json);
        byte[] header = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(header, payload.Length);
        await stream.WriteAsync(header, 0, header.Length, ct);
        await stream.WriteAsync(payload, 0, payload.Length, ct);
        await stream.FlushAsync(ct);
    }

    public static async Task<string> ReadAsync(NetworkStream stream, CancellationToken ct, int maxBytes = 1024 * 1024)
    {
        byte[] header = new byte[4];
        if (!await ReadExactAsync(stream, header, ct)) return null;

        int length = BinaryPrimitives.ReadInt32LittleEndian(header);
        if (length <= 0 || length > maxBytes) return null;

        byte[] payload = new byte[length];
        if (!await ReadExactAsync(stream, payload, ct)) return null;

        return Encoding.UTF8.GetString(payload);
    }

    static async Task<bool> ReadExactAsync(NetworkStream stream, byte[] buffer, CancellationToken ct)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer, offset, buffer.Length - offset, ct);
            if (read == 0) return false;
            offset += read;
        }
        return true;
    }

    [Serializable] private class TypeOnly { public string type; }
    public static string PeekType(string json)
    {
        try { return JsonUtility.FromJson<TypeOnly>(json).type; }
        catch { return null; }
    }
}