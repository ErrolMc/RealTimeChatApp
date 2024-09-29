using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using ChatApp.Shared.TableDataSimple;
using ChatAppFrontEnd.Source.Other.Caching.Data;

namespace ChatApp.Shared
{
    public static class SharedStaticMethods
    {
        public static string CreateHashedDirectMessageID(string userId1, string userId2)
        {
            string combined = string.CompareOrdinal(userId1, userId2) < 0 ? $"{userId1}-{userId2}" : $"{userId2}-{userId1}";
            
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(combined));
                
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }
        
        public static List<UserSimple> ToUserSimpleList(this List<UserSimpleCache> userCacheList)
        {
            List<UserSimple> userSimples = new List<UserSimple>();
            foreach (UserSimpleCache userCache in userCacheList)
                userSimples.Add(userCache);
            return userSimples;
        }
    }
}