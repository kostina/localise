using System;

namespace Localisation
{
    public class CacheKey
    {
        public string Slug;
        public string ISOCOde;

        public CacheKey(string slug, string iso)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                throw new ArgumentNullException(nameof(slug));
            }

            if (string.IsNullOrWhiteSpace(iso))
            {
                throw new ArgumentNullException(nameof(iso));
            }

            Slug = slug;
            ISOCOde = iso;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(CacheKey))
            {
                return false;
            }

            var other = (CacheKey)obj;
            if (string.IsNullOrWhiteSpace(other.Slug) || string.IsNullOrWhiteSpace(other.ISOCOde))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(Slug) || string.IsNullOrWhiteSpace(ISOCOde))
            {
                return false;
            }

            if (other.Slug.ToLower() == Slug.ToLower() && other.ISOCOde.ToLower() == ISOCOde.ToLower())
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Slug.GetHashCode() ^ ISOCOde.GetHashCode();
        }
    }
}
