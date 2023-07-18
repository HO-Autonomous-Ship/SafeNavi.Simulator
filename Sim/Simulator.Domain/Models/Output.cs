using System;
using System.Waf.Foundation;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using System.Windows.Media;

namespace SyDLab.Usv.Simulator.Domain.Models
{
    public class Output:Model
    {
        public PlatformBase Target { get; set; }
        public string Item { get; set; }
        public string Type { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Mean { get; set; }
        public double Interval => Math.Round(Max - Min, 3);
        public Color Color { get; set; }
    }
}