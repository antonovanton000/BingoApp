using GameItemTracker.Classes;
using System;
using System.Collections.Generic;
using System.Threading;

public class SekiroIGTScanner
{
    private readonly MemoryReader mem;

    public SekiroIGTScanner(MemoryReader mem)
    {
        this.mem = mem;
    }
    
    public IntPtr? FindSekiroIGT()
    {
        byte[] pattern = {
        0x48,0x8B,0x05, 0x00,0x00,0x00,0x00,
        0x32,0xD2,0x48,0x8B,0x48,0x08,
        0x48,0x85,0xC9,0x74,0x13,0x80,0xB9,0xBA
    };
        string mask = "xxx????xxxxxxxxxxxxxx"; // длина 21 — совпадает с pattern

        if (mask.Length != pattern.Length)
            throw new InvalidOperationException($"Mask length {mask.Length} != pattern length {pattern.Length}");

        IntPtr baseAddr = mem.BaseAddress;
        int size = mem.ModuleSize;

        const int CHUNK = 1 << 20; // 1MB
                                   // перекрываем границы, чтобы не потерять матч на стыке блоков
        for (int offset = 0; offset < size; offset += (CHUNK - pattern.Length))
        {
            int len = Math.Min(CHUNK, size - offset);
            byte[] buffer;
            try { buffer = mem.ReadBytes(baseAddr + offset, len); }
            catch { continue; }

            for (int i = 0; i <= buffer.Length - pattern.Length; i++)
            {
                bool ok = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    char m = mask[j];
                    if (m != '?' && buffer[i + j] != pattern[j]) { ok = false; break; }
                }
                if (!ok) continue;

                // нашли сигнатуру: читаем rel32 RIP-relative
                IntPtr matchAddr = baseAddr + offset + i;

                byte[] relBytes;
                try { relBytes = mem.ReadBytes(matchAddr + 3, 4); }
                catch { continue; }
                if (relBytes.Length < 4) continue;

                int rel = BitConverter.ToInt32(relBytes, 0);
                IntPtr ripTarget = matchAddr + 7 + rel; // RIP после инструкции + rel32

                // SoulSplitter: .AddPointer(_igt, 0x0, 0x9C)
                // Т.е. сначала читаем сам указатель по ripTarget (+0), затем поле IGT по +0x9C
                IntPtr structPtr;
                try { structPtr = mem.ReadPointer(ripTarget + 0x0); }
                catch { continue; }

                if (structPtr == IntPtr.Zero) continue;
                
                IntPtr igtPtr = structPtr + 0x9C; // float секунд
                return igtPtr;
            }
        }
        return null;
    }


}
