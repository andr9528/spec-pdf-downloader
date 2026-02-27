using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pdf_downloader.Domain.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string email) 
            : base($"User with email '{email}' already exists.") {}
    }
}