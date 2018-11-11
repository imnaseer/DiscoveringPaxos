using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoveringPaxos
{
    public class Proposal
    {
        public Proposal(string proposerName, int id, string value)
        {
            this.ProposerName = ProposerName;
            this.Id = id;
            this.Value = value;
        }

        public string ProposerName { get; private set; }

        public int Id { get; private set; }

        public string Value { get; private set; }
       
        public static bool LessThan(Proposal p1, Proposal p2)
        {
            if (p1.Id != p2.Id)
            {
                return p1.Id < p2.Id;
            }

            return p1.ProposerName.CompareTo(p2.ProposerName) < 0;
        }
    }
}
