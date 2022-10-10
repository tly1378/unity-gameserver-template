using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetworkLibrary
{
    [Serializable]
    public class Command
    {
        public string sender;
        public string cmd;
        public object[] parameters;

        public static bool TryParse(string cmd, out Command command)
        {
            command = new Command();
            List<string> output = new List<string>();

            cmd = cmd.Trim();
            string[] cmds = cmd.Split(new char[] { '\\', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //寻找符合要求的函数
            foreach (var method in typeof(Command).GetMethods()){
                if(method.Name == cmds[0])
                {
                    CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>();
                    if (attribute != null)
                    {
                        object[] parameters = new object[method.GetParameters().Length];
                        int index = 0;
                        //遍历该函数的参数
                        foreach (var parameter in method.GetParameters())
                        {
                            if (parameter.ParameterType == typeof(string))
                            {
                                parameters[index] = cmds[index+1].ToString();
                            }
                            else
                            {
                                bool parsable = false;
                                //寻找参数的解析函数
                                foreach (var parameterMethod in parameter.ParameterType.GetMethods())
                                {
                                    if (parameterMethod.Name == nameof(TryParse))
                                    {
                                        parsable = true;
                                        object result = null;
                                        bool b = (bool)parameterMethod.Invoke(parameter.ParameterType, new object[] { cmds[index+1], result });
                                        if (b)
                                        {
                                            parameters[index] = result;
                                            break;
                                        }
                                        else
                                        {
                                            throw new Exception("传入参数无法解析");
                                        }
                                    }
                                }
                                if (parsable == false)
                                {
                                    throw new Exception($"命令{cmds[index + 1]}的参数没有解析函数");
                                }
                            }
                            index++;
                        }
                        command = (Command)method.Invoke(typeof(Command), parameters);
                        return true;
                    }
                }
            }
            command.parameters = output.ToArray();
            return false;
        }

        public Command SetSender(string sender)
        {
            this.sender = sender;
            return this;
        }

        public Command SetCommand(string command)
        {
            this.cmd = command;
            return this;
        }

        public Command SetParameters(params object[] parameters)
        {
            this.parameters = parameters;
            return this;
        }

        public Command AddParameters(params object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return this;
            if (this.parameters == null || this.parameters.Length == 0)
            {
                this.parameters = parameters;
            }
            else
            {
                int length = this.parameters.Length + parameters.Length;
                object[] newParameters = new object[length];
                this.parameters.CopyTo(newParameters, 0);
                parameters.CopyTo(newParameters, this.parameters.Length);
                this.parameters = newParameters;
            }
            return this;
        }

        // 更改昵称
        [Command]
        public static Command SetName(string name)
        {
            Command command = new Command()
                .SetCommand(nameof(SetName))
                .AddParameters(name);
            return command;
        }
        public string GetName() => (string)parameters[0];

        // 发送消息
        [Command]
        public static Command SendTo(string message, string receiver)
        {
            Command command = new Command()
                .SetCommand(nameof(SendTo))
                .AddParameters(message)
                .AddParameters(receiver);
            return command;
        }

        public static Command SendTo(string message, params string[] receivers)
        {
            Command command = new Command()
                .SetCommand(nameof(SendTo))
                .AddParameters(message)
                .AddParameters(receivers);
            return command;
        }
        public string GetMessage() => (string)parameters[0];
        public string[] GetReceivers() 
        {
            List<string> receivers = new List<string>();
            foreach(var v in parameters.Skip(1).ToArray()){
                if (v is string receiver)
                    receivers.Add(receiver);
            }
            return receivers.ToArray();
        }

        // 连接成功
        [Command]
        public static Command Success()
        {
            Command command = new Command()
                .SetCommand("Success");
            return command;
        }
    }
}
