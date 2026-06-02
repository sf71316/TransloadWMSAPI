using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface ISequenceRepository
    {

        //IActionResult<ISequenceModel> Find(Guid belongtoUID, string belongtoTag);
        //IActionResult<bool> UpdateSeqeuce(Guid belongtoUID, string belongtoTag);
        IActionResult<ISequenceModel> GetSeqeuce(string belongtoUID, string belongtoTag);
        IActionResult<List<ISequenceModel>> GetSeqeuceByBatch(string belongtoUID, string belongtoTag, int BatchCount);
    }
}
