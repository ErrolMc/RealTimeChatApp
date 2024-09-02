const dbName = "ErrolChatDB";
const storeName = "Users";
const tokenStoreName = "LoginTokens";

export function openDatabase() 
{
    return new Promise((resolve, reject) => 
    {
        const request = indexedDB.open(dbName);

        request.onupgradeneeded = function(event) 
        {
            const db = event.target.result;
            console.log(`Upgrading DB to version ${event.newVersion}`);
            //if (!db.objectStoreNames.contains(storeName)) 
            //{
            //    db.createObjectStore(storeName, { keyPath: "UserID" });
            //}
            if (!db.objectStoreNames.contains(tokenStoreName)) 
            {
                db.createObjectStore(tokenStoreName);
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
            console.log(`Opened DB version ${db.version} with stores: ${Array.from(db.objectStoreNames).join(", ")}`);
            resolve(db);
        };
    });
}

export async function addUser(user) {
    console.log('addUser');
    const db = await openDatabase();
    const transaction = db.transaction([storeName], "readwrite");
    const store = transaction.objectStore(storeName);
    return new Promise((resolve, reject) => {
        const request = store.add(JSON.parse(user)); // Parsing user object received from C#
        request.onsuccess = () => resolve("User added successfully");
        request.onerror = () => reject("Failed to add user");
    });
}

export async function getAllUsers() {
    console.log('getAllUsers');
    const db = await openDatabase();
    const transaction = db.transaction([storeName], "readonly");
    const store = transaction.objectStore(storeName);
    return new Promise((resolve, reject) => {
        const request = store.getAll();
        request.onsuccess = () => resolve(JSON.stringify(request.result)); // Stringify result to pass back to C#
        request.onerror = () => reject("Failed to retrieve users");
    });
}

export async function saveLoginToken(token)
{
    const db = await openDatabase();
    const transaction = db.transaction([tokenStoreName], "readwrite");
    const store = transaction.objectStore(tokenStoreName);

    return new Promise((resolve, reject) =>
    {
        const request = store.put(token, "authToken");
        request.onsuccess = () => resolve("Token added successfully");
        request.onerror = () => reject(request.result);
    });
}

export async function getLoginToken()
{
    const db = await openDatabase();
    const transaction = db.transaction([tokenStoreName], "readwrite");
    const store = transaction.objectStore(tokenStoreName);

    return new Promise((resolve, reject) =>
    {
        const request = store.get("authToken");
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject("Get token failed");
    });
}