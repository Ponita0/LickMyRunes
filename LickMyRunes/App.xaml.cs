using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace LickMyRunes
{
    public partial class App : Application
    {
        internal static string LickMyRunesTitle => "LickMyRunes " + utils.Version;
        public static int port;
        public static string token;
        public static Process proc;
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }      
       

    }
}
