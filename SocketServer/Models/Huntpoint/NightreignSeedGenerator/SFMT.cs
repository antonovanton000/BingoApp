
namespace NightreignRandomizer;
public class SFMT : RandomBase
{
    protected int MEXP;
    protected int POS1;
    protected int SL1;
    protected int SL2;
    protected int SR1;
    protected int SR2;
    protected uint MSK1;
    protected uint MSK2;
    protected uint MSK3;
    protected uint MSK4;
    protected uint PARITY1;
    protected uint PARITY2;
    protected uint PARITY3;
    protected uint PARITY4;
    protected int N;
    protected int N32;
    protected int SL2_x8;
    protected int SR2_x8;
    protected int SL2_ix8;
    protected int SR2_ix8;
    protected uint[] sfmt;
    protected int idx;

    public SFMT()
      : this(Environment.TickCount, 19937)
    {
    }

    public SFMT(int seed)
      : this(seed, 19937)
    {
    }

    public SFMT(int seed, MTPeriodType period)
      : this(seed, (int)period)
    {
    }

    public SFMT(int seed, int mexp)
    {
        this.MEXP = mexp;
        switch (mexp)
        {
            case 607:
                this.POS1 = 2;
                this.SL1 = 15;
                this.SL2 = 3;
                this.SR1 = 13;
                this.SR2 = 3;
                this.MSK1 = 4261361663U;
                this.MSK2 = 4018093949U;
                this.MSK3 = 4286020477U;
                this.MSK4 = 2146958127U;
                this.PARITY1 = 1U;
                this.PARITY2 = 0U;
                this.PARITY3 = 0U;
                this.PARITY4 = 1502015572U;
                break;
            case 1279 /*0x04FF*/:
                this.POS1 = 7;
                this.SL1 = 14;
                this.SL2 = 3;
                this.SR1 = 5;
                this.SR2 = 1;
                this.MSK1 = 4160684029U;
                this.MSK2 = 2146422783U;
                this.MSK3 = 2951999295U;
                this.MSK4 = 3053453183U;
                this.PARITY1 = 1U;
                this.PARITY2 = 0U;
                this.PARITY3 = 0U;
                this.PARITY4 = 536870912U /*0x20000000*/;
                break;
            case 2281:
                this.POS1 = 12;
                this.SL1 = 19;
                this.SL2 = 1;
                this.SR1 = 5;
                this.SR2 = 1;
                this.MSK1 = 3220701119U;
                this.MSK2 = 4261412862U;
                this.MSK3 = 4160745343U;
                this.MSK4 = 4076325823U;
                this.PARITY1 = 1U;
                this.PARITY2 = 0U;
                this.PARITY3 = 0U;
                this.PARITY4 = 1105176064U;
                break;
            case 4253:
                this.POS1 = 17;
                this.SL1 = 20;
                this.SL2 = 1;
                this.SR1 = 7;
                this.SR2 = 1;
                this.MSK1 = 2675703807U;
                this.MSK2 = 2684354399U;
                this.MSK3 = 1056964603U;
                this.MSK4 = 4294965179U;
                this.PARITY1 = 2818572289U /*0xA8000001*/;
                this.PARITY2 = 2941489315U;
                this.PARITY3 = 3074470904U;
                this.PARITY4 = 1813071981U;
                break;
            case 11213:
                this.POS1 = 68;
                this.SL1 = 14;
                this.SL2 = 3;
                this.SR1 = 7;
                this.SR2 = 3;
                this.MSK1 = 4026529787U;
                this.MSK2 = 4294967279U;
                this.MSK3 = 3755982847U;
                this.MSK4 = 2147474429U;
                this.PARITY1 = 1U;
                this.PARITY2 = 0U;
                this.PARITY3 = 3893657600U;
                this.PARITY4 = 3502747555U;
                break;
            case 19937:
                this.POS1 = 122;
                this.SL1 = 18;
                this.SL2 = 1;
                this.SR1 = 11;
                this.SR2 = 1;
                this.MSK1 = 3758096367U;
                this.MSK2 = 3724462975U;
                this.MSK3 = 3220897791U;
                this.MSK4 = 3221225462U;
                this.PARITY1 = 1U;
                this.PARITY2 = 0U;
                this.PARITY3 = 0U;
                this.PARITY4 = 331998852U;
                break;
            case 44497:
                this.POS1 = 330;
                this.SL1 = 5;
                this.SL2 = 3;
                this.SR1 = 9;
                this.SR2 = 3;
                this.MSK1 = 4026531835U;
                this.MSK2 = 3753820159U;
                this.MSK3 = 3216997359U;
                this.MSK4 = 2684189695U;
                this.PARITY1 = 1U;
                this.PARITY2 = 0U;
                this.PARITY3 = 2745974784U;
                this.PARITY4 = 3972084346U;
                break;
            case 86243:
                this.POS1 = 366;
                this.SL1 = 6;
                this.SL2 = 7;
                this.SR1 = 19;
                this.SR2 = 1;
                this.MSK1 = 4257217535U;
                this.MSK2 = 3220700991U;
                this.MSK3 = 4252495871U;
                this.MSK4 = 3214930943U;
                this.PARITY1 = 1U;
                this.PARITY2 = 0U;
                this.PARITY3 = 0U;
                this.PARITY4 = 3914501509U;
                break;
            case 132049:
                this.POS1 = 110;
                this.SL1 = 19;
                this.SL2 = 1;
                this.SR1 = 21;
                this.SR2 = 1;
                this.MSK1 = 4294949727U;
                this.MSK2 = 4218339221U;
                this.MSK3 = 4294901754U;
                this.MSK4 = 3489103871U;
                this.PARITY1 = 1U;
                this.PARITY2 = 0U;
                this.PARITY3 = 3411148800U;
                this.PARITY4 = 3353943165U;
                break;
            case 216091:
                this.POS1 = 627;
                this.SL1 = 11;
                this.SL2 = 3;
                this.SR1 = 10;
                this.SR2 = 1;
                this.MSK1 = 3220684791U /*0xBFF7BFF7*/;
                this.MSK2 = 3221225471U /*0xBFFFFFFF*/;
                this.MSK3 = 3221224063U;
                this.MSK4 = 4292738043U;
                this.PARITY1 = 4160749569U /*0xF8000001*/;
                this.PARITY2 = 2313684745U;
                this.PARITY3 = 1003664971U;
                this.PARITY4 = 207925732U;
                break;
            default:
                throw new ArgumentException();
        }
        this.init_gen_rand(seed);
    }

    public override uint NextUInt32()
    {
        if (this.idx >= this.N32)
        {
            this.gen_rand_all();
            this.idx = 0;
        }
        return this.sfmt[this.idx++];
    }

    protected void init_gen_rand(int seed)
    {
        this.N = this.MEXP / 128 /*0x80*/ + 1;
        this.N32 = this.N * 4;
        this.SL2_x8 = this.SL2 * 8;
        this.SR2_x8 = this.SR2 * 8;
        this.SL2_ix8 = 64 /*0x40*/ - this.SL2 * 8;
        this.SR2_ix8 = 64 /*0x40*/ - this.SR2 * 8;
        this.sfmt = new uint[this.N32];
        this.sfmt[0] = (uint)seed;
        for (int index = 1; index < this.N32; ++index)
            this.sfmt[index] = (uint)((ulong)(uint)(1812433253 * ((int)this.sfmt[index - 1] ^ (int)(this.sfmt[index - 1] >> 30))) + (ulong)index);
        this.period_certification();
        this.idx = this.N32;
    }

    protected void period_certification()
    {
        uint[] numArray = new uint[4]
        {
      this.PARITY1,
      this.PARITY2,
      this.PARITY3,
      this.PARITY4
        };
        uint num1 = 0;
        for (int index = 0; index < 4; ++index)
            num1 ^= this.sfmt[index] & numArray[index];
        for (int index = 16 /*0x10*/; index > 0; index >>= 1)
            num1 ^= num1 >> index;
        if ((num1 & 1U) == 1U)
            return;
        for (int index1 = 0; index1 < 4; ++index1)
        {
            uint num2 = 1;
            for (int index2 = 0; index2 < 32 /*0x20*/; ++index2)
            {
                if (((int)num2 & (int)numArray[index1]) != 0)
                {
                    this.sfmt[index1] ^= num2;
                    return;
                }
                num2 <<= 1;
            }
        }
    }

    protected virtual void gen_rand_all()
    {
        if (this.MEXP == 19937)
        {
            this.gen_rand_all_19937();
        }
        else
        {
            int index1 = 0;
            int index2 = this.POS1 * 4;
            int index3 = (this.N - 2) * 4;
            int index4 = (this.N - 1) * 4;
            do
            {
                ulong num1 = (ulong)this.sfmt[index1 + 3] << 32 /*0x20*/ | (ulong)this.sfmt[index1 + 2];
                ulong num2 = (ulong)this.sfmt[index1 + 1] << 32 /*0x20*/ | (ulong)this.sfmt[index1];
                ulong num3 = num1 << this.SL2_x8 | num2 >> this.SL2_ix8;
                ulong num4 = num2 << this.SL2_x8;
                ulong num5 = (ulong)this.sfmt[index3 + 3] << 32 /*0x20*/ | (ulong)this.sfmt[index3 + 2];
                ulong num6 = (ulong)this.sfmt[index3 + 1] << 32 /*0x20*/ | (ulong)this.sfmt[index3];
                ulong num7 = num3 ^ num5 >> this.SR2_x8;
                ulong num8 = num4 ^ (num6 >> this.SR2_x8 | num5 << this.SR2_ix8);
                this.sfmt[index1 + 3] = (uint)((int)this.sfmt[index1 + 3] ^ (int)(this.sfmt[index2 + 3] >> this.SR1) & (int)this.MSK4 ^ (int)this.sfmt[index4 + 3] << this.SL1) ^ (uint)(num7 >> 32 /*0x20*/);
                this.sfmt[index1 + 2] = (uint)((int)this.sfmt[index1 + 2] ^ (int)(this.sfmt[index2 + 2] >> this.SR1) & (int)this.MSK3 ^ (int)this.sfmt[index4 + 2] << this.SL1) ^ (uint)num7;
                this.sfmt[index1 + 1] = (uint)((int)this.sfmt[index1 + 1] ^ (int)(this.sfmt[index2 + 1] >> this.SR1) & (int)this.MSK2 ^ (int)this.sfmt[index4 + 1] << this.SL1) ^ (uint)(num8 >> 32 /*0x20*/);
                this.sfmt[index1] = (uint)((int)this.sfmt[index1] ^ (int)(this.sfmt[index2] >> this.SR1) & (int)this.MSK1 ^ (int)this.sfmt[index4] << this.SL1) ^ (uint)num8;
                index3 = index4;
                index4 = index1;
                index1 += 4;
                index2 += 4;
                if (index2 >= this.N32)
                    index2 = 0;
            }
            while (index1 < this.N32);
        }
    }

    private void gen_rand_all_19937()
    {
        uint[] sfmt = this.sfmt;
        int index1 = 0;
        int index2 = 488;
        int index3 = 616;
        int index4 = 620;
        do
        {
            sfmt[index1 + 3] = (uint)((int)sfmt[index1 + 3] ^ (int)sfmt[index1 + 3] << 8 ^ (int)(sfmt[index1 + 2] >> 24) ^ (int)(sfmt[index3 + 3] >> 8) ^ (int)(sfmt[index2 + 3] >> 11) & -1073741834 ^ (int)sfmt[index4 + 3] << 18);
            sfmt[index1 + 2] = (uint)((int)sfmt[index1 + 2] ^ (int)sfmt[index1 + 2] << 8 ^ (int)(sfmt[index1 + 1] >> 24) ^ (int)sfmt[index3 + 3] << 24 ^ (int)(sfmt[index3 + 2] >> 8) ^ (int)(sfmt[index2 + 2] >> 11) & -1074069505 ^ (int)sfmt[index4 + 2] << 18);
            sfmt[index1 + 1] = (uint)((int)sfmt[index1 + 1] ^ (int)sfmt[index1 + 1] << 8 ^ (int)(sfmt[index1] >> 24) ^ (int)sfmt[index3 + 2] << 24 ^ (int)(sfmt[index3 + 1] >> 8) ^ (int)(sfmt[index2 + 1] >> 11) & -570504321 ^ (int)sfmt[index4 + 1] << 18);
            sfmt[index1] = (uint)((int)sfmt[index1] ^ (int)sfmt[index1] << 8 ^ (int)sfmt[index3 + 1] << 24 ^ (int)(sfmt[index3] >> 8) ^ (int)(sfmt[index2] >> 11) & -536870929 ^ (int)sfmt[index4] << 18);
            index3 = index4;
            index4 = index1;
            index1 += 4;
            index2 += 4;
            if (index2 >= 624)
                index2 = 0;
        }
        while (index1 < 624);
    }
}
