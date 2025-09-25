using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Grocery.Core.Helpers;                
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;

namespace Grocery.Core.Data.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly string _path =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "clients.json");

        private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
        private List<Client> _clients;

        public ClientRepository()
        {
            _clients = Load() ?? Seed();
            if (EnsureSeeds()) Save();
        }

        public Client? Get(string email) =>
            string.IsNullOrWhiteSpace(email) ? null :
            _clients.FirstOrDefault(c => string.Equals(c.EmailAddress, email, StringComparison.OrdinalIgnoreCase));

        public Client? Get(int id) => _clients.FirstOrDefault(c => c.Id == id);

        public List<Client> GetAll() => new(_clients);

        public Client Add(Client client)
        {
            ArgumentNullException.ThrowIfNull(client);
            if (Get(client.EmailAddress) is not null) throw new InvalidOperationException("Email already exists.");
            var id = _clients.Any() ? _clients.Max(c => c.Id) + 1 : 1;
            var created = new Client(id, client.Name, client.EmailAddress, client.Password);
            _clients.Add(created); Save();
            return created;
        }

        public void Update(Client client)
        {
            ArgumentNullException.ThrowIfNull(client);
            var i = _clients.FindIndex(c => c.Id == client.Id);
            if (i >= 0) { _clients[i] = client; Save(); return; }
            i = _clients.FindIndex(c => string.Equals(c.EmailAddress, client.EmailAddress, StringComparison.OrdinalIgnoreCase));
            if (i >= 0)
            {
                var old = _clients[i];
                _clients[i] = new Client(old.Id, client.Name, client.EmailAddress, client.Password);
                Save(); return;
            }
            Add(client);
        }
        
        private List<Client>? Load()
        {
            try
            {
                if (!File.Exists(_path)) return null;
                return JsonSerializer.Deserialize<List<Client>>(File.ReadAllText(_path), _json);
            }
            catch { return null; }
        }

        private void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                File.WriteAllText(_path, JsonSerializer.Serialize(_clients, _json));
            }
            catch { }
        }

        private List<Client> Seed() => new()
        {
            new Client(1, "M.J. Curie",   "user1@mail.com", PasswordHelper.HashPassword("user1")),
            new Client(2, "H.H. Hermans", "user2@mail.com", PasswordHelper.HashPassword("user2")),
            new Client(3, "A.J. Kwak",    "user3@mail.com", PasswordHelper.HashPassword("user3"))
        };

        private bool EnsureSeeds()
        {
            bool changed = false;
            changed |= Ensure("user1@mail.com", "user1", "M.J. Curie");
            changed |= Ensure("user2@mail.com", "user2", "H.H. Hermans");
            changed |= Ensure("user3@mail.com", "user3", "A.J. Kwak");
            return changed;

            bool Ensure(string email, string pwd, string name)
            {
                var c = _clients.FirstOrDefault(x => string.Equals(x.EmailAddress, email, StringComparison.OrdinalIgnoreCase));
                if (c is null)
                {
                    var id = _clients.Any() ? _clients.Max(x => x.Id) + 1 : 1;
                    _clients.Add(new Client(id, name, email, PasswordHelper.HashPassword(pwd)));
                    return true;
                }
                if (!LooksLikeHash(c.Password))
                {
                    c.Password = PasswordHelper.HashPassword(pwd);
                    return true;
                }
                return false;
            }

            static bool LooksLikeHash(string? v)
            {
                if (string.IsNullOrWhiteSpace(v)) return false;
                var p = v.Split('.'); if (p.Length != 2) return false;
                return B64(p[0]) && B64(p[1]);

                static bool B64(string s)
                {
                    try
                    {
                        s = (s ?? "").Trim().Replace('-', '+').Replace('_', '/');
                        var m = s.Length % 4; if (m == 2) s += "=="; else if (m == 3) s += "=";
                        _ = Convert.FromBase64String(s); return true;
                    }
                    catch { return false; }
                }
            }
        }
    }
}
