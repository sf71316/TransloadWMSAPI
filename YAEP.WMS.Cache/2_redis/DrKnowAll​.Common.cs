using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Utilities;
using YAEP.WMS.Cache.Models;
using YAEP.WMS.Cache.Redis.Controllers;

namespace YAEP.WMS.Cache.Redis
{
    /*
    *  Common 
    */
    public static partial class DrKnowAll
    {
        /// <summary>
        /// 取得所有國家物件集合
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<CountryCacheModel> GetCountry()
        {
            var controller = new CountryRedisController();
            var allCountry = controller.RetrieveAll();

            if ((allCountry?.Count() ?? 0) == 0)
            {
                var manager = FactoryUtils.GetCommonFactoryInstance().CreateCountryManager();
                var data = manager.GetAll()?.Content;
                if ((data?.Count() ?? 0) > 0)
                {
                    allCountry = data.Select(o => new CountryCacheModel()
                    {
                        UID = o.UID,
                        ID = o.ID,
                        Name = o.Name,
                        EnglishName = o.EnglishName,
                        DisplayName = o.DisplayName,
                        CultureName = o.CultureName,
                        ISO2 = o.ISO2,
                        ISO3 = o.ISO3,
                        Language2 = o.Language2,
                        Language3 = o.Language3,
                        ISOCurrencySymbol = o.ISOCurrencySymbol,
                        Description = o.Description,
                        Sort = o.Sort,
                        Status = o.Status,
                        CreatedBy = o.CreatedBy,
                        CreatedOn = o.CreatedOn,
                        ModifiedBy = o.ModifiedBy,
                        ModifiedOn = o.ModifiedOn,
                    }).ToArray();

                    controller.Create(allCountry);
                }
            }

            return allCountry;
        }
        /// <summary>
        /// 取得國家物件
        /// </summary>
        /// <param name="countryID"></param>
        /// <returns></returns>
        public static CountryCacheModel GetCountry(string countryID)
        {
            if (String.IsNullOrWhiteSpace(countryID))
            {
                return null;
            }

            return GetCountry().FirstOrDefault(o => o.ID.Equals(countryID, StringComparison.OrdinalIgnoreCase));
        }
        /// <summary>
        /// 取得所有州郡物件集合
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<StateCacheModel> GetState()
        {
            var controller = new StateRedisController();
            var allState = controller.RetrieveAll();

            if ((allState?.Count() ?? 0) == 0)
            {
                var manager = FactoryUtils.GetCommonFactoryInstance().CreateStateManager();
                var data = manager.GetAll()?.Content;
                if ((data?.Count() ?? 0) > 0)
                {
                    allState = data.Select(o => new StateCacheModel()
                    {
                        UID = o.UID,
                        ID = o.ID,
                        Name = o.Name,
                        Country = o.Country,
                        Description = o.Description,
                        Sort = o.Sort,
                        Status = o.Status,
                        CreatedBy = o.CreatedBy,
                        CreatedOn = o.CreatedOn,
                        ModifiedBy = o.ModifiedBy,
                        ModifiedOn = o.ModifiedOn,
                    }).ToArray();

                    controller.Create(allState);
                }
            }

            return allState;
        }
        /// <summary>
        /// 取得州郡物件集合
        /// </summary>
        /// <param name="countryID"></param>
        /// <returns></returns>
        public static IEnumerable<StateCacheModel> GetState(string countryID)
        {
            var collection = GetState();

            return collection.Where(state =>
            {
                string c = state.Country ?? String.Empty;

                bool isMatch = String.IsNullOrEmpty(countryID) || c.Equals(countryID ?? String.Empty, StringComparison.OrdinalIgnoreCase);

                return isMatch;
            }).ToArray();
        }
        /// <summary>
        /// 取得所有城市物件集合
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<CityCacheModel> GetCity()
        {
            var controller = new CityRedisController();
            var allCity = controller.RetrieveAll();

            if ((allCity?.Count() ?? 0) == 0)
            {
                var manager = FactoryUtils.GetCommonFactoryInstance().CreateCityManager();
                var data = manager.GetAll()?.Content;
                if ((data?.Count() ?? 0) > 0)
                {
                    allCity = data.Select(o => new CityCacheModel()
                    {
                        UID = o.UID,
                        ID = o.ID,
                        Name = o.Name,
                        Country = o.Country,
                        State = o.State,
                        Description = o.Description,
                        Sort = o.Sort,
                        Status = o.Status,
                        CreatedBy = o.CreatedBy,
                        CreatedOn = o.CreatedOn,
                        ModifiedBy = o.ModifiedBy,
                        ModifiedOn = o.ModifiedOn,
                    }).ToArray();

                    try
                    {
                        controller.Create(allCity);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return allCity;
        }
        /// <summary>
        /// 取得城市物件
        /// </summary>
        /// <param name="cityID"></param>
        /// <returns></returns>
        public static CityCacheModel GetCityByID(string cityID)
        {
            if (String.IsNullOrWhiteSpace(cityID))
            {
                return null;
            }

            return GetCity().FirstOrDefault(o => o.ID.Equals(cityID, StringComparison.OrdinalIgnoreCase));
        }
        /// <summary>
        /// 取得城市物件集合
        /// </summary>
        /// <param name="countryID"></param>
        /// <param name="stateID"></param>
        /// <returns></returns>
        public static IEnumerable<CityCacheModel> GetCity(string countryID, string stateID)
        {
            var collection = GetCity();

            return collection.Where(city =>
            {
                string c = city.Country ?? String.Empty;
                string s = city.State ?? String.Empty;

                bool match1 = String.IsNullOrEmpty(countryID) || c.Equals(countryID ?? String.Empty, StringComparison.OrdinalIgnoreCase);
                bool match2 = String.IsNullOrEmpty(stateID) || s.Equals(stateID ?? String.Empty, StringComparison.OrdinalIgnoreCase);

                return match1 && match2;
            }).ToArray();
        }
        /// <summary>
        /// 取得所有Zip物件集合
        /// </summary> 
        /// <returns></returns>
        public static IEnumerable<ZipCacheModel> GetZip()
        {
            var controller = new ZipRedisController();
            var allZip = controller.RetrieveAll();

            if ((allZip?.Count() ?? 0) == 0)
            {
                var manager = FactoryUtils.GetCommonFactoryInstance().CreateZipManager();
                var data = manager.GetAll()?.Content;
                if ((data?.Count() ?? 0) > 0)
                {
                    allZip = data.Select(o => new ZipCacheModel()
                    {
                        UID = o.UID,
                        ID = o.ID,
                        City = o.City,
                        State = o.State,
                        Country = o.Country,
                        Description = o.Description,
                        Latitude = o.Latitude,
                        Longtitude = o.Longtitude,
                        Status = o.Status,
                        CreatedBy = o.CreatedBy,
                        CreatedOn = o.CreatedOn,
                        ModifiedBy = o.ModifiedBy,
                        ModifiedOn = o.ModifiedOn,
                    }).ToArray();
                    try
                    {
                        controller.Create(allZip);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return allZip;
        }
        /// <summary>
        /// 取得Zip物件集合
        /// </summary>
        /// <param name="countryID"></param>
        /// <param name="stateID"></param>
        /// <param name="cityID"></param>
        /// <returns></returns>
        public static IEnumerable<ZipCacheModel> GetZip(string countryID, string stateID, string cityID)
        {
            var collection = GetZip();

            return collection.Where(zip =>
            {
                string c = zip.Country ?? String.Empty;
                string s = zip.State ?? String.Empty;
                string ct = zip.City ?? String.Empty;

                bool match1 = String.IsNullOrEmpty(countryID) || c.Equals(countryID ?? String.Empty, StringComparison.OrdinalIgnoreCase);
                bool match2 = String.IsNullOrEmpty(stateID) || s.Equals(stateID ?? String.Empty, StringComparison.OrdinalIgnoreCase);
                bool match3 = String.IsNullOrEmpty(cityID) || ct.Equals(cityID ?? String.Empty, StringComparison.OrdinalIgnoreCase);

                return match1 && match2 && match3;
            }).ToArray();
        }
        /// <summary>
        /// 取得對應識別碼的Zip物件
        /// </summary>
        /// <param name="zipUID">識別碼</param>
        /// <returns></returns>
        public static ZipCacheModel GetZip(string zipUID)
        {
            Guid uid = Utility.ToGuid(zipUID);

            if (uid == Guid.Empty)
            {
                return null;
            }

            var collection = GetZip();

            return collection.FirstOrDefault(zip => zip.UID == uid);
        }

    }

}