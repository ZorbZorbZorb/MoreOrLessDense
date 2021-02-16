using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoreOrLessDense.Dtos {
    public class DensityDTO {
        // This is a singleton to store the current density information
        public static DensityDTO density = null;

        public double minDist { get; set; }
        public double minStepLen { get; set; }
        public double maxStepLen { get; set; }
        public static DensityDTO FromString(string s) {
            string[] data = s.Split(';');
            return new DensityDTO() {
                minDist = double.Parse(data[0]),
                minStepLen = double.Parse(data[1]),
                maxStepLen = double.Parse(data[2])
            };
        }
        public override string ToString() => $"{minDist};{minStepLen};{maxStepLen}";

        public static void SetDensityInformation(float? sliderValue) {
            if ( density == null ) {
                density = new DensityDTO();
            }

            // Actual slider range is 0.25 to 2.5x
            MoreOrLessDense.lastSetSliderValue.Value = sliderValue == null ? 4f : (float)sliderValue;
            double value = sliderValue == null ? 1d : ( (int)sliderValue ) * 0.25d;

            // https://www.desmos.com/calculator/mvy03tixqm
            density.minDist = Math.Round(Math.Log(value + 2.05d, 2d), 1);
            density.minStepLen = Math.Round(Math.Log(value + 2.05d, 2d), 1);
            density.maxStepLen = Math.Round(Math.Exp(( value / 2d ) + 0.4d), 1);
        }
    }
}
