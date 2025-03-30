# Chat Service API Documentation

## Overview
This document outlines the REST API and WebSocket endpoints for a **real-time chat service** built with **.NET SignalR, MSSQL, and JWT authentication**.

## Base URL
```
https://yourdomain.com/api/
```

## Authentication
All secured endpoints require **JWT authentication** via the `Authorization` header:
```
Authorization: Bearer <your_token>
```

---

## **1. Authentication & User Management**

| Method | Endpoint                         | Description                                | Auth Required |
|--------|----------------------------------|--------------------------------------------|--------------|
| POST   | `/api/auth/register`             | Register a new user                        | ❌           |
| POST   | `/api/auth/login`                | Authenticate and generate JWT token        | ❌           |
| POST   | `/api/auth/logout`               | Logout user (invalidate token)             | ✅           |
| POST   | `/api/auth/refresh-token`        | Refresh an existing JWT token              | ✅           |
| GET    | `/api/users/me`                  | Get current user profile                   | ✅           |
| PUT    | `/api/users/me`                  | Update current user profile                | ✅           |
| GET    | `/api/users/{id}`                | Get user profile by ID                     | ✅           |
| GET    | `/api/users/search?query={name}` | Search users by name/username              | ✅           |
| PUT    | `/api/users/{id}/status`         | Update user online/offline status          | ✅           |

### **1.1 Register**
```http
POST /api/auth/register
```
**Request Body:**
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```
**Response:**
```json
{
  "userId": "12345",
  "username": "john_doe",
  "email": "john@example.com"
}
```

### **1.2 Login**
```http
POST /api/auth/login
```
**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```
**Response:**
```json
{
  "token": "your_jwt_token",
  "refreshToken": "your_refresh_token",
  "expiresIn": 3600
}
```

---

## **2. Real-Time Messaging (SignalR Hub)**

| Method | Hub Method                                                   | Description                          |
|--------|--------------------------------------------------------------|--------------------------------------|
| Hub    | `SendMessage(string receiverId, string message)`             | Send a private message               |
| Hub    | `SendGroupMessage(string groupId, string message)`           | Send a message in a group            |
| Hub    | `ReceiveMessage(string senderId, string message)`            | Receive messages from another user   |
| Hub    | `ReceiveGroupMessage(string groupId, string senderId, string message)` | Receive group messages      |
| Hub    | `UpdateTypingStatus(string receiverId, bool isTyping)`       | Notify when a user is typing         |
| Hub    | `UpdateReadStatus(string messageId)`                         | Mark a message as read               |
| Hub    | `UpdateOnlineStatus(string userId, bool isOnline)`           | Update user's online status          |
| Hub    | `JoinGroup(string groupId)`                                  | Join a chat group                    |
| Hub    | `LeaveGroup(string groupId)`                                 | Leave a chat group                   |

### **2.1 SignalR Hub Connection**
Clients must connect to the SignalR hub at:
```
/ws/chat
```
Use JWT for authentication in the connection request.

---

## **3. Direct Messaging**

| Method | Endpoint                                               | Description                              | Auth Required |
|--------|--------------------------------------------------------|------------------------------------------|--------------|
| POST   | `/api/messages/send`                                   | Send a direct message                    | ✅           |
| GET    | `/api/messages/history/{userId}?page=1&pageSize=20`    | Fetch chat history with a user (paginated) | ✅         |
| PUT    | `/api/messages/{messageId}`                            | Edit a sent message                      | ✅           |
| DELETE | `/api/messages/{messageId}`                            | Soft delete a message                    | ✅           |
| PUT    | `/api/messages/{messageId}/read`                       | Mark a message as read                   | ✅           |

### **3.1 Send Message**
```http
POST /api/messages/send
```
**Request Body:**
```json
{
  "receiverId": "67890",
  "message": "Hello!"
}
```
**Response:**
```json
{
  "messageId": "abcdef",
  "timestamp": "2025-03-30T12:00:00Z"
}
```

### **3.2 Get Chat History**
```http
GET /api/messages/history/{userId}?page=1&pageSize=20
```
**Response:**
```json
{
  "messages": [
    {
      "messageId": "abc123",
      "senderId": "12345",
      "receiverId": "67890",
      "message": "Hey!",
      "timestamp": "2025-03-30T11:45:00Z",
      "isRead": true
    }
  ],
  "totalCount": 42,
  "pageCount": 3
}
```

---

## **4. Group Chat**

| Method | Endpoint                                    | Description                             | Auth Required |
|--------|---------------------------------------------|-----------------------------------------|--------------|
| POST   | `/api/groups/create`                        | Create a new group chat                 | ✅           |
| GET    | `/api/groups/{groupId}`                     | Get group chat details                  | ✅           |
| GET    | `/api/groups`                               | Get list of user's groups               | ✅           |
| PUT    | `/api/groups/{groupId}`                     | Update group details                    | ✅ (Admin)   |
| DELETE | `/api/groups/{groupId}`                     | Delete a group                          | ✅ (Admin)   |
| POST   | `/api/groups/{groupId}/members`             | Add users to a group                    | ✅ (Admin)   |
| DELETE | `/api/groups/{groupId}/members/{userId}`    | Remove user from a group                | ✅ (Admin)   |

### **4.1 Create Group**
```http
POST /api/groups/create
```
**Request Body:**
```json
{
  "groupName": "Developers Chat",
  "members": ["12345", "67890"]
}
```
**Response:**
```json
{
  "groupId": "grp001",
  "groupName": "Developers Chat"
}
```

### **4.2 Send Group Message**
```csharp
chatHub.SendGroupMessage(string groupId, string message)
```
**Response:** Broadcasts the message to all group members.

---

## **5. File & Media Sharing**

| Method | Endpoint                        | Description                  | Auth Required |
|--------|--------------------------------|------------------------------|--------------|
| POST   | `/api/files/upload`            | Upload a file/image          | ✅           |
| GET    | `/api/files/{fileId}`          | Get file details             | ✅           |
| DELETE | `/api/files/{fileId}`          | Delete an uploaded file      | ✅           |

### **5.1 Upload File**
```http
POST /api/files/upload
```
**Request:** Multipart file upload.
**Response:**
```json
{
  "fileId": "file123",
  "url": "https://yourstorage.com/file123.png",
  "fileName": "image.png",
  "fileSize": 128000,
  "contentType": "image/png"
}
```

---

## **6. Notifications & Presence**

| Method | Endpoint                    | Description                      | Auth Required |
|--------|----------------------------|----------------------------------|--------------|
| GET    | `/api/notifications`       | Get user's unread notifications  | ✅           |
| PUT    | `/api/notifications/read`  | Mark notifications as read       | ✅           |
| GET    | `/api/users/online`        | Get list of online users         | ✅           |

### **6.1 Get Unread Notifications**
```http
GET /api/notifications
```
**Response:**
```json
{
  "notifications": [
    {
      "id": "noti001",
      "message": "New message from John",
      "timestamp": "2025-03-30T12:10:00Z",
      "isRead": false
    }
  ]
}
```

---

## **7. Moderation & Security**

| Method | Endpoint                      | Description                 | Auth Required |
|--------|-----------------------------|------------------------------|--------------|
| POST   | `/api/reports`               | Report a user/message       | ✅           |
| POST   | `/api/users/{userId}/block`  | Block a user                | ✅           |
| DELETE | `/api/users/{userId}/block`  | Unblock a user              | ✅           |

### **7.1 Report User or Message**
```http
POST /api/reports
```
**Request Body:**
```json
{
  "reportedUserId": "67890",
  "reason": "Spam",
  "messageId": "msg123" // Optional
}
```

---

## **Technical Implementation Notes**

- **WebSocket Authentication**: The SignalR Hub requires JWT authentication.
- **Message Storage**: Messages are stored in MSSQL with pagination support.
- **File Handling**: Media files are stored in Azure Blob Storage or local storage.
- **Performance Optimizations**: Use Redis for online presence tracking.
- **Rate Limiting**: API throttling is applied for spam prevention (5 req/sec per user).
- **SQL Injection Prevention**: Parameterized queries are used throughout the application.

## **Security Measures**
- **CORS**: Properly configured to restrict origins.
- **XSS Protection**: Output encoding and input sanitization.
- **CSRF Protection**: Anti-forgery tokens for relevant operations.
- **JWT Security**: Short-lived tokens with refresh token capability.

---

## **Conclusion**
This API provides a scalable and secure solution for **real-time messaging** with **SignalR**, **MSSQL**, and **JWT authentication**.

---

### **Next Steps**
Would you like a **Swagger/OpenAPI spec** or **SignalR Hub implementation**? 🚀

