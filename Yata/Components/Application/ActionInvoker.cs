using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Application
{
    /// <summary>
    /// 依存性注入して作るアプリ
    /// </summary>
    class ActionInvoker : IInternalApplication
    {

        public ActionInvoker(Action action)
        {
            Action = action;
        }

        public Action Action { get; set; }

        public void Execute(Karasu karasu, string option)
        {
            if (Action != null)
            {
                Action();
            }
        }
    }
}
