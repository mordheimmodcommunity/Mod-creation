using System;

public class Tyche
{
    private const ulong PCG_DEFAULT_INCREMENT_64 = 1442695040888963407uL;

    private ulong m_state;

    private ulong m_inc;

    private bool log;

    public int Count
    {
        get;
        private set;
    }

    public Tyche(int seed, bool log = true)
    {
        this.log = log;
        Seed((ulong)seed);
    }

    public Tyche(ulong initState, ulong initSeq)
    {
        Seed(initState, initSeq);
    }

    private void Seed(ulong initState, ulong initSeq)
    {
        m_state = 0uL;
        m_inc = ((initSeq << 1) | 1);
        Random32();
        m_state += initState;
        Random32();
        Count = 0;
    }

    private void Seed(ulong initState)
    {
        m_state = 0uL;
        m_inc = 1442695040888963407uL;
        Random32();
        m_state += initState;
        Random32();
        Count = 0;
    }

    private uint Random32()
    {
        Count++;
        ulong state = m_state;
        m_state = state * 6364136223846793005L + m_inc;
        uint num = (uint)(((state >> 18) ^ state) >> 27);
        int num2 = (int)(state >> 59);
        return (num >> num2) | (num << (-num2 & 0x1F));
    }

    private double RandomDouble()
    {
        return (double)(Random32() >> 8) * 5.960465E-08;
    }

    private double BoundedFloat(double max, double value)
    {
        return (!(value < max)) ? ((double)BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(value), 0) - 1), 0)) : value;
    }

    public uint Range32(uint exclusiveBound)
    {
        uint num = (uint)((ulong)(4294967296L - exclusiveBound) % (ulong)exclusiveBound);
        uint num2;
        do
        {
            num2 = Random32();
        }
        while (num2 < num);
        return num2 % exclusiveBound;
    }

    public int Rand(int minimum, int exclusiveBound)
    {
        if (minimum == exclusiveBound)
        {
            return exclusiveBound;
        }
        uint exclusiveBound2 = (uint)(exclusiveBound - minimum);
        uint num = Range32(exclusiveBound2);
        return (int)num + minimum;
    }

    public double Rand(double minimum, double exclusiveBound)
    {
        return BoundedFloat(exclusiveBound, RandomDouble() * (exclusiveBound - minimum) + minimum);
    }

    public int RandInt()
    {
        return (int)Random32();
    }
}
