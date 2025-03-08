# API Documentation

## Overview
This document describes the API endpoints available in the Real-Time Chat Application.

## Base URLs
- **Azure Functions API**: `https://{your-function-app}.azurewebsites.net/api`
- **SignalR Hub**: `https://{your-signalr-service}.service.signalr.net`

## Authentication
All API endpoints require authentication using JWT tokens. Include the token in the Authorization header:
```
Authorization: Bearer {your-jwt-token}
```

## REST API Endpoints

### User Management

#### Register User
```http
POST /users/register
Content-Type: application/json

{
    "username": "string",
    "email": "string",
    "password": "string"
}
```

#### Login
```http
POST /users/login
Content-Type: application/json

{
    "email": "string",
    "password": "string"
}
```

#### Get User Profile
```http
GET /users/{userId}
```

#### Update User Profile
```http
PUT /users/{userId}
Content-Type: application/json

{
    "username": "string",
    "avatar": "string",
    "status": "string"
}
```

### Chat Rooms

#### Create Chat Room
```http
POST /chatrooms
Content-Type: application/json

{
    "name": "string",
    "description": "string",
    "isPrivate": boolean
}
```

#### Get Chat Rooms
```http
GET /chatrooms
```

#### Get Chat Room Details
```http
GET /chatrooms/{roomId}
```

#### Join Chat Room
```http
POST /chatrooms/{roomId}/join
```

#### Leave Chat Room
```http
POST /chatrooms/{roomId}/leave
```

### Messages

#### Send Message
```http
POST /messages
Content-Type: application/json

{
    "roomId": "string",
    "content": "string",
    "type": "text|image|file"
}
```

#### Get Messages
```http
GET /messages/{roomId}?before={timestamp}&limit={number}
```

#### Delete Message
```http
DELETE /messages/{messageId}
```

## SignalR Hub Methods

### Client Methods (to call from client)

#### Connect to Chat Room
```csharp
await hubConnection.InvokeAsync("JoinRoom", roomId);
```

#### Send Message
```csharp
await hubConnection.InvokeAsync("SendMessage", new
{
    RoomId = roomId,
    Content = content,
    Type = type
});
```

#### Update Presence
```csharp
await hubConnection.InvokeAsync("UpdatePresence", status);
```

### Server Methods (to handle in client)

#### Receive Message
```csharp
hubConnection.On<ChatMessage>("ReceiveMessage", (message) =>
{
    // Handle new message
});

public class ChatMessage
{
    public string Id { get; set; }
    public string RoomId { get; set; }
    public string SenderId { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}
```

#### User Presence Update
```csharp
hubConnection.On<PresenceUpdate>("UserPresenceChanged", (update) =>
{
    // Handle presence update
});

public class PresenceUpdate
{
    public string UserId { get; set; }
    public string Status { get; set; }
    public DateTime LastSeen { get; set; }
}
```

#### Room Update
```csharp
hubConnection.On<ChatRoom>("RoomUpdated", (room) =>
{
    // Handle room update
});

public class ChatRoom
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> Participants { get; set; }
}
```

## Response Formats

### Success Response
```json
{
    "success": true,
    "data": {
        // Response data
    }
}
```

### Error Response
```json
{
    "success": false,
    "error": {
        "code": "string",
        "message": "string"
    }
}
```

## Rate Limiting
- API requests are limited to 100 requests per minute per user
- WebSocket connections are limited to 5 concurrent connections per user

## Error Codes
- `401`: Unauthorized
- `403`: Forbidden
- `404`: Resource not found
- `429`: Too many requests
- `500`: Internal server error