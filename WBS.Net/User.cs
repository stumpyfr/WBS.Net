using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WBS.Net
{
    public enum GenderType
    {
        Male,
        Female,
        Unknown,
    }

    public class User
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string ShortName { get; set; }
        public GenderType Gender { get; set; }
        public DateTime Birthdate { get; set; }
    }

    [DataContract]
    internal class InternalUser
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public InternalBody body { get; set; }
    }

    
    [DataContract]
    internal class InternalBody
    {
        [DataMember]
        public List<InternalUser2> users { get; set; }
    }
    
    [DataContract]
    internal class InternalUser2
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string firstname { get; set; }
        [DataMember]
        public string lastname { get; set; }
        [DataMember]
        public string shortname { get; set; }
        [DataMember]
        public int gender { get; set; }
        [DataMember]
        public int fatmethod { get; set; }
        [DataMember]
        public int birthdate { get; set; }
    }
}
