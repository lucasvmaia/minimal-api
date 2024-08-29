using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.Enums;

namespace minimal_api.Domain.ModelViews
{
    public record AdministradorModelView
    {
        public int Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        
        public string Perfil { get; set; } = default!;
    }
}