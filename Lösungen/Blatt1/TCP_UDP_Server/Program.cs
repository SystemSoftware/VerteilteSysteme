using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCP_UDP_Server
{
	class Program
	{
		const int Port = 1024;
		static TcpListener listener;
		static Thread listenThread;
		static UdpClient udpServer;

		static TcpClient current;

		static void Listen()
		{
			Console.WriteLine("TCP online");
			byte[] buffer = new byte[8];
			while (true)
			{
				TcpClient client = current = listener.AcceptTcpClient();
				try
				{
					while (true)
					{
						Read(client.GetStream(), buffer);
						int a = BitConverter.ToInt32(buffer, 0);
						int b = BitConverter.ToInt32(buffer, 4);
						int rs = a - b;
						//Console.WriteLine(a + "-" + b + "=" + rs);
						client.GetStream().Write(BitConverter.GetBytes(rs), 0, 4);
					}
				}
				catch (IOException)
				{
					Console.WriteLine("Cliend disconnected");
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
				current = null;
			}
		}

		private static void Read(NetworkStream networkStream, byte[] buffer)
		{
			int at = 0;
			while (at != buffer.Length)
			{
				int read = networkStream.Read(buffer, at, buffer.Length - at);
				if (read <= 0)
					throw new IOException("Bad read (" + read + ")");
				at += read;
			}
		}

		static void Main(string[] args)
		{
			listener = new TcpListener(IPAddress.Any, Port);
			listener.Start();
			listenThread = new Thread(new ThreadStart(Listen));
			listenThread.Start();
			udpServer = new UdpClient(Port);
			while (true)
			{
				IPEndPoint sender = new IPEndPoint(IPAddress.Any, Port);
				byte[] data = udpServer.Receive(ref sender);
				if (data.Length == 4)
				{
					int cnt = BitConverter.ToInt32(data, 0);
					//Console.WriteLine("Bounce "+cnt);
					TcpClient cl = current;
					if (cl != null)
						cl.GetStream().Write(data, 0, 4);
				}
			}


			//listenThread.Join();
		}
	}
}
