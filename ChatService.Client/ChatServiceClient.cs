using ChatService.DataContracts;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Client
{

    public class ChatServiceClient : IChatServiceClient
    {
        private readonly HttpClient httpClient;

        public ChatServiceClient(Uri baseUri)
        {
            httpClient = new HttpClient()
            {
                BaseAddress = baseUri
            };
        }

        public ChatServiceClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> GetVersion()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync("api/application/version");
                if (!response.IsSuccessStatusCode)
                {
                    throw new ChatServiceException("Failed to get version", response.ReasonPhrase, response.StatusCode);
                }

                string content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception e)
            {
                // make sure we don't catch our own exception we threw above
                if (e is ChatServiceException) throw;

                throw new ChatServiceException("Failed to reach chat service", e, "Internal Server Error",
                    HttpStatusCode.InternalServerError);
            }
        }

        public async Task<string> GetEnvironment()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync("api/application/environment");
                if (!response.IsSuccessStatusCode)
                {
                    throw new ChatServiceException("Failed to get version", response.ReasonPhrase, response.StatusCode);
                }

                string content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception e)
            {
                // make sure we don't catch our own exception we threw above
                if (e is ChatServiceException) throw;

                throw new ChatServiceException("Failed to reach chat service", e, "Internal Server Error",
                    HttpStatusCode.InternalServerError);
            }
        }

        public async Task CreateProfile(CreateProfileDto profileDto)
        {
            try
            {
                HttpResponseMessage response = await httpClient.PostAsync("api/profile",
                    new StringContent(JsonConvert.SerializeObject(profileDto), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    throw new ChatServiceException("Failed to create user profile", response.ReasonPhrase, response.StatusCode);
                }
            }
            catch (Exception e)
            {
                // make sure we don't catch our own exception we threw above
                if (e is ChatServiceException) throw;

                throw new ChatServiceException("Failed to reach chat service", e, "Internal Server Error",
                    HttpStatusCode.InternalServerError);
            }
        }

        public async Task<UserInfoDto> GetProfile(string username)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync($"api/profile/{username}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new ChatServiceException("Failed to retrieve user profile", response.ReasonPhrase, response.StatusCode);
                }

                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserInfoDto>(content);
            }
            catch (JsonException e)
            {
                throw new ChatServiceException($"Failed to deserialize profile for user {username}", e,
                    "Serialization Exception", HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                // make sure we don't catch our own exception we threw above
                if (e is ChatServiceException) throw;

                throw new ChatServiceException("Failed to reach chat service", e,
                    "Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ConversationDto> AddConversation(CreateConversationDto createConversationDto)
        {
            try
            {
                HttpResponseMessage response =
                    await httpClient.PostAsync("api/conversations",
                    new StringContent(JsonConvert.SerializeObject(createConversationDto), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    throw new ChatServiceException("Failed to retrieve user profile", response.ReasonPhrase, response.StatusCode);
                }

                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ConversationDto>(content);
            }
            catch (JsonException e)
            {
                throw new ChatServiceException("Failed to deserialize the response", e,
                    "Serialization Exception", HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new ChatServiceException("Failed to reach chat service", e,
                    "Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ListConversationsDto> ListConversations(string username, int limit = 50)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync($"api/conversations/{username}?limit={limit.ToString()}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new ChatServiceException("Failed to retrieve user profile", response.ReasonPhrase, response.StatusCode);
                }

                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ListConversationsDto>(content);
            }
            catch (JsonException e)
            {
                throw new ChatServiceException("Failed to deserialize the response", e,
                    "Serialization Exception", HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new ChatServiceException("Failed to reach chat service", e,
                    "Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ListConversationsDto> ListConversationsByUri(string uri)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    throw new ChatServiceException("Failed to retrieve user profile", response.ReasonPhrase, response.StatusCode);
                }

                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ListConversationsDto>(content);
            }
            catch (JsonException e)
            {
                throw new ChatServiceException("Failed to deserialize the response", e,
                    "Serialization Exception", HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new ChatServiceException("Failed to reach chat service", e,
                    "Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }
        public async Task<ListMessagesDto> ListMessages(string conversationId, int limit = 50)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync($"api/conversation/{conversationId}?limit={limit.ToString()}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new ChatServiceException("Failed to retrieve user profile", response.ReasonPhrase, response.StatusCode);
                }

                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ListMessagesDto>(content);
            }
            catch (JsonException e)
            {
                throw new ChatServiceException("Failed to deserialize the response", e,
                    "Serialization Exception", HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new ChatServiceException("Failed to reach chat service", e,
                    "Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ListMessagesDto> ListMessagesByUri(string uri)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    throw new ChatServiceException("Failed to retrieve user profile", response.ReasonPhrase, response.StatusCode);
                }

                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ListMessagesDto>(content);
            }
            catch (JsonException e)
            {
                throw new ChatServiceException("Failed to deserialize the response", e,
                    "Serialization Exception", HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new ChatServiceException("Failed to reach chat service", e,
                    "Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }


        public async Task SendMessage(string conversationId, SendMessageDto messageDto)
        {
            try
            {
                HttpResponseMessage response =
                    await httpClient.PostAsync($"api/conversation/{conversationId}",
                    new StringContent(JsonConvert.SerializeObject(messageDto), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    throw new ChatServiceException("Failed to retrieve user profile", response.ReasonPhrase, response.StatusCode);
                }
            }
            catch (JsonException e)
            {
                throw new ChatServiceException("Failed to deserialize the response", e,
                    "Serialization Exception", HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new ChatServiceException("Failed to reach chat service", e,
                    "Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }

        public async Task SendMessage(string conversationId, SendMessageDtoV2 messageDto)
        {
            try
            {
                HttpResponseMessage response =
                    await httpClient.PostAsync($"api/v2/conversation/{conversationId}",
                        new StringContent(JsonConvert.SerializeObject(messageDto), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    throw new ChatServiceException("Failed to retrieve user profile", response.ReasonPhrase, response.StatusCode);
                }
            }
            catch (JsonException e)
            {
                throw new ChatServiceException("Failed to deserialize the response", e,
                    "Serialization Exception", HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new ChatServiceException("Failed to reach chat service", e,
                    "Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }
    }
}
