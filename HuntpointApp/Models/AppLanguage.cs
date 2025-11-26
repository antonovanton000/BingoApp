using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Models
{
    public class AppLanguage
    {
        public string LanguageName { get; set; } = null!;

        public CultureInfo Culture { get; set; } = null!;
    }
}
