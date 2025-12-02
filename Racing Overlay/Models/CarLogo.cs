namespace RacingOverlay
{
    public struct CarLogo
    {

        public static string GetLogoUri(string carPath)
        {
            switch (carPath.ToLower())
            {
                case string s when s.StartsWith("acura"):
                    return "acura-logo";
                case "amvantagegt4":
                    return "aston-martin-logo";
                case string s when s.StartsWith("audi"):
                    return "audi-logo";
                case string s when s.StartsWith("bmw"):
                    return "bmw-logo";
                case string s when s.StartsWith("cadillac"):
                    return "cadillac-logo";
                case "chevyvettez06rgt3":
                    return "chevrolet-logo";
                case string s when s.StartsWith("dallara"):
                    return "dallara-logo";
                case string s when s.StartsWith("ferrari"):
                    return "ferrari-logo";
                case string s when s.StartsWith("ford"):
                    return "ford-logo";
                case string s when s.StartsWith("lamborghini"):
                    return "lamborghini-logo";
                case string s when s.StartsWith("mclaren"):
                    return "mclaren-logo";
                case "mx5 mx52016":
                    return "mazda-logo";
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
