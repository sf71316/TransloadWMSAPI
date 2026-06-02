using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.Package.Interfaces;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Api.Code
{
    internal class VersionSerialNumberGenerator : IPackageVersionSerialNumberGenerator
    {
        private readonly ISequenceAgent _Agent;
        public VersionSerialNumberGenerator(ISequenceAgent agent)
        {
            this._Agent = agent;
        }
        public long GetSerialNumber(Guid itemUID)
        {
            int sn = this._Agent.GetSeqenceIndex(itemUID, "Package Version");
            long v;
            if (long.TryParse(sn.ToString(), out v))
            {
                return v;
            }

            return -1;
        }
    }
}