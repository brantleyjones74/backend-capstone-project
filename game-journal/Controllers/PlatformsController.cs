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
using Newtonsoft.Json;

namespace game_journal.Controllers
{
    public class PlatformsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApplicationDbContext _context;

        public PlatformsController(ApplicationDbContext context, IHttpClientFactory clientFactory)
        {
            _context = context;
            _clientFactory = clientFactory;
        }

        // Get all platforms and add to local DB.
        public async Task<IActionResult> Index()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/platforms?fields=name,id&limit=200");
            var client = _clientFactory.CreateClient("igdb");
            var response = await client.SendAsync(request);
            var platformsAsJson = await response.Content.ReadAsStringAsync();
            var deserializedPlatforms = JsonConvert.DeserializeObject<List<Platform>>(platformsAsJson);

            List<Platform> platformsFromApi = new List<Platform>();

            foreach (var platformObj in deserializedPlatforms)
            {
                Platform newPlatform = new Platform
                {
                    ApiPlatformId = platformObj.ApiPlatformId,
                    Name = platformObj.Name
                };

                if (!PlatformExists(platformObj.ApiPlatformId))
                {

                    if (ModelState.IsValid)
                    {
                        _context.Add(newPlatform);
                        await _context.SaveChangesAsync();
                    }

                }
                platformsFromApi.Add(newPlatform);
            }
            return Redirect("/Games/Index");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var platform = await _context.Platforms
                .FirstOrDefaultAsync(m => m.PlatformId == id);
            if (platform == null)
            {
                return NotFound();
            }

            return View(platform);
        }

        private bool PlatformExists(int id)
        {
            return _context.Platforms.Any(e => e.ApiPlatformId == id);
        }
    }
}
