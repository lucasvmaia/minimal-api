using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Context;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interface;
using minimal_api.DTOs;

namespace minimal_api.Domain.Services
{
    public class AdministradorService : IAdministradorService
    {
        private readonly MyContext _myContext;

        public AdministradorService(MyContext myContext)
        {
            _myContext = myContext;
        }

        public Administrador? BuscarPorId(int id)
        {
            return _myContext.Administradores.Where(x => x.Id == id).FirstOrDefault();
        }

        public Administrador Incluir(Administrador administrador)
        {
            _myContext.Administradores.Add(administrador);
            _myContext.SaveChanges();
            return administrador;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _myContext.Administradores.Where(x => x.Email == loginDTO.Email && x.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;
        }

        public List<Administrador> Todos(int? pagina)
        {
            var query = _myContext.Administradores.AsQueryable();
            int itensPorPagina = 10;
            if(pagina != null)
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            
            return query.ToList();
        }
    }
}