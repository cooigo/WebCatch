using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCatch.Core;


namespace WebCatchDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var xx = new WebQuery()
                 .With(me => me.From(me.SubQuery()
                                        .From("http://www.ign.com/cheats/more-cheats/?count=50&page=1&platform=android")
                                        .Select("$..cheaturl", "a"), "a")
                               .Select("dd", "title")
                      )
                     .Exceute(m =>
                     {
                         foreach (var item in m)
                         {
                             Console.WriteLine(item.Value.First().TextContent);
                         }
                         return "";
                     });

            Console.ReadLine();
        }
    }
}
