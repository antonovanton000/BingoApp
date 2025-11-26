using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Classes;
using System;
using System.Diagnostics;
using System.Text;

public class SekiroEventFlags
{
    private readonly MemoryReader mem;
    private IntPtr eventFlagManPtr;
    private IntPtr fieldAreaPtr;

    public SekiroEventFlags(MemoryReader mem)
    {
        this.mem = mem;
    }

    // Быстро: берём EventFlagMan по известному указателю (если он в этой версии игры валиден)
    public bool InitByAbsolutePointer()
    {
        // твой “жёсткий” адрес указателя на менеджер
        IntPtr mgrPtrAddr = (IntPtr)0x143D55FE8;
        try
        {
            eventFlagManPtr = mem.ReadPointer(mgrPtrAddr);
            return eventFlagManPtr != IntPtr.Zero;
        }
        catch { return false; }
    }

    public bool InitBySignature()
    {
        byte[] pattern = {
        0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00,
        0x48, 0x89, 0x5C, 0x24, 0x50,
        0x48, 0x89, 0x6C, 0x24, 0x58,
        0x48, 0x89, 0x74, 0x24, 0x60
    };
        string mask = "xxx????xxxxxxxxxxxxxxx";

        IntPtr baseAddr = mem.BaseAddress;
        int size = mem.ModuleSize;
        const int CHUNK = 1 << 20;

        for (int offset = 0; offset < size; offset += CHUNK - pattern.Length)
        {
            int len = Math.Min(CHUNK, size - offset);
            byte[] buffer;
            try { buffer = mem.ReadBytes(baseAddr + offset, len); }
            catch { continue; }

            for (int i = 0; i <= buffer.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (mask[j] != '?' && buffer[i + j] != pattern[j]) { match = false; break; }
                }
                if (!match) continue;

                IntPtr matchAddr = baseAddr + offset + i;
                int rel = BitConverter.ToInt32(mem.ReadBytes(matchAddr + 3, 4), 0);
                IntPtr ripTarget = matchAddr + 7 + rel;
                IntPtr flagMan = mem.ReadPointer(ripTarget);
                if (flagMan != IntPtr.Zero)
                {
                    eventFlagManPtr = flagMan; // ✅ без доп. разыменования
                    return true;
                }
            }
        }

        App.Logger.Error("❌ EventFlagMan not found.");
        return false;
    }


    public bool IsReady => eventFlagManPtr != IntPtr.Zero;

    // Базовый адрес “группы” (0..9) = *(eventFlagMan + 0x218 + group*0x18)
    private IntPtr GetGroupBase(uint id)
    {
        int group = (int)((id / 10000000) % 10);
        var arrBase = mem.ReadPointer(eventFlagManPtr + 0x218);
        if (arrBase == IntPtr.Zero) return IntPtr.Zero;
        return mem.ReadPointer((IntPtr)(arrBase.ToInt64() + group * 0x18));
    }

    // “Плоский” адрес блока без категории (категория добавляется выше)
    private IntPtr GetFlatBlock(uint id)
    {
        // последний “разряд тысяч”: (id/1000)%10
        int idDiv1000 = (int)((id / 1000) % 10);
        var groupBase = GetGroupBase(id);
        if (groupBase == IntPtr.Zero) return IntPtr.Zero;
        return groupBase + (idDiv1000 << 4); // базовый сдвиг без категории
    }

    private bool ReadFlagFromBlock(IntPtr addr, uint id)
    {
        if (addr == IntPtr.Zero)
            return false;

        IntPtr data = mem.ReadPointer(addr);
        if (data == IntPtr.Zero)
            return false;

        long valueOffset = ((id % 10000u) >> 5) * 4;
        IntPtr valueAddr = (IntPtr)(data.ToInt64() + valueOffset);
        uint value = mem.ReadUInt32(valueAddr);

        // ← правильный порядок битов для Sekiro
        uint mask = 1u << (31 - ((byte)(id % 32)));
        bool result = (value & mask) != 0;

        //App.Logger.Info($"[Flag {id}] data=0x{data.ToInt64():X} off={valueOffset} val=0x{value:X8} mask=0x{mask:X8} -> {(result ? "ON" : "OFF")}");
        return result;
    }

    private bool WriteFlagInBlock(IntPtr addr, uint id, bool state)
    {
        if (addr == IntPtr.Zero) return false;
        IntPtr data = mem.ReadPointer(addr);
        if (data == IntPtr.Zero) return false;

        long valueOffset = ((id % 10000u) >> 5) * 4; // ✅ так же, как при чтении
        IntPtr valueAddr = (IntPtr)(data.ToInt64() + valueOffset);

        uint value = mem.ReadUInt32(valueAddr);
        uint mask = 1u << (31 - ((byte)(id % 32))); // ✅ тот же порядок битов
        uint newValue = state ? (value | mask) : (value & ~mask);

        return mem.WriteUInt32(valueAddr, newValue);
    }

    // “Умное” чтение: перебираем категории 0..7 (при необходимости расширь до 0..15)
    public bool ReadFlagSmart(uint id, int maxCategories = 8)
    {
        var flat = GetFlatBlock(id);
        if (flat == IntPtr.Zero) return false;

        for (int cat = 0; cat < maxCategories; cat++)
        {
            IntPtr addr = flat + cat * 0xA8;
            if (ReadFlagFromBlock(addr, id))
                return true;
        }
        return false;
    }

    public bool WriteFlagSmart(uint id, bool state, int maxCategories = 8)
    {
        var flat = GetFlatBlock(id);
        if (flat == IntPtr.Zero) return false;

        for (int cat = 0; cat < maxCategories; cat++)
        {
            IntPtr addr = flat + cat * 0xA8;
            // Пишем в первую категорию, где этот бит “существует” (или в 0, если нужно просто форснуть)
            if (WriteFlagInBlock(addr, id, state))
                return true;
        }
        return false;
    }

    // Прямое чтение, если категорию уже знаем (например 0 для глобальных)
    public bool ReadFlagWithCategory(uint id, int category)
    {
        var flat = GetFlatBlock(id);
        return ReadFlagFromBlock(flat + category * 0xA8, id);
    }

    public bool WriteFlag(uint id, bool state, int category)
    {
        var flat = GetFlatBlock(id);
        return WriteFlagInBlock(flat + category * 0xA8, id, state);
    }

    public bool ReadFlag(uint id)
    {
        // 1) “официальный” путь
        IntPtr addr = GetEventFlagAddress(id);
        if (addr != IntPtr.Zero && ReadFlagFromBlock(addr, id))
            return true;

        // 2) Fallback: перебор категорий 0..15 на корректном flat-адресе
        IntPtr flat = GetFlatAddressNoCategory(id);
        if (flat == IntPtr.Zero) return false;

        for (int cat = 0; cat < 16; cat++)
        {
            IntPtr addrCat = (IntPtr)(flat.ToInt64() + cat * 0xA8);
            if (ReadFlagFromBlock(addrCat, id))
                return true;
        }
        return false;
    }



    private IntPtr GetEventFlagAddress(uint id, byte param3 = 0)
    {
        if (eventFlagManPtr == IntPtr.Zero)
            return IntPtr.Zero;

        // Проверка isActive
        if (mem.ReadByte((IntPtr)(eventFlagManPtr.ToInt64() + 0x228)) == 0)
            return IntPtr.Zero;

        // Группа (0..9)
        int group = (int)((id / 10000000) % 10);
        // 1000s
        int idDiv1000 = (int)((id / 1000) % 10);
        // Area
        int area = (int)((id / 100000) % 100);
        // 10000s
        int idDiv10000 = (int)((id / 10000) % 10);

        // Читаем категорию (iVar2)
        int category = GetCategoryIndex(eventFlagManPtr, area, idDiv10000, param3);
        if (category < 0)
            return IntPtr.Zero;

        // lVar1 = *( *(param1 + 0x218) + group*0x18 )
        IntPtr basePtr1 = mem.ReadPointer((IntPtr)(eventFlagManPtr.ToInt64() + 0x218));
        IntPtr lVar1 = mem.ReadPointer((IntPtr)(basePtr1.ToInt64() + group * 0x18));
        if (lVar1 == IntPtr.Zero)
            return IntPtr.Zero;

        // Возврат вычисленного адреса
        long addr;
        if (param3 == 0)
            addr = lVar1.ToInt64() + category * 0xA8 + idDiv1000 * 0x10;
        else
            addr = lVar1.ToInt64() + idDiv1000 * 0x10 + category * 0xA8;

        return (IntPtr)addr;
    }

    private int GetCategoryIndex(IntPtr eventFlagMan, int area, int idDiv10000, byte param3)
    {
        // TODO: реализовать позже по аналогии с SoulSplitter.GetEventFlagAddress
        // Пока возвращаем 0 — этого достаточно для глобальных и некоторых area-флагов
        return 0;
    }

    private IntPtr GetFlatAddressNoCategory(uint id)
    {
        int group = (int)((id / 10000000) % 10);
        int idDiv1000 = (int)((id / 1000) % 10);

        var arrBase = mem.ReadPointer(eventFlagManPtr + 0x218);
        if (arrBase == IntPtr.Zero) return IntPtr.Zero;

        var lVar1 = mem.ReadPointer((IntPtr)(arrBase.ToInt64() + group * 0x18));
        if (lVar1 == IntPtr.Zero) return IntPtr.Zero;

        return (IntPtr)(lVar1.ToInt64() + (idDiv1000 << 4)); // без категории
    }


}
