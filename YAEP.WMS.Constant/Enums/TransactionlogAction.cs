using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum TransactionlogAction
    {
        Receiving = 100,
        Move = 200,
        Pack = 300,
        PickAll = 400,
        AddInventory = 500,
        ModifiedInventory = 510,
        ModifiedInventoryMoveTicket = 520,
        ModifiedInventoryChangeSlot = 530,
        ModifiedInventoryChangePackage = 540,
        ChangePackageSlot = 550,
        ModifiedInventoryChangePackageSlot = 560,
        DeleteInventory = 570,
        ChangeSlot = 580,
        ChangePackage = 590,
        ReturnTicketToOpen = 600,
        AddInventorySetType = 610,
        ModifiedInventorySetType = 620,
    }
}
