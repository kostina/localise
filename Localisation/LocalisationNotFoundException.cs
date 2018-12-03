using System;

namespace Localisation
{
    public class LocalisationNotFoundException : Exception
    {
        public LocalisationNotFoundException(string message, string slug) : base(message)
        {
            Slug = slug;
        }

        public string Slug { get; set; }
        
    }
}
