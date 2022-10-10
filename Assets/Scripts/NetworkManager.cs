using NetworkLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkManager : MonoBehaviour
{
    public Client client;
    public bool started = false;

    public string host;
    public int port;
    public TMPro.TMP_InputField field;
    public TMPro.TMP_Text text;
    public ClientExecuter executer;
    public string message;
    public string targetId;
    public string TargetId { set => targetId = value; get => targetId; }

    private void Start()
    {
        executer = new ClientExecuter(this);
        ConnectedToServer();
    }

    public void ConnectedToServer(string name = null)
    {
        if(string.IsNullOrWhiteSpace(name))
            name = ColorUtility.ToHtmlStringRGB(Random.ColorHSV());
        TargetId = name;

        client = new Client(host, port, name);
        Task.Run(() => { client.Connect(); });

        //StartCoroutine(CheckConnection());
    }

    IEnumerator CheckConnection()
    {
        float time = 2;
        while (time>0)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        if (started)
        {
            Println("<color=green>连接成功</color>");
        }
        else
        {
            Println("<color=red>连接失败</color>");
        }
    }

    private void Update()
    {
        if (client?.run == true)
        {
            object package = client.GetObject();
            if(package is Command command)
            {
                executer.Execute(command);
            }
            else if(package is string message)
            {
                Println(message);
            }
            else if (package != null)
            {
                Println($"<color=grey>{package}</color>");
            }
        }
    }

    public void Send()
    {
        if (client == null)
        {
            Println("<color=yellow>尚未连接服务器</color>");
            Println(message);
            return;
        }
        if (message.StartsWith('\\'))
        {
            if (Command.TryParse(message, out Command cmd))
            {
                if (cmd != null && !string.IsNullOrWhiteSpace(cmd.cmd))
                {
                    client.Send(cmd);
                }
            }
            else
            {
                foreach (var txt in cmd.parameters)
                    print(txt);
            }
        }
        else
        {
            client.Send(Command.SendTo(message, TargetId));
        }
    }

    public void UpdateMessage()
    {
        message = field.text;
    }

    public void Println(object text)
    {
        this.text.text += $"{text}\n";
    }
}
