using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Module
{
    public class TicketProcessModel : IProcessModel
    {
        public TicketProcessModel()
        {
            this.ProcessTime = DateTime.Now;
        }
        public T ConvertData<T>()
        {
            return (T)Data;
        }
        public bool Equal(object objectData)
        {
            if (Data is Guid && objectData is Guid)
            {
                return (Guid)Data == (Guid)objectData;
            }
            else
            {
                return false;
            }
        }

        public object Data { get; set; }
        public DateTime ProcessTime { get; set; }
    }
}
