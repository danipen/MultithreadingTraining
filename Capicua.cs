namespace MultithreadingTraining
{
    public static class Capicua
    {
        public static long Next(long n)
        {
            while (true)
            {
                n++;
                if (IsCapicua(n))
                {
                    return n;
                }
            }
        }

        public static bool IsCapicua(long n)
        {
            string s = n.ToString();
            int i = 0;
            int j = s.Length - 1;
            while (i < j)
            {
                if (s[i] != s[j])
                {
                    return false;
                }
                i++;
                j--;
            }
            return true;
        }
    }
}