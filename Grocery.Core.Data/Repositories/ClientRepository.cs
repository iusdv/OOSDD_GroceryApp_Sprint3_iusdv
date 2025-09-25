using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;

namespace Grocery.Core.Data.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

        private List<Client> clientList;

        public ClientRepository()
        {
            _filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "clients.json"
            );

            // Try to load saved users; if no file (first run), seed and save once
            var loaded = LoadFromDisk();
            if (loaded != null)
            {
                clientList = loaded;
            }
            else
            {
                clientList = new List<Client>
                {
                    new Client(1, "M.J. Curie",   "user1@mail.com", "IunRhDKa+fWo8+4/Qfj7Pg==.kDxZnUQHCZun6gLIE6d9oeULLRIuRmxmH2QKJv2IM08="),
                    new Client(2, "H.H. Hermans", "user2@mail.com", "dOk+X+wt+MA9uIniRGKDFg==.QLvy72hdG8nWj1FyL75KoKeu4DUgu5B/HAHqTD2UFLU="),
                    new Client(3, "A.J. Kwak",    "user3@mail.com", "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EUNY2YbvkvndhbnkIHA=")
                };
                SaveToDisk();
            }
        }
        public Client? Get(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            return clientList.FirstOrDefault(c =>
                string.Equals(c.EmailAddress, email, StringComparison.OrdinalIgnoreCase));
        }

        public Client? Get(int id)
        {
            return clientList.FirstOrDefault(c => c.Id == id);
        }

        public List<Client> GetAll()
        {
            return new List<Client>(clientList);
        }

        public Client Add(Client client)
        {
            ArgumentNullException.ThrowIfNull(client);
            
            if (Get(client.EmailAddress) is not null)
                throw new InvalidOperationException("A client with this email already exists.");

            
            var newId = clientList.Any() ? clientList.Max(c => c.Id) + 1 : 1;

         
            var created = new Client(newId, client.Name, client.EmailAddress, client.Password);

            clientList.Add(created);
            SaveToDisk();  

            return created;
        }

        private List<Client>? LoadFromDisk()
        {
            try
            {
                if (!File.Exists(_filePath)) return null;
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<Client>>(json, _jsonOptions);
            }
            catch
            {
             
                return null;
            }
        }

        private void SaveToDisk()
        {
            try
            {
                var folder = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var json = JsonSerializer.Serialize(clientList, _jsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch
            {
             
            }
        }
    }
}
