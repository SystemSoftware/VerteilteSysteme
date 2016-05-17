using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCP_UDP_Client
{
	class Program
	{
		private static void Read(NetworkStream networkStream, byte[] buffer)
		{
			int at = 0;
			while (at != buffer.Length)
			{
				int read = networkStream.Read(buffer, at, buffer.Length - at);
				if (read <= 0)
					throw new Exception("Bad read ("+read+")");
				at += read;
			}
		}

		static TcpClient client;


		static Thread clientThread;
		static ConcurrentDictionary<int, bool> unconfirmed = new ConcurrentDictionary<int, bool>();

		static Stopwatch watch = new Stopwatch();
		static void Reader()
		{
			while (true)
			{
				byte[] rcv = new byte[4];
				Read(client.GetStream(), rcv);
				int rs = BitConverter.ToInt32(rcv, 0);

				watch.Reset();
				watch.Start();
				bool b;
				if (!unconfirmed.TryRemove(rs,out b))
				{
					Console.WriteLine("bad confirm: " + rs);
				}
			}
		}

		static void Main(string[] args)
		{
			try
			{
				client = new TcpClient("infcip67.uni-trier.de", 1024);




				watch.Start();
				clientThread = new Thread(new ThreadStart(Reader));
				clientThread.Start();

				UdpClient udpClient = new UdpClient(0);
				Random rnd = new Random();
				IPEndPoint udpDest = new IPEndPoint(((IPEndPoint)client.Client.RemoteEndPoint).Address, 1024);

				for (int i = 0; i < 1000; i++)
				{
					unconfirmed.TryAdd(i, true);
					udpClient.Send(BitConverter.GetBytes(i), 4, udpDest);
				}
				while (watch.Elapsed.TotalSeconds < 1.0)
				{
					Thread.Sleep(100);
				}
				Console.WriteLine("Unconfirmed: " + unconfirmed.Count);
				clientThread.Join();


				/*byte[] snd = new byte[8], rcv = new byte[4];

				int iterations = 0;
				Stopwatch w = new Stopwatch();
				w.Start();
				while (w.Elapsed.TotalSeconds < 10)
				{
					int a = rnd.Next(0, 100),
						b = rnd.Next(1, 100);
					//Console.Write(a + "-" + b);
					BitConverter.GetBytes(a).CopyTo(snd, 0);
					BitConverter.GetBytes(b).CopyTo(snd, 4);
					client.GetStream().Write(snd, 0, 8);

					//Console.Write("=");

					Read(client.GetStream(), rcv);
					int rs = BitConverter.ToInt32(rcv, 0);
					Debug.Assert(rs == a - b);
					iterations++;
					//Console.WriteLine(rs);
				}
				double t = w.Elapsed.TotalSeconds;
				w.Stop();
				Console.WriteLine("Iterations per second: " + (double)iterations / t);

				*/

				client.Close();

				Console.ReadLine();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
