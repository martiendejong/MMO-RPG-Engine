using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Game_Engine;
using Microsoft.AspNet.Identity;

namespace MMO_RPG_Engine.Controllers
{
    public class CharacterController : Controller
    {
        protected ICharacterRepository CharacterRepository;

        public CharacterController(ICharacterRepository characterRepository)
        {
            CharacterRepository = characterRepository;
        }

        // GET: Character
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Character character)
        {
            if (ModelState.IsValid)
            {
                character.PlayerId = User.Identity.GetUserId();
                //character.X = 1;
                //character.Y = 2;
                CharacterRepository.Create(character);
                return RedirectToAction("Index");
            }

            return View(character);
        }
    }
}