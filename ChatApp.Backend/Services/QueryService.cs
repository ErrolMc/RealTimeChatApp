using ChatApp.Shared.Tables;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos;
using User = ChatApp.Shared.Tables.User;
using ChatApp.Shared;

namespace ChatApp.Backend.Services
{
    public class QueryService
    {
        private const string GENERIC_DATABASE_ERROR = "Database Connection/Query Error";
        private readonly DatabaseService _db;

        public QueryService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<Result<User>> GetUserFromUsername(string username)
        {
            try
            {
                IQueryable<User> query = _db.UsersContainer.GetItemLinqQueryable<User>().Where(u => u.Username == username);
                FeedIterator<User> iterator = query.ToFeedIterator();
                FeedResponse<User> users = await iterator.ReadNextAsync();

                if (!users.Any())
                    return new Result<User>(ResultType.NotFound, $"No user with username {username}");

                return Result<User>.Success(users.First());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserFromUserName Error {ex.Message}");
                return Result<User>.Failure(GENERIC_DATABASE_ERROR);
            }
        }

        public async Task<Result<User>> GetUserFromUserID(string userID)
        {
            try
            {
                var userResponse = await _db.UsersContainer.ReadItemAsync<User>(userID, new PartitionKey(userID));

                if (userResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"GetUserFromUserID Error {userResponse.StatusCode}");
                    return new Result<User>(ResultType.NotFound, $"Cant find user {userID}");
                }

                return Result<User>.Success(userResponse.Resource);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserFromUserID Error {ex.Message}");
                return Result<User>.Failure(GENERIC_DATABASE_ERROR);
            }
        }

        public async Task<Result<List<User>>> GetUsers(List<string> userIDs)
        {
            if (userIDs == null || userIDs.Count() == 0)
                return new Result<List<User>>(ResultType.InputError, "No user ids provided", new List<User>());

            try
            {
                string inClause = string.Join(", ", userIDs.Select(id => $"'{id}'"));
                string queryString = $"SELECT * FROM c WHERE c.id IN ({inClause})";
                QueryDefinition queryDefinition = new QueryDefinition(queryString);
                FeedIterator<User> queryResultSetIterator = _db.UsersContainer.GetItemQueryIterator<User>(queryDefinition);

                List<User> users = new List<User>();
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<User> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (var user in currentResultSet)
                    {
                        users.Add(user);
                    }
                }

                if (userIDs.Count != users.Count)
                    return new Result<List<User>>(ResultType.FoundButInvalid, "Coundn't get all users", users);

                return Result<List<User>>.Success(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUsers Error {ex.Message}");
                return Result<List<User>>.Failure(GENERIC_DATABASE_ERROR);
            }
        }

        public async Task<Result<List<Message>>> GetMessagesByThreadID(string threadID)
        {
            try
            {
                List<Message> messages = new List<Message>();
                IQueryable<Message> query = _db.MessagesContainer.GetItemLinqQueryable<Message>().Where(m => m.ThreadID == threadID);
                FeedIterator<Message> iterator = query.ToFeedIterator();

                while (iterator.HasMoreResults)
                {
                    FeedResponse<Message> response = await iterator.ReadNextAsync();
                    messages.AddRange(response.ToList());
                }
                return Result<List<Message>>.Success(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving messages for thread ID {threadID}: {ex.Message}");
                return Result<List<Message>>.Failure(GENERIC_DATABASE_ERROR);
            }
        }

        public async Task<Result<List<Message>>> GetMessagesByThreadIDAfterTimeStamp(string threadID, long timeStamp)
        {
            try
            {
                List<Message> messages = new List<Message>();
                IQueryable<Message> query = _db.MessagesContainer.GetItemLinqQueryable<Message>().Where(m => m.ThreadID == threadID && m.TimeStamp > timeStamp);
                FeedIterator<Message> iterator = query.ToFeedIterator();

                while (iterator.HasMoreResults)
                {
                    FeedResponse<Message> response = await iterator.ReadNextAsync();
                    messages.AddRange(response.ToList());
                }
                return Result<List<Message>>.Success(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving messages for thread ID {threadID}: {ex.Message}");
                return Result<List<Message>>.Failure(GENERIC_DATABASE_ERROR);
            }
        }

        public async Task<Result> DeleteMessagesByThreadID(string threadID)
        {
            try
            {
                var getResponse = await GetMessagesByThreadID(threadID);
                if (getResponse.IsSuccessful == false)
                    return Result.Failure(getResponse.ErrorMessage);

                List<Message> messagesToDelete = getResponse.Data;

                var deleteTasks = messagesToDelete.Select(async message =>
                {
                    await _db.MessagesContainer.DeleteItemAsync<Message>(message.ID, new PartitionKey(message.ThreadID));
                });

                await Task.WhenAll(deleteTasks);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting messages for thread ID {threadID}: {ex.Message}");
                return Result.Failure(GENERIC_DATABASE_ERROR);
            }
        }

        public async Task<Result<ChatThread>> GetChatThreadFromThreadID(string threadID)
        {
            try
            {
                var groupResponse = await _db.ChatThreadsContainer.ReadItemAsync<ChatThread>(threadID, new PartitionKey(threadID));

                if (groupResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"GetGroupDMFromGroupID Error {groupResponse.StatusCode}");
                    return new Result<ChatThread>(ResultType.NotFound, $"Cant find Chat Thread {threadID}");
                }

                return Result<ChatThread>.Success(groupResponse.Resource);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetGroupDMFromGroupID Error {ex.Message}");
                return Result<ChatThread>.Failure(GENERIC_DATABASE_ERROR);
            }
        }

        public async Task<Result<List<ChatThread>>> GetChatThreadsFromIDs(List<string> threadIDs)
        {
            if (threadIDs == null || threadIDs.Count() == 0)
                return new Result<List<ChatThread>>(ResultType.InputError, "No group ids provided");

            try
            {
                string inClause = string.Join(", ", threadIDs.Select(id => $"'{id}'"));
                string queryString = $"SELECT * FROM c WHERE c.id IN ({inClause})";
                QueryDefinition queryDefinition = new QueryDefinition(queryString);
                FeedIterator<ChatThread> queryResultSetIterator = _db.ChatThreadsContainer.GetItemQueryIterator<ChatThread>(queryDefinition);

                List<ChatThread> groupDMs = new List<ChatThread>();
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<ChatThread> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (ChatThread groupDM in currentResultSet)
                    {
                        groupDMs.Add(groupDM);
                    }
                }

                return Result<List<ChatThread>>.Success(groupDMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetGroupDMs Error {ex.Message}");
                return Result<List<ChatThread>>.Failure(GENERIC_DATABASE_ERROR);
            }
        }
    }
}
