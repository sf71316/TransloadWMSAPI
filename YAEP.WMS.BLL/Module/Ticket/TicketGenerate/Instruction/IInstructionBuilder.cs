using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{ 
    internal interface IInstructionBuilder
    {
        /// <summary>
        /// 加入產生器
        /// </summary>
        /// <param name="generator"></param>
        //void Append(IInstructionGenerator generator);
        /// <summary>
        /// 取得Instruction
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        string GetInstruction(ITicketGeneratoreDataModel model);
        /// <summary>
        /// 取得Instruction
        /// </summary>
        /// <param name="model"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        string GetInstruction(ITicketGeneratoreDataModel model, Action<StringBuilder, string> action);
        
    } 

}
