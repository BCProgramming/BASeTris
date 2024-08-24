using BASeTris.Choosers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public class RandomHelpers
    {
        public static IRandomizer Construct()
        {
            return new DotNetRandomizer();
        }
        public static IRandomizer Construct(int seed)
        {
            return new DotNetRandomizer(seed);

        }


        public IRandomizer def_rgen = Construct();
        public static RandomHelpers Static = new RandomHelpers();
        public T[] Choose<T>(IEnumerable<T> ChooseArray, int numselect, Random rgen = null)
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
        public IEnumerable<T> Shuffle<T>(IEnumerable<T> Shufflethese, IRandomizer rgen)
        {
            if (rgen == null) rgen = RandomHelpers.Construct();
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

            return sl.Select(iterator => iterator.Value);
        }


        public T Select<T>(T[] items, float[] Probabilities)
        {
            return Select(items, Probabilities, def_rgen);
        }

        public T Select<T>(T[] items, float[] Probabilities, IRandomizer rgen)
        {
            float[] sumulator = null;
            return Select(items, Probabilities, rgen, ref sumulator);
        }

        public T Select<T>(T[] items, float[] Probabilities, ref float[] sumulations)
        {
            return Select(items, Probabilities, def_rgen, ref sumulations);
        }

        public T Select<T>(T[] items, float[] Probabilities, IRandomizer rgen, ref float[] sumulations)
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


    public interface IRandomizer
    {
        double NextDouble();
        int Next(int Max);
        int Next(int MinValue, int MaxValue);

        int Next();
    }


    public abstract class BaseRandomizer : IRandomizer
    {
        protected ulong bytesprocessed = 0; //keep track of the current state.


        public BaseRandomizer() { }

        

        public virtual double NextDouble()
        {
            var bytes = new Byte[8];
            GetBytes(bytes);
            // Step 2: bit-shift 11 and 53 based on double's mantissa bits
            var ul = BitConverter.ToUInt64(bytes, 0) / (1 << 11);
            Double d = ul / (Double)(1UL << 53);
            return d;

        }
        public virtual int Next()
        {
            byte[] target = new byte[sizeof(int)];
            GetBytes(target);

            return BitConverter.ToInt32(target);
        }
        public virtual int Next(int MinValue, int MaxValue)
        {
            return (int)(NextDouble() * (double)(MaxValue-MinValue)) + MinValue;
        }
        public virtual int Next(int Max)
        {
            return (int)(NextDouble() * (double)Max);
        }

        public void GetBytes(byte[] target)
        {
            InternalGetBytes(target);
            bytesprocessed += (ulong)target.Length;
        }
        protected abstract void InternalGetBytes(byte[] target);

    }
  

    public class DotNetRandomizer : BaseRandomizer
    {
        Random _Impl = null;
        int Seed;
        public DotNetRandomizer():this(Environment.TickCount)
        {
        }
        public DotNetRandomizer(int pSeed)

        {
            Seed = pSeed;
            _Impl = new Random(Seed);
        }

        protected override void InternalGetBytes(byte[] src)
        {
            _Impl.NextBytes(src);
            
            
        }
        

    }

   




}
