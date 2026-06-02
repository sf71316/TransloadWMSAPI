using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Utilities.Attributes;

namespace YAEP.WMS.Constant.Enums
{
    public enum PayloadTransactionLogTypes
    {
        //[EnumFieldInfo(Sort = 1)]
        //RECEIVING = 100,
        //[EnumFieldInfo(Sort = 2)]
        //PACK = 200,
        //[EnumFieldInfo(Sort = 3)]
        //Move = 300,
        //[EnumFieldInfo(Sort = 4)]
        //Modified_ONHAND = 301,
        //[EnumFieldInfo(Sort = 5)]
        //Inventory_COUNTING = 400,
        //[EnumFieldInfo(Sort = 6)]
        //Add_ONHAND = 500,
        //[EnumFieldInfo(Sort = 7)]
        //CHANGE_FROM_MODIFY_ONHAND = 600,

        [EnumFieldInfo(Sort = 8)]
        General_App_Receiving = 1101100,
        [EnumFieldInfo(Sort = 9)]
        General_App_Move = 1101200,
        [EnumFieldInfo(Sort = 10)]
        General_App_Pack = 1101300,
        [EnumFieldInfo(Sort = 11)]
        General_Web_Pick_All = 1102400,
        [EnumFieldInfo(Sort = 12)]
        General_Packing_Station_Pick_All = 1103400,
        [EnumFieldInfo(Sort = 13)]
        General_Exteral_Service_Pick_All = 1104400,
        [EnumFieldInfo(Sort = 13)]
        Transfer_App_Move = 1201200,
        [EnumFieldInfo(Sort = 14)]
        Transfer_App_Pack = 1201300,
        [EnumFieldInfo(Sort = 15)]
        Transfer_App_Receiving = 1201100,
        [EnumFieldInfo(Sort = 16)]
        Adjust_Web_Add_Inventory = 1302500,
        [EnumFieldInfo(Sort = 17)]
        Adjust_Web_Modified_Inventory = 1302510,
        [EnumFieldInfo(Sort = 18)]
        Adjust_Web_Modified_Inventory_Move_Ticket = 1302520,
        [EnumFieldInfo(Sort = 19)]
        Adjust_Web_Modified_Inventory_Change_Slot = 1302530,
        [EnumFieldInfo(Sort = 20)]
        Adjust_Web_Modified_Inventory_Change_Package = 1302540,
        [EnumFieldInfo(Sort = 21)]
        Adjust_Web_Change_Package_Slot = 1302550,
        [EnumFieldInfo(Sort = 22)]
        Adjust_Web_Modified_Inventory_Change_Package_Slot = 1302560,
        [EnumFieldInfo(Sort = 23)]
        Adjust_Web_Delete_Inventory = 1302570,
        [EnumFieldInfo(Sort = 24)]
        Adjust_Web_Change_Slot = 1302580,
        [EnumFieldInfo(Sort = 25)]
        Adjust_Web_Change_Package = 1302590,
        [EnumFieldInfo(Sort = 26)]
        Adjust_App_Add_Inventory = 1401500,
        [EnumFieldInfo(Sort = 27)]
        Adjust_App_Modified_Inventory = 1401510,
        [EnumFieldInfo(Sort = 28)]
        Adjust_App_Modified_Inventory_Move_Ticket = 1401520,
        [EnumFieldInfo(Sort = 29)]
        Adjust_App_Modified_Inventory_Change_Slot = 1401530,
        [EnumFieldInfo(Sort = 30)]
        Adjust_App_Modified_Inventory_Change_Package = 1401540,
        [EnumFieldInfo(Sort = 31)]
        Adjust_App_Change_Package_Slot = 1401550,
        [EnumFieldInfo(Sort = 32)]
        Adjust_App_Modified_Inventory_Change_Package_Slot = 1401560,
        [EnumFieldInfo(Sort = 33)]
        Adjust_App_Delete_Inventory = 1401570,
        [EnumFieldInfo(Sort = 34)]
        Adjust_App_Change_Slot = 1401580,
        [EnumFieldInfo(Sort = 35)]
        Adjust_App_Change_Package = 1401590,
        [EnumFieldInfo(Sort = 36)]
        Adjust_App_Return_Ticket_To_Open = 1102600,
        [EnumFieldInfo(Sort = 37)]
        Adjust_App_Add_Inventory_SetType = 1401610,
        [EnumFieldInfo(Sort = 38)]
        Adjust_App_Modified_Inventory_SetType = 1401620
    }
}
