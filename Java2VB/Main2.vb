Public Class Main2
	Public Shared Sub Main2()
		Dim server As New SynchronousSocketServer()
		server.StartServer(11_000)


		Dim client As New SynchronousSocketClient()
		client.ConnectToServer("127.0.0.1", 11_000)

	End Sub
End Class
