﻿using System.Collections.Generic;
using System.Xml.Linq;

namespace PastebinAPI
{
    public class User
    {
        private readonly string userKey;

        internal User(string userKey)
        {
            this.userKey = userKey;
            RequestPreferences();
        }

        public string Name { get; private set; }
        public Language PreferedLanguage { get; private set; }
        public Expiration PreferedExpiration { get; private set; }
        public string AvatarURL { get; private set; }
        public Visibility PreferedVisibility { get; private set; }
        public string Website { get; private set; }
        public string Email { get; private set; }
        public string Location { get; private set; }
        public bool IsPro { get; private set; }

        /// <summary>
        /// Creates a new paste from this user and uploads it to pastebin.
        /// To create anonymous paste use Paste.Create() or User.Guest.CreatePaste()
        /// </summary>
        /// <param name="language">If left out then user's PreferedLanguage will be used</param>
        /// <param name="visibility">If left out then user's PreferedVisibility will be used</param>
        /// <param name="expiration">If left out then user's PreferedExpiration will be used</param>
        /// <returns>Paste object containing the Url given from Pastebin</returns>
        public Paste CreatePaste(string text, string title = "", Language language = null, Visibility? visibility = null, Expiration expiration = null)
        {
            return Paste.Create(userKey, text, title, language ?? PreferedLanguage, visibility ?? PreferedVisibility, expiration ?? PreferedExpiration);
        }

        public IEnumerable<Paste> ListPastes(int resultsLimit = 50)
        {
            var result = Utills.PostRequest(Utills.URL_API,
                                            "api_dev_key=" + Pastebin.DevKey,
                                            "api_user_key=" + userKey,
                                            "api_results_limit=" + resultsLimit,
                                            "api_option=" + "list");

            if (result.Contains(Utills.ERROR))
                throw new PastebinException(result);

            return Utills.PastesFromXML(result);
        }

        public void DeletePaste(Paste paste)
        {
            var result = Utills.PostRequest(Utills.URL_API,
                                            "api_dev_key=" + Pastebin.DevKey,
                                            "api_user_key=" + userKey,
                                            "api_paste_key=" + paste.Key,
                                            "api_option=" + "delete");

            if (result.Contains(Utills.ERROR))
                throw new PastebinException(result);
        }

        public void RequestPreferences()
        {
            var result = Utills.PostRequest(Utills.URL_API,
                                            "api_dev_key=" + Pastebin.DevKey,
                                            "api_user_key=" + userKey,
                                            "api_option=" + "userdetails");

            if (result.Contains(Utills.ERROR))
                throw new PastebinException(result);

            /* Example user xml
             <user>
                 <user_name>wiz_kitty</user_name>
                 <user_format_short>text</user_format_short>
                 <user_expiration>N</user_expiration>
                 <user_avatar_url>http://pastebin.com/cache/a/1.jpg</user_avatar_url>
                 <user_private>1</user_private> (0 Public, 1 Unlisted, 2 Private)
                 <user_website>http://myawesomesite.com</user_website>
                 <user_email>oh@dear.com</user_email>
                 <user_location>New York</user_location>
                 <user_account_type>1</user_account_type> (0 normal, 1 PRO)
             </user>*/
            XElement xuser = XDocument.Parse(result).Element("user");
            Name = xuser.Element("user_name").Value;
            PreferedLanguage = Language.Parse(xuser.Element("user_format_short").Value);
            PreferedExpiration = Expiration.Parse(xuser.Element("user_expiration").Value);
            AvatarURL = xuser.Element("user_avatar_url").Value;
            PreferedVisibility = (Visibility)int.Parse(xuser.Element("user_private").Value);
            Website = xuser.Element("user_website").Value;
            Email = xuser.Element("user_email").Value;
            Location = xuser.Element("user_location").Value;
            IsPro = int.Parse(xuser.Element("user_account_type").Value) == 1;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
