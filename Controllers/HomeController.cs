using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using redisCore.Models;

namespace redisCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDistributedCache _distributedCache;
        public HomeController(ILogger<HomeController> logger, IDistributedCache distributedCache)
        {
            _logger = logger;
            _distributedCache = distributedCache;

            Person person = new Person();
            person.Name = "Ömer Can";
            person.Surname = "Sucu";
            person.Age = 23;
            person.Gender = true;


            var data = JsonConvert.SerializeObject(person);
            _distributedCache.SetString("Person", data);
            /* 
             * var dataByte = Encoding.UTF8.GetBytes(data);
             * _distributedCache.Set("Person", dataByte);
             */

        }

        public async Task<IActionResult> Index()
        {
            var cacheKey = "Time";
            var existingTime = _distributedCache.GetString(cacheKey);
            if (string.IsNullOrEmpty(existingTime))
            {
                existingTime = DateTime.UtcNow.ToString();
                var option = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(5));
                option.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                string name = await _distributedCache.GetStringAsync("Name");
                await _distributedCache.SetStringAsync(cacheKey, $"{name}: {existingTime}", option);
            }
            ViewBag.Time = await _distributedCache.GetStringAsync(cacheKey);

            var personString = await _distributedCache.GetStringAsync("Person");
            var person = JsonConvert.DeserializeObject<Person>(personString);
            return View(person);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
