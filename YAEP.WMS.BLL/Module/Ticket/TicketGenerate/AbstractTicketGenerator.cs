using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractTicketGenerator
    {
        public AbstractTicketGenerator(
            TicketGeneratorParameters parameters)
        {
            this.TicketRepository = parameters.TicketRepository;
            this.TicketInfoRepository = parameters.TicketInfoRepository;
            this.WorkOrderManager = parameters.WorkOrderManager;
            this.SequenceAgent = parameters.SequenceAgent;
            this.TicketRelationRepository = parameters.TicketRelationRepository;
            this.PackageManager = parameters.PackageManager;
            this.PackageUomManager = parameters.PackageUomManager;
            this.LabelRepository = parameters.LabelRepository;
            this.AuthenticationProvider = parameters.AuthenticationProvider;
            this.TracingAgent = parameters.TracingAgent;
        }
        protected IEnumerable<ITicketGeneratoreDataModel> GetData(dynamic condition)
        {
            var tgom = new TicketGeneralOperationalModule(this.PackageManager, this.PackageUomManager);
            IEnumerable<ITicketGeneratoreDataModel> resultCollection =
                this.TicketRepository.GetGeneratoreTicketData(condition).Content;

            var _allLabel = resultCollection.Select(p => p.PodUID).ToList();

            _allLabel.AddRange(resultCollection.Select(p => p.PayloadUID));

            var _r_allLabel = this.LabelRepository.GetLabels(_allLabel.ToArray());
            foreach (var item in resultCollection)
            {
                var uomForLabelType = tgom.PackageforUOM(item.PackageUID);
                item.Labels = _r_allLabel.Content
                           .Where(p => (p.BelongToUID == item.PodUID)
                           || (p.BelongToUID == item.PayloadUID) && uomForLabelType.Any(x => x == p.Type))
                           .Select(
                            p => new TicketLabelInnerModel
                            {
                                Barcode = p.Content,
                                AttachmentUID = p.FileUID,
                                BarcodeType = (int)p.Type,
                                BarcodeTypeName = p.Type.ToString(),
                                BelongToType = (int)p.BelongToType,
                                BelongToUID = p.BelongToUID,
                                Status = p.Status,
                                StatusName = ((LabelStatus)p.Status).ToString()
                            } as ITicketLabelViewModel
                           ).ToArray();
            }
            return resultCollection;
        }
        protected IEnumerable<ITicketGeneratoreDataModel> GetDataByMoveManifest(dynamic condition)
        {
            var tgom = new TicketGeneralOperationalModule(this.PackageManager, this.PackageUomManager);
            IEnumerable<ITicketGeneratoreDataModel> resultCollection =
                this.TicketRepository.GetGeneratoreTicketDataByMoveManifest(condition).Content;
            var _allLabel = resultCollection.Select(p => p.PodUID).ToList();
            _allLabel.AddRange(resultCollection.Select(p => p.PayloadUID));
            var _r_allLabel = this.LabelRepository.GetLabels(_allLabel.ToArray());
            foreach (var item in resultCollection)
            {
                var uomForLabelType = tgom.PackageforUOM(item.PackageUID);
                item.Labels = _r_allLabel.Content
                           .Where(p => (p.BelongToUID == item.PodUID)
                           || (p.BelongToUID == item.PayloadUID) && uomForLabelType.Any(x => x == p.Type))
                           .Select(
                            p => new TicketLabelInnerModel
                            {
                                Barcode = p.Content,
                                AttachmentUID = p.FileUID,
                                BarcodeType = (int)p.Type,
                                BarcodeTypeName = p.Type.ToString(),
                                BelongToType = (int)p.BelongToType,
                                BelongToUID = p.BelongToUID,
                                Status = p.Status,
                                StatusName = ((LabelStatus)p.Status).ToString()
                            } as ITicketLabelViewModel
                           ).ToArray();
            }
            return resultCollection;
        }
        protected ServiceItemProcessModuleParameters GetServiceParameters()
        {
            return new ServiceItemProcessModuleParameters
            {
                PackageManager = this.PackageManager,
                PackageUomManager = this.PackageUomManager,
                SequenceAgent = this.SequenceAgent,
                AuthenticationProvider = this.AuthenticationProvider
            };

        }
        public static AbstractTicketGenerator GetInstance(ManifestType type,
           TicketGeneratorParameters parameters)
        {
            switch (type)
            {
                case ManifestType.Inbound:
                    return new InboundTicketGenerator(parameters);
                case ManifestType.Outbound:
                    //return new OutboundTicketGenerator(parameters);
                    return new OutboundTicketBatchGenerator(parameters);
                case ManifestType.Move:
                    return new MoveTicketGenerator(parameters);
                case ManifestType.BlukPick:
                    return new BulkTicketGenerateor(parameters);
                case ManifestType.InventoryCounting:
                    return new InventoryCountingTicketGenerator(parameters);
                default:
                    return null;
            }
        }


        public abstract IActionResult<bool> Execute(ITicketGenerateParameter parameter);
        protected ITicketRepository TicketRepository { get; set; }
        protected ITicketRelationRepository TicketRelationRepository { get; set; }
        protected ITicketInfoRepository TicketInfoRepository { get; set; }
        protected IWorkOrderManager WorkOrderManager { get; set; }
        protected PackageCacheManager PackageManager { get; set; }
        protected IPackageUomManager PackageUomManager { get; set; }
        protected ISequenceAgent SequenceAgent { get; set; }
        protected ILabelRepository LabelRepository { get; set; }
        protected IAuthenticationProvider AuthenticationProvider { get; set; }
        protected ITracingAgent TracingAgent { get; set; }
        /// <summary>
        /// Ticket 產生類別(要照順序)
        /// </summary>
        protected abstract ServiceProcessItem[] ServiceItems { get; }
        protected virtual IActionResult<bool> CheckData(ITicketGenerateParameter parameter, IEnumerable<ITicketGeneratoreDataModel> collection)
        {
            return null;
        }
        public abstract ManifestType Type { get; }
    }
}
