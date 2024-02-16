using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public class RandomHelpers
    {
        public Random def_rgen = new Random();
        public static RandomHelpers Static = new RandomHelpers();
        public T[] Choose<T>(IEnumerable<T> ChooseArray, int numselect,Random rgen = null)
        {
            if (rgen == null) rgen = new Random();
            T[] returnarray = new T[numselect];
            SortedList<double, T> sorttest = new SortedList<double, T>();
            foreach (T loopvalue in ChooseArray)
            {
                sorttest.Add(rgen.NextDouble(), loopvalue);
            }
            //Array.Copy(sorttest.ToArray(), returnarray, numselect);
            var usearray = sorttest.ToArray();
            for (int i = 0; i < numselect; i++)
            {
                returnarray[i] = usearray[i].Value;
            }
            return returnarray;
        }
        public IEnumerable<T> Shuffle<T>(IEnumerable<T> Shufflethese,Random rgen)
        {
            if (rgen == null) rgen = new Random();
            var sl = new SortedList<float, T>();
            foreach (T iterate in Shufflethese)
            {
                bool AddError = true;
                while (AddError)
                {
                    try
                    {
                        sl.Add((float)rgen.NextDouble(), iterate);
                        AddError = false;
                    }
                    catch (Exception exr)
                    {
                        AddError = true;
                    }
                }
            }
            Random rg = new Random();

            return sl.Select(iterator => iterator.Value);
        }


        public T Select<T>(T[] items, float[] Probabilities)
        {
            return Select(items, Probabilities, def_rgen);
        }

        public T Select<T>(T[] items, float[] Probabilities, Random rgen)
        {
            float[] sumulator = null;
            return Select(items, Probabilities, rgen, ref sumulator);
        }

        public T Select<T>(T[] items, float[] Probabilities, ref float[] sumulations)
        {
            return Select(items, Probabilities, def_rgen, ref sumulations);
        }

        public T Select<T>(T[] items, float[] Probabilities, Random rgen, ref float[] sumulations)
        {
            //first, sum all the probabilities; unless a cached value is being given to us.
            //we do this manually because we will also build a corresponding list of the sums up to that element.
            float getsum = 0;
            if (sumulations == null)
            {
                sumulations = new float[Probabilities.Length + 1];
                for (int i = 0; i < Probabilities.Length; i++)
                {
                    sumulations[i] = getsum;
                    getsum += Probabilities[i];
                }

                sumulations[sumulations.Length - 1] = getsum; //add this last value in...
            }
            else
            {
                getsum = sumulations[sumulations.Length - 1];
            }

            //get a percentage using nextDouble. we use doubles, just in case the probabilities array uses rather large numbers to attempt to prevent
            //abberations as a result of floating point errors.
            double usepercentage = rgen.NextDouble();
            //convert this percentage into a value we can use, that corresponds to the sum of float values:
            float searchtotal = (float)(usepercentage * getsum);
            //now find the corresponding index and return the corresponding value in the items array.
            for (int i = 0; i < Probabilities.Length; i++)
            {
                if (searchtotal > sumulations[i] && searchtotal < sumulations[i + 1])
                    return items[i];
            }

            return default(T);
        }


    }




}
