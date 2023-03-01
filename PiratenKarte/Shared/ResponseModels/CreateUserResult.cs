using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiratenKarte.Shared.ResponseModels;
public class CreateUserResult {
    public Guid Id { get; set; }

    public bool UserCreated { get; set; }
    public bool UsernameAlreadyUsed { get; set; }
    public bool ValidationFailure { get; set; }
}