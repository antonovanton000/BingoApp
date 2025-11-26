
namespace NightreignRandomizer;
public abstract class RandomBase
{
    public abstract uint NextUInt32();

    public virtual int NextInt32() => (int)this.NextUInt32();

    public virtual ulong NextUInt64()
    {
        return (ulong)this.NextUInt32() << 32 /*0x20*/ | (ulong)this.NextUInt32();
    }

    public virtual long NextInt64()
    {
        return (long)this.NextUInt32() << 32 /*0x20*/ | (long)this.NextUInt32();
    }

    public virtual void NextBytes(byte[] buffer)
    {
        int num1 = 0;
        while (num1 + 4 <= buffer.Length)
        {
            uint num2 = this.NextUInt32();
            byte[] numArray1 = buffer;
            int index1 = num1;
            int num3 = index1 + 1;
            int num4 = (int)(byte)num2;
            numArray1[index1] = (byte)num4;
            byte[] numArray2 = buffer;
            int index2 = num3;
            int num5 = index2 + 1;
            int num6 = (int)(byte)(num2 >> 8);
            numArray2[index2] = (byte)num6;
            byte[] numArray3 = buffer;
            int index3 = num5;
            int num7 = index3 + 1;
            int num8 = (int)(byte)(num2 >> 16 /*0x10*/);
            numArray3[index3] = (byte)num8;
            byte[] numArray4 = buffer;
            int index4 = num7;
            num1 = index4 + 1;
            int num9 = (int)(byte)(num2 >> 24);
            numArray4[index4] = (byte)num9;
        }
        if (num1 >= buffer.Length)
            return;
        uint num10 = this.NextUInt32();
        byte[] numArray5 = buffer;
        int index5 = num1;
        int num11 = index5 + 1;
        int num12 = (int)(byte)num10;
        numArray5[index5] = (byte)num12;
        if (num11 >= buffer.Length)
            return;
        byte[] numArray6 = buffer;
        int index6 = num11;
        int num13 = index6 + 1;
        int num14 = (int)(byte)(num10 >> 8);
        numArray6[index6] = (byte)num14;
        if (num13 >= buffer.Length)
            return;
        byte[] numArray7 = buffer;
        int index7 = num13;
        int num15 = index7 + 1;
        int num16 = (int)(byte)(num10 >> 16 /*0x10*/);
        numArray7[index7] = (byte)num16;
    }

    public virtual double NextDouble()
    {
        return ((double)this.NextUInt32() * 4096.0 + (double)this.NextUInt32()) / 4194304.0;
    }
}
