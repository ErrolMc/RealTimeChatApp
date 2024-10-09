const dbName = "ErrolChatDB";
const friendsStoreName = "Friends";
const stringStoreName = "Strings";
const threadStoreName = "Threads";
const messagesStoreName = "Messages";

// #region open/close
export function OpenDatabase() 
{
    return new Promise((resolve, reject) => 
    {
        const request = indexedDB.open(dbName);

        request.onupgradeneeded = function(event) 
        {
            const db = event.target.result;
            console.log(`Upgrading DB to version ${event.newVersion}`);
            
            if (!db.objectStoreNames.contains(friendsStoreName))
                db.createObjectStore(friendsStoreName, { keyPath: "UserID" });

            if (!db.objectStoreNames.contains(stringStoreName))
                db.createObjectStore(stringStoreName);
            
            if (!db.objectStoreNames.contains(threadStoreName))
                db.createObjectStore(threadStoreName, { keyPath: "ThreadID" });

            if (!db.objectStoreNames.contains(messagesStoreName))
            {
                const messageStore = db.createObjectStore(messagesStoreName, { keyPath: "MessageID" });
                messageStore.createIndex("ThreadID", "ThreadID", { unique: false });
            }
                
        };

        request.onerror = function(event) 
        {
            console.error("Database error: " + event.target.errorCode);
            reject(event.target.errorCode);
        };

        request.onsuccess = function(event) 
        {
            const db = event.target.result;
            resolve(db);
        };
    });
}

export function ClearCache() 
{
    return new Promise((resolve, reject) => 
    {
        const deleteRequest = indexedDB.deleteDatabase(dbName);

        deleteRequest.onsuccess = function() 
        {
            resolve("Database cleared successfully");
        };

        deleteRequest.onerror = function(event) 
        {
            reject("Failed to clear the database: " +  event.target.errorCode);
        };

        deleteRequest.onblocked = function() 
        {
            reject("Database deletion blocked");
        };
    });
}
// #endregion



// #region shared
export async function SaveString(key, value)
{
    const db = await OpenDatabase();
    const transaction = db.transaction([stringStoreName], "readwrite");
    const store = transaction.objectStore(stringStoreName);

    return new Promise((resolve, reject) =>
    {
        const request = store.put(value, key);
        request.onsuccess = () => resolve("String added successfully");
        request.onerror = () => reject(request.result);
    });
}

export async function GetString(key)
{
    const db = await OpenDatabase();
    const transaction = db.transaction([stringStoreName], "readwrite");
    const store = transaction.objectStore(stringStoreName);

    return new Promise((resolve, reject) =>
    {
        const request = store.get(key);
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject("Get string failed");
    });
}
// #endregion

// #region friends
export async function CacheFriends(friendsJson)
{
    const friends = JSON.parse(friendsJson); 
    const db = await OpenDatabase();
    const transaction = db.transaction([friendsStoreName], "readwrite");
    const store = transaction.objectStore(friendsStoreName);

    return new Promise((resolve, reject) => 
    {
        const clearRequest = store.clear();

        clearRequest.onsuccess = () =>
        {
            const promises = friends.map(friend => 
            {
                return new Promise((innerResolve, innerReject) => 
                {
                    const request = store.add(friend); 
                    request.onsuccess = () => innerResolve();
                    request.onerror = () => innerReject(`Failed to cache friend: ${friend.id}`);
                });
            });

            Promise.all(promises)
                .then(() => resolve("All friends cached successfully"))
                .catch(err => reject(err));
        };

        clearRequest.onerror = () => reject("Failed to clear the store");
    });
}

export async function GetFriends() 
{
    const db = await OpenDatabase();
    const transaction = db.transaction([friendsStoreName], "readonly");
    const store = transaction.objectStore(friendsStoreName);
    return new Promise((resolve, reject) => 
    {
        const request = store.getAll();
        request.onsuccess = () => resolve(JSON.stringify(request.result)); // Stringify result to pass back to C#
        request.onerror = () => reject("Failed to retrieve users");
    });
}
// #endregion

// #region threads
export async function AddThreads(threadsJson)
{
    const threads = JSON.parse(threadsJson);
    const db = await OpenDatabase();
    const transaction = db.transaction([threadStoreName], "readwrite");
    const store = transaction.objectStore(threadStoreName);

    return new Promise((resolve, reject) =>
    {
        const promises = threads.map(thread =>
        {
            return new Promise((innerResolve, innerReject) =>
            {
                const request = store.put(thread);
                request.onsuccess = () => innerResolve();
                request.onerror = () => innerReject(`Failed to cache thread: ${thread.id}`);
            });
        });

        Promise.all(promises)
            .then(() => resolve("Threads added successfully"))
            .catch(err => reject(err));
    });
}

export async function RemoveThreads(threadIDsJson)
{
    const threads = JSON.parse(threadIDsJson);
    const db = await OpenDatabase();
    const transaction = db.transaction([threadStoreName], "readwrite");
    const store = transaction.objectStore(threadStoreName);

    return new Promise((resolve, reject) =>
    {
        const promises = threads.map(threadID =>
        {
            return new Promise((innerResolve, innerReject) =>
            {
                const request = store.delete(threadID);
                request.onsuccess = () => innerResolve();
                request.onerror = () => innerReject(`Failed to remove thread: ${threadID}`);
            });
        });

        Promise.all(promises)
            .then(() => resolve("Threads removed successfully"))
            .catch(err => reject(err));
    });
}

export async function GetAllThreads()
{
    const db = await OpenDatabase();
    const transaction = db.transaction([threadStoreName], "readonly");
    const store = transaction.objectStore(threadStoreName);
    return new Promise((resolve, reject) =>
    {
        const request = store.getAll();
        request.onsuccess = () => resolve(JSON.stringify(request.result)); // Stringify result to pass back to C#
        request.onerror = () => reject("Failed to retrieve threads");
    });
}

export async function UpdateThread(threadJSON)
{
    const thread = JSON.parse(threadJSON);
    const db = await OpenDatabase();
    const transaction = db.transaction([threadStoreName], "readwrite");
    const store = transaction.objectStore(threadStoreName);

    return new Promise((resolve, reject) =>
    {
        const request = store.put(thread);
        request.onsuccess = () => resolve("String added successfully");
        request.onerror = () => reject(request.result);
    });
}
// #endregion

// #region messages
export async function CacheMessages(messagesJson)
{
    const messages = JSON.parse(messagesJson);
    const db = await OpenDatabase();
    const transaction = db.transaction([messagesStoreName], "readwrite");
    const store = transaction.objectStore(messagesStoreName);

    return new Promise((resolve, reject) =>
    {
        const promises = messages.map(thread =>
        {
            return new Promise((innerResolve, innerReject) =>
            {
                const request = store.put(thread);
                request.onsuccess = () => innerResolve();
                request.onerror = () => innerReject(`Failed to cache message: ${thread.id}`);
            });
        });

        Promise.all(promises)
            .then(() => resolve("Messages added successfully"))
            .catch(err => reject(err));
    });
}

export async function GetMessagesFromThread(threadID)
{
    const db = await OpenDatabase();
    const transaction = db.transaction([messagesStoreName], "readonly");
    const store = transaction.objectStore(messagesStoreName);
    const index = store.index("ThreadID");

    return new Promise((resolve, reject) =>
    {
        const request = index.getAll(threadID);
        request.onsuccess = () => resolve(JSON.stringify(request.result));
        request.onerror = () => reject("Failed to retrieve messages");
    });
}

export async function ClearMessageThread(threadID)
{
    const db = await OpenDatabase();
    const transaction = db.transaction([messagesStoreName], "readwrite");
    const store = transaction.objectStore(messagesStoreName);
    const index = store.index("ThreadID");

    return new Promise((resolve, reject) =>
    {
        const request = index.openCursor(IDBKeyRange.only(threadID));
        request.onsuccess = function(event)
        {
            const cursor = event.target.result;
            if (cursor)
            {
                cursor.delete();
                cursor.continue();
            }
            else
            {
                resolve("Messages deleted successfully");
            }
        };
        request.onerror = () => reject("Failed to delete messages");
    });
}
// #endregion
