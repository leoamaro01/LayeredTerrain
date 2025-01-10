namespace LayeredTerrain.Utilities
{
    public static class Utils
    {
        public static int GetCombinedSeed(int x, int y, int z)
        {
            static int Rotl32(int n, int k)
            {
                int i = n << k;
                int j = n >> (32 - k);
                return i | j;
            }

            int a = x,
                b = y,
                c = z;

            b ^= Rotl32(a + c, 7);
            c ^= Rotl32(b + a, 9);
            a ^= Rotl32(c + b, 18);
            b ^= Rotl32(a + c, 7);
            c ^= Rotl32(b + a, 9);
            a ^= Rotl32(c + b, 18);
            b ^= Rotl32(a + c, 7);
            c ^= Rotl32(b + a, 9);
            a ^= Rotl32(c + b, 18);

            return a + b + c + x + y + z;
        }
    }
}
