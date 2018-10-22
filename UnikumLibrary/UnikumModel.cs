using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnikumLibrary
{
    public class RootObject
    {
        public Statisticts _stats { get; set; }
        public string ExportId { get; set; }
        public Person[] Persons { get; set; }
        public Group[] Groups { get; set; }
        public Membership[] Memberships { get; set; }
    }

    public class Statisticts
    {
        public int FilteredPersons { get; set; }
    }

    public class Person
    {
        public SourcedId sourcedId { get; set; }
        public SourcedId[] sourcedIds { get; set; }
        public bool ShouldSerializesourcedIds() { return sourcedIds != null; }
        public Name name { get; set; }
        public Telephone tel { get; set; }
        public bool ShouldSerializetel() { return tel != null; }
        public Address adr { get; set; }
        public bool ShouldSerializeadr() { return adr != null; }
        public string email { get; set; }
        public bool ShouldSerializeemail() { return email != null; }
        public string privateEmail { get; set; }
        public bool ShouldSerializeprivateEmail() { return privateEmail != null; }
        public PersonExtension extension { get; set; }
        public bool ShouldSerializeextension() { return extension != null; }
        public Telephone telWork { get; set; }
        public bool ShouldSerializetelWork() { return telWork != null; }
        public Telephone telMobile { get; set; }
        public bool ShouldSerializetelMobile() { return telMobile != null; }
    }

    public class SourcedId
    {
        public string source { get; set; }
        public string id { get; set; }
    }

    public class Name
    {
        public string given { get; set; }
        public string family { get; set; }
    }

    public class Telephone
    {
        public string tel { get; set; }
        public string telType { get; set; }
    }

    public class Address
    {
        public string street { get; set; }
        public string pcode { get; set; }
        public string locality { get; set; }
    }

    public class PersonExtension
    {
        public bool hideContactDetails { get; set; } = true;
        public bool ShouldSerializehideContactDetails() { return !hideContactDetails; }
        public bool allowNotificationMail { get; set; } = true;
        public bool ShouldSerializeallowNotificationMail() { return !allowNotificationMail; }
        public bool allowSchoolMail { get; set; } = true;
        public bool ShouldSerializeallowSchoolMail() { return !allowSchoolMail; }
        public bool allowNewsLetter { get; set; } = true;
        public bool ShouldSerializeallowNewsLetter() { return !allowNewsLetter; }
        public int schoolYear { get; set; } = 0;
        public bool ShouldSerializeshoolYear() { return schoolYear > 0; }
    }

    public class Group
    {
        public SourcedId sourcedId { get; set; }
        public SourcedId[] sourcedIds { get; set; }
        public bool ShouldSerializesourcedIds() { return sourcedIds != null; }
        public Description description { get; set; }
        public bool ShouldSerializedescription() { return description != null; }
        public string groupType { get; set; }
        public GroupExtension extension { get; set; }
        public bool ShouldSerializeextension() { return extension != null; }
        public string url { get; set; }
        public bool ShouldSerializeurl() { return url != null; }
    }

    public class Description
    {
        public string @short { get; set; }
        public string @long { get; set; }
    }

    public class GroupExtension
    {
        public string schoolType { get; set; }
        public VisitingAddress visitingAddress { get; set; }
        public string extraAddress { get; set; }
        public string tel { get; set; }
        public string telFax { get; set; }
        public string sisSchoolUnitCode { get; set; }
        public int ageRangeFrom { get; set; }
        public int ageRangeTo { get; set; }
        public string courseCode { get; set; }
    }

    public class VisitingAddress
    {
        public string street { get; set; }
        public string pcode { get; set; }
        public string locality { get; set; }
    }

    public class Membership
    {
        public SourcedId sourcedId { get; set; }
        public bool ShouldSerializesourcedId() { return sourcedId != null; }
        public Relation[] rel { get; set; }
    }

    public class Relation
    {
        public Member member { get; set; }
        public string roleType { get; set; }
        public string idType { get; set; }
        public DateTime? startDate { get; set; }
        public bool ShouldSerializestartDate()
        {
            return startDate != null;
        }
    }

    public class Member
    {
        public string source { get; set; }
        public string id { get; set; }
    }
}
