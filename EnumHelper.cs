using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public static class EnumHelper
    {
        public static List<T> GetAllEnums<T>()
    where T : Enum
        {
            // The return type of Enum.GetValues is Array but it is effectively int[] per docs
            // This bit converts to int[]
            var values = Enum.GetValues(typeof(T)).Cast<int>().ToArray();

            if (!typeof(T).GetCustomAttributes(typeof(FlagsAttribute), false).Any())
            {
                // We don't have flags so just return the result of GetValues
                return Enum.GetValues(typeof(T)).Cast<T>().ToList();
            }

            var valuesInverted = values.Select(v => ~v).ToArray();
            int max = 0;
            for (int i = 0; i < values.Length; i++)
            {
                max |= values[i];
            }

            var result = new List<T>();
            for (int i = 0; i <= max; i++)
            {
                int unaccountedBits = i;
                for (int j = 0; j < valuesInverted.Length; j++)
                {
                    // This step removes each flag that is set in one of the Enums thus ensuring that an Enum with missing bits won't be passed an int that has those bits set
                    unaccountedBits &= valuesInverted[j];
                    if (unaccountedBits == 0)
                    {
                        result.Add((T)(object)i);
                        break;
                    }
                }
            }

            //Check for zero
            try
            {
                if (string.IsNullOrEmpty(Enum.GetName(typeof(T), (T)(object)0)))
                {
                    result.Remove((T)(object)0);
                }
            }
            catch
            {
                result.Remove((T)(object)0);
            }

            return result;
        }
    }
}
