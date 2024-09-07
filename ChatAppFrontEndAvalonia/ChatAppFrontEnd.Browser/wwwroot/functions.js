const dbName = "ErrolChatDB";
const friendsStoreName = "Friends";
const stringStoreName = "Strings";

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