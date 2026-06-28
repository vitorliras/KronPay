using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Banks
{
    public sealed record ConnectTokenResponse(
       string AccessToken
    );

}
