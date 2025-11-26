//// See https://aka.ms/new-console-template for more information
//using GameItemTracker.Classes;
//using SekiroIGTReader;
//using System;

//var mem = new MemoryReader();
//if (mem.Attach("sekiro"))
//{
//    var scaner = new SekiroIGTScanner(mem);
//    IntPtr? igtAddr = scaner.FindSekiroIGT();
//    if (igtAddr is not null)
//    {
//        float value = mem.Read(igtAddr.Value);
//        Console.WriteLine($"IGT: {value:F3} sec");


//        for (int off = 0; off <= 0x120; off += 4)
//        {
//            float v = mem.ReadFloat((IntPtr)igtAddr + off);
//            Console.WriteLine($"+0x{off:X} = {v}");
//        }
//    }
//    else
//    {
//        Console.WriteLine("IGT not found.");
//    }
//}