﻿using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    [ChooserCompatibility(typeof(StandardTetrisHandler))]
    public class BagChooser : BlockGroupChooser
    {
        Queue<Func<Nomino>> WorkQueue = new Queue<Func<Nomino>>();


        public static IEnumerable<T> Shuffle<T>(IRandomizer rgen, IEnumerable<T> Shufflethese)
        {
            var sl = new SortedList<float, T>();
            foreach (T iterate in Shufflethese)
            {
                sl.Add((float) rgen.NextDouble(), iterate);
            }

            return sl.Select(iterator => iterator.Value);
        }

        public BagChooser(Func<Nomino>[] SelectionFunctions, int Seed) : base(SelectionFunctions,Seed)
        {
        }
       
        private void RefillQueue()
        {
            var Shuffled = Shuffle(rgen, _Available).ToArray();
            for (int i = 0; i < Shuffled.Length; i++)
            {
                WorkQueue.Enqueue(Shuffled[i]);
            }
        }

        protected override Nomino GetNext()
        {
            if (WorkQueue.Count == 0) RefillQueue();
            return WorkQueue.Dequeue()();
        }
    }
}