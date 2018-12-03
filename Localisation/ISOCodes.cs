using System.Collections.Generic;

namespace Localisation
{
    public class ISOCodes
    {
        public static Dictionary<string, string> Mapper = new Dictionary<string, string>
        {
            {"dk", "dan"},
            {"da", "dan"}
        };

        /// <summary>
        /// Mapping ISO codes to database ISO codes
        /// </summary>
        public static Dictionary<string, string> CustomMapper = new Dictionary<string, string>
        {
            {"dan","dk"}
        };
    }
}
