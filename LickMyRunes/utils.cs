using HtmlAgilityPack;
using LickMyRunes;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using MessageBox = System.Windows.Forms.MessageBox;

namespace LickMyRunes
{
    public class utils
    {
        public enum Champion
        {
            NONE = -1,
            ANNIE = 1,
            OLAF = 2,
            GALIO = 3,
            TWISTEDFATE = 4,
            XINZHAO = 5,
            URGOT = 6,
            LEBLANC = 7,
            VLADIMIR = 8,
            FIDDLESTICKS = 9,
            KAYLE = 10,
            MASTERYI = 11,
            ALISTAR = 12,
            RYZE = 13,
            SION = 14,
            SIVIR = 15,
            SORAKA = 16,
            TEEMO = 17,
            TRISTANA = 18,
            WARWICK = 19,
            nunu = 20,
            MISSFORTUNE = 21,
            ASHE = 22,
            TRYNDAMERE = 23,
            JAX = 24,
            MORGANA = 25,
            ZILEAN = 26,
            SINGED = 27,
            EVELYNN = 28,
            TWITCH = 29,
            KARTHUS = 30,
            CHOGATH = 31,
            AMUMU = 32,
            RAMMUS = 33,
            ANIVIA = 34,
            SHACO = 35,
            DRMUNDO = 36,
            SONA = 37,
            KASSADIN = 38,
            IRELIA = 39,
            JANNA = 40,
            GANGPLANK = 41,
            CORKI = 42,
            KARMA = 43,
            TARIC = 44,
            VEIGAR = 45,
            TRUNDLE = 48,
            SWAIN = 50,
            CAITLYN = 51,
            BLITZCRANK = 53,
            MALPHITE = 54,
            KATARINA = 55,
            NOCTURNE = 56,
            MAOKAI = 57,
            RENEKTON = 58,
            JARVANIV = 59,
            ELISE = 60,
            ORIANNA = 61,
            WUKONG = 62,
            BRAND = 63,
            LEESIN = 64,
            VAYNE = 67,
            RUMBLE = 68,
            CASSIOPEIA = 69,
            SKARNER = 72,
            HEIMERDINGER = 74,
            NASUS = 75,
            NIDALEE = 76,
            UDYR = 77,
            POPPY = 78,
            GRAGAS = 79,
            PANTHEON = 80,
            EZREAL = 81,
            MORDEKAISER = 82,
            YORICK = 83,
            AKALI = 84,
            KENNEN = 85,
            GAREN = 86,
            LEONA = 89,
            MALZAHAR = 90,
            TALON = 91,
            RIVEN = 92,
            KOGMAW = 96,
            SHEN = 98,
            LUX = 99,
            XERATH = 101,
            SHYVANA = 102,
            AHRI = 103,
            GRAVES = 104,
            FIZZ = 105,
            VOLIBEAR = 106,
            RENGAR = 107,
            VARUS = 110,
            NAUTILUS = 111,
            VIKTOR = 112,
            SEJUANI = 113,
            FIORA = 114,
            ZIGGS = 115,
            LULU = 117,
            DRAVEN = 119,
            HECARIM = 120,
            KHAZIX = 121,
            DARIUS = 122,
            JAYCE = 126,
            LISSANDRA = 127,
            DIANA = 131,
            QUINN = 133,
            SYNDRA = 134,
            AURELIONSOL = 136,
            KAYN = 141,
            ZOE = 142,
            ZYRA = 143,
            KAISA = 145,
            SERAPHINE = 147,
            GNAR = 150,
            ZAC = 154,
            YASUO = 157,
            VELKOZ = 161,
            TALIYAH = 163,
            CAMILLE = 164,
            BRAUM = 201,
            JHIN = 202,
            KINDRED = 203,
            JINX = 222,
            TAHMKENCH = 223,
            VIEGO = 234,
            SENNA = 235,
            LUCIAN = 236,
            ZED = 238,
            KLED = 240,
            EKKO = 245,
            QIYANA = 246,
            VI = 254,
            AATROX = 266,
            NAMI = 267,
            AZIR = 268,
            YUUMI = 350,
            SAMIRA = 360,
            THRESH = 412,
            ILLAOI = 420,
            REKSAI = 421,
            IVERN = 427,
            KALISTA = 429,
            BARD = 432,
            RAKAN = 497,
            XAYAH = 498,
            ORNN = 516,
            SYLAS = 517,
            NEEKO = 518,
            APHELIOS = 523,
            RELL = 526,
            PYKE = 555,
            YONE = 777,
            SETT = 875,
            LILLIA = 876,
            GWEN = 887
        }

        public static async void pushRunes(int[] RuneIDs, int PrimaryTree, int SecondaryTree, string name)
        {
            try
            {
                string body = Newtonsoft.Json.JsonConvert.SerializeObject(new { name = name, primaryStyleId = PrimaryTree, subStyleId = SecondaryTree, selectedPerkIds = RuneIDs, current = true });
                var reqResponse = MainWindow.LeagueClient.Request("POST", "/lol-perks/v1/pages", body).Result.Content.ReadAsStringAsync().Result;
                if (reqResponse.Contains("Max pages reached"))
                {
                    var CurrentPage = MainWindow.LeagueClient.Request("GET", "/lol-perks/v1/currentpage", null);
                    dynamic myData = JObject.Parse(CurrentPage.Result.Content.ReadAsStringAsync().Result);
                    var deletePage = MainWindow.LeagueClient.Request("DELETE", "/lol-perks/v1/pages/" + (int)myData["id"], null);
                    await MainWindow.LeagueClient.Request("POST", "/lol-perks/v1/pages", body);
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private static string GetRoleUrl(int championId, string position)
        => $"https://eune.op.gg/champion/{Enum.GetName(typeof(Champion), championId)}/statistics/{position}";
        public static async Task<runepage> GetRunePage(int championId, string position, bool isARAM)
        {
            using (WebClient web1 = new WebClient())
            {
                string data = "";
                if (isARAM)
                {
                    data = web1.DownloadString($"https://op.gg/aram/{Enum.GetName(typeof(Champion), championId)}/statistics/");
                }
                else
                {
                    data = web1.DownloadString($"https://eune.op.gg/champion/{Enum.GetName(typeof(Champion), championId)}/statistics/{position}");
                }

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data);
                var pages = doc.DocumentNode.Descendants().Where(o => o.HasClass("perk-page")).Take(2);
                Debug.WriteLine(pages);
                int[][] pageRows = pages.SelectMany(o => o.Descendants().Select(ParseRow)).Select(o => o.ToArray()).Where(o => o.Length > 0).ToArray();
                var perks = pageRows.Where((_, i) => i != 0 && i != 5).SelectMany(o => o).ToList();
                var fragments = doc.DocumentNode.Descendants().Where(o => o.HasClass("fragment__row")).Take(3);
                perks.AddRange(fragments.Select(o =>
                {
                    var src = o.Descendants().Single(i => i.HasClass("tip") && i.HasClass("active")).GetAttributeValue("src", "");
                    return int.Parse(Regex.Match(src, @"(?<=perkShard\/).*?(?=\.png)").Value);
                }));
                return new runepage(perks.ToArray(), pageRows[0][0], pageRows[5][0]);
            }
        }
        private static IEnumerable<int> ParseRow(HtmlNode row)
        {
            foreach (var item in row.Descendants().Where(o => o.HasClass("perk-page__item--mark")
                                                              || o.HasClass("perk-page__item--active")))
            {
                var img = item.Descendants().First(o => o.Name == "img" && o.HasClass("tip"));

                yield return int.Parse(Regex.Match(img.GetAttributeValue("src", ""), @"(?<=\/)\d+(?=\.)").Value);
            }
        }
        public static async Task<int> GetCurrentChamp()
        {
            var champ = (await MainWindow.LeagueClient.Request("GET", "/lol-champ-select/v1/current-champion", null));
            return int.Parse(champ.Content.ReadAsStringAsync().Result);
        }
        internal static string Version
        {
            get
            {
                var version = System.Reflection.Assembly.GetEntryAssembly()!.GetName().Version;
                return "v" + version.Major + "." + version.Minor + "." + version.Build;
            }
        }
        public static async Task CheckForUpdates()
        {
            try
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.UserAgent.Add(
                    new ProductInfoHeaderValue("LickMyRunes", Version));
                var response =
                    await httpClient.GetAsync("https://api.github.com/repos/Ponita0/LickMyRunes/releases/latest");
                var content = await response.Content.ReadAsStringAsync();
                if (content.Contains("API rate limit exceeded"))
                {
                    MessageBox.Show("sorry but you rly opened the app too much times in a small amount of time , u need to chill ");
                }
                dynamic release = SimpleJson.DeserializeObject(content);
                string latestVersion = release["tag_name"];

                // If failed to fetch or already latest or newer, return.
                if (latestVersion == null) return;
                var githubVersion = new Version(latestVersion.Replace("v", ""));
                var assemblyVersion = new Version(Version.Replace("v", ""));
                // Earlier = -1, Same = 0, Later = 1
                if (assemblyVersion.CompareTo(githubVersion) != -1) return;

                var result = MessageBox.Show(
                    $"There is a new version of LickMyRunes available: {latestVersion}.\n" +
                    $" You are currently using LickMyRunes {Version}.\n" +
                    $"LMR updates usually fix critical bugs or adapt to changes by Riot, so it is recommended that you install the latest version.\n\n" +
                    $"Press OK to visit the download page, or press Cancel to visit the download page too :)",
                    App.LickMyRunesTitle,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1
                );

                if (result == DialogResult.OK)
                {
                    // Open the url in the browser.
                    //Process.Start("https://github.com/Ponita0/LickMyRunes");
                    Process.Start(release["html_url"]);
                    Environment.Exit(0);
                }
                else
                {
                    Process.Start(release["html_url"]);
                    Environment.Exit(0);
                }
            }
            catch
            {
                // Ignored.
            }
        }
    }
    public static class WindowsSecurity
    {
        public static bool isAdminStrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
         .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}

