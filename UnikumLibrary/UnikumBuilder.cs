using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnikumLibrary
{
    public class UnikumBuilder
    {
        public static Group BuildGroup(string id, string name, string type, string schoolType, string courseCode)
        {
            Group result = new Group()
            {
                description = new Description()
                {
                    @long = name,
                    @short = name
                },
                groupType = type,
                sourcedId = new SourcedId()
                {
                    source = "EXTID",
                    id = id
                }
            };

            if (type == "SCHOOL" || courseCode != null)
            {
                result.extension = new GroupExtension();
                if (type == "SCHOOL")
                    result.extension.schoolType = schoolType;
                if (courseCode != null)
                    result.extension.courseCode = courseCode;
            }

            return result;
        }

        public static Membership BuildMembership(string groupId, string groupIdType, string memberId, string memberRole, string memberIdType)
        {
            return new Membership
            {
                sourcedId = new SourcedId
                {
                    id = groupId,
                    source = (groupIdType == "PERSON" ? "PID" : "EXTID")
                },
                rel = new Relation[] { new Relation
                        {
                            idType = memberIdType,
                            roleType = memberRole,
                            member = new Member
                            {
                                id = memberId,
                                source = (memberIdType == "PERSON" ? "PID" : "EXTID")
                            }
                        } }
            };
        }

        public static Person BuildPerson(string personnr, string fornamn, string efternamn, string epost, string telefon, string mobil, string postadress, string postnummer, string postort, int? schoolYear)
        {
            string p = personnr;
            return new Person()
            {
                sourcedId = new SourcedId()
                {
                    id = personnr,
                    source = "PID"
                },
                name = new Name()
                {
                    family = efternamn,
                    given = fornamn
                },
                adr = new Address()
                {
                    street = postadress,
                    pcode = postnummer,
                    locality = postort
                },
                email = epost,
                tel = (telefon == null ? null : new Telephone()
                {
                    telType = "1",
                    tel = telefon
                }),
                telMobile = (mobil == null ? null : new Telephone()
                {
                    telType = "3",
                    tel = mobil
                }),
                extension = (schoolYear == null ? null : new PersonExtension()
                {
                    schoolYear = (int)schoolYear
                })
            };
        }
    }
}
