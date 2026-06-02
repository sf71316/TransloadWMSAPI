using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class BoardRedisController : AbstractDefaultConnectSettingController<Board>
    {
        public BoardRedisController() : base(o => o.ID)
        {
            this.AppendIndex(nameof(Board.IsLoaded), o => o.IsLoaded);
            this.AppendIndex(nameof(Board.ExpiresAt), o => o.ExpiresAt); 
        }

        public void SetLoaded(string key, bool isLoaded)
        {
            this.Delete(key);
            this.Create(new Board()
            {
                ID = key,
                IsLoaded = true
            });

        }

 

    }

}
