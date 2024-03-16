using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadsOrTails
{
    public static class CommonFunctions
    {
        public static async Task AnimateButton(Button button)
        {
            await button.ScaleTo(1.15, 100);
            await button.ScaleTo(1, 100);
        }
    }
}
