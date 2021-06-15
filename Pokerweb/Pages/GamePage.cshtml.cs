using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Pokerweb.Data;
using Pokerweb.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Dodělat css. Umístit tlačítka předchozí a následující. Dát tam ikony. Uspořádat karty hráčů.
/// Udělat něco na styl razítka s oznámením výsledků. Něco podobnýho s fold.
/// Pak vyřešít overflow:hidden na mobilu a zmenšit fonty a mělo by to být okk.
/// Hráč by měl vsadit vše, ale nejít pod nulu.
/// Měnění názvu tlačítek při hře.
/// </summary>

namespace Pokerweb.Pages
{
    public class GamePageModel : PageModel
    {
        public string Name { get; set; }
        public int Key { get; set; }
        public Room Room { get; set; }
        public Player Player { get; set; }

        public IActionResult OnGet(string key, string name)
        {
            Regex rgxNum = new Regex("^[0-9]*$");
            bool isNum = rgxNum.IsMatch(key);

            Name = name;
            Player player;

            if (isNum)
            {
                Key = Convert.ToInt32(key);
            }
            else
            {
                return RedirectToPage("Index");
            }

            if (RoomsDbContext.RoomsList.Find(x => x.KeyNumber == Key) != null)
            {
                player = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == Key).Players.Find(x => x.PlayerName == name);
            }
            else
            {
                return RedirectToPage("Index");
            }

            if (player == null)
            {
                return RedirectToPage("Index");
            }
            else if (player.Left == false)
            {
                return RedirectToPage("Index");
            }

            return Page();
        }

        public PartialViewResult OnGetPlayersPartial(string key, string name)
        {
            Key = Convert.ToInt32(key);
            Room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == Key);
            Room room = Room.ShallowCopy();
            room.PagePartialHelper = name;
            PartialViewResult _resultPartialPage = new PartialViewResult()
            {
                ViewName = "_PlayersPartial",
                ViewData = new ViewDataDictionary<Room>(ViewData, room),
            };
            return _resultPartialPage;
        }

        public void OnGetClose(string key, string name)
        {
            Key = Convert.ToInt32(key);
            Name = name;
            Player = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == Key).Players.Find(x => x.PlayerName == name);
            Player.InGame = false;
            Player.Left = true;
        }

    }
}