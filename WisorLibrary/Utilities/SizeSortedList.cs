using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;

namespace WisorLib
{
    
    public class SizeSortedListHigh2Low<T> : SortedList<int, T>
    {
        int maxNumberOfElements;

        public SizeSortedListHigh2Low(int numOfElements) : base(new InvertedComparer())
        {
            maxNumberOfElements = numOfElements;
        }

        public void Add(int key, T value)
        {
            if (Count >= maxNumberOfElements)
            {
                // should it be added
                int res = Comparer.Compare(Keys[maxNumberOfElements - 1], key);
                Console.WriteLine("Got: " + key + " and result: " + res);
  
                // TBD shuky 
                if (0 < res)
                {
                    RemoveAt(maxNumberOfElements - 1);
                    base.Add(key, value);
                }
            }
            else
            {
                base.Add(key, value);
            }
        }
    }

    /////////////

    public class SizeSortedListLow2High<T> : SortedList<int, T>
    {
        int maxNumberOfElements;

        public SizeSortedListLow2High(int numOfElements) : base(new SafeComparer())
        {
            maxNumberOfElements = numOfElements;
        }

        public void Add(int key, T value)
        {
            if (Count >= maxNumberOfElements)
            {
                // should it be added
                int res = Comparer.Compare(Keys[maxNumberOfElements - 1], key);
     
                // TBD shuky 
                if (0 > res)
                {
                    RemoveAt(maxNumberOfElements - 1);
                    base.Add(key, value);
                }
            }
            else
            {
                base.Add(key, value);
            }
        }
    }

    /////////////

    public class InvertedComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            int result = y.CompareTo(x);
            if (result == 0)
                return 1;   // Handle equality as beeing greater
            return result;
        }
    }

    public class SafeComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            int result = x.CompareTo(y);
            if (result == 0)
                return 1;   // Handle equality as beeing greater
            return result;
        }
    }
}
