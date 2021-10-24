using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Scripting.Python;
using UnityEngine;

public class ServerClient : MonoBehaviour
{
    Server server;
    // Start is called before the first frame update
    void Start()
    {
        server = new Server();
        server.Start();
        //PythonRunner.RunFile(Directory.GetCurrentDirectory() + "/Assets/PythonFiles/TEST.py");
        string strCmdText = "python "+ Directory.GetCurrentDirectory() + "/Assets/PythonFiles/server.py";
        System.Diagnostics.Process.Start("powershell.exe", strCmdText);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public class Server : RunAbleThread
{
    protected override void Run()
    {
        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet
        using (RequestSocket client = new RequestSocket())
        {
            client.Connect("tcp://localhost:5555");

            for (int i = 0; i < 10 && Running; i++)
            {
                Debug.Log("Sending Hello");
                client.SendFrame("Hello");
                // ReceiveFrameString() blocks the thread until you receive the string, but TryReceiveFrameString()
                // do not block the thread, you can try commenting one and see what the other does, try to reason why
                // unity freezes when you use ReceiveFrameString() and play and stop the scene without running the server
                //                string message = client.ReceiveFrameString();
                //                Debug.Log("Received: " + message);
                string message = null;
                bool gotMessage = false;
                while (Running)
                {
                    gotMessage = client.TryReceiveFrameString(out message); // this returns true if it's successful
                    if (gotMessage) break;
                }

                if (gotMessage) Debug.Log("Received " + message);
            }
        }
        NetMQConfig.Cleanup(); // this line is needed to prevent unity freeze after one use, not sure why yet
    }
}
