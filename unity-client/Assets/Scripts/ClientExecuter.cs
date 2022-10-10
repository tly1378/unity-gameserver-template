using NetworkLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

public class ClientExecuter : Executer
{
    NetworkManager networkManager;
    public ClientExecuter(NetworkManager networkManager)
    {
        this.networkManager = networkManager;
    }

    // 设置消息发送对象
    public string setTarget;
    public void SetTarget(Command command)
    {
        if(command.parameters[0] is string targetId)
            networkManager.targetId = targetId;
    }

    // 通知客户端连接成功
    public void Success(Command _)
    {
        networkManager.started = true;
    }
}
