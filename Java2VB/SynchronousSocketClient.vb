Imports System.Net
Imports System.Net.Sockets
Imports System.Text

Public Class SynchronousSocketClient
    Public Sub ConnectToServer(ipAddress As String, port As Integer)
        Dim serverEndPoint As IPEndPoint = ConvertToIPEndPoint(ipAddress, port)
        Dim clientSocket As Socket = New Socket(AddressFamily.InterNetwork,
                                                SocketType.Stream,
                                                ProtocolType.Tcp)

        clientSocket.Connect(serverEndPoint)
        Console.WriteLine("Connected to server: {0}", serverEndPoint.ToString())

        Using output As New NetworkStream(clientSocket, IO.FileAccess.Write) 'clientSocket.GetStream()
            Dim message As String = "Hello from client!"
            Dim messageBytes As Byte() = Encoding.ASCII.GetBytes(message)
            output.Write(messageBytes, 0, messageBytes.Length)
        End Using

        Using input As New NetworkStream(clientSocket, IO.FileAccess.Read)
            Dim buffer(1024) As Byte
            Dim bytesReceived As Integer = input.Read(buffer, 0, buffer.Length)

            Dim response As String = Encoding.ASCII.GetString(buffer, 0, bytesReceived)
            Console.WriteLine("Received from server: {0}", response)
        End Using

        clientSocket.Close()
    End Sub

    Private Function ConvertToIPEndPoint(ipAddress As String, port As Integer) As IPEndPoint
        Dim address As IPAddress = System.Net.IPAddress.Parse(ipAddress)
        Dim endpoint As IPEndPoint = New IPEndPoint(address, port)
        Return endpoint
    End Function

    'Public Shared Sub Main(args As String())
    '    Dim serverIP As String = "127.0.0.1" ' Replace with server IP address
    '    Dim port As Integer = 12345  ' Same port as server

    '    ConnectToServer(serverIP, port)
    'End Sub
End Class