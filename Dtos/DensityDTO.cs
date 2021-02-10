using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoreOrLessDense.Dtos {
    public class DensityDTO {
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
        public override string ToString() {
            return $"{minDist};{minStepLen};{maxStepLen}";
        }
    }
}
