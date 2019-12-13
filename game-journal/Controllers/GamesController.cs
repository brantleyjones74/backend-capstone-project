﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using game_journal.Data;
using game_journal.Models;
using System.Net.Http;
using System.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace game_journal.Controllers
{
    public class GamesController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);


        public GamesController(ApplicationDbContext context, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _context = context;
        }
        // GET: Games
        public async Task<IActionResult> IndexAsync() // returns a list of games from DB
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/games?fields=*");
            var client = _clientFactory.CreateClient("igdb");
            var response = await client.SendAsync(request);
            var gamesAsJson = await response.Content.ReadAsStringAsync();
            var deserializedGames = JsonConvert.DeserializeObject<List<Game>>(gamesAsJson);

            List<Game> gamesFromApi = new List<Game>();

            foreach (var game in deserializedGames)
            {
                Game newGame = new Game
                {
                    GameId = game.GameId,
                    Name = game.Name,
                };
                gamesFromApi.Add(newGame);
            }

            return View(gamesFromApi);
        }

        // GET: Search Games By Name
        public async Task<IActionResult> SearchByName(string gameName)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"games?search={gameName}&fields=id,name,genres");
            var client = _clientFactory.CreateClient("igdb");
            var response = await client.SendAsync(request);
            var gamesAsJson = await response.Content.ReadAsStringAsync();
            var deserializedGames = JsonConvert.DeserializeObject<List<Game>>(gamesAsJson);

            List<Game> searchedGame = new List<Game>();

            foreach (var game in deserializedGames)
            {
                Game newGame = new Game
                {
                    GameId = game.GameId,
                    Name = game.Name,
                    GenreIds = game.GenreIds
                };
                searchedGame.Add(newGame);
            }

            return View(searchedGame);

        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(string id) // gets detail of selected game
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api-v3.igdb.com/games?fields=*&filter[id][eq]={id}");
            var client = _clientFactory.CreateClient("igdb");
            var response = await client.SendAsync(request);
            var gamesAsJson = await response.Content.ReadAsStringAsync();
            var deserializedGame = JsonConvert.DeserializeObject<List<Game>>(gamesAsJson);

            List<Game> gameFromApi = new List<Game>();

            foreach (var game in deserializedGame)
            {
                Game newGame = new Game
                {
                    GameId = game.GameId,
                    Name = game.Name,
                    ReleaseDate = game.ReleaseDate,

                };
                gameFromApi.Add(newGame);
            }

            Game singleGameFromApi = gameFromApi[0];

            return View(singleGameFromApi);
        }

        public async Task<IActionResult> SaveGame(int id)
        {
            // save id in a gameId variable
            var gameId = id;
            // get user 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // gets user id
            var game = _context.Games.Where(g => g.GameId == gameId);
            var newGame = new Game
            {
                UserId = userId
            };

            _context.Add(newGame);
            await _context.SaveChangesAsync();
            
            return View();
        }
        // GET: Games/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        // POST: Games/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("GameId,Name")] Game game)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(game);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(game);
        //}

        // GET: Games/Edit/5
        //public async Task<IActionResult> Edit(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var game = await _context.Games.FindAsync(id);
        //    if (game == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(game);
        //}

        // POST: Games/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(string id, [Bind("GameId,Name")] Game game)
        //{
        //    if (id != game.GameId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(game);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!GameExists(game.GameId))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(game);
        //}

        // GET: Games/Delete/5
        //public async Task<IActionResult> Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var game = await _context.Games
        //        .FirstOrDefaultAsync(m => m.GameId == id);
        //    if (game == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(game);
        //}

        // POST: Games/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(string id)
        //{
        //    var game = await _context.Games.FindAsync(id);
        //    _context.Games.Remove(game);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool GameExists(string id)
        //{
        //    return _context.Games.Any(e => e.GameId == id);
        //}
    }
}
