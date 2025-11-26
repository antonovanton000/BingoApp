using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HuntpointApp.Classes;

public class MemoryReader
{
    [DllImport("kernel32.dll")]
    static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
    

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(
    IntPtr hProcess,
    IntPtr lpBaseAddress,
    byte[] lpBuffer,
    int nSize,
    out IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr hObject);

    public const uint PROCESS_VM_READ = 0x0010;
    public const uint PROCESS_VM_WRITE = 0x0020;
    public const uint PROCESS_VM_OPERATION = 0x0008;
    public const uint PROCESS_QUERY_INFORMATION = 0x0400;
    public const uint PROCESS_CREATE_THREAD = 0x0002;
    public const uint PROCESS_ALL_ACCESS = 0x001F0FFF;

    private IntPtr processHandle;
    private Process sekiroProcess;

    public bool Attach(Process process)
    {        
        sekiroProcess = process;
        processHandle = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, sekiroProcess.Id);
        return processHandle != IntPtr.Zero;
    }

    public void Detach()
    {
        try
        {
            if (processHandle != IntPtr.Zero)
                CloseHandle(processHandle);
        }
        catch (Exception) { }
    }

    public IntPtr BaseAddress => sekiroProcess?.MainModule?.BaseAddress ?? IntPtr.Zero;
    public int ModuleSize => sekiroProcess?.MainModule?.ModuleMemorySize ?? 0;

    public byte[] ReadBytes(IntPtr address, int count)
    {
        byte[] buffer = new byte[count];
        ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);
        return buffer;
    }

    public bool WriteBytes(IntPtr address, byte[] data)
    {
        IntPtr bytesWritten;
        return WriteProcessMemory(processHandle, address, data, data.Length, out bytesWritten)
               && bytesWritten.ToInt64() == data.Length;
    }

    public IntPtr ReadPointer(IntPtr address)
    {
        byte[] buffer = new byte[IntPtr.Size];
        ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);
        return IntPtr.Size == 8
            ? (IntPtr)BitConverter.ToInt64(buffer, 0)
            : (IntPtr)BitConverter.ToInt32(buffer, 0);
    }

    public float ReadFloat(IntPtr address)
    {
        byte[] buffer = new byte[4];
        ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);
        return BitConverter.ToSingle(buffer, 0);
    }

    public int ReadInt32(IntPtr address, int defaultValue = 0)
    {
        try
        {
            byte[] buffer = ReadBytes(address, 4);
            return BitConverter.ToInt32(buffer, 0);
        }
        catch
        {
            return defaultValue;
        }
    }




    public IntPtr FindPattern(byte[] pattern, string mask)
    {
        if (mask.Length != pattern.Length)
            throw new InvalidOperationException($"Mask length {mask.Length} != pattern length {pattern.Length}");

        IntPtr baseAddr = BaseAddress;
        int moduleSize = ModuleSize;

        const int CHUNK_SIZE = 1 << 20; // 1 MB
        byte[] buffer = new byte[CHUNK_SIZE];

        for (int offset = 0; offset < moduleSize; offset += CHUNK_SIZE)
        {
            int bytesToRead = Math.Min(CHUNK_SIZE, moduleSize - offset);
            buffer = ReadBytes(baseAddr + offset, bytesToRead);

            // Перебор возможных позиций
            for (int i = 0; i <= bytesToRead - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (mask[j] != '?' && buffer[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return baseAddr + offset + i;
            }
        }

        return IntPtr.Zero;
    }

    public uint ReadUInt32(IntPtr address)
    {
        var bytes = ReadBytes(address, 4);
        return BitConverter.ToUInt32(bytes, 0);
    }

    public byte ReadByte(IntPtr address)
    {
        byte[] buffer = new byte[1];
        if (ReadProcessMemory(processHandle, address, buffer, 1, out int bytesRead) && bytesRead == 1)
            return buffer[0];
        return 0;
    }


    public bool WriteUInt32(IntPtr address, uint value)
    {
        var data = BitConverter.GetBytes(value);
        return WriteBytes(address, data);
    }


    public void ListModules()
    {
        Console.WriteLine("=== Loaded Modules ===");
        foreach (ProcessModule m in sekiroProcess.Modules)
        {
            System.Diagnostics.Debug.WriteLine($"{m.ModuleName} @ 0x{m.BaseAddress.ToInt64():X}  Size: 0x{m.ModuleMemorySize:X}");
        }
    }
}
