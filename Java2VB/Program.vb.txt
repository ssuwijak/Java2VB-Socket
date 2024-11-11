Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text

Public Class Program

	Private Shared IP As String = "192.168.1.61" ' machine ip address
	Private Shared SERVER_PORT As Integer ' machine port
	Private Shared SLAVE_ADDRESS As Integer

	Private Const STX As Byte = &H2
	Private Const ETX As Byte = &H3
	Private Const COMMAND_ID As Byte = &H1D '29
	Private Const ESC As Byte = &H1B '27
	Private Const EOT As Byte = &H4 ' n/a in java, used to debug and communicate with SocketServer project.
	Public Shared Sub Main(args() As String)
#If DEBUG Then
		ReDim args(1)
		args(0) = GetLocalIPAddress()
		args(1) = 11_000
#End If

		IP = args(0)
		SERVER_PORT = Integer.Parse(args(1))
		'SLAVE_ADDRESS = Integer.Parse(IP.Substring(IP.LastIndexOf(".") + 1, IP.Length))
		SLAVE_ADDRESS = Integer.Parse(IP.Substring(IP.LastIndexOf(".") + 1))

		Dim data As New List(Of String)
		Dim size As Integer = args.Length

		If args.Length > 2 Then
			data.Add(args(2))
		Else
			data.Add("odak test1")
		End If

		If args.Length > 3 Then
			data.Add(args(3))
		Else
			data.Add("blackdot2")
		End If

		If args.Length > 4 Then
			data.Add(args(4))
		Else
			data.Add("yasin")
		End If

		Dim fieldIds As New List(Of Integer)
		fieldIds.Add(135)
		fieldIds.Add(136)
		fieldIds.Add(137)

		sendData(data, fieldIds.Count, fieldIds)
	End Sub

	Private Shared Sub sendData(data As List(Of String), numberOfFields As Integer, fieldIds As List(Of Integer))
		Dim s As Socket = Nothing

		Try
			' Open socket
			s = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
			s.Connect(New IPEndPoint(IPAddress.Parse(IP), SERVER_PORT))

			sendData(data, numberOfFields, fieldIds, s)
		Catch ex As SocketException
			Console.WriteLine("Socket: " & ex.Message)
		Catch ex As EndOfStreamException
			Console.WriteLine("EOF:" & ex.Message)
		Catch ex As IOException
			Console.WriteLine("IO:" & ex.Message)
		Catch ex As Exception
			Console.WriteLine("EX:" & ex.Message)
		Finally
			If s IsNot Nothing Then
				Try
					s.Close()
				Catch ex As IOException
					Console.WriteLine("Closing socket failed cause of: " & ex.Message)
				End Try
			End If
		End Try
	End Sub

	Private Shared Sub sendData(data As List(Of String), numberOfFields As Integer, fieldIds As List(Of Integer), s As Socket)
		Try
			Dim checksum As Integer = calculateChecksum(data, numberOfFields, fieldIds)

			' Open streams
			Dim input As New NetworkStream(s, FileAccess.Read)
			Dim output As New NetworkStream(s, FileAccess.Write)

			' Write data
			writeData(output, data, checksum, numberOfFields, fieldIds)

			' Read reply
			Dim digits As New List(Of Byte)
			Dim flag As Boolean = False

			Do
				digits.Add(input.ReadByte())

				If digits.Count >= 2 Then
					flag = digits(digits.Count - 2) <> ESC OrElse digits(digits.Count - 1) <> ETX
				End If
			Loop While digits.Count < 2 OrElse flag

#If DEBUG Then
			' n/a in java, used to debug and communicate with SocketServer project.
			Dim ss As String = Encoding.ASCII.GetString(digits.ToArray())
#Else
			' Read checksum
			digits.Add(input.ReadByte())
#End If

			printReplyMessage(digits)
		Catch ex As Exception
			Dim ss As String = ex.Message
		End Try
	End Sub
	Private Shared Function sumOfIntArr(arr() As Integer) As Integer
		Dim res As Integer = 0

		For Each value In arr
			res += value
		Next

		Return res
	End Function
	Private Shared Function hexIntArray(text As String) As Integer()
		Dim array(text.Length) As Integer
		Dim textCharArr() As Char = text.ToCharArray()

		For i As Integer = 0 To text.Length - 1
			'array(i) = Integer.Parse(toHexadecimal(New String(textCharArr(i), 1)), 16)
			array(i) = Convert.ToInt32(toHexadecimal(New String(textCharArr(i), 1)), 16)
		Next

		Return array
	End Function

	Private Shared Function toHexadecimal(text As String) As String
		Dim myBytes As Byte() = Encoding.UTF8.GetBytes(text)
		Dim hexString As String = BitConverter.ToString(myBytes).Replace("-", "")

		Return hexString
	End Function
	Private Shared Function calculateChecksum(data As List(Of String), numberOfFields As Integer, fieldIds As List(Of Integer)) As Integer
		' Step 1: Sum of all bytes in the message
		Dim checksum As Integer = ESC + ESC + STX + ETX + SLAVE_ADDRESS + COMMAND_ID + numberOfFields

		For i As Integer = 0 To data.Count - 1
			' Add sum of hex values in each string
			checksum += sumOfIntArr(hexIntArray(data(i)))
			' Add field ID
			checksum += fieldIds(i)
			' Add length of the string (byte count)
			checksum += Encoding.UTF8.GetBytes(data(i)).Length
		Next

		' Step 2: Bitwise AND with 0xFF
		checksum = checksum And &HFF   'masked any binary size to be an 8-bits data size (0xFF masked)
		Dim chk() As Integer = {checksum, 0, 0}

		' Step 3: Two's complement
		checksum = Not checksum + 1
		chk(1) = checksum ' original java method
		chk(2) = 256 - chk(0) ' new decimal arithmatic method
#If DEBUG Then
		checksum = chk(2)
#End If

		Return checksum
	End Function
	Private Shared Sub writeData(output As NetworkStream, data As List(Of String), checksum As Integer, numberOfFields As Integer, fieldIds As List(Of Integer))
		Try
			' Write data in the specified order
			output.WriteByte(ESC)
			output.WriteByte(STX)
			output.WriteByte(SLAVE_ADDRESS)
			output.WriteByte(COMMAND_ID)
			output.WriteByte(numberOfFields)

			For i As Integer = 0 To data.Count - 1
				output.WriteByte(fieldIds(i))
				output.WriteByte(Encoding.UTF8.GetBytes(data(i)).Length)
				output.Write(Encoding.UTF8.GetBytes(data(i)), 0, Encoding.UTF8.GetBytes(data(i)).Length)
			Next

			output.WriteByte(ESC)
			output.WriteByte(ETX)
			output.WriteByte(checksum)
#If DEBUG Then
			' n/a in java, used to debug and communicate with SocketServer project.
			output.WriteByte(EOT)
#End If

			Console.WriteLine("Sending.......")
			output.Flush()
			Console.WriteLine("Sent.......")
		Catch ex As Exception
			Dim ss As String = ex.Message
		End Try

	End Sub
	Private Shared Sub printReplyMessage(digits As List(Of Byte))
		Select Case digits(1)
			Case &H6 '6  ' 0x06 
				Console.WriteLine("Message was sent successfully")
			Case &H15 '21 ' 0x15
				Console.WriteLine("Message has an error")

				Select Case digits(3)
					Case &H6 '6  ' 0x06
						Console.WriteLine("Invalid command start")
					Case &H7 '7  ' 0x07
						Console.WriteLine("Invalid command end")
					Case &H8 '8  ' 0x08
						Console.WriteLine("Invalid checksum")
					Case &H9 '9  ' 0x09
						Console.WriteLine("Invalid number of field")
					Case &HA '10 ' 0x0A
						Console.WriteLine("Invalid module type")
					Case &H11 '17 ' 0x11
						Console.WriteLine("Invalid command ID")
					Case &H3C '60 ' 0x3C
						Console.WriteLine("Invalid print mode")
					Case Else
						Console.WriteLine("No idea what is going wrong")
				End Select
			Case Else
				Console.WriteLine("No info")
		End Select
	End Sub

	''' <summary>
	''' n/a in java, used to debug and communicate with SocketServer project.
	''' </summary>
	''' <returns></returns>
	Public Shared Function GetLocalIPAddress() As String
		Dim hostEntry As IPHostEntry = Dns.GetHostEntry(Dns.GetHostName())
		Dim ipList As IPAddress() = hostEntry.AddressList

		' Loop through addresses and return the first non-loopback address (IPv4)
		For Each address As IPAddress In ipList
			'If Not address.Then Then End If
			If address.AddressFamily = AddressFamily.InterNetwork Then  ' Check for IPv4
				Return address.ToString()
			End If
		Next

		' Return an empty string if no suitable address found
		Return String.Empty
	End Function
End Class