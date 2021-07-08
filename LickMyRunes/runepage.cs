using System;
using System.Collections.Generic;
using System.Text;

namespace LickMyRunes
{
    public class runepage
    {
        public int[] RuneIDs { get; set; }
        public int PrimaryTree { get; set; }
        public int SecondaryTree { get; set; }



        public runepage()
        {
        }

        public runepage(int[] runeIDs, int primaryTree, int secondaryTree)
        {
            this.RuneIDs = runeIDs;
            this.PrimaryTree = primaryTree;
            this.SecondaryTree = secondaryTree;
        }

    }
}
