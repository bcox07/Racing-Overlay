using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRacing_Standings
{
    public class CarLogo
    {

        public static string GetLogoUri(string carPath)
        {
            switch (carPath)
            {
                case "acuraarx06gtp":
                case "acuransxevo22gt3":
                    return "acura-logo";
                case "amvantagegt4":
                    return "aston-martin-logo";
                case "audir8gt3":
                case "audir8lmsevo2gt3":
                    return "audi-logo";
                case "bmwm4evogt4":
                case "bmwlmdh":
                case "bmwm4gt3":
                    return "bmw-logo";
                case "cadillacvseriesrgtp":
                    return "cadillac-logo";
                case "chevyvettez06rgt3":
                    return "chevrolet-logo";
                case "dallarap217":
                    return "dallara-logo";
                case "ferrari296gt3":
                case "ferrari499p":
                    return "ferrari-logo";
                case "fordmustanggt3":
                    return "ford-logo";
                case "mclaren570sgt4":
                case "mclaren720sgt3":
                    return "mclaren-logo";
                case "ligierjsp320":
                    return "ligier-logo";
                case "mercedesamggt4":
                case "mercedesamgevogt3":
                    return "mercedes-logo";
                case "porsche718gt4":
                case "porsche992rgt3":
                case "porsche963gtp":
                    return "porsche-logo";
                default:
                    return "default-logo";
            }
        }
    }
}
