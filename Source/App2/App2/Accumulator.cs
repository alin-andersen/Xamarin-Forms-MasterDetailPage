using System;
using System.Collections.Generic;
using System.Text;

namespace App2
{
    public class Accumulator
    {
        public double Value { get; private set; } = 0.0;

        bool firstTime = true;

        double lastValue;

        public void Accumulate(double currentValue)
        {
            if(firstTime)
            {
                firstTime = false;
            }
            else
            {
                Value += lastValue - currentValue;
            }

            lastValue = currentValue;
        }

        public void Reset()
        {
            Value = 0.0;
            firstTime = true;
        }
    }
}
