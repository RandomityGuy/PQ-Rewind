using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpFramework;

namespace PQ_Rewind
{
    public class RewindService : TcpServiceClientBase
    {
        public static TcpService<RewindService> Instance;

        internal Stack<FrameData> frames = new Stack<FrameData>();

        int popcount;

        bool isReplay = false;

        protected override ValueTask OnClose()
        {
            return new ValueTask();
        }
        protected override void OnConnect()
        {
            Console.WriteLine("Connected " + this.ClientEndPoint.ToString());
            var args = CommandLineParser.cmdline;
            if (args != null)
            {
                if (args.Count() > 0)
                {
                    frames.Clear();
                    //We are loading a saved replay on the stack
                    var replaydata = File.ReadLines(args[0]);
                    foreach (var line in replaydata)
                    {
                        if (line == "") continue;
                        if (line == "pushFrame") continue;
                        frames.Push(new FrameData(GetWords(line,1,18)));
                    }
                    Console.WriteLine("Loaded Replay " + args[0]);
                    isReplay = true;
                }
            }
            StartReceive();
        }

        protected override ValueTask OnReceive(Memory<byte> segment)
        {
            var bytestr = string.Concat(Encoding.ASCII.GetChars(segment.ToArray()));

            var msgtype = GetWord<string>(bytestr, 0);

            if (msgtype == "pushFrame")
            {
                var rawdata = GetWords(bytestr, 1, 18);
                Console.WriteLine("PushFrame: " + rawdata);
                Console.WriteLine("Frames: " + frames.Count);
                PushFrame(rawdata);
            }
            if (msgtype == "popFrame")
            {
                if (frames.Count == 0) return new ValueTask();
                var frame = PopFrame();
                var msg = new List<string>() { "FRAME", frame.ms.ToString(), frame.deltaMs.ToString(), frame.position.x.ToString(), frame.position.y.ToString(), frame.position.z.ToString(), frame.velocity.x.ToString(), frame.velocity.y.ToString(), frame.velocity.z.ToString(), frame.spin.x.ToString(), frame.spin.y.ToString(), frame.spin.z.ToString(), frame.powerup,frame.timebonus.ToString(),frame.mpstates,frame.gemcount.ToString(),frame.gemstates,frame.ttstates }.Aggregate((i, j) => i + " " + j) + Environment.NewLine;
                var bytemsg = Encoding.ASCII.GetBytes(msg.ToCharArray());
                Console.WriteLine("PopFrame: " + msg);
                Console.WriteLine("Frames: " + frames.Count);
                popcount++;
                Send(bytemsg, 0, bytemsg.Count());
            }

            if (msgtype == "clearFrames")
            {
                if (isReplay) return new ValueTask();
                Console.WriteLine("Clearing Frames");
                popcount = 0;
                var s = new StreamWriter(File.OpenWrite(DateTime.Now.ToFileTime().ToString() + ".rwx"));
                foreach (var frame in frames)
                {
                    var msg = new List<string>() { "FRAME", frame.ms.ToString(), frame.deltaMs.ToString(), frame.position.x.ToString(), frame.position.y.ToString(), frame.position.z.ToString(), frame.velocity.x.ToString(), frame.velocity.y.ToString(), frame.velocity.z.ToString(), frame.spin.x.ToString(), frame.spin.y.ToString(), frame.spin.z.ToString(), frame.powerup, frame.timebonus.ToString(), frame.mpstates, frame.gemcount.ToString(), frame.gemstates,frame.ttstates }.Aggregate((i, j) => i + " " + j);
                    s.WriteLine(msg);
                }
                s.Flush();
                s.Close();
                frames.Clear();
            }
            return new ValueTask();
        }

        public static void Start()
        {
            Instance = TcpService<RewindService>.Create(28005);
            Instance.Start();

        }

        public static void Stop()
        {
            Instance.Dispose();
        }

        public void PushFrame(string rawframe)
        {
            try
            {
                frames.Push(new FrameData(rawframe));
            }
            catch (Exception) { };
        }

        public FrameData PopFrame()
        {
            return (frames.Count != 0) ? frames.Pop() : new FrameData("0 0 0 0 0 0 0 0 0 0 0 none 0 [] 0 []");
        }

        T GetWord<T>(string str, int pos)
        {
            return (T)Convert.ChangeType(str.Split(' ')[pos], typeof(T));
        }

        string GetWords(string str, int pos,int count)
        {
            var skipped = str.Split(' ').Skip(pos);
            var ret = skipped.Take(count);
            return ret.Aggregate((i, j) => i + " " + j);
        }
    }
}
