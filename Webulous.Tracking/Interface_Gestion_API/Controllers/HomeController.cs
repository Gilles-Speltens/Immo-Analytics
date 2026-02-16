using Interface_Gestion_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Interface_Gestion_API.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _client = new HttpClient();
        private static readonly IpListViewModel _ipList = new IpListViewModel();

        public async Task<IActionResult> Index()
        {
            if(!_ipList.IpV4.Any() && !_ipList.IpV6.Any())
            {
                var response = await _client.GetAsync("https://localhost:7042/TrackingDatas");

                if (response.IsSuccessStatusCode)
                {
                    RefreshListIp(response);
                }
            }

            return View(_ipList);
        }

        [HttpPost]
        public async Task<IActionResult> AddIp(string ip)
        {
            if(checkIpValidity(ip))
            {
                var json = JsonSerializer.Serialize(ip);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync("https://localhost:7042/TrackingDatas/AddIp", content);

                if (response.IsSuccessStatusCode)
                {
                    RefreshListIp(response);

                    // RedirectToAction pour éviter d'avoir des infos dans l'url.
                    return RedirectToAction("Index");
                }
            }

            TempData["Error"] = "Adresse IP invalide";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteIp(string ip)
        {
            if(checkIpValidity(ip))
            {
                var json = JsonSerializer.Serialize(ip);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync("https://localhost:7042/TrackingDatas/DeleteIp", content);

                if (response.IsSuccessStatusCode)
                {
                    RefreshListIp(response);
                }
            }
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async void RefreshListIp(HttpResponseMessage response)
        {
            var list = await JsonSerializer.DeserializeAsync<List<string>>(
                await response.Content.ReadAsStreamAsync()
            );

            _ipList.IpV4 = new List<string>();
            _ipList.IpV6 = new List<string>();
            foreach (var i in list)
            {
                // Check if ipv4
                if (i.IndexOf(":") == -1)
                {
                    var parts = i.Split('/');
                    if(parts.Length == 2 && Int32.Parse(parts[1]) == 32)
                    {
                        _ipList.IpV4.Add(parts[0]);
                    } else
                    {
                        _ipList.IpV4.Add(i);
                    }
                }
                else
                {
                    var parts = i.Split('/');
                    if (parts.Length == 2 && Int32.Parse(parts[1]) == 128)
                    {
                        _ipList.IpV6.Add(parts[0]);
                    }
                    else
                    {
                        _ipList.IpV6.Add(i);
                    }
                }
            }
        }

        private bool checkIpValidity(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            string[] parts = ip.Split('/');

            if (parts.Length == 1 || parts.Length == 2)
            {
                if (!IPAddress.TryParse(parts[0], out var address))
                {
                    return false;
                }

                if (parts.Length == 2)
                {
                    if (!int.TryParse(parts[1], out int mask))
                    {
                        return false;
                    }

                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (mask < 0 || mask > 32)
                        {
                            return false;
                        }
                    }
                    else if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        if (mask < 0 || mask > 128)
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
