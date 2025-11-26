using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public class NewsItem
    {
        public string Title { get; set; }

        public DateTime Date { get; set; }

        public string Content { get; set; }

        public string FileName { get; set; }

        public bool IsViewed { get; set; }

        public string ContentHtml
        {
            get
            {
                var StyleCss = "html,body {height:100%; width:100%; margin:0; background-color:#202124; color:#bdc1c6; font-family: 'Segoe UI';} a{color:#53749d;}";
                var html = $"<html><head><meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge\" /><meta http-equiv=\"Content-Type\" content=\"text/html;charset=UTF-8\" /><style rel=\"stylesheet\">{StyleCss}</style></head><body oncontextmenu=\"return false;\"><div>{Content}</div></body></html>";
                return html;
            }
        }
    }
}
