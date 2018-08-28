using System;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace UnikumLibrary
{
    [Serializable]
    public class UnikumException : Exception
    {
        public string method { get; set; }
        public string query { get; set; }
        public string json { get; set; }
        public UnikumException(string msg) : base(msg)
        {
        }
        public UnikumException(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }

    public class Unikum : IDisposable
    {

        private string unikum_URL;

        private string identity;
        private string server_token;

        private HashSet<string> seenGroups = new HashSet<string>();
        private HashSet<string> seenPersons = new HashSet<string>();

        private HttpClient client;

        public string AuthType { get; set; } = "Basic";

        public void Init(string unikum_URL, string identity, string server_token)
        {
            this.identity = identity;
            this.server_token = server_token;
            this.unikum_URL = unikum_URL;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                client.Dispose();
                client = null;
            }
        }

        private string GetAccessToken()
        {
            if (identity == null || server_token == null)
                throw new InvalidOperationException("Component not initialized with identity and server token information.");
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(identity + ":" + server_token));
        }

        private HttpClient GetHttpClient()
        {
            if (client == null)
            {
                client = new HttpClient();
                client.BaseAddress = new Uri(unikum_URL);
                client.DefaultRequestHeaders.Add(
                    "Authorization",
                    AuthType + " " + GetAccessToken()
                );
            }
            //result.DefaultRequestHeaders.Add("Content-Type", "application/json; charset=utf-8");
            return client;
        }

        private string FormatPID(string pid)
        {
            return Regex.Replace(pid, @"(\d{8})-?(\w{4})", @"$1-$2");
        }

        public Group GetGroup(string id)
        {
            Group result = null;
            string query = unikum_URL + "/v1/groups/sourcedId/EXTID/" + id;
            HttpClient client = GetHttpClient();
            HttpResponseMessage resp = client.GetAsync(query).Result;
            if (resp.IsSuccessStatusCode)
            {
                seenGroups.Add(id);
                result = JsonConvert.DeserializeObject<Group>(resp.Content.ReadAsStringAsync().Result);
            } else if (resp.StatusCode != HttpStatusCode.NotFound)
                throw new UnikumException("Could not get group [" + id + "]: " + resp.ReasonPhrase) { method = "GET", query = query };
            return result;
        }

        public bool GroupExists(string id)
        {
            return seenGroups.Contains(id) || GetGroup(id) != null;
        }

        public void AddGroup(Group group)
        {
            if (!GroupExists(group.sourcedId.id))
            {
                UpdateGroup(group);
            }
        }

        public void UpdateGroup(Group group)
        {
            string query_uri = unikum_URL + "/v1/groups";
            string postData = JsonConvert.SerializeObject(group);
            HttpClient client = GetHttpClient();
            HttpContent content = new StringContent(postData, Encoding.UTF8, "application/json");
            HttpResponseMessage resp = client.PostAsync(query_uri, content).Result;
            if (resp.IsSuccessStatusCode)
            {
                Group respGrp = JsonConvert.DeserializeObject<Group>(resp.Content.ReadAsStringAsync().Result);
                seenGroups.Add(respGrp.sourcedId.id);
                Console.WriteLine("Group " + respGrp.sourcedId.id + " updated.");
            }
            else
                throw new UnikumException("Could not update group [" + group.sourcedId.id + "]: " + resp.ReasonPhrase) { method = "POST", query = query_uri, json = postData };
        }

        public void RemoveGroup(Group group)
        {
            string query_uri = unikum_URL + "/v1/groups/sourcedId/" + group.sourcedId.source + "/" + group.sourcedId.id;
            HttpClient client = GetHttpClient();
            HttpResponseMessage resp = client.DeleteAsync(query_uri).Result;
            if (resp.IsSuccessStatusCode)
            {
                seenGroups.Remove(group.sourcedId.id);
                Console.WriteLine("Group " + group.sourcedId.id + " removed.");
            }
            else
                throw new UnikumException("Could not remove group [" + group.sourcedId.id + "]: " + resp.ReasonPhrase) { method = "DELETE", query = query_uri, json = ""};
        }

        public Person GetPerson(string id)
        {
            Person result = null;
            string query = unikum_URL + "/v1/persons/sourcedId/PID/" + id;
            HttpClient client = GetHttpClient();
            HttpResponseMessage resp = client.GetAsync(query).Result;
            if (resp.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<Person>(resp.Content.ReadAsStringAsync().Result);
                seenPersons.Add(result.sourcedId.id);
            }
            else if (resp.StatusCode != HttpStatusCode.NotFound)
                throw new UnikumException("Could not get person [" + id + "]: " + resp.ReasonPhrase) { method = "GET", query = query };
            return result;
        }

        public bool PersonExists(string id)
        {
            return seenPersons.Contains(id) || GetPerson(id) != null;
        }

        public void AddPerson(Person person)
        {
            string query = unikum_URL + "/v1/persons";
            HttpClient client = GetHttpClient();
            string postData = JsonConvert.SerializeObject(person);
            HttpContent content = new StringContent(postData, Encoding.UTF8, "application/json");
            HttpResponseMessage resp = client.PostAsync(query, content).Result;
            if (resp.IsSuccessStatusCode)
            {
                Person respPerson = JsonConvert.DeserializeObject<Person>(resp.Content.ReadAsStringAsync().Result);
                seenPersons.Add(respPerson.sourcedId.id);
                Console.WriteLine("User " + respPerson.sourcedId.id + " updated.");
            }
            else
                throw new UnikumException("Could not add person [" + person.sourcedId.id + "]: " + resp.ReasonPhrase) { method = "POST", query = query, json = postData };
        }

        public void RemovePerson(string id)
        {
            throw new NotImplementedException("Persons are removed automatically by Unikum when the last membership is removed.");
        }

        public void UpdatePerson(Person person)
        {
            AddPerson(person);
        }

        public void AddMembership(Membership mb)
        {
            String query_uri = unikum_URL + "/v1/memberships/sourcedId/" + mb.sourcedId.source + "/" + mb.sourcedId.id;
            foreach (Relation rel in mb.rel)
            {
                HttpClient client = GetHttpClient();
                string postData = JsonConvert.SerializeObject(rel);
                HttpContent content = new StringContent(postData, Encoding.UTF8, "application/json");
                HttpResponseMessage resp = client.PutAsync(query_uri, content).Result;
                if (resp.IsSuccessStatusCode)
                {
                    Console.WriteLine("User " + rel.member.id + " added as " + rel.roleType + " to " + mb.sourcedId.source + " " + mb.sourcedId.id + ".");
                }
                else
                    throw new UnikumException("Could not add [" + rel.member.id + "] (" + rel.idType + ") membership as " + rel.roleType + " vis [" + mb.sourcedId.id + "]: " + resp.ReasonPhrase) { method = "PUT", query = query_uri, json = postData };
            }
        }

        public void RemoveMembership(Membership mb)
        {
            String query_uri = unikum_URL + "/v1/memberships/sourcedId/" + mb.sourcedId.source + "/" + mb.sourcedId.id;
            foreach (Relation rel in mb.rel)
            {
                HttpClient client = GetHttpClient();
                string postData = JsonConvert.SerializeObject(rel);
                HttpContent content = new StringContent(postData, Encoding.UTF8, "application/json");
                HttpRequestMessage req = new HttpRequestMessage()
                {
                    Content = content,
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(query_uri)
                };
                HttpResponseMessage resp = client.SendAsync(req).Result;
                if (!resp.IsSuccessStatusCode && resp.StatusCode != HttpStatusCode.NotFound)
                    throw new UnikumException("Could not remove [" + mb.rel[0].member.id + "] (" + mb.rel[0].idType + ") membership as " + mb.rel[0].roleType + " from [" + mb.sourcedId.id + "]: " + resp.ReasonPhrase) { method = "POST", query = query_uri, json = postData };
            }
        }
    }
}
