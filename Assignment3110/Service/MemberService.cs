using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Assignment3110.Entity;
using Newtonsoft.Json;

namespace Assignment3110.Service
{
    class MemberService : IMemberService
    {
        public Member Register(Member member)
        {
            var httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(member), Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(ProjectConfiguration.MEMBER_REGISTER_URL, content).GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<Member>(response.Content.ReadAsStringAsync().Result);
        }

        public MemberCredential Login(MemberLogin memberLogin)
        {
            var httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(memberLogin), Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(ProjectConfiguration.MEMBER_LOGIN_URL, content).GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<MemberCredential>(response.Content.ReadAsStringAsync().Result);
        }

        public Member GetInformation(MemberCredential memberCredential)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(memberCredential.token);
            var response = httpClient.GetAsync(ProjectConfiguration.MEMBER_GET_INFORMATION).GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<Member>(response.Content.ReadAsStringAsync().Result);
        }
    }
}
