using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoveringPaxos
{
    public class Proposal
    {
        public Proposal(string proposerName, int id)
        {
            this.ProposerName = proposerName;
            this.Id = id;
        }

        public string ProposerName { get; private set; }

        public int Id { get; private set; }

        public bool GreaterThan(Proposal p2)
        {
            if (Id != p2.Id)
            {
                return Id > p2.Id;
            }

            return ProposerName.CompareTo(p2.ProposerName) > 0;
        }

        public override string ToString()
        {
            return ProposerName + ":" + Id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is Proposal))
            {
                return false;
            }

            var other = (Proposal)obj;

            return Id == other.Id && ProposerName == other.ProposerName;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
