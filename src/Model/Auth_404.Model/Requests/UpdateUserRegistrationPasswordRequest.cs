﻿using ServiceStack;

namespace Auth_404.Model.Requests
{
    [Route("/UpdateUserRegistrationEmailRequest", "POST")]
    public class UpdateUserRegistrationPasswordRequest : IReturn<UpdateUserRegistrationPasswordResponse>
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}