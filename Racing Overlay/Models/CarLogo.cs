namespace RacingOverlay
{
    public struct CarLogo
    {

        public static string GetLogoUri(string carPath)
        {
            switch (carPath.ToLower())
            {
                case string s when s.Contains("acura"):
                    return "acura-logo";
                case "amvantagegt4":
                    return "aston-martin-logo";
                case string s when s.Contains("audi"):
                    return "audi-logo";
                case string s when s.Contains("bmw"):
                    return "bmw-logo";
                case string s when s.Contains("cadillac"):
                    return "cadillac-logo";
                case "chevyvettez06rgt3":
                    return "chevrolet-logo";
                case string s when s.Contains("dallara"):
                    return "dallara-logo";
                case string s when s.Contains("ferrari"):
                    return "ferrari-logo";
                case string s when s.Contains("ford"):
                case "fr500s":
                    return "ford-logo";
                case string s when s.Contains("honda"):
                    return "honda-logo";
                case string s when s.Contains("hyundai"):
                    return "hyundai-logo";
                case string s when s.Contains("lamborghini"):
                    return "lamborghini-logo";
                case string s when s.Contains("mclaren"):
                    return "mclaren-logo";
                case "mx5 mx52016":
                    return "mazda-logo";
                case "ligierjsp320":
                    return "ligier-logo";
                case string s when s.Contains("mercedes"):
                    return "mercedes-logo";
                case string s when s.Contains("porsche"):
                    return "porsche-logo";
                case string s when s.Contains("toyota"):
                    return "toyota-logo";
                default:
                    return "default-logo";
            }
        }
    }
}
