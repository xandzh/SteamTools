using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Models
{
    public class AppIdWithPublicKey
    {
        public Guid AppId { get; set; }

        public string? PublicKey { get; set; }
    }
}