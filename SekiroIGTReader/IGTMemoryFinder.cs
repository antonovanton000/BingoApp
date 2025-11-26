using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameItemTracker.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SekiroIGTReader;


public class IGTMemoryFinder
{
    private readonly MemoryReader mem;
    private readonly List<IntPtr> candidates = new();

    public IGTMemoryFinder(MemoryReader mem) => this.mem = mem;

    public IntPtr FindIGT()
    {
        Console.WriteLine("Searching for increasing float values...");

        var baseAddr = mem.BaseAddress;
        int size = mem.ModuleSize;
        byte[] data1 = mem.ReadBytes(baseAddr, size);

        Thread.Sleep(3000); // подождём секунду
        byte[] data2 = mem.ReadBytes(baseAddr, size);

        for (int i = 0; i < size - 4; i += 4)
        {
            float v1 = BitConverter.ToSingle(data1, i);
            float v2 = BitConverter.ToSingle(data2, i);

            if (v1 > 0.5f && v2 > v1 && v2 - v1 < 2.0f)
                candidates.Add(baseAddr + i);
        }

        Console.WriteLine($"Found {candidates.Count} candidates.");
        for (int i = 0; i < 10 && i < candidates.Count; i++)
        {
            float val = mem.ReadFloat(candidates[i]);
            Console.WriteLine($"{i}: 0x{candidates[i].ToInt64():X} = {val}");
        }



        return candidates.Count > 0 ? candidates[0] : IntPtr.Zero;
    }
}

