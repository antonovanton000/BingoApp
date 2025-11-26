using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameItemTracker.Classes;

public class MemoryReader
{
    [DllImport("kernel32.dll")]
    static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
    [DllImport("kernel32.dll")]
    static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr hObject);

    const int PROCESS_VM_READ = 0x0010;
    const int PROCESS_QUERY_INFORMATION = 0x0400;

    private IntPtr processHandle;
    private Process sekiroProcess;

    public bool Attach(string processName)
    {
        var procs = Process.GetProcessesByName(processName);
        if (procs.Length == 0) return false;
        sekiroProcess = procs[0];
        processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, sekiroProcess.Id);
        return processHandle != IntPtr.Zero;
    }

    public void Detach()
    {
        if (processHandle != IntPtr.Zero)
            CloseHandle(processHandle);
    }

    public IntPtr BaseAddress => sekiroProcess?.MainModule?.BaseAddress ?? IntPtr.Zero;
    public int ModuleSize => sekiroProcess?.MainModule?.ModuleMemorySize ?? 0;

    public byte[] ReadBytes(IntPtr address, int count)
    {
        byte[] buffer = new byte[count];
        ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);
        return buffer;
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



    public IntPtr FindPattern(byte[] pattern, string mask)
    {
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


    public void ListModules()
    {
        Console.WriteLine("=== Loaded Modules ===");
        foreach (ProcessModule m in sekiroProcess.Modules)
        {
            System.Diagnostics.Debug.WriteLine($"{m.ModuleName} @ 0x{m.BaseAddress.ToInt64():X}  Size: 0x{m.ModuleMemorySize:X}");
        }
    }
}
