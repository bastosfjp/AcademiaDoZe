using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademiaDoZe.Domain.Exceptions
{
    // classe base para exceções de domínio
    // pemitindo exceções específicas de regras de negócio
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }
        public DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
