using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacingOverlay
{
    public class CarLogo
    {

        public static string GetLogoUri(string carPath)
        {
            switch (carPath)
            {
                case string s when s.StartsWith("acura"):
                    return "acura-logo";
                case "amvantagegt4":
                    return "aston-martin-logo";
                case string s when s.StartsWith("audi"):
                    return "audi-logo";
                case string s when s.StartsWith("bmw"):
                    return "bmw-logo";
                case "cadillacvseriesrgtp":
                    return "cadillac-logo";
                case "chevyvettez06rgt3":
                    return "chevrolet-logo";
                case "dallarap217":
                    return "dallara-logo";
                case string s when s.StartsWith("ferrari"):
                    return "ferrari-logo";
                case "fordmustanggt3":
                    return "ford-logo";
                case string s when s.StartsWith("mclaren"):
                    return "mclaren-logo";
                case "ligierjsp320":
                    return "ligier-logo";
                case string s when s.StartsWith("mercedes"):
                    return "mercedes-logo";
                case string s when s.StartsWith("porsche"):
                    return "porsche-logo";
                default:
                    return "default-logo";
            }
        }
    }
}
