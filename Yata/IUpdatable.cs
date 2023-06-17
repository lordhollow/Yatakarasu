using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata
{
    public interface IUpdatable
    {
        /// <summary>
        /// 更新レート(1=100ms)
        /// </summary>
        int UpdateRate { get; }

        /// <summary>
        /// 更新指示
        /// </summary>
        bool Update();
    }
}
