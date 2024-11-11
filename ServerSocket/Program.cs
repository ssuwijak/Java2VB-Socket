using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Server");

var hostName = Dns.GetHostName();
IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);
// This is the IP address of the local machine
IPAddress localIpAddress = localhost.AddressList[3];

IPEndPoint ipEndPoint = new IPEndPoint(localIpAddress, 11_000);

using Socket listener = new(
	ipEndPoint.AddressFamily,
	SocketType.Stream,
	ProtocolType.Tcp);

listener.Bind(ipEndPoint);
listener.Listen(10);

var handler = listener.Accept();
listen(handler);

//var handler = await listener.AcceptAsync();
//listenAsync(handler);

//while (true)
//{
//	// Receive message.
//	var buffer = new byte[1_024];
//	var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
//	var response = Encoding.UTF8.GetString(buffer, 0, received);

//	var eom = "<|EOM|>";
//	if (response.IndexOf(eom) > -1 /* is end of message */)
//	{
//		Console.WriteLine(
//			$"Socket server received message: \"{response.Replace(eom, "")}\"");

//		var ackMessage = "<|ACK|>";
//		var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
//		await handler.SendAsync(echoBytes, 0);
//		Console.WriteLine(
//			$"Socket server sent acknowledgment: \"{ackMessage}\"");

//		break;
//	}
//	// Sample output:
//	//    Socket server received message: "Hi friends 👋!"
//	//    Socket server sent acknowledgment: "<|ACK|>"
//}

async void listenAsync(Socket s)
{
	Console.WriteLine("Asynchronous mode");

	while (true)
	{
		// Receive message.
		var buffer = new byte[1_024];
		var received = await s.ReceiveAsync(buffer, SocketFlags.None);
		var response = Encoding.UTF8.GetString(buffer, 0, received);

		var eom = "<|EOM|>";
		if (response.IndexOf(eom) > -1 /* is end of message */)
		{
			Console.WriteLine(
				$"Socket server received message: \"{response.Replace(eom, "")}\"");

			var ackMessage = "<|ACK|>";
			var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
			await s.SendAsync(echoBytes, 0);
			Console.WriteLine(
				$"Socket server sent acknowledgment: \"{ackMessage}\"");

			break;
		}
		// Sample output:
		//    Socket server received message: "Hi friends 👋!"
		//    Socket server sent acknowledgment: "<|ACK|>"
	}
}
void listen(Socket s)
{
	Console.WriteLine("Synchronous mode");

	var eom = (char)0x04; // "<|EOM|>";
	var nul = (char)0x00;
	var etx = (char)0x03;
	var ack = (char)0x06;
	var esc = (char)0x1b;

	while (true)
	{
		// Receive message.
		var buffer = new byte[1_024];
		var received = s.Receive(buffer, SocketFlags.None);
		var response = Encoding.UTF8.GetString(buffer, 0, received);

		//var eom = "<|EOM|>";
		if (response.IndexOf(eom) > -1 /* is end of message */)
		{
			Console.WriteLine(
				$"Socket server received message: '{response.Replace(eom, nul)}'");

			var ackMessage = "*" + ack + "hello world" + esc + etx;// "<|ACK|>"
			var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
			s.Send(echoBytes, 0);
			Console.WriteLine(
				$"Socket server sent acknowledgment: '{ackMessage}'");

			//break;
		}
		// Sample output:
		//    Socket server received message: "Hi friends 👋!"
		//    Socket server sent acknowledgment: "<|ACK|>"
	}
}