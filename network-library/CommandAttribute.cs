using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLibrary
{
    [AttributeUsage(
        AttributeTargets.Method |
        AttributeTargets.Property,
       AllowMultiple = false)]
    class CommandAttribute : Attribute
    {

    }
}
