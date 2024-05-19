using System;
using ChatApp.Services;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Tables;
using Newtonsoft.Json;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class NotificationService : INotificationService
    {
        private readonly Lazy<IFriendService> _friendService;
        private readonly Lazy<IChatService> _chatService;

        public NotificationService(Lazy<IFriendService> friendService, Lazy<IChatService> chatService)
        {
            _friendService = friendService;
            _chatService = chatService;
        }
        
        public bool HandleNotification(string json)
        {
            Console.WriteLine("HandleNotification: " + json);
            
            var notificationData = JsonConvert.DeserializeObject<NotificationData>(json);
            if (notificationData == null)
                return false;

            NotificationType notificationType = (NotificationType)notificationData.NotificationType;
            switch (notificationType)
            {
                case NotificationType.FriendRequest:
                    {
                        var notification = JsonConvert.DeserializeObject<FriendRequestNotification>(notificationData.NotificationJson);
                        if (notification == null)
                            return false;
                        Console.WriteLine("Friend request from " + notification.FromUser.UserName);
                        if (_friendService.IsValueCreated)
                            _friendService.Value?.OnReceiveFriendRequestNotification(notification);   
                    }
                    break;
                case NotificationType.FriendRequestRespond:
                    {
                        var notification = JsonConvert.DeserializeObject<FriendRequestRespondNotification>(notificationData.NotificationJson);
                        Console.WriteLine($"Friend request responded from {notification.ToUser.UserName}: {notification.Status}");
                        if (_friendService.IsValueCreated)
                            _friendService.Value.ProcessFriendRequestResponse(notification);
                    }
                    break;
                case NotificationType.FriendRequestCancel:
                    {
                        var fromUser = JsonConvert.DeserializeObject<UserSimple>(notificationData.NotificationJson);
                        
                        Console.WriteLine($"Friend request canceled from: {fromUser.UserName}");
                        if (_friendService.IsValueCreated)
                            _friendService.Value.OnCancelFriendRequest(fromUser);
                    }
                    break;
                case NotificationType.DirectMessage:
                    {
                        var message = JsonConvert.DeserializeObject<Message>(notificationData.NotificationJson);
                        
                        Console.WriteLine($"Message from {message.FromUser.UserName}");
                        if (_chatService.IsValueCreated)
                            _chatService.Value.OnReceiveMessage(message);
                    }
                    break;
            }

            return true;
        }
    }
}