using System;

namespace Localisation
{
    public class LocaliseConfig
    {
        public string URL { get; set; }
        public string Token { get; set; }
        public string ProjectId { get; set; }

        public void Verify()
        {
            if (string.IsNullOrWhiteSpace(URL))
                throw new ArgumentNullException(nameof(URL));
            if (string.IsNullOrWhiteSpace(Token))
                throw new ArgumentNullException(nameof(Token));
            if (string.IsNullOrWhiteSpace(ProjectId))
                throw new ArgumentNullException(nameof(ProjectId));
        }
    }
}
