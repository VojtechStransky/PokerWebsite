using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pokerweb.Data;
using Pokerweb.Models;
using System;
using System.Text.RegularExpressions;

namespace Pokerweb.Pages
{
    public class IndexModel : PageModel
    {


        public void OnGet()
        {

        }


        [BindProperty]
        public string KeyIn { get; set; }

        [BindProperty]
        public string NameIn { get; set; }
        public string Message { get; set; } = null;

        public int Key;


        public IActionResult OnPostNew()
        {
            Random random = new Random();
            Key = random.Next(100000, 999999);

            string N = Request.Form[nameof(NameIn)];

            Regex rgx = new Regex("^[a-zA-Z0-9À-ž_]*$");
            bool isOk = rgx.IsMatch(N);

            if ((N.Length > 0) && (N.Length < 25) && isOk)
            {
                RoomsDbContext.RoomsList.Add(new Room { KeyNumber = Key });
                RoomsDbContext.RoomsList.Find(x => x.KeyNumber == Key).AddPlayer(new Player { PlayerName = N, Founder = true });

                RoomsDbContext.DBFunction();

                return RedirectToPage("GamePage", new { key = Key, name = N });
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
            string Ks = Request.Form[nameof(KeyIn)];
            string N = Request.Form[nameof(NameIn)];
            int K;

            Regex rgx = new Regex("^[a-zA-Z0-9À-ž_]*$");
            bool isOk = rgx.IsMatch(N);

            Regex rgxNum = new Regex("^[0-9]*$");
            bool isNum = rgxNum.IsMatch(Ks);


            //is key correct?
            if (!isNum)
            {
                Message = "Klíč musí být číslo";
                return Page();
            }
            else if (Ks.Length == 6)
            {
                K = Convert.ToInt32(Ks);
            }
            else if (Ks.Length == 0)
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
            if (!(N.Length > 0))
            {
                Message = "jméno musí být zadáno";
                return Page();
            }
            else if (N.Length > 25)
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
            if (IsInDatabase(K) != true)
            {
                Message = "klíč neexistuje";
                return Page();
            }
            else if (AlreadyUsed(K, N) != false)
            {
                Message = "jméno již bylo použito";
                return Page();
            }
            else if (RoomsDbContext.RoomsList.Find(x => x.KeyNumber == K).InGame != false)
            {
                Message = "místonst je ve hře";
                return Page();
            }
            else if (RoomsDbContext.RoomsList.Find(x => x.KeyNumber == K).Players.Count > 12)
            {
                Message = "místonst je už plně obsazena";
                return Page();
            }

            RoomsDbContext.RoomsList.Find(x => x.KeyNumber == K).AddPlayer(new Player { PlayerName = N });

            return RedirectToPage("GamePage", new { key = K, name = N });
        }

        private bool AlreadyUsed(int K, string N)
        {
            if (RoomsDbContext.RoomsList.Find(x => x.KeyNumber == K).Players.Find(x => x.PlayerName == N) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsInDatabase(int K)
        {
            if (RoomsDbContext.RoomsList.Find(x => x.KeyNumber == K) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}