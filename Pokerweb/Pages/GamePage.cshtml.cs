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
        public int Key { get; set; }
        public string Name { get; set; }

        public IActionResult OnGet(string _key, string name)
        {
            Name = name;
            Regex rgxNum = new Regex("^[0-9]*$");
            bool isNum = rgxNum.IsMatch(_key);
            Room room = new Room();
            List<Player> players = new List<Player>();
            Player player = new Player();

            if (isNum)
            {
                Key = Convert.ToInt32(_key);
                room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == Key);
            }
            else
            {
                return RedirectToPage("Index");
            }

            if (room != null)
            {
                players = room.Players;
                player = players.Find(x => x.PlayerName == Name);
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
            int _key = Convert.ToInt32(key);
            Room roomOrig = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == _key);
            Room room = roomOrig.ShallowCopy();
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
            int _key = Convert.ToInt32(key);
            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == _key);
            Player player = room.Players.Find(x => x.PlayerName == name);
            player.InGame = false;
            player.Left = true;
        }

    }
}