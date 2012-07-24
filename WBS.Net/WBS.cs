using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Storage.Basic;
using DevDefined.OAuth.Tests;

namespace WBS.Net
{
    public class WBS
    {
        private readonly string consumerKey;
        private readonly string consumerSecret;
        private string token;
        private string tokenSecret;
        private int userId;

        private const string requestUrl = "http://oauth.withings.com/account/request_token";
        private const string userAuthorizeUrl = "http://oauth.withings.com/account/authorize";
        private const string accessUrl = "http://oauth.withings.com/account/access_token";
        private OAuthSession session;
        private IToken requestToken;
        private OAuthConsumerContext context;

        public User User { get; private set; }

        public WBS(string consumerKey, string consumerSecret, string token = "", string tokenSecret = "", int userId = -1)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.tokenSecret = tokenSecret;
            this.token = token;
            this.userId = userId;

            this.CreateSession();

            if (!string.IsNullOrWhiteSpace(this.token) && !string.IsNullOrWhiteSpace(this.tokenSecret) && userId != -1)
                this.UpdateProfil();
        }

        public async Task<string> Connect()
        {
            return await Task.Run(() =>
            {
                requestToken = session.GetRequestToken();
                return session.GetUserAuthorizationUrlForToken(requestToken);
            });
        }

        private void CreateSession()
        {
            X509Certificate2 certificate = TestCertificates.OAuthTestCertificate();
            var consumerContext = new OAuthConsumerContext
            {
                SignatureMethod = SignatureMethod.HmacSha1,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                UseHeaderForOAuthParameters = false,
            };
            this.context = consumerContext;
            this.session = new OAuthSession(this.context, requestUrl, userAuthorizeUrl, accessUrl);
        }

        public async void Valid(string verifier, int userId)
        {
            await Task.Run(() =>
                {
                    IToken accessToken = session.ExchangeRequestTokenForAccessToken(requestToken, "GET", verifier);

                    this.token = accessToken.Token;
                    this.tokenSecret = accessToken.TokenSecret;
                    this.userId = userId;

                    this.UpdateProfil();
                });
        }

        public async Task<List<MeasureGroup>> GetMeasures(DateTime startDate, DateTime endDate)
        {
            return await Get(string.Format("http://wbsapi.withings.net/measure?action=getmeas&userid={0}&startdate={1}&enddate={2}", userId, ToUnixTime(startDate), ToUnixTime(endDate)));
        }

        public async Task<List<MeasureGroup>> GetMeasures(DateTime startDate, DateTime endDate, int offset = -1, int limit = -1)
        {
            var str = string.Format("http://wbsapi.withings.net/measure?action=getmeas&userid={0}&startdate={1}&enddate={2}", userId, ToUnixTime(startDate), ToUnixTime(endDate));
            if (offset >= 0)
                str += string.Format("&offset={0}", offset);
            if (limit >= 0)
                str += string.Format("&limit={0}", offset);

            return await Get(str);
        }

        public async Task<List<MeasureGroup>> GetMeasures(DateTime lastUpdate)
        {
            return await Get(string.Format("http://wbsapi.withings.net/measure?action=getmeas&userid={0}&lastupdate={1}", userId, ToUnixTime(lastUpdate)));
        }

        public async Task<List<MeasureGroup>> GetMeasures(DateTime lastUpdate, int offset = -1, int limit = -1)
        {
            var str = string.Format("http://wbsapi.withings.net/measure?action=getmeas&userid={0}&lastupdate={1}", userId, ToUnixTime(lastUpdate));
            if (offset >= 0)
                str += string.Format("&offset={0}", offset);
            if (limit >= 0)
                str += string.Format("&limit={0}", offset);

            return await Get(str);
        }

        private async Task<List<MeasureGroup>> Get(string request)
        {
            return await Task.Run<List<MeasureGroup>>(() =>
            {
                var accessToken = CreateAccessToken();

                try
                {
                    string responseText = session.Request(accessToken).Get().ForUrl(request).ToString();
                    return TransformMeasures(responseText);
                }
                catch (WebException ex)
                {
                    throw;
                }
            });
        }

        private AccessToken CreateAccessToken()
        {
            var accessToken = new AccessToken()
            {
                ConsumerKey = this.consumerKey,
                Token = this.token,
                TokenSecret = this.tokenSecret,
            };
            return accessToken;
        }

        private static List<MeasureGroup> TransformMeasures(string responseText)
        {
            DataContractJsonSerializer sr = new DataContractJsonSerializer(typeof(InternalMeasure));
            Byte[] bytes = Encoding.Unicode.GetBytes(responseText);
            InternalMeasure mesu = null;
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                mesu = sr.ReadObject(stream) as InternalMeasure;
            }
            if (mesu != null)
            {
                List<MeasureGroup> ret = new List<MeasureGroup>();
                foreach (var m in mesu.body.measuregrps)
                {
                    var me = new MeasureGroup();
                    me.Attribution = (AttributionType)m.attrib;
                    me.Category = (CategoryType)m.category;
                    me.Date = FromUnixTime(m.date);
                    me.Id = m.grpid;

                    me.Measures = new List<Measure>();
                    foreach (var internalMe in m.measures)
                    {
                        var newMe = new Measure();
                        newMe.MeasureType = (MeasureType)internalMe.type;
                        newMe.Value = internalMe.value * (Math.Pow(10, internalMe.unit));
                        me.Measures.Add(newMe);
                    }

                    ret.Add(me);
                }

                return ret;
            }
            else
                throw new Exception();
        }

        private async void UpdateProfil()
        {
            await Task.Run(() =>
            {
                var accessToken = new AccessToken()
                {
                    ConsumerKey = this.consumerKey,
                    Token = this.token,
                    TokenSecret = this.tokenSecret,
                };

                string responseText = session.Request(accessToken).Get().ForUrl(string.Format("http://wbsapi.withings.net/user?action=getbyuserid&userid={0}", userId)).ToString();
                DataContractJsonSerializer sr = new DataContractJsonSerializer(typeof(InternalUser));
                Byte[] bytes = Encoding.Unicode.GetBytes(responseText);
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    InternalUser user = sr.ReadObject(stream) as InternalUser;
                    this.User = new User()
                    {
                        FirstName = user.body.users.First().firstname,
                        LastName = user.body.users.First().lastname,
                        Id = user.body.users.First().id,
                        Gender = (GenderType)user.body.users.First().gender,
                        ShortName = user.body.users.First().shortname,
                        Birthdate = FromUnixTime(user.body.users.First().birthdate),
                    };
                }
            });
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static long ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

    }
}
