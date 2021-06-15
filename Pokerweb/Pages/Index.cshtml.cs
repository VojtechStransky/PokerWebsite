using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pokerweb.Data;
using Pokerweb.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pokerweb.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public string KeyIn { get; set; }
        [BindProperty]
        public string NameIn { get; set; }
        public string Message { get; set; } = null;
        public int Key { get; set; }

        public IActionResult OnPostNew()
        {
            List<Room> rooms = RoomsDbContext.RoomsList;
            Random random = new Random();
            Key = random.Next(100000, 999999);

            string N = Request.Form[nameof(NameIn)];

            Regex rgx = new Regex("^[a-zA-Z0-9À-ž_]*$");
            bool isOk = rgx.IsMatch(N);

            if ((N.Length > 0) && (N.Length < 25) && isOk)
            {
                rooms.Add(new Room { KeyNumber = Key });
                rooms.Find(x => x.KeyNumber == Key).AddPlayer(new Player { PlayerName = N, Founder = true });

                RoomsDbContext.DBFunction();

                return RedirectToPage("GamePage", new { _key = Key, name = N });
            }
            else if (!(N.Length > 0))
            {
                Message = "jméno musí být zadáno";

                return Page();
            }
            else if (N.Length > 25)
            {
                Message = "jméno musí být kratší 25 znaků";

                return Page();
            }
            else
            {
                Message = "jméno musí obsahovat pouze písmena, čislice a _";

                return Page();
            }
        }
        public IActionResult OnPostIn()
        {
            Room room = new Room();
            string _key = Request.Form[nameof(KeyIn)];
            string name = Request.Form[nameof(NameIn)];
            int key;

            Regex rgx = new Regex("^[a-zA-Z0-9À-ž_]*$");
            bool isOk = rgx.IsMatch(name);

            Regex rgxNum = new Regex("^[0-9]*$");
            bool isNum = rgxNum.IsMatch(_key);

            //is key correct?
            if (!isNum)
            {
                Message = "Klíč musí být číslo";
                return Page();
            }
            else if (_key.Length == 6)
            {
                key = Convert.ToInt32(_key);
                room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);
            }
            else if (_key.Length == 0)
            {
                Message = "Klíč musí být zadán";
                return Page();
            }
            else
            {
                Message = "Klíč musí mít 6 míst";
                return Page();
            }

            //is name correct?
            if (name.Length <= 0)
            {
                Message = "jméno musí být zadáno";
                return Page();
            }
            else if (name.Length > 25)
            {
                Message = "jméno musí být kratší 25 znaků";
                return Page();
            }
            else if (!isOk)
            {
                Message = "jméno musí obsahovat pouze písmena, čislice a _";
                return Page();
            }

            //is interaction with room correct?
            if (IsInDatabase(key) != true)
            {
                Message = "klíč neexistuje";
                return Page();
            }
            else if (AlreadyUsed(key, name) != false)
            {
                Message = "jméno již bylo použito";
                return Page();
            }
            else if (room.InGame != false)
            {
                Message = "místonst je ve hře";
                return Page();
            }
            else if (room.Players.Count > 12)
            {
                Message = "místonst je už plně obsazena";
                return Page();
            }

            room.AddPlayer(new Player { PlayerName = name });

            return RedirectToPage("GamePage", new { _key = key, name = name });
        }

        private bool AlreadyUsed(int key, string name)
        {
            Player player = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key).Players.Find(x => x.PlayerName == name);
            return (player != null);
        }

        private bool IsInDatabase(int K)
        {
            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == K);
            return (room != null);
        }
    }
}