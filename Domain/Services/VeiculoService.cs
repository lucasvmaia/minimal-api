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
    public class VeiculoService : IVeiculoService
    {
        private readonly MyContext _myContext;

        public VeiculoService(MyContext myContext)
        {
            _myContext = myContext;
        }

        public void Apagar(Veiculo veiculo)
        {
            _myContext.Veiculos.Remove(veiculo);
            _myContext.SaveChanges();
        }

        public void Atualizar(Veiculo veiculo)
        {
            _myContext.Veiculos.Update(veiculo);
            _myContext.SaveChanges();
        }

        public Veiculo? BuscarPorId(int id)
        {
            return _myContext.Veiculos.Where(x=> x.Id == id).FirstOrDefault();
        }

        public void Incluir(Veiculo veiculo)
        {
            _myContext.Veiculos.Add(veiculo);
            _myContext.SaveChanges();
        }

        public List<Veiculo> Todos(int? pagina = 1, string nome = null, string marca = null)
        {
            var query = _myContext.Veiculos.AsQueryable();
            if(!string.IsNullOrEmpty(nome))
            {
                query = query.Where(x => EF.Functions.Like(x.Nome.ToLower(), $"%{nome}%"));
            }
            int itensPorPagina = 10;
            if(pagina != null)
                query = query.Skip((int)(pagina - 1) * itensPorPagina).Take(itensPorPagina);
            
            return query.ToList();
        }
    }
}