using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor
{
    [Serializable]
    public class MapData
    {
        public int pixHeight;
        public int pixWidth;
        public int maxX;
        public int maxY;
        public int title_wh;
        public int version;
        public int[] discardList;
        public int[] grids;
    }
}
