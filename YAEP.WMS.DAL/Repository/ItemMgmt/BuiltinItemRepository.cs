using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Constants;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.Utilities.Extensions;

namespace YAEP.WMS.DAL.Repository
{
    public class BuiltinItemRepository<T> : IItemRepository where T : class, IItemModel
    {
        private readonly IRepositoryHandler<T> _Handler;
        public BuiltinItemRepository(IRepositoryHandler<T> handler)
        {
            this._Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }
        public IActionResult<IEnumerable<R>> GetItems<R>(IItemParameterize parameters) where R : IItemModel, new()
        {
            if (parameters == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<R>>(nameof(parameters));
            }

            List<string> conditions = this.getConditions(parameters);

            if (conditions.Count == 0)
            {
                return ActionResultTemplates.ArgumentExceptionResult<IEnumerable<R>>("no condition is forbidden.");
            }

            string where = String.Join(" AND ", conditions.ToArray());

            string query1 =
$@"
SELECT DISTINCT YAEP_Item.* 
FROM YAEP_Item 
		    LEFT OUTER JOIN YAEP_Item_Category_Relation  ON YAEP_Item_Category_Relation.ItemUID = YAEP_Item.UID
		    LEFT OUTER JOIN YAEP_Item_Category ON YAEP_Item_Category.UID = YAEP_Item_Category_Relation.CategoryUID
            LEFT OUTER JOIN YAEP_Item_Properties ON YAEP_Item_Properties.ItemUID = YAEP_Item.UID
WHERE {where}
";
            string query2 =
$@"
SELECT  DISTINCT
            YAEP_Item_Properties.[UID], 
		    YAEP_Item_Properties.[ItemUID], 
		    YAEP_Item_Properties.[Name], 
		    YAEP_Item_Properties.[Value], 
		    YAEP_Item_Properties.[DataType]
FROM YAEP_Item_Properties
		    INNER JOIN [YAEP_Item] ON YAEP_Item.[UID] = YAEP_Item_Properties.ItemUID 
		    LEFT OUTER JOIN YAEP_Item_Category_Relation  ON YAEP_Item_Category_Relation.ItemUID = YAEP_Item.[UID]
		    LEFT OUTER JOIN YAEP_Item_Category ON YAEP_Item_Category.[UID] = YAEP_Item_Category_Relation.CategoryUID
WHERE {where} 
";

            var resultContainer = ActionResultTemplates.Result<IEnumerable<R>>();

            try
            {
                var gridReader = this._Handler.Instance.QueryMultiple($"{query1};{query2}", parameters);
                var itemCollection = gridReader.Read<ItemModel>();
                var itemProperties = gridReader.Read<ItemPropertiesModel>();
                //var itemCollection = this._Handler.Instance.Query<ItemModel>(query1, parameters);
                //var itemProperties = this._Handler.Instance.Query<ItemPropertiesModel>(query2, parameters);

                var list = new List<R>();

                foreach (var item in itemCollection)
                {
                    R r = this.Parse<R>(item, itemProperties);

                    list.Add(r);
                }

                if (list.Count == 0)
                {
                    resultContainer.Message = "Not Found.";
                    resultContainer.TypeCode = 404;
                }
                else
                {
                    resultContainer.Success = true;
                    // 排序
                    list = list.OrderBy(o => o.ID).ThenBy(o => o.CreatedOn).ToList();

                    resultContainer.Content = list;
                }
            }
            catch (Exception ex)
            {
                resultContainer.Message = "Error";
                resultContainer.InnerException = ex;
                resultContainer.TypeCode = 503;
            }

            return resultContainer;
        }
        private R Parse<R>(IItemModel item, IEnumerable<IItemPropertiesModel> properties) where R : IItemModel, new()
        {
            var r = new R();

            // Item
            var refItemProperties = typeof(ItemModel).GetCacheProperties();
            var refProperties = typeof(R).GetCacheProperties();
            foreach (var propertyInfo in refItemProperties)
            {
                refProperties.FirstOrDefault(p => p.Name.Equals(propertyInfo.Name))?.SetValue(r, propertyInfo.GetValue(item));
            }

            // Item itemExtendProperties
            var itemExtendProperties = properties.Where(o => o.ItemUID == item.UID);
            foreach (var itemProperty in itemExtendProperties)
            {
                var prop = refProperties.FirstOrDefault(p => p.Name.Equals(itemProperty.Name));

                if (prop == null)
                {
                    continue;
                }

                try
                {
                    var dataType = YAEP.Utilities.EnumerableData.Parse<ItemDataTypes>(itemProperty.DataType);

                    prop.SetValue(r, this.ConvertTo(dataType, itemProperty.Value, prop.PropertyType));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return r;
        }
        private List<string> getConditions(IItemParameterize parameters)
        {
            List<string> conditions = new List<string>();

            if (parameters == null)
            {
                return conditions;
            }

            var formatter = this._Handler.SqlFormatter;
            string tableName = "YAEP_Item";// this._Handler.GetTableName(true);

            if (parameters.UID.HasValue && parameters.UID != Guid.Empty)
            {
                conditions.Add($" ({tableName}.{formatter.Column(nameof(parameters.UID))} = @{nameof(parameters.UID)}) ");
            }
            else
            {
                if (parameters.ListOfItemUID?.Count() > 0)
                {
                    conditions.Add($" ({tableName}.{formatter.Column("UID")} IN @{nameof(parameters.ListOfItemUID)}) ");
                }

                if (parameters.ListOfGroupUID?.Count() > 0)
                {
                    if (parameters.GroupUID.HasValue && !(parameters.ListOfGroupUID?.Any(g => g == parameters.GroupUID.Value) ?? false))
                    {
                        parameters.ListOfGroupUID?.Add(parameters.GroupUID.Value);
                    }

                    conditions.Add($" ({tableName}.{formatter.Column(nameof(parameters.GroupUID))} IN {formatter.Parameter(nameof(parameters.ListOfGroupUID))}) ");
                }
                else if (parameters.GroupUID.HasValue)
                {
                    conditions.Add($" ({tableName}.{formatter.Column(nameof(parameters.GroupUID))} = {formatter.Parameter(nameof(parameters.GroupUID))}) ");
                }

                if (parameters.ListOfItemID?.Count() > 0)
                {
                    if (!String.IsNullOrWhiteSpace(parameters.ID) && !(parameters.ListOfItemID?.Any(g => g.Equals(parameters.ID, StringComparison.OrdinalIgnoreCase)) ?? false))
                    {
                        parameters.ListOfItemID?.Add(parameters.ID);
                    }

                    conditions.Add($" ({tableName}.{formatter.Column(nameof(parameters.ID))} IN {formatter.Parameter(nameof(parameters.ListOfItemID))}) ");
                }
                else if (!String.IsNullOrEmpty(parameters.ID))
                {
                    conditions.Add($" ({tableName}.{formatter.Column(nameof(parameters.ID))} LIKE {formatter.Parameter(nameof(parameters.ID))}+'%') ");
                }

                if (!String.IsNullOrEmpty(parameters.Name))
                {
                    conditions.Add($" ({tableName}.{formatter.Column(nameof(parameters.Name))} LIKE {formatter.Parameter(nameof(parameters.Name))} + '%') ");
                }

                if (parameters.Status.HasValue)
                {
                    conditions.Add($" ({tableName}.{formatter.Column(nameof(parameters.Status))} = {formatter.Parameter(nameof(parameters.Status))}) ");
                }
                else
                {
                    conditions.Add($" ({tableName}.{formatter.Column(nameof(parameters.Status))} = {(int)ItemStatus.Active}) ");
                }

                if (parameters.ItemCategories != null && parameters.ItemCategories.Count > 0)
                {
                    conditions.Add($" ({formatter.Table("YAEP_Item_Category")}.{formatter.Column("UID")} IN {formatter.Parameter(nameof(parameters.ItemCategories))}) ");
                }

                if (parameters.ItemProperties != null && parameters.ItemProperties.Count > 0)
                {
                    var listOfPropertyConditions = new List<string>();

                    foreach (var itemProperty in parameters.ItemProperties)
                    {
                        if (String.IsNullOrWhiteSpace(itemProperty.Name))
                        {
                            continue;
                        }

                        listOfPropertyConditions.Add(
$@"
SELECT [ItemUID]
FROM [YAEP_Item_Properties]
WHERE ([Name] = '{itemProperty.Name}') 
                AND ([Value]= '{itemProperty.Value}')
"
);

                    }
                    conditions.Add($" ({tableName}.{formatter.Column("UID")} IN ({String.Join("INTERSECT", listOfPropertyConditions)})) ");
                }
            }

            return conditions;
        }
        private object ConvertTo(ItemDataTypes dataType, string value, Type sourceType = null)
        {
            object result = null;
            switch (dataType)
            {
                case ItemDataTypes.BOOLEAN:
                    result = (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase));
                    break;
                case ItemDataTypes.DECIMAL:
                    result = Convert.ChangeType(value, typeof(decimal));
                    break;
                case ItemDataTypes.DOUBLE:
                    result = Convert.ChangeType(value, typeof(double));
                    break;
                case ItemDataTypes.FLOAT:
                    result = Convert.ChangeType(value, typeof(float));
                    break;
                case ItemDataTypes.INT16:
                    result = Convert.ChangeType(value, typeof(short));
                    break;
                case ItemDataTypes.INT32:
                    result = Convert.ChangeType(value, typeof(int));
                    break;
                case ItemDataTypes.INT64:
                    result = Convert.ChangeType(value, typeof(long));
                    break;
                case ItemDataTypes.GUID:
                    result = YAEP.Utilities.Utility.ToGuid(value);
                    break;
                default:
                    if (sourceType != null && sourceType == typeof(Guid))
                    {
                        result = YAEP.Utilities.Utility.ToGuid(value);
                    }
                    else
                    {
                        result = value;
                    }
                    break;
            }

            return result;
        }

        IActionResult<bool> IItemRepository.Create(IItemModel item)
        {
            throw new NotImplementedException();
        }

        IActionResult<bool> IItemRepository.Update(IItemModel item)
        {
            throw new NotImplementedException();
        }

        IActionResult<bool> IItemRepository.Delete(Guid UID)
        {
            throw new NotImplementedException();
        }

        IActionResult<IItemModel> IItemRepository.GetItem(Guid UID)
        {
            throw new NotImplementedException();
        }

        IActionResult<IItemModel> IItemRepository.GetItem(Guid groupUID, string id)
        {
            throw new NotImplementedException();
        }

        IActionResult<IEnumerable<IItemModel>> IItemRepository.GetItems(IItemParameterize parameters)
        {
            throw new NotImplementedException();
        }

        IActionResult<IEnumerable<IItemModel>> IItemRepository.GetItems(IEnumerable<Guid> itemUID)
        {
            throw new NotImplementedException();
        }

        IActionResult<IEnumerable<IItemModel>> IItemRepository.GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
