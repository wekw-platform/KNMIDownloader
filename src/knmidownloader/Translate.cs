using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace knmidownloader
{
    public class Translate
    {
        public static string TranslateArchToRespected(string arch)
        {
            string archToLower = arch.ToLower();
            switch (archToLower)
            {
                case "x86":
                    return "x86";
                case "x64":
                    return "x86-64";
                case "arm":
                    return "arm32";
                case "arm64":
                    return "aarch64";
                default:
                    return archToLower;
            }
        }
    }
}
