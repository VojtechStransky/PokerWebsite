using System;
using System.Collections.Generic;

namespace Pokerweb.Data
{
    public static class RoomsDbContext
    {
        public static List<Models.Room> RoomsList = new List<Models.Room>();
        public static DateTime lastTime { get; set; } = DateTime.UtcNow;
        public static void DBFunction()
        {
            if (((DateTime.Now - lastTime).Hours > 1) && RoomsList.Count > 20)
            {
                foreach (var x in RoomsList)
                {
                    if ((DateTime.Now - x.TimeStamp).Minutes > 10)
                    {
                        RoomsList.Remove(x);
                    }
                }
            }

            lastTime = DateTime.UtcNow;
        }
    }
}