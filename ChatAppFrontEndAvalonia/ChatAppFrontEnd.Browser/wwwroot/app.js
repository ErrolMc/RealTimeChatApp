const dbName = "MyDatabase";
const storeName = "Users";

export function openDatabase() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(dbName, 1);

        request.onupgradeneeded = function(event) {
            const db = event.target.result;
            if (!db.objectStoreNames.contains(storeName)) {
                db.createObjectStore(storeName, { keyPath: "UserID" });
            }
        };

        request.onerror = function(event) {
            console.error("Database error: " + event.target.errorCode);
            reject(event.target.errorCode);
        };

        request.onsuccess = function(event) {
            resolve(event.target.result);
        };
    });
}

export async function addUser(user) {
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
    const db = await openDatabase();
    const transaction = db.transaction([storeName], "readonly");
    const store = transaction.objectStore(storeName);
    return new Promise((resolve, reject) => {
        const request = store.getAll();
        request.onsuccess = () => resolve(JSON.stringify(request.result)); // Stringify result to pass back to C#
        request.onerror = () => reject("Failed to retrieve users");
    });
}