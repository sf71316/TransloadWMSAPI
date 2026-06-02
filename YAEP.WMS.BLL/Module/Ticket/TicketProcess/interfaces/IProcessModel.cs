using System;

namespace YAEP.WMS.BLL.Module
{
    public interface IProcessModel
    {
        T ConvertData<T>();
        object Data { get; set; }
        DateTime ProcessTime { get; set; }
        bool Equal(object objectData);
    }
}