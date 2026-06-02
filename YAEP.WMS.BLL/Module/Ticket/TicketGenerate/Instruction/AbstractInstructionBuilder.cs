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
    internal abstract class AbstractInstructionBuilder : IInstructionBuilder
    {
        private readonly List<IInstructionGenerator> _Generators;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeDefault">是否包含預設產生器</param>
        public AbstractInstructionBuilder(bool includeDefault = false)
        {
            this._Generators = new List<IInstructionGenerator>();

            if (includeDefault)
            {
                this._Generators.Add(new DefaultInstructionGenerator());
            }
        }
        /// <summary>
        /// 加入產生器
        /// </summary>
        /// <param name="generator"></param>
        public void Append(IInstructionGenerator generator)
        {
            this._Generators.Add(generator);
        }
        /// <summary>
        /// 取得Instruction
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual string GetInstruction(ITicketGeneratoreDataModel model)
        {
            return this.GetInstruction(model, this.appendLine);
        }
        /// <summary>
        /// 取得Instruction
        /// </summary>
        /// <param name="model"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual string GetInstruction(ITicketGeneratoreDataModel model, Action<StringBuilder, string> action)
        {
            if (this._Generators.Count == 0)
            {
                return "";
            }

            var sb = new StringBuilder();

            foreach (var generator in this._Generators)
            {
                string instruction = generator.GetInstruction(model);

                if (action == null)
                {
                    this.appendLine(sb, instruction);
                }
                else
                {
                    action(sb, instruction);
                }        
            }

            return sb.ToString();
        }

        private void appendLine(StringBuilder stringBuilder, string instruction)
        {
            if (!String.IsNullOrWhiteSpace(instruction))
            {
                stringBuilder.AppendLine(instruction);
            }
        }
    }

}
