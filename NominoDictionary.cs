using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public class NominoDictionary<T> where T:class
    {
        private Dictionary<String, T> _Dict = new Dictionary<string, T>();


        public T this[Nomino Indexer]
        {
            get
            {
                return GetElement(Indexer);
            }
            set
            {
                AddElement(Indexer, value);
            }
        }

        public static String GetNominoKey(Nomino Source)
        {
            var Points1 = NNominoGenerator.GetNominoPoints(Source);
            String sRep1 = NNominoGenerator.StringRepresentation(Points1);
            return sRep1;
        }
        public void AddElement(Nomino Key, T Value)
        {
            var Points1 = NNominoGenerator.GetNominoPoints(Key);
            var Points2 = NNominoGenerator.RotateCW(Points1);
            var Points3 = NNominoGenerator.RotateCW(Points2);
            var Points4 = NNominoGenerator.RotateCW(Points3);
            String sRep1 = NNominoGenerator.StringRepresentation(Points1);
            String sRep2 = NNominoGenerator.StringRepresentation(Points2);
            String sRep3 = NNominoGenerator.StringRepresentation(Points3);
            String sRep4 = NNominoGenerator.StringRepresentation(Points4);

            _Dict[sRep1] = Value;
            _Dict[sRep2] = Value;
            _Dict[sRep3] = Value;
            _Dict[sRep4] = Value;

        }
        public bool HasElement(Nomino Key)
        {
            String sKey = GetNominoKey(Key);
            return _Dict.ContainsKey(sKey);

        }
        public T GetElement(Nomino Key)
        {
            var Points1 = NNominoGenerator.GetNominoPoints(Key);
            String sRep1 = NNominoGenerator.StringRepresentation(Points1);
            if (_Dict.ContainsKey(sRep1)) return _Dict[sRep1];

            return null;
        }

    }
}
