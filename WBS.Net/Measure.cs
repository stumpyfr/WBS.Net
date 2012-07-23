using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WBS.Net
{
    public enum AttributionType
    {
        ByDeviceAndValid = 0,
        ByDeviceButAmbiguous = 1,
        Manually = 2,
        ManuallyAtCreation = 4,
    }

    public enum CategoryType
    {
        Measure = 1,
        Target = 2,
    }

    public class MeasureGroup
    {
        public int Id { get; set; }
        public AttributionType Attribution { get; set; }
        public DateTime Date { get; set; }
        public CategoryType Category { get; set; }

        public List<Measure> Measures { get; set; }
    }

    public enum MeasureType
    {
        Weight = 1,
        Height = 4,
        FateFreeMass = 5,
        FatRatio = 6,
        FatMassWeight = 8,
        DiastolicBloodPressure = 9,
        SystolicBloodPressure = 10,
        HeartPulse = 11,
    }

    public class Measure
    {
        public MeasureType MeasureType { get; set; }
        public double Value { get; set; } //real_value = value * 10^unit 
    }

    [DataContract]
    internal class InternalMeasure
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public InternalMeasure2 body { get; set; }
    }

    [DataContract]
    internal class InternalMeasure2
    {
        [DataMember]
        public int updatetime { get; set; }
        [DataMember]
        public List<InternalMeasuregrps> measuregrps { get; set; }
    }

    [DataContract]
    internal class InternalMeasuregrps
    {
        [DataMember]
        public int grpid { get; set; }
        [DataMember]
        public int attrib { get; set; }
        [DataMember]
        public int date { get; set; }
        [DataMember]
        public int category { get; set; }

        [DataMember]
        public List<InternalMeasure3> measures { get; set; }
    }

    [DataContract]
    internal class InternalMeasure3
    {
        [DataMember]
        public int value { get; set; }
        [DataMember]
        public int type { get; set; }
        [DataMember]
        public int unit { get; set; }
    }
}
