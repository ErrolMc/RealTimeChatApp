using System;
using ChatApp.Services;
using ChatApp.Shared.Enums;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.Messages;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatApp.Source.Services;
using Newtonsoft.Json;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class NotificationService : INotificationService
    {
        private readonly Lazy<IFriendService> _friendService;
        private readonly Lazy<IChatService> _chatService;
        private readonly Lazy<IGroupService> _groupService;

        public NotificationService(Lazy<IFriendService> friendService, Lazy<IChatService> chatService, Lazy<IGroupService> groupService)
        {
            _friendService = friendService;
            _chatService = chatService;
            _groupService = groupService;
        }
        
        public bool HandleNotification(string json)
        {
            Console.WriteLine("HandleNotification: " + json);
            
            NotificationData notificationData = null;

            try
            {
                notificationData = JsonConvert.DeserializeObject<NotificationData>(json);
                if (notificationData == null)
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }

            NotificationType notificationType = (NotificationType)notificationData.NotificationType;
            switch (notificationType)
            {
                case NotificationType.FriendRequest:
                    {
                        var notification = JsonConvert.DeserializeObject<FriendRequestNotification>(notificationData.NotificationJson);
                        if (notification == null)
                            return false;
                        Console.WriteLine("Friend request from " + notification.FromUser.UserName);
                        _friendService.Value?.OnReceiveFriendRequestNotification(notification);   
                    }
                    break;
                case NotificationType.FriendRequestRespond:
                    {
                        var notification = JsonConvert.DeserializeObject<FriendRequestRespondNotification>(notificationData.NotificationJson);
                        Console.WriteLine($"Friend request responded from {notification.ToUser.UserName}: {notification.Status}");
                        _friendService.Value?.ProcessFriendRequestResponse(notification);
                    }
                    break;
                case NotificationType.Unfriend:
                    {
                        var notification = JsonConvert.DeserializeObject<UnfriendNotification>(notificationData.NotificationJson);
                        Console.WriteLine($"Unfriended by: {notification.FromUserID}");
                        _friendService.Value?.OnUnfriend(notification);
                    }
                    break;
                case NotificationType.FriendRequestCancel:
                    {
                        var fromUser = JsonConvert.DeserializeObject<UserSimple>(notificationData.NotificationJson);
                        
                        Console.WriteLine($"Friend request canceled from: {fromUser.UserName}");
                        _friendService.Value?.OnCancelFriendRequest(fromUser);
                    }
                    break;
                case NotificationType.DirectMessage:
                case NotificationType.GroupDMMessage:
                    {
                        var message = JsonConvert.DeserializeObject<Message>(notificationData.NotificationJson);
                        
                        Console.WriteLine($"{((MessageType)message.MessageType).ToString()} message of type from {message.FromUser.UserName}");
                        _chatService.Value?.OnReceiveMessage(message);
                    }
                    break;
                case NotificationType.AddedToGroup:
                    {
                        GroupDMSimple groupDM = JsonConvert.DeserializeObject<GroupDMSimple>(notificationData.NotificationJson);
                            
                        Console.WriteLine($"Invited to group {groupDM.GroupID}");
                        _groupService.Value?.AddGroupLocally(groupDM);
                    }
                    break;
                case NotificationType.GroupUpdated:
                    {
                        GroupDMSimple groupDM = JsonConvert.DeserializeObject<GroupDMSimple>(notificationData.NotificationJson);
                        
                        Console.WriteLine($"Group Updated {groupDM.GroupID}");
                        _groupService.Value?.UpdateGroupLocally(groupDM, GroupUpdateReason.DoesntMatter);
                    }
                    break;
                case NotificationType.KickedFromGroup:
                    {
                        GroupDMSimple groupDM = JsonConvert.DeserializeObject<GroupDMSimple>(notificationData.NotificationJson);
                        
                        Console.WriteLine($"Kicked from group {groupDM.GroupID}");
                        _groupService.Value?.UpdateGroupLocally(groupDM, GroupUpdateReason.ThisUserKicked);
                    }
                    break;
                case NotificationType.GroupDeleted:
                    {
                        GroupDMSimple groupDM = JsonConvert.DeserializeObject<GroupDMSimple>(notificationData.NotificationJson);
                            
                        Console.WriteLine($"Deleted group {groupDM.GroupID}");
                        _groupService.Value?.UpdateGroupLocally(groupDM, GroupUpdateReason.GroupDeleted);
                    }
                    break;
            }

            return true;
        }
    }
}