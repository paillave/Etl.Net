using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Rest
{
    public class RestRepositoryBase : HttpClient
    {
        public RestRepositoryBase(RestRepositorySetup restRepositorySetup) : base(new HttpClientHandler { UseDefaultCredentials = restRepositorySetup.UseDefaultCredentials }, true)
        {
            this.BaseAddress = new Uri(restRepositorySetup.BaseAddress);
            this.DefaultRequestHeaders.Accept.Clear();
            this.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var authenticationHeader = restRepositorySetup.GetAuthenticationHeader();
            if (authenticationHeader != null)
                this.DefaultRequestHeaders.Authorization = authenticationHeader;
        }
        private string GetUriParameters(Dictionary<string, object> queryValues = null)
        {
            return string.Join("&", GetValuePairs(queryValues).Select(i => $"{i.Key}={Uri.EscapeDataString(i.Value.ToString())}"));
        }
        private IEnumerable<KeyValuePair<string, object>> GetValuePairs(Dictionary<string, object> queryValues = null)
        {
            var fromObjectList = (queryValues ?? new Dictionary<string, object>())
                .Where(i => (i.Value as IEnumerable) == null || (i.Value as string) != null)
                .Select(i => new KeyValuePair<string, object>(i.Key, i.Value));
            var fromEnumerableList = queryValues
                    .Where(i => (i.Value as IEnumerable) != null && (i.Value as string) == null)
                    .SelectMany(i => ((IEnumerable)i.Value)
                        .OfType<object>()
                        .Select(value => new KeyValuePair<string, object>(i.Key, value)));
            return fromObjectList
                .Union(fromEnumerableList);
        }
        private Dictionary<string, object> GetDictionaryValues(object obj)
        {
            if (obj == null) return new Dictionary<string, object>();
            return obj.GetType().GetProperties().Select(i => new { Key = i.Name, Value = i.GetValue(obj) }).ToDictionary(i => i.Key, i => i.Value);
        }
        private Uri GetUri(string path = null, Dictionary<string, object> queryValues = null)
        {
            UriBuilder uriBuilder = new UriBuilder(this.BaseAddress);
            uriBuilder.Path = path;
            uriBuilder.Query = this.GetUriParameters(queryValues);
            return uriBuilder.Uri;
        }
        protected async Task<T> ExecuteGetAsync<T>(string path = null, object queryValues = null)
        {
            if (queryValues as Dictionary<string, object> != null)
                return await this.InternalExecuteGetAsync<T>(path, (Dictionary<string, object>)queryValues);
            else
                return await this.InternalExecuteGetAsync<T>(path, this.GetDictionaryValues(queryValues));
        }
        protected async Task ExecuteGetAsync(string path = null, object queryValues = null)
        {
            if (queryValues as Dictionary<string, object> != null)
                await this.InternalExecuteGetAsync(path, (Dictionary<string, object>)queryValues);
            else
                await this.InternalExecuteGetAsync(path, this.GetDictionaryValues(queryValues));
        }
        protected async Task<T> ExecuteDeleteAsync<T>(string path = null, object queryValues = null)
        {
            if (queryValues as Dictionary<string, object> != null)
                return await this.InternalExecuteDeleteAsync<T>(path, (Dictionary<string, object>)queryValues);
            else
                return await this.InternalExecuteDeleteAsync<T>(path, this.GetDictionaryValues(queryValues));
        }
        protected async Task ExecuteDeleteAsync(string path = null, object queryValues = null)
        {
            if (queryValues as Dictionary<string, object> != null)
                await this.InternalExecuteDeleteAsync(path, (Dictionary<string, object>)queryValues);
            else
                await this.InternalExecuteDeleteAsync(path, this.GetDictionaryValues(queryValues));
        }
        private async Task<T> InternalExecuteGetAsync<T>(string path = null, Dictionary<string, object> queryValues = null)
        {
            T ret = default(T);
            using (HttpResponseMessage response = await GetAsync(this.GetUri(path, queryValues)))
                if (response.IsSuccessStatusCode)
                    ret = await response.Content.ReadAsAsync<T>();
            return ret;
        }
        private async Task InternalExecuteGetAsync(string path = null, Dictionary<string, object> queryValues = null)
        {
            await GetAsync(this.GetUri(path, queryValues));
        }
        private async Task<T> InternalExecuteDeleteAsync<T>(string path = null, Dictionary<string, object> queryValues = null)
        {
            T ret = default(T);
            using (HttpResponseMessage response = await DeleteAsync(this.GetUri(path, queryValues)))
                if (response.IsSuccessStatusCode)
                    ret = await response.Content.ReadAsAsync<T>();
            return ret;
        }
        private async Task InternalExecuteDeleteAsync(string path = null, Dictionary<string, object> queryValues = null)
        {
            await DeleteAsync(this.GetUri(path, queryValues));
        }
        protected async Task<R> ExecutePostAsync<P, R>(P post, string path = null, object queryValues = null)
        {
            if (queryValues as Dictionary<string, object> != null)
                return await this.InternalExecutePostAsync<P, R>(post, path, (Dictionary<string, object>)queryValues);
            else
                return await this.InternalExecutePostAsync<P, R>(post, path, this.GetDictionaryValues(queryValues));
        }
        protected async Task ExecutePostAsync<P>(P post, string path = null, object queryValues = null)
        {
            if (queryValues as Dictionary<string, object> != null)
                await this.InternalExecutePostAsync<P>(post, path, (Dictionary<string, object>)queryValues);
            else
                await this.InternalExecutePostAsync<P>(post, path, this.GetDictionaryValues(queryValues));
        }
        private async Task<R> InternalExecutePostAsync<P, R>(P post, string path = null, Dictionary<string, object> queryValues = null)
        {
            R ret = default(R);

            var content = JsonConvert.SerializeObject(post);
            var contentBis = new StringContent(content, Encoding.UTF8, "application/json");

            using (HttpResponseMessage response = await this.PostAsync(this.GetUri(path, queryValues), contentBis))
                if (response.IsSuccessStatusCode)
                    ret = await response.Content.ReadAsAsync<R>();
            return ret;
        }
        private async Task InternalExecutePostAsync<P>(P post, string path = null, Dictionary<string, object> queryValues = null)
        {
            var content = JsonConvert.SerializeObject(post);
            var contentBis = new StringContent(content, Encoding.UTF8, "application/json");

            await this.PostAsync(this.GetUri(path, queryValues), contentBis);
        }
    }
}
