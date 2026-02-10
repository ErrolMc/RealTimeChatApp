using ChatApp.Shared.Messages;
using ChatApp.Shared.Tables;
using Microsoft.Azure.Cosmos;
using User = ChatApp.Shared.Tables.User;
using ChatApp.Shared;
using ChatApp.Backend.Services;

namespace ChatApp.Backend.Repositories
{
    public class MessagesRepository
    {
        private readonly DatabaseService _db;
        private readonly QueryService _queries;

        public MessagesRepository(DatabaseService db, QueryService queries)
        {
            _db = db;
            _queries = queries;
        }

        public async Task<(SendMessageResponseData result, List<string> recipientUserIds)> SendMessage(SendMessageRequestData requestData)
        {
            var fromUserResp = await _queries.GetUserFromUserID(requestData.FromUserID);
            if (fromUserResp.connectionSuccess == false)
                return (new SendMessageResponseData { Success = false, NotificationSuccess = false, ResponseMessage = fromUserResp.message }, new List<string>());

            if (fromUserResp.user == null)
                return (new SendMessageResponseData { Success = false, NotificationSuccess = false, ResponseMessage = $"Couldnt find user {requestData.FromUserID}" }, new List<string>());

            Message message = new Message()
            {
                ID = Guid.NewGuid().ToString(),
                FromUser = fromUserResp.user.ToUserSimple(),
                ThreadID = requestData.ThreadID,
                MessageContents = requestData.Message,
                MessageType = requestData.MessageType,
                TimeStamp = DateTime.UtcNow.Ticks,
            };

            ItemResponse<Message> messageResponse = await _db.MessagesContainer.CreateItemAsync(message, new PartitionKey(message.ThreadID));

            if (messageResponse == null)
                return (new SendMessageResponseData { Success = false, NotificationSuccess = false, ResponseMessage = $"Message {message.ID} couldnt be added to DB" }, new List<string>());

            List<string> recipientUserIds = new List<string>();

            switch ((MessageType)requestData.MessageType)
            {
                case MessageType.DirectMessage:
                    recipientUserIds.Add(requestData.MetaData);
                    break;
                case MessageType.GroupMessage:
                    {
                        var getGroupDMResponse = await _queries.GetChatThreadFromThreadID(requestData.ThreadID);
                        if (getGroupDMResponse.connectionSuccess == false)
                            return (new SendMessageResponseData { Success = true, NotificationSuccess = false, ResponseMessage = $"Message {message.ID} added to DB but couldnt get group from DB to send notifications" }, new List<string>());

                        var getParticipantsResp = await _queries.GetUsers(getGroupDMResponse.thread.Users);
                        if (getParticipantsResp.connectionSuccess == false)
                            return (new SendMessageResponseData { Success = true, NotificationSuccess = false, ResponseMessage = $"Message {message.ID} added to DB but couldnt get group participants from DB to send notifications" }, new List<string>());

                        recipientUserIds = getParticipantsResp.users
                            .Where(user => user.UserID != requestData.FromUserID)
                            .Select(user => user.UserID)
                            .ToList();
                    }
                    break;
            }

            return (new SendMessageResponseData { Success = true, NotificationSuccess = true, ResponseMessage = $"Message {message.ID} sent and added to DB", Message = message }, recipientUserIds);
        }

        public async Task<GetMessagesResponseData> GetMessages(GetMessagesRequestData requestData)
        {
            var messagesResp = await _queries.GetMessagesByThreadIDAfterTimeStamp(requestData.ThreadID, requestData.LocalTimeStamp);

            if (messagesResp.connectionSuccess == false)
                return new GetMessagesResponseData { Success = false, ResponseMessage = $"Failed to get messages for Thread {requestData.ThreadID}" };

            return new GetMessagesResponseData { Success = true, ResponseMessage = "Success", Messages = messagesResp.messages };
        }
    }
}
