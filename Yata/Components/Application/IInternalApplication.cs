using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Application
{
    interface IInternalApplication
    {
        void Execute(Karasu karasu, string option);

    }
}
