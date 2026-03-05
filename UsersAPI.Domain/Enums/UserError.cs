using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Domain.Enums
{
    public enum UserError
    {
        None = 0,
        Validation = 1,
        NotFound = 2,
        Conflict = 3
    }
}
