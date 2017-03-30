using System;
using System.Threading.Tasks;
using LiteDB;
using steam_friendlist_crawler.api;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;

namespace steam_friendlist_crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine("You need to pass a valid steamkey");
                return;
            }
            LiteDatabase database = new LiteDatabase("database.db");
            var pendingCollection = database.GetCollection<User>("pendingusers");
            var usersCollection = database.GetCollection<User>("users");

            var task = MainAsync(args[0]);
            task.Wait();
            Console.ReadKey();
        }

        private static async Task MainAsync(string steamkey)
        {
            LiteDatabase database = new LiteDatabase("database.db");
            var pendingCollection = database.GetCollection<User>("pendingusers");
            var usersCollection = database.GetCollection<User>("users");
            if (!pendingCollection.Exists(x => x.id == 76561198107082789L))
            {
                pendingCollection.Insert(new User { id=76561198107082789L});
            }
            var api = new SteamFriendsApi(steamkey);

            while(true)
            {
                IEnumerable<ulong> pending = pendingCollection.FindAll().Take(8).Select(x => x.id);
                if (pending.Count() == 0)
                {
                    break;
                }
                List<Task<User>> tasks = new List<Task<User>>();
                foreach (var p in pending)
                {
                    tasks.Add(api.GetFriends(p));
                }
                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (AggregateException)
                {
                    System.Console.WriteLine("Requisição rejeitada.");
                    System.Console.WriteLine("Esperando um pouco para tentar novamente");
                    await Task.Delay(2000);
                    continue;
                }
                foreach (var t in tasks)
                {
                    User user = t.Result;
                    usersCollection.Insert(user);
                    pendingCollection.Delete(x => x.id == user.id);
                    foreach (var friend in user.friends)
                    {
                        if (!usersCollection.Exists(x => x.id == friend.steamid) && 
                            !pendingCollection.Exists(x => x.id == friend.steamid))
                        {
                            pendingCollection.Insert(new User { id =friend.steamid });
                        }
                    }
                }
            }
        }
    }
}
