using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Package.Interfaces.Models;
using YAEP.Utilities;

namespace YAEP.WMS.DAL.Repository
{
    public class BuiltinPackageRepository<T> : IPackageVersionRepository where T : class, IPackageVersionModel
    {
        private readonly IRepositoryHandler<T> _Handler;
        public BuiltinPackageRepository(IRepositoryHandler<T> handler)
        {
            this._Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }
        public IActionResult<bool> CheckExists(Guid itemUID, string versionId)
        {
            throw new NotImplementedException();
        }

        public IActionResult<Guid> Create(Guid itemUID, long serialNumber, string versionId)
        {
            throw new NotImplementedException();
        }

        public IActionResult<bool> Create(IPackageVersionModel packageVersion)
        {
            throw new NotImplementedException();
        }

        public IActionResult<bool> Delete(Guid versionUID)
        {
            throw new NotImplementedException();
        }

        public IActionResult<IEnumerable<IPackageVersionModel>> GetAll()
        {
            throw new NotImplementedException();
        }

        public IActionResult<IPackageVersionModel> GetPackageVersion(Guid versionUID)
        {
            throw new NotImplementedException();
        }

        public IActionResult<IEnumerable<IPackageVersionViewModel>> GetPackageVersionByPackage(IEnumerable<Guid> pkgUIDs)
        {
            var resultContainer = ActionResultTemplates.Result<IEnumerable<IPackageVersionViewModel>>();

            try
            {
                var query = @"SELECT PKG.UID PackageUID,Version.* FROM YAEP_Package AS PKG
                INNER JOIN YAEP_Package_Version AS Version ON PKG.VersionUID=Version.UID
                WHERE PKG.Status>0 AND Version.Status>0 AND PKG.UID in @pkgUIDs";
                var collection = this._Handler.Instance.Query<PackageVersionViewModel>(query, new { pkgUIDs = pkgUIDs });

                if (collection?.Count() > 0)
                {
                    foreach (var version in collection)
                    {
                        version.VersionId = GetVersionId(version.VersionId, version.SerialNumber);
                    }

                    resultContainer.Success = true;
                    resultContainer.Content = collection;
                }
                else
                {
                    resultContainer.Message = "Not Found.";
                }
            }
            catch (Exception ex)
            {
                resultContainer.Message = "Error";
                resultContainer.InnerException = ex;
            }

            return resultContainer;
        }

        public IActionResult<IPackageVersionModel> GetPackageVersionByPackage(Guid packageUID)
        {
            throw new NotImplementedException();
        }

        public IActionResult<IEnumerable<IPackageVersionModel>> GetPackageVersions(Guid itemUID)
        {
            throw new NotImplementedException();
        }

        public IActionResult<bool> Update(IPackageVersionModel packageVersion)
        {
            throw new NotImplementedException();
        }
        private string GetVersionId(string versionId, long serialNumber)
        {
            return $"{versionId} ver.{serialNumber}";
        }
    }
}
