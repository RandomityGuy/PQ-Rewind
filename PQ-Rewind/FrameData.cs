using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PQ_Rewind
{
    public class FrameData
    {
        public int ms;
        public int deltaMs;
        public (double x, double y, double z) position;
        public (double x, double y, double z) velocity;
        public (double x, double y, double z) spin;
        public string powerup;
        public float timebonus;
        public string mpstates;
        public int gemcount;
        public string gemstates;
        public string ttstates;

        public FrameData(string raw)
        {
            try
            {
                ms = GetWord<int>(raw, 0);
                deltaMs = GetWord<int>(raw, 1);
                position = (GetWord<double>(raw, 2), GetWord<double>(raw, 3), GetWord<double>(raw, 4));
                velocity = (GetWord<double>(raw, 5), GetWord<double>(raw, 6), GetWord<double>(raw, 7));
                spin = (GetWord<double>(raw, 8), GetWord<double>(raw, 9), GetWord<double>(raw, 10));
                powerup = GetWord<string>(raw, 11);
                timebonus = GetWord <float>(raw, 12);
                mpstates = GetWord<string>(raw, 13);
                gemcount = GetWord<int>(raw, 14);
                gemstates = GetWord<string>(raw, 15);
                ttstates = GetWord<string>(raw, 16);
                //itemstates = GetWord<string>(raw, 12);
            }
            catch (Exception e)
            {
                 throw;
            }
        }

        T GetWord<T>(string str,int pos)
        {
            return (T)Convert.ChangeType(str.Split(' ')[pos], typeof(T));
        }
    }
}
