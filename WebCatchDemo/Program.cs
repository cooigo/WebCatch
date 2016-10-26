using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCatch.Core;
using Enums = WebCatch.Core.Enums;


namespace WebCatchDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var Headers = new Dictionary<String, String>
                {
                    {"Cookie", "foo=bar"},
                    {"xxx", "a"},
                };


            var xx = new WebQuery()
                 .With(me => me.From(me.SubQuery()
                                        .From("http://www.gamezebo.com/wp-admin/admin-ajax.php?page=1",
                                        "action=gz_load_posts&args%5BqueryName%5D=&args%5Bsearch%5D=&args%5Brequest%5D%5BPHP_SELF%5D=%2Findex.php&args%5Brequest%5D%5BREQUEST_URI%5D=%2Fplatform%2Fandroid%2F%3Ffilter%3Dthe-best&args%5Bpage%5D=1&args%5Bcategory_name%5D=the-best&ajax_request=true", Headers
                                        )
                                        .Select( new[] {
                                            new ParseObject { SourceType= Enums.CatchParseType.json, TargetType= Enums.CatchParseType.html, ParseString="$..html"  },
                                            new ParseObject { SourceType= Enums.CatchParseType.html, TargetType= Enums.CatchParseType.anchor, ParseString=".post-title > a"  },
                                        } , "a"))
                               .Select(new[] {
                                            new ParseObject { SourceType= Enums.CatchParseType.html, TargetType= Enums.CatchParseType.text, ParseString=".post-header .post-title"  },
                                        }, "title")
                              // .Select(".container_24 .grid_16", "body", WebCatch.Core.Enums.CatchValueType.Html)
                               .Select(new[] {
                                            new ParseObject { SourceType= Enums.CatchParseType.html, TargetType= Enums.CatchParseType.text, ParseString=".author > a"  },
                                        }, "author")
                      )
                     .Exceute(m =>
                     {
                         try
                         {
                             foreach (var item in m)
                             {
                                 Console.WriteLine(item.Value.ParseResult.First());
                             }
                         }
                         catch (Exception ex)
                         {
                             Console.WriteLine(ex.Message);
                         }

                         return "";
                         
                     });

            Console.ReadLine();
        }
    }
}
