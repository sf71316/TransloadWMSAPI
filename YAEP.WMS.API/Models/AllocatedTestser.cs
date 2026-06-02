using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Api.Code;

namespace YAEP.WMS.BLL.Module
{
    public class AllocatedTestser : AbstractApiController
    {
        public void Execute(IEnumerable<dynamic> parameter)
        {
            InitDIRoot();
            var factory = this.GetIdentityFactory();
            var manager = factory.CreateGroupManager();
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
            {
                var rs = _instance.AllocatedTest(parameter);

            }
        }
    }
}
