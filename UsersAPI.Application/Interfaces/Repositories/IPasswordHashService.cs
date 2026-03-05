using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Interfaces.Repositories
{
    public interface IPasswordHashService
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }
}
