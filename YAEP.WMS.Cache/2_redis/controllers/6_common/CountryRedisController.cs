using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class CountryRedisController : AbstractDefaultConnectSettingController<CountryCacheModel>
    {
        public CountryRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(CountryCacheModel.ID), o => o.ID);
            this.AppendIndex(nameof(CountryCacheModel.Name), o => o.Name);
            this.AppendIndex(nameof(CountryCacheModel.EnglishName), o => o.EnglishName);
            this.AppendIndex(nameof(CountryCacheModel.DisplayName), o => o.DisplayName);
            this.AppendIndex(nameof(CountryCacheModel.CultureName), o => o.CultureName);
            this.AppendIndex(nameof(CountryCacheModel.ISO2), o => o.ISO2);
            this.AppendIndex(nameof(CountryCacheModel.ISO3), o => o.ISO3);
            this.AppendIndex(nameof(CountryCacheModel.Language2), o => o.Language2);
            this.AppendIndex(nameof(CountryCacheModel.Language3), o => o.Language3);
            this.AppendIndex(nameof(CountryCacheModel.ISOCurrencySymbol), o => o.ISOCurrencySymbol);
        }

    }

}
