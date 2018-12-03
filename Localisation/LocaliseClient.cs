using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Localisation.Request;
using Localisation.Response;
using Serilog;

namespace Localisation
{
    public class LocaliseClient
    {
        public LocalisationCache Cache { get; }

        private LocaliseConfig _config;
        private ILogger _logger;

        public LocaliseClient(IOptions<LocaliseConfig> config, ILogger logger)
        {
            if (config == null || config.Value == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _logger = logger ?? throw new ArgumentException(nameof(logger));

            config.Value.Verify();
            _config = config.Value;

            _config.URL = _config.URL.Replace("{project_id}", _config.ProjectId);

            Cache = new LocalisationCache();
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            try
            {
                var zipurl = await DownloadLocalisationAsync(cancellationToken);
                await DownloadJsonAsync(zipurl, cancellationToken);
                AddToCache();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to download localisation archive, trying to fetch keys with translations");
                await ListKeys();
            }
        }

        async Task<string> DownloadLocalisationAsync(CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_config.URL + @"files/download");

                var fileRequest = new FileRequest() { Format = "json", OriginalFilenames = false, };

                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.Headers.Add("x-api-token", _config.Token);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(JsonConvert.SerializeObject(fileRequest));
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    request.Content = content;

                    var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                        .ConfigureAwait(false);

                    try
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var responseData = response.Content == null
                                ? null
                                : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            var result = default(FileResponse);
                            try
                            {
                                result = JsonConvert.DeserializeObject<FileResponse>(responseData);
                                return result.BundleId;
                            }
                            catch (Exception exception)
                            {
                                throw new Exception(
                                    string.Format("Could not deserialize the response body: {0}", responseData),
                                    exception);
                            }
                        }
                        else
                        {
                            throw new HttpResponseException((int)response.StatusCode, response.ReasonPhrase);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            string.Format("Exception during downloading localisation file for project {0}",
                                _config.ProjectId), ex);
                    }
                }
            }
        }

        async Task DownloadJsonAsync(string url, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);

                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Get;

                    var file = client.GetStreamAsync(url);

                    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                    try
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            if (File.Exists(string.Format(@"{0}{1}{2}", Environment.CurrentDirectory,
                                Path.DirectorySeparatorChar, "temp")))
                            {
                                File.Delete(string.Format(@"{0}{1}{2}", Environment.CurrentDirectory,
                                    Path.DirectorySeparatorChar, "temp"));
                            }

                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                using (FileStream output = new FileStream(string.Format(@"{0}{1}{2}",
                                    Environment.CurrentDirectory,
                                    Path.DirectorySeparatorChar, "temp"), FileMode.Create))
                                {
                                    stream.CopyTo(output);
                                }
                            }

                            Directory.CreateDirectory(string.Format(@"{0}{1}{2}", Environment.CurrentDirectory,
                                Path.DirectorySeparatorChar, "jsons"));

                            try
                            {
                                using (var archive =
                                    ZipFile.Open(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "temp",
                                        ZipArchiveMode.Update))
                                {
                                    archive.ExtractToDirectory(
                                        string.Format(@"{0}{1}{2}", Environment.CurrentDirectory,
                                            Path.DirectorySeparatorChar, "jsons"), true);
                                }
                            }
                            catch (Exception exception)
                            {
                                throw new Exception("Could not unzip localisation file", exception);
                            }

                            if (File.Exists(string.Format(@"{0}{1}{2}", Environment.CurrentDirectory,
                                Path.DirectorySeparatorChar, "temp")))
                            {
                                File.Delete(string.Format(@"{0}{1}{2}", Environment.CurrentDirectory,
                                    Path.DirectorySeparatorChar, "temp"));
                            }
                        }
                        else
                        {
                            throw new HttpResponseException((int)response.StatusCode, response.ReasonPhrase);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            string.Format("Exception during downloading localisation file for project {0}",
                                _config.ProjectId), ex);
                    }
                }
            }
        }

        void AddToCache()
        {
            var files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "jsons", "locale"));

            foreach (var file in files)
            {
                using (StreamReader r = new StreamReader(file))
                {
                    string json = r.ReadToEnd();
                    var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                    var locale = Path.GetFileNameWithoutExtension(file);

                    foreach (var item in items)
                    {
                        Cache.AddItem(ISOCodes.CustomMapper[locale], item.Key, item.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Get localised string based on english string(used as a key)
        /// </summary>
        /// <param name="source">for example email / categories / default</param>
        /// <param name="englishString"></param>
        /// <returns></returns>
        public string LocaliseStringFromFile(string isoCode, string source, string englishString)
        {
            string result = "";
            var localiseISO = ISOCodes.Mapper[isoCode];

            using (StreamReader r = new StreamReader(string.Format(@"{0}{1}locale{1}{2}.json", Environment.CurrentDirectory, Path.DirectorySeparatorChar, localiseISO)))
            {
                string json = r.ReadToEnd();
                var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                result = items[englishString];
            }

            return result;
        }

        public async Task ListKeys()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_config.URL + "keys?limit=5000");

                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Get;
                    request.Headers.Add("x-api-token", _config.Token);

                    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                        .ConfigureAwait(false);

                    try
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var responseData = response.Content == null
                                ? null
                                : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            var result = default(List<KeyResponse>);
                            try
                            {
                                result = JsonConvert.DeserializeObject<List<KeyResponse>>(responseData);

                                foreach (var keyResponse in result)
                                {
                                    foreach (var translation in keyResponse.Translations)
                                    {
                                        Cache.AddItem(translation.LanguageISO, keyResponse.KeyName["web"],
                                            translation.Translation);
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                throw new Exception(
                                    string.Format("Could not deserialize the response body: {0}", responseData),
                                    exception);
                            }
                        }
                        else
                        {
                            throw new HttpResponseException((int)response.StatusCode, response.ReasonPhrase);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            string.Format("Exception during downloading localisations for project {0}",
                                _config.ProjectId), ex);
                    }
                }
            }
        }
    }
}
