using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Common.DI;
using YAEP.Common.Interfaces.Models;
using YAEP.Utilities;

namespace YAEP.WMS.Api.Code.Cache
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
        public static IEnumerable<ICountryModel> GetCountry()
        {
            var enumType = DrKnowAllKeys.Country;

            var knowledge = Instance.Recollect<ICountryModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetCommonFactory().CreateCountryManager();
                var dataResult = manager.GetAll();
                if (dataResult.Success)
                {
                    knowledge.SetData(dataResult.Content); 
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData(); 
        }
        /// <summary>
        /// 取得國家物件
        /// </summary>
        /// <param name="countryID"></param>
        /// <returns></returns>
        public static ICountryModel GetCountry(string countryID)
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
        public static IEnumerable<IStateModel> GetState()
        {
            var enumType = DrKnowAllKeys.State;

            var knowledge = Instance.Recollect<IStateModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetCommonFactory().CreateStateManager();
                var dataResult = manager.GetAll();
                if (dataResult.Success)
                {
                    knowledge.SetData(dataResult.Content);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
        }
        /// <summary>
        /// 取得州郡物件集合
        /// </summary>
        /// <param name="countryID"></param>
        /// <returns></returns>
        public static IEnumerable<IStateModel> GetState(string countryID)
        {
            var collection = GetState();

            return collection.Where(state =>
            {
                string c = state.Country ?? String.Empty;

                bool isMatch = String.IsNullOrEmpty(countryID) || c.Equals(countryID ?? String.Empty, StringComparison.OrdinalIgnoreCase);

                return isMatch;
            });
        }
        /// <summary>
        /// 取得所有城市物件集合
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ICityModel> GetCity()
        {
            var enumType = DrKnowAllKeys.City;

            var knowledge = Instance.Recollect<ICityModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetCommonFactory().CreateCityManager();
                var dataResult = manager.GetAll();
                if (dataResult.Success)
                {
                    knowledge.SetData(dataResult.Content);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
        }
        /// <summary>
        /// 取得城市物件
        /// </summary>
        /// <param name="cityID"></param>
        /// <returns></returns>
        public static ICityModel GetCityByID(string cityID)
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
        public static IEnumerable<ICityModel> GetCity(string countryID, string stateID)
        {
            var collection = GetCity();

            return collection.Where(city =>
            {
                string c = city.Country ?? String.Empty;
                string s = city.State ?? String.Empty;

                bool match1 = String.IsNullOrEmpty(countryID) || c.Equals(countryID ?? String.Empty, StringComparison.OrdinalIgnoreCase);
                bool match2 = String.IsNullOrEmpty(stateID) || s.Equals(stateID ?? String.Empty, StringComparison.OrdinalIgnoreCase);

                return match1 && match2;
            });
        }
        /// <summary>
        /// 取得所有Zip物件集合
        /// </summary> 
        /// <returns></returns>
        public static IEnumerable<IZipModel> GetZip()
        {
            var enumType = DrKnowAllKeys.Zip;

            var knowledge = Instance.Recollect<IZipModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetCommonFactory().CreateZipManager();
                var dataResult = manager.GetAll();
                if (dataResult.Success)
                {
                    knowledge.SetData(dataResult.Content);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
        }
        /// <summary>
        /// 取得Zip物件集合
        /// </summary>
        /// <param name="countryID"></param>
        /// <param name="stateID"></param>
        /// <param name="cityID"></param>
        /// <returns></returns>
        public static IEnumerable<IZipModel> GetZip(string countryID, string stateID, string cityID)
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
            });
        }
        /// <summary>
        /// 取得對應識別碼的Zip物件
        /// </summary>
        /// <param name="zipUID">識別碼</param>
        /// <returns></returns>
        public static IZipModel GetZip(string zipUID)
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