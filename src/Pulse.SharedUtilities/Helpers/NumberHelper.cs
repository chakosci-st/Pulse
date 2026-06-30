using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.SharedUtilities.Helpers
{
    public static class NumberHelper
    {
        /// <summary>
        /// Zero-fills an integer to the specified length.
        /// </summary>
        /// <param name="number">The integer to zero-fill.</param>
        /// <param name="length">The desired length of the output string.</param>
        /// <returns>A string representation of the number, zero-filled to the specified length.</returns>
        public static string ZeroFill(int number, int length)
        {
            return number.ToString().PadLeft(length, '0');
        }
    }
}
