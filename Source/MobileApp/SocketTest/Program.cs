using System.Net.Sockets;
using System.Text;

Console.WriteLine("Hello, World!");

string deviceIp = "192.168.1.73";
int port = 9000;

using (TcpClient client = new TcpClient())
{
    try
    {
        // Connect to the device
        Console.WriteLine($"Connecting to {deviceIp}:{port}...");
        await client.ConnectAsync(deviceIp, port);
        Console.WriteLine("Connected to the device.");

        // Get the network stream for sending/receiving data
        NetworkStream stream = client.GetStream();

        var n = 0;

        //Send a command to the device
        string command = "0.0,40.0,0.0"; // (0) Manual / (1) Auto | Set Point Temp | Duty Cycle in Manual Mode
        byte[] commandBytes = Encoding.ASCII.GetBytes(command);
        await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
        Console.WriteLine($"Sent: {command}");

        while (n < 10)
        {
            // Receive a response from the device
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received: {response}");

            Thread.Sleep(1000);
            n++;
        }

        stream.Close();
        //client.Client.Shutdown(SocketShutdown.Both);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}