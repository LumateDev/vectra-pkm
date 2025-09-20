using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vectra.Modules.Documents.Domain.Enums
{
    public enum BlockType
    {
        Text = 0,
        Heading1 = 1,
        Heading2 = 2,
        Heading3 = 3,
        BulletList = 4,
        NumberedList = 5,
        Checkbox = 6,
        Code = 7,
        Quote = 8,
        Divider = 9,
        Image = 10,
        Table = 11,
        Toggle = 12,
        Callout = 13,
        Video = 14,
        File = 15,
        Bookmark = 16,
        Equation = 17
    }
}
