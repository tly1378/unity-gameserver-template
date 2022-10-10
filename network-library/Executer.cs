using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkLibrary
{
    public abstract class Executer
    {
        public Queue<Exception> errors;

        public virtual bool Execute(Command command)
        {
            if (string.IsNullOrWhiteSpace(command.cmd))
                return false;

            string cmd = command.cmd;
            var methodInfo = GetType().GetMethod(cmd);
            if (methodInfo != null)
            {
                Task.Run(() =>
                {
                    object[] parameter = new object[] { command };
                    try
                    {
                        methodInfo.Invoke(this, parameter);
                    }
                    catch(Exception e)
                    {
                        errors.Enqueue(e);
                    }
                });
            }
            return true;
        }
    }
}
