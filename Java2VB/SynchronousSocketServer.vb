Imports System.Net
Imports System.Net.Sockets
Imports System.Text

Public Class SynchronousSocketServer

    Public Sub StartServer(port As Integer)
        Dim ipAddress As IPAddress = IPAddress.Any  ' Listen on all interfaces
        Dim localEndPoint As IPEndPoint = New IPEndPoint(ipAddress, port)

        Dim serverSocket As Socket = New Socket(AddressFamily.InterNetwork,
                                                SocketType.Stream,
                                                ProtocolType.Tcp)

        serverSocket.Bind(localEndPoint)
        serverSocket.Listen(10)  ' Listen for up to 10 connections

        Console.WriteLine("Server started at {0}:{1}", localEndPoint.Address, localEndPoint.Port)

        While True
            Dim clientSocket As Socket = serverSocket.Accept()
            Console.WriteLine("Client connected: {0}", clientSocket.RemoteEndPoint.ToString())

            Using input As New NetworkStream(clientSocket, IO.FileAccess.Read) ' clientSocket.GetStream()
                Dim buffer(1024) As Byte
                Dim bytesReceived As Integer = input.Read(buffer, 0, buffer.Length)

                Dim data As String = Encoding.ASCII.GetString(buffer, 0, bytesReceived)
                Console.WriteLine("Received from client: {0}", data)
            End Using

            Using output As New NetworkStream(clientSocket, IO.FileAccess.Write)
                ' Process received data (implement your logic here)

                Dim response As String = "Hello from server!"
                Dim responseBytes As Byte() = Encoding.ASCII.GetBytes(response)
                output.Write(responseBytes, 0, responseBytes.Length)
            End Using

            clientSocket.Close()
        End While

        serverSocket.Close()
    End Sub

    'Public Shared Sub Main(args As String())
    '    Dim port As Integer = 12345  ' Replace with your desired port number

    '    StartServer(port)
    'End Sub
End Class