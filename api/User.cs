using System.Collections.Generic;

namespace steam_friendlist_crawler.api
{
    public class User
    {
        public ulong id { get; set; }
        public List<Friend> friends { get; set; }
    }
}